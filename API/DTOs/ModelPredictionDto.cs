namespace API.DTOs;

public class ModelPredictionDto
{
    public DateOnly Date { get; set; }
    public int ScheduledArrTime { get; set; }
    public string AirlineName { get; set; }
    public string UniqueCarrier { get; set; }
    public string TailNum { get; set; }
    public string OrgAirport { get; set; }
    public string OrgAirportCode { get; set; }
    public string DestAirport { get; set; }
    public string DestAirportCode { get; set; }
    public int Distance { get; set; }
    public int ScheduledDepTime { get; set; }
    public double DepTemperature { get; set; } = 0.0;
    public double DepWindSpeed { get; set; } = 0.0;
    public double DepWindDirection { get; set; } = 0.0;
    public double DepPrecipitation { get; set; } = 0.0;
    public double DepRain { get; set; } = 0.0;
    public double DepSnowFall { get; set; } = 0.0;
    public double ArrTemperature { get; set; } = 0.0;
    public double ArrWindSpeed { get; set; } = 0.0;
    public double ArrWindDirection { get; set; } = 0.0;
    public double ArrPrecipitation { get; set; } = 0.0;
    public double ArrRain { get; set; } = 0.0;
    public double ArrSnowFall { get; set; } = 0.0;
}
