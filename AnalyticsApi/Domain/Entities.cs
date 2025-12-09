namespace AnalyticsApi.Domain;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
}

public class Event
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int ItemId { get; set; }

    public string EventType { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}