namespace API.DTOs;

public class FeedbackDto
{
    public string Username { get; set; }
    public DateOnly Date { get; set; }
    public int ScheduledDepTime { get; set; }
    public bool IsDelayedActual { get; set; }
}
