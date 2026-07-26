using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheOracle2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Data;
using Server.OracleRoller;
using TheOracle2.Data;

namespace TheOracle2.Tests;

[TestClass()]
public class OracleCommandTests
{
    IOracleRepository oracles;
    IMoveRepository moves;
    public OracleCommandTests()
    {
        oracles = new JsonOracleRepository();
        moves = new JsonMoveRepository();
    }

    [TestMethod()]
    //[DataRow("Ironsworn/Oracles/Name/Ironlander/A")]
    //[DataRow("Ironsworn/Oracles/Moves/Pay_the_Price")]
    //[DataRow("Starforged/Oracles/Vaults/Interior/First_Look")]
    [DataRow("Starforged/Oracles/Planets/Desert/Settlements/Outlands")]
    [DataRow("SunderedIsles/Oracles/Cave/Type")]
    [DataRow("SunderedIsles/Oracles/Character/Name/Given_Name")]
    public void RollOracleTest(string oracle)
    {
        var firstOracle = oracles.GetOracleById(oracle);
        Assert.IsNotNull(firstOracle);
    }

    [TestMethod()]
    //[DataRow("Guild", "Starforged/Oracles/Factions/Guild")]
    //[DataRow("Vault", "Starforged/Oracles/Vaults/Interior/First_Look")]
    [DataRow("Observed", "Starforged/Oracles/Planets/Desert/Observed_From_Space")]
    [DataRow("Sundered Isles Cave Type", "SunderedIsles/Oracles/Cave/Type")]
    public void OracleSearchResultsTest(string query, string desiredOption)
    {
        var desiredOracle = oracles.GetOracleById(desiredOption);
        Assert.IsNotNull(desiredOracle);

        var game = IronGameExtenstions.GetIronGameInString(query);
        var gameName = game?.ToString();
        var remainder = IronGameExtenstions.RemoveIronGameInString(query);
        var searchResults = oracles.GetOracles()
            .Where(o => gameName == null || o.Id.Contains(gameName, StringComparison.OrdinalIgnoreCase))
            .GetOraclesFromUserInput(remainder);

        Assert.IsTrue(searchResults.Any(sr => sr.Id == desiredOption), $"Couldn't find {desiredOption} in {query} results");
        Assert.AreEqual(1, searchResults.Count(sr => sr.Id == desiredOption));
    }

    [TestMethod()]
    public void SunderedIslesRootsLoadTest()
    {
        var roots = oracles.GetOracleRoots().Where(r => r.Id.Contains("SunderedIsles", StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.AreEqual(16, roots.Count);
        Assert.IsTrue(roots.All(r => r.Oracles?.Count > 0));
    }

    [TestMethod()]
    public void SunderedIslesEveryLeafTableRollsTest()
    {
        var roller = new RandomOracleRoller(new Random(), oracles, new HardCodedEmoteRepo());
        var leaves = new List<Oracle>();

        void collect(IEnumerable<Oracle> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Table?.Count > 0) leaves.Add(node);
                if (node.Oracles?.Count > 0) collect(node.Oracles);
            }
        }

        var sunderedRoots = oracles.GetOracleRoots().Where(r => r.Id.Contains("SunderedIsles", StringComparison.OrdinalIgnoreCase));
        foreach (var root in sunderedRoots) collect(root.Oracles);

        Assert.IsTrue(leaves.Count > 200, $"Expected 200+ leaf tables, found {leaves.Count}");

        foreach (var leaf in leaves)
        {
            for (int i = 0; i < 5; i++)
            {
                var result = roller.GetRollResult(leaf);
                Assert.IsNotNull(result.Description, $"{leaf.Id} produced a null result on roll {result.Roll}");
            }
        }
    }

    [TestMethod()]
    public void SunderedIslesMovesLoadTest()
    {
        var siMoves = moves.GetMoves().Where(m => m.Id.Contains("SunderedIsles", StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.AreEqual(8, siMoves.Count);
        // Every Sundered Isles move reskins a real Starforged move it must be able to replace.
        foreach (var m in siMoves)
        {
            Assert.IsNotNull(m.Replaces, $"{m.Id} is missing a Replaces target");
            foreach (var replacedId in m.Replaces)
            {
                Assert.IsNotNull(moves.GetMove(replacedId), $"{m.Id} replaces unknown move {replacedId}");
            }
        }
    }

    [TestMethod()]
    public void SunderedIslesLayersOnStarforgedMovesTest()
    {
        var game = IronGame.SunderedIsles;
        var visible = moves.GetMoves().Where(m => game.IncludesContentId(m.Id)).ToList();

        var replacedIds = visible.SelectMany(m => m.Replaces ?? new List<string>()).ToHashSet();
        visible = visible.Where(m => !replacedIds.Contains(m.Id)).ToList();

        // Base Starforged moves the expansion doesn't touch remain available...
        Assert.IsTrue(visible.Any(m => m.Id == "Starforged/Moves/Adventure/Face_Danger"),
            "Sundered Isles player should still see base Starforged moves like Face Danger");
        // ...the reskinned Starforged originals are hidden...
        Assert.IsFalse(visible.Any(m => m.Id == "Starforged/Moves/Exploration/Undertake_an_Expedition"),
            "Replaced Starforged move should be hidden for a Sundered Isles player");
        // ...and the Sundered Isles version wins.
        Assert.IsTrue(visible.Any(m => m.Id == "SunderedIsles/Moves/Exploration/Undertake_an_Expedition"),
            "Sundered Isles override move should be visible");
    }

    [TestMethod()]
    public void StarforgedPlayerDoesNotSeeSunderedIslesContentTest()
    {
        var game = IronGame.Starforged;
        var visibleMoves = moves.GetMoves().Where(m => game.IncludesContentId(m.Id));
        var visibleOracles = oracles.GetOracles().Where(o => game.IncludesContentId(o.Id));

        Assert.IsFalse(visibleMoves.Any(m => m.Id.Contains("SunderedIsles")),
            "A Starforged player should not see Sundered Isles moves");
        Assert.IsFalse(visibleOracles.Any(o => o.Id.Contains("SunderedIsles")),
            "A Starforged player should not see Sundered Isles oracles");
        // And the original Starforged move is untouched for a Starforged player.
        Assert.IsTrue(visibleMoves.Any(m => m.Id == "Starforged/Moves/Exploration/Undertake_an_Expedition"));
    }

    [TestMethod()]
    public void SunderedIslesAssetsLoadTest()
    {
        var assets = new JsonAssetRepository();
        var siAssets = assets.GetAssets().Where(a => a.Id.Contains("SunderedIsles", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.AreEqual(60, siAssets.Count, "Expected 60 Sundered Isles assets");
        // Every asset must have at least one ability (the embed renders ability text).
        Assert.IsTrue(siAssets.All(a => a.Abilities?.Count > 0), "Every asset should have abilities");
        // Ability ids must be present and unique per asset (used as select-menu values).
        foreach (var a in siAssets)
        {
            var ids = a.Abilities.Select(ab => ab.Id).ToList();
            Assert.IsTrue(ids.All(id => !string.IsNullOrWhiteSpace(id)), $"{a.Id} has an ability with no id");
            Assert.AreEqual(ids.Count, ids.Distinct().Count(), $"{a.Id} has duplicate ability ids");
        }
        // Spot-check a companion's condition meter converted correctly.
        var albatross = assets.GetAsset("SunderedIsles/Assets/Companion/Albatross");
        Assert.IsNotNull(albatross);
        Assert.IsNotNull(albatross.ConditionMeter);
        Assert.AreEqual("Health", albatross.ConditionMeter.Name);
    }

    [TestMethod()]
    public void SunderedIslesLayersOnStarforgedAssetsTest()
    {
        var assets = new JsonAssetRepository();
        var game = IronGame.SunderedIsles;
        var visible = assets.GetAssets().Where(a => game.IncludesContentId(a.Id)).ToList();

        Assert.IsTrue(visible.Any(a => a.Id.Contains("SunderedIsles")), "Sundered Isles player should see Sundered Isles assets");
        Assert.IsTrue(visible.Any(a => a.Id.Contains("Starforged")), "Sundered Isles player should still see Starforged assets");
    }

    [TestMethod()]
    public void SunderedIslesNpcTemplateResolvesTest()
    {
        var entities = new Server.Data.JsonEntityRepository();
        var npc = entities.GetEntity("SunderedIsles_NPC");

        Assert.IsNotNull(npc, "SunderedIsles_NPC template should exist");
        Assert.AreEqual(IronGame.SunderedIsles, npc.Game);
        Assert.IsTrue(npc.InitialOracles.Count > 0 && npc.FollowUpOracles.Count > 0);
        AssertTemplateOraclesResolve(npc);
    }

    [TestMethod()]
    public void SunderedIslesCreateTemplatesResolveTest()
    {
        var entities = new Server.Data.JsonEntityRepository();
        var siTemplates = entities.GetEntities().Where(e => e.Game == IronGame.SunderedIsles).ToList();

        // NPC + Settlement + 4 environment Beasts.
        Assert.AreEqual(6, siTemplates.Count, "Expected 6 Sundered Isles /create templates");
        CollectionAssert.AreEquivalent(
            new[] { "SunderedIsles_NPC", "SunderedIsles_Settlement", "SunderedIsles_Beast_Sea",
                    "SunderedIsles_Beast_Land", "SunderedIsles_Beast_Shore_And_River", "SunderedIsles_Beast_Sky" },
            siTemplates.Select(t => t.Id).ToList());

        // Every oracle id every template references must resolve, or /create renders blanks.
        foreach (var t in siTemplates) AssertTemplateOraclesResolve(t);
    }

    private void AssertTemplateOraclesResolve(Server.GameInterfaces.OracleGameEntity template)
    {
        var referenced = new List<string>();
        var titleAndFields = new List<string> { template.Title };
        titleAndFields.AddRange(template.InitialOracles.Concat(template.FollowUpOracles).Select(a => a.FieldValue));

        foreach (var value in titleAndFields)
        {
            if (string.IsNullOrEmpty(value)) continue;
            var matches = System.Text.RegularExpressions.Regex.Matches(value, @"\[\[(.+?)\]\]");
            if (matches.Count > 0)
            {
                foreach (System.Text.RegularExpressions.Match m in matches)
                    referenced.Add(m.Groups[1].Value);
            }
            else if (value.Contains("/Oracles/"))
            {
                referenced.Add(value);
            }
        }

        foreach (var id in referenced)
            Assert.IsNotNull(oracles.GetOracleById(id), $"{template.Id} references unknown oracle {id}");
    }
}
