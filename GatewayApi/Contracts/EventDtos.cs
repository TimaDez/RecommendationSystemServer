namespace GatewayApi.Contracts;

public class CreateEventRequest
{
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public string EventType { get; set; } = string.Empty; // "view", "click", "like"
}

public class EventResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}