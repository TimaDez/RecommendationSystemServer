using AnalyticsApi.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException(
                "Missing configuration: Jwt:Key (Jwt__Key env var).");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Analytics API",
        Version = "v1",
        Description = "Analytics service for the Recommendation System"
    });
});

// DbContext – PostgreSQL
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
                           ?? "Host=localhost;Port=5432;Database=recommendationdb;Username=admin;Password=7114";

    options.UseNpgsql(connectionString);
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Swagger always on
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Analytics API v1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

// No HTTPS redirection in Docker to avoid that warning
// (add it back later for real prod)
app.MapControllers();

// Health
app.MapGet("/health", () => Results.Ok("Analytics API is running"));

// Debug endpoints listing (optional but useful)
app.MapGet("/debug/endpoints", (IEnumerable<EndpointDataSource> sources) =>
{
    var endpoints = sources
        .SelectMany(s => s.Endpoints)
        .OfType<RouteEndpoint>()
        .Select(e => new
        {
            RoutePattern = e.RoutePattern.RawText,
            DisplayName = e.DisplayName
        });

    return Results.Ok(endpoints);
});

app.Run();