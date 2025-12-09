namespace GatewayApi.Contracts;

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
}

// Used for GET /api/users and GET /api/users/{id}
public class UserResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Used only for POST /api/users
public class CreateUserResponse
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}