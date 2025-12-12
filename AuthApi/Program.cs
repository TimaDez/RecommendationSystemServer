using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApi.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===== Configuration (JWT) =====
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey     = jwtSection.GetValue<string>("Key")     ?? "super_dev_secret_change_me";
var jwtIssuer  = jwtSection.GetValue<string>("Issuer")  ?? "RecommendationSystem";
var jwtAudience= jwtSection.GetValue<string>("Audience")?? "RecommendationSystemClients";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

// ===== Services =====
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "Authentication service (sign-up & login) for the Recommendation System"
    });

    // JWT header in Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// EF Core – PostgreSQL (same DB server as others)
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Host=localhost;Port=5432;Database=recommendationdb;Username=admin;Password=7114";

    options.UseNpgsql(connectionString);
});

// JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = signingKey,
            ClockSkew                = TimeSpan.Zero
        };
    });

var app = builder.Build();

// ===== Middleware =====
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
    c.RoutePrefix = "swagger";
});

// No HTTPS redirection inside Docker for now
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health
app.MapGet("/health", () => Results.Ok("Auth API is running"));

app.Run();
