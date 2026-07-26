using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Server.DiscordServer;
using Server.GameInterfaces;
using TheOracle2;
using TheOracle2.Data;

namespace Server.Data;

public class PlayerDataFactory
{
    private readonly IDbContextFactory<ApplicationContext> dbFactory;
    private IAssetRepository Assets { get; }
    private IMoveRepository Moves { get; }
    private IOracleRepository Oracles { get; }
    public IEntityRepository Entities { get; }

    public PlayerDataFactory(IAssetRepository assets, IMoveRepository moves, IOracleRepository oracles, IEntityRepository entities, IDbContextFactory<ApplicationContext> dbFactory)
    {
        Assets = assets;
        Moves = moves;
        Oracles = oracles;
        Entities = entities;
        this.dbFactory = dbFactory;
    }

    private async Task<IronGame> ResolveGame(ulong PlayerId, IronGame? gameOverride)
    {
        using var db = dbFactory.CreateDbContext();
        return gameOverride ?? (await db.Players.FindAsync(PlayerId))?.Game ?? default;
    }

    public async Task<IEnumerable<Asset>> GetPlayerAssets(ulong PlayerId, IronGame? gameOverride = null)
    {
        var playerGame = await ResolveGame(PlayerId, gameOverride);
        var assets = Assets.GetAssetRoots().SelectMany(ar => ar.Assets);

        return assets.Where(a => playerGame.IncludesContentId(a.Id));
    }

    public async Task<IEnumerable<OracleGameEntity>> GetPlayerEntites(ulong PlayerId, IronGame? gameOverride = null)
    {
        var playerGame = await ResolveGame(PlayerId, gameOverride);
        var includedGames = playerGame.IncludedGames();

        return Entities.GetEntities().Where(a => includedGames.Contains(a.Game));
    }

    public async Task<IEnumerable<Move>> GetPlayerMoves(ulong PlayerId, IronGame? gameOverride = null)
    {
        var playerGame = await ResolveGame(PlayerId, gameOverride);

        var moves = Moves.GetMoveRoots().SelectMany(mr => mr.Moves)
            .Where(a => playerGame.IncludesContentId(a.Id)).ToList();

        return ApplyOverrides(moves, m => m.Id, m => m.Replaces);
    }

    public async Task<IEnumerable<Oracle>> GetPlayerOracles(ulong PlayerId, IronGame? gameOverride = null)
    {
        var playerGame = await ResolveGame(PlayerId, gameOverride);
        var playerOracles = Oracles.GetOracleRoots()
            .SelectMany(or => or.Oracles)
            .Where(a => playerGame.IncludesContentId(a.Id)).ToList();

        foreach(var cat in Oracles.GetOracleRoots().Where(or => or.Categories?.Count > 0).SelectMany(or => or.Categories))
        {
            if (playerGame.IncludesContentId(cat.Id))
            {
                playerOracles.AddRange(cat.Oracles);
            }
        }

        return ApplyOverrides(playerOracles, o => o.Id, o => o.Replaces);
    }

    public async Task<IEnumerable<OracleRoot>> GetPlayerOraclesRoots(ulong PlayerId, IronGame? gameOverride = null)
    {
        var playerGame = await ResolveGame(PlayerId, gameOverride);
        return Oracles.GetOracleRoots().Where(or => playerGame.IncludesContentId(or.Id));
    }

    // Removes base-game items that an in-scope (e.g. Sundered Isles) item supersedes
    // via its Replaces list, so the expansion's reskinned version wins.
    private static List<T> ApplyOverrides<T>(List<T> items, Func<T, string> idOf, Func<T, List<string>?> replacesOf)
    {
        var replacedIds = items
            .SelectMany(i => replacesOf(i) ?? Enumerable.Empty<string>())
            .ToHashSet();

        if (replacedIds.Count == 0) return items;

        return items.Where(i => !replacedIds.Contains(idOf(i))).ToList();
    }
}
