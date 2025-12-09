using GatewayApi.Infrastructure;
using GatewayApi.Services;
using Microsoft.EntityFrameworkCore;

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
app.MapControllers();
app.MapGet("/", () => "Gateway API is running");

app.Run();