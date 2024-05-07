using System.Globalization;
using CsvHelper.Configuration;

namespace API.Entities;

public partial class Training
{
    public DateOnly Date { get; set; }

    public int ScheduledArrTime { get; set; }

    public string AirlineName { get; set; }

    public string TailNum { get; set; }

    public string OrgAirport { get; set; }

    public string DestAirport { get; set; }

    public int Distance { get; set; }

    public int ScheduledDepTime { get; set; }

    public double DepTemperature { get; set; }

    public double DepWindSpeed { get; set; }

    public double DepWindDirection { get; set; }

    public double DepPrecipitation { get; set; }

    public double DepRain { get; set; }

    public double DepSnowFall { get; set; }

    public double ArrTemperature { get; set; }

    public double ArrWindSpeed { get; set; }

    public double ArrWindDirection { get; set; }

    public double ArrPrecipitation { get; set; }

    public double ArrRain { get; set; }

    public double ArrSnowFall { get; set; }

    public bool IsDelayed { get; set; }

    public virtual Airline AirlineNameNavigation { get; set; }

    public virtual Airport DestAirportNavigation { get; set; }

    public virtual Airport OrgAirportNavigation { get; set; }

    public virtual TailNumber TailNumNavigation { get; set; }
}

public sealed class TrainingMap : ClassMap<Training>
{
    public TrainingMap()
    {
        Map(m => m.Date).Name("Date");
        Map(m => m.ScheduledArrTime).Name("ScheduledArrTime");
        Map(m => m.AirlineName).Name("AirlineName");
        Map(m => m.TailNum).Name("TailNum");
        Map(m => m.OrgAirport).Name("OrgAirport");
        Map(m => m.DestAirport).Name("DestAirport");
        Map(m => m.Distance).Name("Distance");
        Map(m => m.ScheduledDepTime).Name("ScheduledDepTime");
        Map(m => m.DepTemperature).Name("DepTemperature");
        Map(m => m.DepWindSpeed).Name("DepWindSpeed");
        Map(m => m.DepWindDirection).Name("DepWindDirection");
        Map(m => m.DepPrecipitation).Name("DepPrecipitation");
        Map(m => m.DepRain).Name("DepRain");
        Map(m => m.DepSnowFall).Name("DepSnowFall");
        Map(m => m.ArrTemperature).Name("ArrTemperature");
        Map(m => m.ArrWindSpeed).Name("ArrWindSpeed");
        Map(m => m.ArrWindDirection).Name("ArrWindDirection");
        Map(m => m.ArrPrecipitation).Name("ArrPrecipitation");
        Map(m => m.ArrRain).Name("ArrRain");
        Map(m => m.ArrSnowFall).Name("ArrSnowFall");
        Map(m => m.IsDelayed).Name("IsDelayed");
    }
}