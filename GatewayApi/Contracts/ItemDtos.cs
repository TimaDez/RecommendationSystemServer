using System.ComponentModel.DataAnnotations;

namespace GatewayApi.Contracts;

public sealed record CreateItemRequest(
    [Required, MinLength(2), MaxLength(80)]
    string Name,
    [Required, MinLength(2), MaxLength(40)]
    string Category
);

public sealed record UpdateItemRequest(
    [Required, MinLength(2), MaxLength(80)] string Name,
    [Required, MinLength(2), MaxLength(40)] string Category
);

public sealed record ItemResponse(
    int Id,
    string Name,
    string Category
);
