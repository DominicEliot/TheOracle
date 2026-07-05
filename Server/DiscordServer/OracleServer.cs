global using System.Linq;
global using Discord;
global using Microsoft.Extensions.DependencyInjection;
global using Newtonsoft.Json;
using System.Reflection;
using Discord.Interactions;
using Discord.Net.Queue;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using OracleCommands;
using Serilog;
using Server.Data;
using Server.DiscordServer;
using Server.OracleRoller;
using TheOracle2;

class OracleServer
{
    internal static Serilog.ILogger logger = new LoggerConfiguration().WriteTo.Console().CreateLogger(); //This gets overwritten by the ConfiguredServices

    public static async Task Main()
    {
        using var services = ConfigureServices();

        var client = services.GetRequiredService<DiscordSocketClient>();
        var commands = services.GetRequiredService<InteractionService>();
        var config = services.GetRequiredService<IConfiguration>();
        var handler = services.GetRequiredService<CommandHandler>();
        var db = services.GetRequiredService<ApplicationContext>();

        try
        {
            Log.Logger = logger;

            logger.Information($"Starting TheOracle v{Assembly.GetEntryAssembly()?.GetName().Version}");

            await db.Database.EnsureCreatedAsync().ConfigureAwait(false);

            await handler.Initialize().ConfigureAwait(false);

            await client.LoginAsync(TokenType.Bot, GetToken(services)).ConfigureAwait(false);

            client.Log += LogAsync;
            commands.Log += LogAsync;

            await client.StartAsync().ConfigureAwait(false);

            await client.SetGameAsync($"v{Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)}", "", ActivityType.Playing).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToString());
        }

        await Task.Delay(Timeout.Infinite);
    }

    private static Task LogAsync(LogMessage msg)
    {
        switch (msg.Exception)
        {
            case GatewayReconnectException gre:
                logger.Warning(gre.Message);
                return Task.CompletedTask;
            case TimeoutException te:
                logger.Warning(te.Message);
                return Task.CompletedTask;
            case Discord.Net.HttpException httpEx:
                string json = (httpEx.Request is JsonRestRequest jsonRequest) ? $"\n" + jsonRequest.Json : "";
                logger.Warning(httpEx, httpEx.Reason + json);
                return Task.CompletedTask;
            case Exception ex when ex.Message == "WebSocket connection was closed":
                logger.Warning(ex.Message);
                return Task.CompletedTask;
            default:
                break;
        }

        switch (msg.Severity)
        {
            case LogSeverity.Critical:
                logger.Fatal(msg.ToString());
                break;
            case LogSeverity.Error:
                logger.Error(msg.ToString());
                break;
            case LogSeverity.Warning:
                logger.Warning(msg.ToString());
                break;
            case LogSeverity.Info:
                logger.Information(msg.ToString());
                break;
            case LogSeverity.Verbose:
                logger.Verbose(msg.ToString());
                break;
            case LogSeverity.Debug:
                logger.Debug(msg.ToString());
                break;
            default:
                break;
        }
        return Task.CompletedTask;
    }

    static ServiceProvider ConfigureServices()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("token.json", optional: true, reloadOnChange: true)
            .AddJsonFile("dbSettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var dbConnectionString = ResolveNpgsqlConnectionString(config);

        var clientConfig = new DiscordSocketConfig { MessageCacheSize = 100, LogLevel = LogSeverity.Info, GatewayIntents = GatewayIntents.DirectMessages | GatewayIntents.GuildMessages | GatewayIntents.Guilds };
        var interactionServiceConfig = new InteractionServiceConfig() { UseCompiledLambda = true, LogLevel = LogSeverity.Info, AutoServiceScopes = true };

        logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.File(Path.Combine("logs", "log.txt"), rollingInterval: RollingInterval.Day)
                    .Enrich.FromLogContext()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .CreateLogger();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(interactionServiceConfig)
            .AddSingleton<IRestClientProvider>(sp => sp.GetRequiredService<DiscordSocketClient>())
            .AddSingleton<InteractionService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<Random>()
            .AddSingleton<IOracleRoller, RandomOracleRoller>()
            .AddSingleton<IOracleRepository, JsonOracleRepository>()
            .AddSingleton<IMoveRepository, JsonMoveRepository>()
            .AddSingleton<IAssetRepository, JsonAssetRepository>()
            .AddSingleton<IEmoteRepository, HardCodedEmoteRepo>()
            .AddSingleton<IEntityRepository, JsonEntityRepository>()
            .AddSingleton<IMemoryCache, MemoryCache>()
            .AddScoped<PlayerDataFactory>()
            .AddDbContextFactory<ApplicationContext>()
            .AddLogging(builder => builder.AddSerilog(logger)
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                .AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning)
                )
            .AddDbContext<ApplicationContext>(options => options.UseNpgsql(dbConnectionString))
            .BuildServiceProvider();
    }

    // Resolves the Postgres connection string, in order of preference:
    //  1. DATABASE_URL (postgres://user:pass@host:port/db) as handed out by Railway,
    //     Render, Neon, Fly, etc. — the easiest path for hosted deployments.
    //  2. dbConnectionString + dbPassword, from environment variables or dbSettings.json.
    private static string ResolveNpgsqlConnectionString(IConfiguration config)
    {
        var url = config.GetSection("DATABASE_URL").Value;
        if (!string.IsNullOrWhiteSpace(url) &&
            (url.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
             url.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':', 2);
            return new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
                // Hosted Postgres requires TLS; trust the provider's certificate by default.
                SslMode = SslMode.Require,
                TrustServerCertificate = true,
            }.ConnectionString;
        }

        var dbConn = config.GetSection("dbConnectionString").Value;
        if (string.IsNullOrWhiteSpace(dbConn))
        {
            throw new InvalidOperationException(
                "No database configuration found. Set DATABASE_URL, or provide dbConnectionString " +
                "and dbPassword via environment variables or a dbSettings.json file.");
        }

        var dbPass = config.GetSection("dbPassword").Value;
        return new NpgsqlConnectionStringBuilder(dbConn) { Password = dbPass }.ConnectionString;
    }

    private static string GetToken(IServiceProvider services)
    {
        var token = Environment.GetEnvironmentVariable("DiscordToken");
        token ??= services.GetRequiredService<IConfiguration>().GetSection("DiscordToken").Value;

        if (token == null)
        {
            if (Console.IsInputRedirected)
            {
                throw new InvalidOperationException(
                    "DiscordToken is not set. Provide it via the DiscordToken environment variable " +
                    "(recommended for hosted deployments) or a token.json file.");
            }

            Console.WriteLine($"Couldn't find a discord token. Please enter it here. (It will be saved to the token.json file in your bin folder)");
            token = Console.ReadLine();

            var json = JsonConvert.SerializeObject(new { DiscordToken = token });
            File.WriteAllText("token.json", json);
        }

        return token ?? string.Empty;
    }
}
