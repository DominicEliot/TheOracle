# Deploying the bot

The bot is a long-running .NET 10 process that holds a Discord **gateway**
(WebSocket) connection. It makes only outbound connections, so it needs **no
public IP, port, or domain** — just an always-on host and a Postgres database.
It creates its own database schema on first run, so an empty database is enough.

Configuration is read from environment variables (preferred for hosting) or,
for local runs, from `dbSettings.json` / `token.json`.

| Variable | Required | Purpose |
|---|---|---|
| `DiscordToken` | yes | Bot token from the Discord Developer Portal |
| `DATABASE_URL` | one of these | Full URL: `postgres://user:pass@host:port/db` (what Railway/Render/Neon/Fly give you). TLS is required and enabled automatically. |
| `dbConnectionString` + `dbPassword` | one of these | Npgsql-style string (`Host=...;Port=...;Database=...;Username=...`) plus the password, if you aren't using `DATABASE_URL` |

---

## Option A — Self-host with Docker Compose (local machine or a VPS)

Runs the bot **and** a Postgres database together.

```bash
cp .env.example .env
# edit .env: set DISCORD_TOKEN and DB_PASSWORD
docker compose up -d --build
docker compose logs -f bot     # to confirm it connected
```

Update after pulling new code: `docker compose up -d --build`.
Stop: `docker compose down` (data is kept in the `pgdata` volume).

## Option B — Managed platform

Host the bot process on a platform and use its managed Postgres. The image
builds straight from the `Dockerfile`; no web port is needed (it's a worker).

1. Create a **Postgres** instance on the platform (or a provider like Neon).
2. Create a service/worker from this repo (it auto-detects the `Dockerfile`).
   - **Railway:** "Deploy from repo" + add the Postgres plugin.
   - **Render:** create a **Background Worker** (not a Web Service) + Render Postgres.
   - **Fly.io:** `fly launch` (choose *no* public services) + `fly postgres`.
3. Set environment variables on the service:
   - `DiscordToken` = your bot token
   - `DATABASE_URL` = the connection URL the platform provides
     (Railway/Render often expose it automatically; otherwise paste it in).
4. Deploy and check the logs for the startup banner and "Ready".

## Discord setup

1. Create an application at <https://discord.com/developers/applications>, add a
   **Bot**, and copy its token into `DiscordToken`.
2. No privileged intents are required (the bot uses Guilds, GuildMessages,
   DirectMessages only).
3. Invite it with the `bot` + `applications.commands` scopes.

## First-run notes

- **Slash commands register globally** and can take up to ~1 hour to appear the first time.
- Logs go to stdout and to `logs/log.txt` inside the working directory.
