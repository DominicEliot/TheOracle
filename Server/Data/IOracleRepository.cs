using TheOracle2.Data;

namespace Server.Data;

public interface IOracleRepository
{
    IEnumerable<Oracle> GetOracles();

    IEnumerable<OracleRoot> GetOracleRoots();

    Oracle? GetOracleById(string id);
}

public static class OracleRepositoryExtenstions
{
    public static IEnumerable<Oracle> GetOraclesFromUserInput(this IEnumerable<Oracle> oracles, string query, StringComparison comparer = StringComparison.OrdinalIgnoreCase)
    {
        var nameMatch = oracles.Where(x => x.Name.Contains(query, comparer) || x.Display?.Title?.Contains(query, comparer) == true);
        var parentNameMatch = oracles.Where(x => x.Parent?.Name.Contains(query, comparer) == true).Select(x => x.Parent);
        var parentNameOracles = parentNameMatch.SelectMany(p => p.Oracles);
        var categoryOracles = oracles.Where(o => o.Category?.Contains(query, comparer) == true);

        var parentAliasMatch = oracles.Where(x => x.Parent?.Aliases?.Any(s => s.Contains(query, comparer)) == true);
        var aliasMatch = oracles.Where(x => x.Aliases?.Any(s => s.Contains(query, comparer)) == true);

        return nameMatch
            .Concat(parentNameOracles)
            .Concat(categoryOracles)
            .Concat(parentAliasMatch)
            .Concat(aliasMatch)
            .DistinctBy(o => o.Id);
    }
}

public class JsonOracleRepository : IOracleRepository
{
    private List<OracleRoot>? _oracles;

    public Oracle? GetOracleById(string id)
    {
        var topLevelOracle = GetOracles().FirstOrDefault(o => o.Id == id);
        if (topLevelOracle != null) return topLevelOracle;

        var subOracle = GetOracles().SelectMany(o => o.Oracles?.Where(sub => sub.Id == id) ?? Array.Empty<Oracle>()).FirstOrDefault();
        if (subOracle != null) return subOracle;

        var cats = GetOracleRoots().Where(or => or.Categories != null).SelectMany(or => or.Categories);
        var catOracle = cats.SelectMany(c => c.Oracles).FirstOrDefault(o => o.Id == id);
        if (catOracle != null) return catOracle;

        foreach (var cat in cats)
        {
            var oracle = findOracleRecursive(cat.Oracles, id);
            if (oracle != null) return oracle;
        }

        var partial = GetOracles().SingleOrDefault(o => o.Id.Contains(id))
            ?? GetOracles().SelectMany(o => o.Oracles?.Where(sub => sub.Id.Contains(id)) ?? Array.Empty<Oracle>()).SingleOrDefault();

        return partial;
    }

    private static void SetParentRecursive(Oracle oracle, OracleRoot root)
    {
        oracle.Parent = root;

        foreach (var child in oracle.Oracles ?? Enumerable.Empty<Oracle>())
        {
            SetParentRecursive(child, root);
        }
    }

    private Oracle? findOracleRecursive(List<Oracle> oracles, string id)
    {
        if (oracles.Count == 0) return null;
        foreach (var oracle in oracles)
        {
            if (oracle.Id == id) return oracle;

            if (oracle.Oracles != null)
            {
                var test = findOracleRecursive(oracle.Oracles, id);
                if (test != null) return test;
            }
        }

        return null;
    }

    public IEnumerable<OracleRoot> GetOracleRoots()
    {
        if (_oracles == null)
        {
            _oracles = new List<OracleRoot>();
            var files = new DirectoryInfo(Path.Combine("Data", "ironsworn")).GetFiles("*oracle*.json").ToList();
            files.AddRange(new DirectoryInfo(Path.Combine("Data", "starforged")).GetFiles("*oracle*.json").ToList());
            files.AddRange(new DirectoryInfo(Path.Combine("Data", "sundered")).GetFiles("*oracle*.json").ToList());

            foreach (var file in files)
            {
                using var fileStream = file.OpenText();
                string text = fileStream.ReadToEnd();

                var root = JsonConvert.DeserializeObject<List<OracleRoot>>(text, new JsonSerializerSettings() { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });

                if (root != null) _oracles.AddRange(root);
            }

            foreach (var node in _oracles)
            {
                foreach (var oracle in node.Oracles)
                {
                    SetParentRecursive(oracle, node);
                }

                if (node.Categories?.Count > 0)
                {
                    foreach (var cat in node.Categories)
                    {
                        foreach (var catOracle in cat.Oracles)
                        {
                            SetParentRecursive(catOracle, node);
                        }
                    }
                }
            }
        }

        return _oracles;
    }

    public IEnumerable<Oracle> GetOracles()
    {
        return GetOracleRoots().SelectMany(root => root.Oracles);
    }
}
