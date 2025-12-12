# === Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AuthApi/AuthApi.csproj", "AuthApi/"]
RUN dotnet restore "AuthApi/AuthApi.csproj"

COPY . .
RUN dotnet publish "AuthApi/AuthApi.csproj" -c Release -o /app/publish

# === Runtime stage ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AuthApi.dll"]
