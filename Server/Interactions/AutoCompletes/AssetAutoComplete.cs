using Discord.Interactions;
using Server.Data;
using Server.DiscordServer;

namespace TheOracle2;

public class AssetAutoComplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var assets = services.GetRequiredService<PlayerDataFactory>();
        try
        {
            IEnumerable<AutocompleteResult> successList = new List<AutocompleteResult>();
            var userText = autocompleteInteraction.Data.Current.Value as string;
            var userId = autocompleteInteraction.User.Id;
            var guildId = context.Guild?.Id ?? autocompleteInteraction.User.Id;

            if (assets == null) return (AutocompletionResult.FromSuccess(successList));

            if (userText?.Length > 0)
            {
                var game = IronGameExtenstions.GetIronGameInString(userText);
                userText = IronGameExtenstions.RemoveIronGameInString(userText);
                successList = (await assets.GetPlayerAssets(context.User.Id, game))
                        .Where(m => m.Name.Contains(userText, StringComparison.OrdinalIgnoreCase) || m.Parent?.Name.Contains(userText, StringComparison.OrdinalIgnoreCase) == true)
                        .OrderBy(m => m.Name)
                        .Take(SelectMenuBuilder.MaxOptionCount)
                        .Select(m => new AutocompleteResult($"{m.Name} [{m.Parent?.Name}]", m.Id.ToString()));
            }

            return (AutocompletionResult.FromSuccess(successList));
        }
        catch (Exception ex)
        {
            return (AutocompletionResult.FromError(ex));
        }
    }
}
