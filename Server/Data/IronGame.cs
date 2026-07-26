using System.Text.RegularExpressions;
using Discord.Interactions;

namespace TheOracle2;

public enum IronGame
{
    Ironsworn,
    Starforged,
    [ChoiceDisplay("Sundered Isles")]
    SunderedIsles
}

public static class IronGameExtenstions
{
    private static readonly Dictionary<string, IronGame> GameNamesMap = new()
    {
        ["Sundered Isles"] = IronGame.SunderedIsles,
        ["SunderedIsles"] = IronGame.SunderedIsles,
        ["Ironsworn"] = IronGame.Ironsworn,
        ["Starforged"] = IronGame.Starforged,
    };

    public static IronGame? GetIronGameInString(string value)
    {
        if (GameNamesMap.TryGetValue(value, out var game))
        {
            return game;
        }
        
        foreach(var enumValue in Enum.GetValues<IronGame>())
        {
            if (value.Contains(enumValue.ToString(), StringComparison.OrdinalIgnoreCase)) return game;
        }

        return null;
    }

    public static string RemoveIronGameInString(string value)
    {
        foreach (var game in GameNamesMap)
        {
            value = Regex.Replace(value, game.Key + " ?", "", RegexOptions.IgnoreCase);
        }

        return value.Trim();
    }

    // Sundered Isles is an expansion played on top of Starforged: a Sundered Isles
    // player uses Starforged's base content plus the Sundered Isles additions/overrides.
    // Every other game is self-contained.
    public static IReadOnlyList<IronGame> IncludedGames(this IronGame game) => game switch
    {
        IronGame.SunderedIsles => new[] { IronGame.Starforged, IronGame.SunderedIsles },
        _ => new[] { game },
    };

    // True if the given content id ("Starforged/Oracles/...", "SunderedIsles/Moves/...", etc.)
    // belongs to any game layer available to this player's selected game.
    public static bool IncludesContentId(this IronGame game, string id)
    {
        foreach (var included in game.IncludedGames())
        {
            if (id.Contains(included.ToString(), StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }
}
