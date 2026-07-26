# syntax=docker/dockerfile:1

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached unless the project file changes).
COPY ["Server/Server.csproj", "Server/"]
RUN dotnet restore "Server/Server.csproj"

# Build and publish the bot. Game data (oracles/moves/assets/entities) is copied to
# the output by the csproj, so it ends up in /app/publish/Data.
COPY . .
RUN dotnet publish "Server/Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The bot is a gateway (outbound-only) client — no ports are exposed.
# Configuration is supplied via environment variables (see DEPLOY.md):
#   DiscordToken        - required
#   DATABASE_URL        - postgres://user:pass@host:port/db   (or...)
#   dbConnectionString  - Host=...;Port=...;Database=...;Username=...
#   dbPassword          - password for dbConnectionString
ENTRYPOINT ["dotnet", "Server.dll"]
