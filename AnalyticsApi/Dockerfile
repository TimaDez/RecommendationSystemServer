# === Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["AnalyticsApi/AnalyticsApi.csproj", "AnalyticsApi/"]
RUN dotnet restore "AnalyticsApi/AnalyticsApi.csproj"

# Copy the rest of the source
COPY . .

# Publish AnalyticsApi
RUN dotnet publish "AnalyticsApi/AnalyticsApi.csproj" -c Release -o /app/publish

# === Runtime stage ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AnalyticsApi.dll"]
