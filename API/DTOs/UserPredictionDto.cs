namespace API.DTOs;

public class UserPredictionDto
{
    public DateOnly Date { get; set; }
    public int ScheduledArrTime { get; set; }
    public string DestAirportCode { get; set; }
    public string OrgAirportCode { get; set; }
    public int ScheduledDepTime { get; set; }
    public bool IsDelayedPredicted { get; set; }
    public bool? IsDelayedActual { get; set; }
}
