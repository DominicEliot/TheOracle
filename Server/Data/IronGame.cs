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
    // Alternate, human-typed spellings for games whose enum name isn't a single word.
    private static readonly Dictionary<IronGame, string> AlternateNames = new()
    {
        [IronGame.SunderedIsles] = "Sundered Isles"
    };

    public static IronGame? GetIronGameInString(string value)
    {
        foreach(var game in Enum.GetValues<IronGame>())
        {
            if (value.Contains(game.ToString(), StringComparison.OrdinalIgnoreCase)) return game;
            if (AlternateNames.TryGetValue(game, out var alt) && value.Contains(alt, StringComparison.OrdinalIgnoreCase)) return game;
        }

        return null;
    }

    public static string RemoveIronGameInString(string value)
    {
        foreach (var game in Enum.GetValues<IronGame>())
        {
            value = Regex.Replace(value, game.ToString() + " ?", "", RegexOptions.IgnoreCase);
            if (AlternateNames.TryGetValue(game, out var alt))
            {
                value = Regex.Replace(value, alt + " ?", "", RegexOptions.IgnoreCase);
            }
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
