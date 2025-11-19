# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["GetIpApi.csproj", "./"]
COPY ["getip.sln", "./"]
RUN dotnet restore GetIpApi.csproj


# Copy everything else and build
COPY . .

# Publish stage
FROM build AS publish
RUN dotnet publish "GetIpApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

# Run as non-root user for security
USER $APP_UID

ENTRYPOINT ["dotnet", "GetIpApi.dll"]
