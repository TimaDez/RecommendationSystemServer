using GatewayApi.Infrastructure;
using GatewayApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// AI service URL – default is localhost:8000 for local dev
var aiBaseUrl = builder.Configuration["AiService:BaseUrl"] ?? "http://localhost:8000";

builder.Services.AddHttpClient<RecommendationClient>(client =>
{
    client.BaseAddress = new Uri(aiBaseUrl);
});

// ✅ STEP 4: JWT Authentication + Authorization 
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKeyRaw = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKeyRaw))
            throw new InvalidOperationException("Missing configuration: Jwt:Key (Jwt__Key env var).");

        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKeyRaw))
            throw new InvalidOperationException("Missing configuration: Jwt:Key (Jwt__Key env var).");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyRaw)),

            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthorization();

var app = builder.Build();

// Apply migrations automatically on startup (dev-friendly)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Swagger etc.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ STEP 4: middleware order matters
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();  // routes like /api/Users -> analyticsapi
app.MapControllers();
app.MapGet("/", () => "Gateway API is running");

app.Run();
