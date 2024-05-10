from flask import Flask, request
import pandas as pd
import numpy as np
import openmeteo_requests
import requests_cache
from retry_requests import retry
import requests
import pickle
from datetime import datetime, timedelta
from fitter import Fitter, get_common_distributions
from sklearn.preprocessing import StandardScaler
from sklearn.preprocessing import MinMaxScaler
from sklearn.ensemble import RandomForestClassifier

pd.set_option("future.no_silent_downcasting", True)

app = Flask(__name__)


def convertToDataframe(data):
    if isinstance(data, dict) and all(
        not isinstance(val, (list, dict)) for val in data.values()
    ):
        df = pd.DataFrame([data])  # Wrap the dict inside a list
    else:
        df = pd.DataFrame(data)

    return df


def insert_scheduled_elapsed_time(df: pd.DataFrame):
    # Convert columns to string type
    df["ScheduledArrTime"] = df["ScheduledArrTime"].astype(str)
    df["ScheduledDepTime"] = df["ScheduledDepTime"].astype(str)

    # Pad columns with leading zeros to ensure it has 4 digits
    df["ScheduledArrTime"] = df["ScheduledArrTime"].str.zfill(4)
    df["ScheduledDepTime"] = df["ScheduledDepTime"].str.zfill(4)

    # Replace '2400' with '0000' in columns
    df["ScheduledArrTime"] = df["ScheduledArrTime"].replace("2400", "0000")
    df["ScheduledDepTime"] = df["ScheduledDepTime"].replace("2400", "0000")

    # Convert columns to datetime format
    df["ScheduledArrTime"] = pd.to_datetime(df["ScheduledArrTime"], format="%H%M")
    df["ScheduledDepTime"] = pd.to_datetime(df["ScheduledDepTime"], format="%H%M")

    # Calculate the scheduled elapsed time and create a new column 'ScheduledElapsedTime'
    df["ScheduledElapsedTime"] = (
        (
            df["ScheduledArrTime"] - df["ScheduledDepTime"] + pd.Timedelta(days=1)
        ).dt.total_seconds()
        / 60
    ).astype(int)

    # Use modulo operation to limit the elapsed time within 24 hours
    df["ScheduledElapsedTime"] = df["ScheduledElapsedTime"] % (24 * 60)

    # Convert 'ScheduledArrTime' and 'ScheduledDepTime' back to the original format
    df["ScheduledArrTime"] = df["ScheduledArrTime"].dt.strftime("%H%M")
    df["ScheduledDepTime"] = df["ScheduledDepTime"].dt.strftime("%H%M")

    # Convert 'ScheduledArrTime' and 'ScheduledDepTime' to int
    df["ScheduledArrTime"] = df["ScheduledArrTime"].astype(int)
    df["ScheduledDepTime"] = df["ScheduledDepTime"].astype(int)


def expand_date_col(df: pd.DataFrame):
    # Convert the date column to datetime
    df["Date"] = pd.to_datetime(df["Date"], format="%Y-%m-%d")

    # Create the Day, Month and Year columns
    df["Day"] = df["Date"].dt.day
    df["Month"] = df["Date"].dt.month


def insert_DayOfWeek(df: pd.DataFrame):
    df["DayOfWeek"] = df["Date"].dt.dayofweek + 1


def geocode(address):
    url = "https://maps.googleapis.com/maps/api/geocode/json"
    params = {"address": address, "key": "AIzaSyCeWJLbBvTsN3WoA7R8y4M3DzGkKQHJp80"}
    response = requests.get(url, params=params)
    if response.status_code == 200:
        data = response.json()
        if "results" in data and len(data["results"]) > 0:
            location = data["results"][0]["geometry"]["location"]
            return location["lat"], location["lng"]
    return None, None


def get_weather_conditions(lat, long, start_date, end_date):
    # Setup the Open-Meteo API client with cache and retry on error
    cache_session = requests_cache.CachedSession(".cache", expire_after=-1)
    retry_session = retry(cache_session, retries=5, backoff_factor=0.2)
    openmeteo = openmeteo_requests.Client(session=retry_session)

    date_object = datetime.strptime(start_date, "%Y-%m-%d")
    yesterday = datetime.today() - timedelta(days=1)

    if date_object < yesterday:
        url = "https://archive-api.open-meteo.com/v1/archive"
    else:
        url = "https://api.open-meteo.com/v1/gfs"

    params = {
        "latitude": lat,
        "longitude": long,
        "start_date": start_date,
        "end_date": end_date,
        "hourly": [
            "temperature_2m",
            "precipitation",
            "rain",
            "snowfall",
            "wind_speed_10m",
            "wind_direction_10m",
        ],
    }
    responses = openmeteo.weather_api(url, params=params)

    # Process hourly data. The order of variables needs to be the same as requested.
    hourly = responses[0].Hourly()
    hourly_temperature_2m = hourly.Variables(0).ValuesAsNumpy()  # type:ignore
    hourly_precipitation = hourly.Variables(1).ValuesAsNumpy()  # type:ignore
    hourly_rain = hourly.Variables(2).ValuesAsNumpy()  # type:ignore
    hourly_snowfall = hourly.Variables(3).ValuesAsNumpy()  # type:ignore
    hourly_wind_speed_10m = hourly.Variables(4).ValuesAsNumpy()  # type:ignore
    hourly_wind_direction_10m = hourly.Variables(5).ValuesAsNumpy()  # type:ignore

    hourly_data = {
        "date": pd.date_range(
            start=pd.to_datetime(hourly.Time(), unit="s", utc=True),  # type: ignore
            end=pd.to_datetime(hourly.TimeEnd(), unit="s", utc=True),  # type: ignore
            freq=pd.Timedelta(seconds=hourly.Interval()),  # type: ignore
            inclusive="left",
        )
    }
    hourly_data["Temperature"] = hourly_temperature_2m  # type:ignore
    hourly_data["Precipitation"] = hourly_precipitation  # type:ignore
    hourly_data["Rain"] = hourly_rain  # type:ignore
    hourly_data["SnowFall"] = hourly_snowfall  # type:ignore
    hourly_data["WindSpeed"] = hourly_wind_speed_10m  # type:ignore
    hourly_data["WindDirection"] = hourly_wind_direction_10m  # type:ignore

    return pd.DataFrame(data=hourly_data)


def correct_airport_names(df: pd.DataFrame):
    df["OrgAirport"] = df["OrgAirport"].replace(
        {
            "Rogue Valley International Airport": "Rogue Valley International Medford Airport"
        }
    )
    df["OrgAirport"] = df["OrgAirport"].replace(
        {
            "Gen. Edward Lawrence Logan International Airport": "Boston Logan International Airport"
        }
    )

    df["DestAirport"] = df["DestAirport"].replace(
        {
            "Rogue Valley International Airport": "Rogue Valley International Medford Airport"
        }
    )
    df["DestAirport"] = df["DestAirport"].replace(
        {
            "Gen. Edward Lawrence Logan International Airport": "Boston Logan International Airport"
        }
    )


def insert_weather_conditions_cols(df: pd.DataFrame):
    weather_columns = [
        "Temperature",
        "WindSpeed",
        "WindDirection",
        "Precipitation",
        "Rain",
        "SnowFall",
    ]
    for prefix in ["Dep", "Arr"]:
        for column in weather_columns:
            df[f"{prefix}{column}"] = None
    return weather_columns


def create_locations_dict(unique_locations, df: pd.DataFrame):
    locations_dict = {value: pd.DataFrame() for value in unique_locations}
    for i in range(len(unique_locations)):
        lat, long = geocode(unique_locations[i])

        if lat != None and long != None:
            locations_dict[unique_locations[i]] = get_weather_conditions(
                lat,
                long,
                str(df["Date"].iloc[0]).split(" ")[0],
                str(df["Date"].iloc[0]).split(" ")[0],
            )
    return locations_dict


def add_weather_condtions(locations_dict, weather_columns, df):
    j = 0
    for index, row in df.iterrows():
        if (
            not locations_dict[row["OrgAirport"]].empty
            and not locations_dict[row["DestAirport"]].empty
        ):
            # Create temporary DataFrames for the operation
            df_row = pd.DataFrame(row).transpose().copy()
            dep_df = locations_dict[row["OrgAirport"]].copy()
            arr_df = locations_dict[row["DestAirport"]].copy()

            df_row["ScheduledDepTime"] = df_row["ScheduledDepTime"].astype(str)
            df_row["ScheduledDepTime"] = df_row["ScheduledDepTime"].str.zfill(4)

            df_row["ScheduledArrTime"] = df_row["ScheduledArrTime"].astype(str)
            df_row["ScheduledArrTime"] = df_row["ScheduledArrTime"].str.zfill(4)

            df_row["ScheduledDepTime"] = pd.to_timedelta(
                str(df_row["ScheduledDepTime"].values[0])[:2]
                + ":"
                + str(df_row["ScheduledDepTime"].values[0])[2:]
                + ":00"
            )
            df_row["ScheduledArrTime"] = pd.to_timedelta(
                str(df_row["ScheduledArrTime"].values[0])[:2]
                + ":"
                + str(df_row["ScheduledArrTime"].values[0])[2:]
                + ":00"
            )

            df_row["DepDateTime"] = df_row["Date"] + df_row["ScheduledDepTime"]
            df_row["ArrDateTime"] = df_row["Date"] + df_row["ScheduledArrTime"]

            # Convert 'Date' column in df2 to Datetime format without timezone
            dep_df["date"] = dep_df["date"].dt.tz_convert(None)
            arr_df["date"] = arr_df["date"].dt.tz_convert(None)

            # # Extract Date and hour from 'DateTime' in df1 and 'Date' in df2
            df_row["DepDateTime"] = pd.to_datetime(df_row["DepDateTime"]).dt.floor("h")
            df_row["ArrDateTime"] = pd.to_datetime(df_row["ArrDateTime"]).dt.floor("h")
            dep_df["date"] = dep_df["date"].dt.floor("h")
            arr_df["date"] = arr_df["date"].dt.floor("h")

            # # Merge the two DataFrames on the Datetime column
            dep_weather = pd.merge(
                df_row, dep_df, left_on="DepDateTime", right_on="date"
            )
            arr_weather = pd.merge(
                df_row, arr_df, left_on="ArrDateTime", right_on="date"
            )

            # # Drop the temporary columns
            dep_weather = dep_weather.drop(columns=["DepDateTime", "date"])
            arr_weather = arr_weather.drop(columns=["ArrDateTime", "date"])

            for column in weather_columns:
                df.at[index, f"Dep{column}"] = dep_weather[column][0]
                df.at[index, f"Arr{column}"] = arr_weather[column][0]

            j += 1
        else:
            for column in weather_columns:
                df.at[index, f"Dep{column}"] = 0
                df.at[index, f"Arr{column}"] = 0


def create_weather_conditons_cols(df: pd.DataFrame):
    correct_airport_names(df)

    unique_locations = (
        pd.concat([df["OrgAirport"], df["DestAirport"]]).unique().tolist()
    )

    # Create new columns in the DataFrame for weather conditions
    weather_columns = insert_weather_conditions_cols(df)

    # Create a dictionary with unique locations as keys and empty DataFrames as values
    locations_dict = create_locations_dict(unique_locations, df)

    # Match the weather conditions with the departure and arrival times
    add_weather_condtions(locations_dict, weather_columns, df)


def OHE_Prediction(df):
    with open("Utilities/column-names.txt", "r") as file:
        text = file.read()

    Columns_List = text.split("\n")

    Cols_List = [item.strip() for item in Columns_List if item.strip()]

    train_df = pd.DataFrame(columns=Cols_List, index=range(0, len(df)))

    train_df["ScheduledArrTime"] = df["ScheduledArrTime"]
    train_df["ScheduledDepTime"] = df["ScheduledDepTime"]
    train_df["ScheduledElapsedTime"] = df["ScheduledElapsedTime"]
    train_df["Distance"] = df["Distance"]
    train_df["Day"] = df["Date"].dt.day
    train_df["DepTemperature"] = df["DepTemperature"]
    train_df["DepWindSpeed"] = df["DepWindSpeed"]
    train_df["DepWindDirection"] = df["DepWindDirection"]
    train_df["DepPrecipitation"] = df["DepPrecipitation"]
    train_df["DepRain"] = df["DepRain"]
    train_df["DepSnowFall"] = df["DepSnowFall"]
    train_df["ArrTemperature"] = df["ArrTemperature"]
    train_df["ArrWindSpeed"] = df["ArrWindSpeed"]
    train_df["ArrWindDirection"] = df["ArrWindDirection"]
    train_df["ArrPrecipitation"] = df["ArrPrecipitation"]
    train_df["ArrRain"] = df["ArrRain"]
    train_df["ArrSnowFall"] = df["ArrSnowFall"]

    for index, row in df.iterrows():
        DP_Airline = "UniqueCarrier_" + row["UniqueCarrier"]
        train_df.at[index, DP_Airline] = 1

        DP_TailNum = "TailNum_" + row["TailNum"]
        train_df.at[index, DP_TailNum] = 1

        DP_Origin = "OrgAirportCode_" + row["OrgAirportCode"]
        train_df.at[index, DP_Origin] = 1

        DP_Dest = "DestAirportCode_" + row["DestAirportCode"]
        train_df.at[index, DP_Dest] = 1

        DP_Weekday = "DayOfWeek_" + str(row["DayOfWeek"])
        train_df.at[index, DP_Weekday] = 1

        DP_Month = "Month_" + str(row["Date"].month)
        train_df.at[index, DP_Month] = 1

        train_df.fillna(0, inplace=True)

    return train_df


def OHE_Training(df):
    # Extract day from date
    df["Day"] = df["Date"].dt.day

    # One-hot encoding for categorical columns
    categorical_cols = ["UniqueCarrier", "TailNum", "OrgAirportCode", "DestAirportCode"]
    for col in categorical_cols:
        dummies = pd.get_dummies(df[col], prefix=col, dtype="int8")
        df = pd.concat([df, dummies], axis=1)

    # One-hot encoding for DayOfWeek and Month
    df = pd.concat(
        [df, pd.get_dummies(df["DayOfWeek"], prefix="DayOfWeek", dtype="int8")], axis=1
    )
    df = pd.concat(
        [df, pd.get_dummies(df["Date"].dt.month, prefix="Month", dtype="int8")], axis=1
    )

    df.drop(
        columns=[
            "UniqueCarrier",
            "TailNum",
            "OrgAirportCode",
            "DestAirportCode",
            "DayOfWeek",
            "Date",
            "AirlineName",
            "OrgAirport",
            "DestAirport",
            "Month",
        ],
        inplace=True,
    )

    return df


def normalize_row(df: pd.DataFrame):
    stats = pd.read_csv("Utilities/stats.csv")

    for row in range(len(stats)):
        column = stats["Column_Name"].iloc[row]
        if stats["Distribution"].iloc[row] == "norm":
            mean = stats["Mean"].iloc[row]
            sd = stats["Standard_Deviation"].iloc[row]
            df[column] = (df[column] - mean) / sd

        elif stats["Distribution"].iloc[row] == "uniform":
            if df[column].iloc[0] < stats["Min"].iloc[row]:
                stats.at[row, "Min"] = df[column]
            elif df[column].iloc[0] > stats["Max"].iloc[row]:
                stats.at[row, "Max"] = df[column]

            minValue = stats["Min"].iloc[row]
            maxValue = stats["Max"].iloc[row]
            df[column] = (df[column] - minValue) / (maxValue - minValue)

        else:
            data = np.array(df[column]).reshape(-1, 1)
            df[column] = np.log(np.abs(np.float32(data.flatten())) + 0.1)

    stats.to_csv("Utilities/stats.csv", index=False)


def get_normalized_data(data, dist):
    if dist == "uniform":
        return MinMaxScaler().fit_transform(data)
    elif dist == "norm":
        return StandardScaler().fit_transform(data)
    else:
        return np.log(np.abs(np.float32(data.flatten())) + 1)


def get_best_distribution(df: pd.DataFrame):
    numeric_columns = [
        "ScheduledArrTime",
        "ScheduledDepTime",
        "ScheduledElapsedTime",
        "Distance",
        "DepTemperature",
        "DepWindSpeed",
        "DepWindDirection",
        "DepPrecipitation",
        "DepRain",
        "DepSnowFall",
        "ArrTemperature",
        "ArrWindSpeed",
        "ArrWindDirection",
        "ArrPrecipitation",
        "ArrRain",
        "ArrSnowFall",
        "Day",
    ]

    columns_distributions_dict = {column: "" for column in numeric_columns}

    for column in numeric_columns:
        print("###### " + column + " ######")

        data = df[column].values

        f = Fitter(
            data,
            distributions=get_common_distributions(),
        )
        f.fit()
        f.summary(plot=False)
        dist = f.get_best(method="sumsquare_error")
        best_dist = ""
        for key in dist.keys():
            best_dist = key

        columns_distributions_dict[column] = str(best_dist)
        print(column)
        print(f"Best Distribution: {best_dist}")
        print()

    return columns_distributions_dict


def normalize_df(df: pd.DataFrame):
    columns_distributions_dict = get_best_distribution(df)

    for column in columns_distributions_dict.keys():
        data = np.array(df[column]).reshape(-1, 1)
        df[column] = get_normalized_data(
            data=data, dist=columns_distributions_dict[column]
        )

    return columns_distributions_dict


def compute_stats(df: pd.DataFrame, dist_dict: dict):
    stats = []
    for c in dist_dict:

        mean_value = df[c].mean()
        min_value = df[c].min()
        max_value = df[c].max()
        sd = df[c].std()
        dist = dist_dict[c]

        stats.append(
            {
                "Column_Name": c,
                "Min": min_value,
                "Max": max_value,
                "Mean": mean_value,
                "Standard_Deviation": sd,
                "Distribution": dist,
            }
        )

    stats_df = pd.DataFrame(stats)
    stats_df.to_csv("Utilities/stats.csv", index=False)


def preprocessForPrediction(data):
    df = convertToDataframe(data)
    insert_scheduled_elapsed_time(df)
    expand_date_col(df)
    insert_DayOfWeek(df)
    create_weather_conditons_cols(df)
    train_df_dto = pd.DataFrame()
    train_df_dto["ScheduledArrTime"] = df["ScheduledArrTime"]
    train_df_dto["ScheduledDepTime"] = df["ScheduledDepTime"]
    train_df_dto["ScheduledElapsedTime"] = df["ScheduledElapsedTime"]
    train_df_dto["Distance"] = df["Distance"]
    train_df_dto["Day"] = df["Date"].dt.day
    train_df_dto["DepTemperature"] = df["DepTemperature"]
    train_df_dto["DepWindSpeed"] = df["DepWindSpeed"]
    train_df_dto["DepWindDirection"] = df["DepWindDirection"]
    train_df_dto["DepPrecipitation"] = df["DepPrecipitation"]
    train_df_dto["DepRain"] = df["DepRain"]
    train_df_dto["DepSnowFall"] = df["DepSnowFall"]
    train_df_dto["ArrTemperature"] = df["ArrTemperature"]
    train_df_dto["ArrWindSpeed"] = df["ArrWindSpeed"]
    train_df_dto["ArrWindDirection"] = df["ArrWindDirection"]
    train_df_dto["ArrPrecipitation"] = df["ArrPrecipitation"]
    train_df_dto["ArrRain"] = df["ArrRain"]
    train_df_dto["ArrSnowFall"] = df["ArrSnowFall"]
    train_df = OHE_Prediction(df)
    train_df.drop(columns=["IsDelayed"], inplace=True)
    normalize_row(train_df)
    return train_df, train_df_dto


def preprocessForTraining(data):
    df = convertToDataframe(data)
    insert_scheduled_elapsed_time(df)
    expand_date_col(df)
    insert_DayOfWeek(df)
    train_df = df.copy()
    train_df = OHE_Training(train_df)
    dist_dict = normalize_df(train_df)
    compute_stats(df, dist_dict)
    column = train_df.pop("IsDelayed")
    train_df.insert(len(train_df.columns), "IsDelayed", column)
    X = train_df.iloc[:, :-1]
    y = train_df.iloc[:, -1]
    float_columns = X.select_dtypes(include=["float64", "float32"]).columns
    X[float_columns] = X[float_columns].astype("float16")
    return X, y


def getPrediction(df: pd.DataFrame):
    with open("model.pkl", "rb") as file:
        model = pickle.load(file)

    prediction = model.predict(df)
    return prediction


def TrainModel(X: pd.DataFrame, y: pd.Series):
    with open("model.pkl", "rb") as file:
        model = pickle.load(file)

    print("Training...")
    model.fit(X, y)

    print("Finished training")
    with open("model.pkl", "wb") as file:
        pickle.dump(model, file)

    return True


@app.route("/predict", methods=["POST"])
def Predict():
    data = request.json
    train_df, train_df_dto = preprocessForPrediction(data)
    prediction = getPrediction(train_df.to_numpy())
    train_df_dto["IsDelayedPredicted"] = prediction
    return train_df_dto.to_dict("list")


@app.route("/train", methods=["POST"])
def Tain():
    data = request.json
    X, y = preprocessForTraining(data)
    if TrainModel(X, y):
        return "1"

    return "0"


@app.route("/")
def index():
    return "Hello, World"


if __name__ == "__main__":
    app.run(debug=True, port=4000)
