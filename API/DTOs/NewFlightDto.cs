namespace API.DTOs;

public class NewFlightDto
{
    public string Username { get; set; }

    public DateOnly Date { get; set; }

    public int ScheduledArrTime { get; set; }

    public string AirlineName { get; set; }

    public string TailNum { get; set; }

    public string OrgAirport { get; set; }

    public string DestAirport { get; set; }

    public int Distance { get; set; }

    public int ScheduledDepTime { get; set; }
}
