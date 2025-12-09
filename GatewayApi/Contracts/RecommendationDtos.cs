namespace GatewayApi.Contracts;

public class UserEventForRecommendationDto
{
    public int ItemId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class RecommendationRequestDto
{
    public int UserId { get; set; }
    public List<UserEventForRecommendationDto> Events { get; set; } = new();
}

public class RecommendationItemDto
{
    public int ItemId { get; set; }
    public double Score { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RecommendationResponseDto
{
    public int UserId { get; set; }
    public List<RecommendationItemDto> Recommendations { get; set; } = new();
}

// This is what YOUR API will return to the client:
public class RecommendationItemDetailedDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Dictionary<string, int> EventCounts { get; set; } = new();
}

public class RecommendationDetailedResponseDto
{
    public int UserId { get; set; }
    public List<RecommendationItemDetailedDto> Recommendations { get; set; } = new();
}