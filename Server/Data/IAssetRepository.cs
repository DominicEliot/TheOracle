using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOracle2.Data;

namespace Server.Data;

public interface IAssetRepository
{
    IEnumerable<Asset> GetAssets();
    IEnumerable<AssetRoot> GetAssetRoots();

    Asset? GetAsset(string id);
}

public class JsonAssetRepository : IAssetRepository
{
    private List<AssetRoot>? _Assets;

    public Asset? GetAsset(string id)
    {
        return GetAssets().FirstOrDefault(o => o.Id == id);
    }

    public IEnumerable<AssetRoot> GetAssetRoots()
    {
        if (_Assets == null)
        {
            _Assets = new List<AssetRoot>();
            var files = new DirectoryInfo(Path.Combine("Data", "ironsworn")).GetFiles("*assets*.json").ToList();
            files.AddRange(new DirectoryInfo(Path.Combine("Data", "starforged")).GetFiles("*assets*.json").ToList());
            files.AddRange(new DirectoryInfo(Path.Combine("Data", "sundered")).GetFiles("*assets*.json").ToList());

            foreach (var file in files)
            {
                using var fileStream = file.OpenText();
                string text = fileStream.ReadToEnd();

                var root = JsonConvert.DeserializeObject<List<AssetRoot>>(text, new JsonSerializerSettings() { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });

                if (root != null) _Assets.AddRange(root);
            }

            foreach (var node in _Assets)
            {
                foreach (var Asset in node.Assets)
                {
                    Asset.Parent = node;
                }
            }
        }

        return _Assets;
    }

    public IEnumerable<Asset> GetAssets()
    {
        return GetAssetRoots().SelectMany(root => root.Assets);
    }
}
