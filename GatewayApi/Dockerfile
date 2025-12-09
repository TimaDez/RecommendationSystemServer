# Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["GatewayApi/GatewayApi.csproj", "GatewayApi/"]
RUN dotnet restore "GatewayApi/GatewayApi.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/GatewayApi"
RUN dotnet build "GatewayApi.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "GatewayApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080


ENTRYPOINT ["dotnet", "GatewayApi.dll"]
