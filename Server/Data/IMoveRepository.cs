using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.DiscordServer;
using TheOracle2;
using TheOracle2.Data;

namespace Server.Data;

public interface IMoveRepository
{
    IEnumerable<Move> GetMoves();
    IEnumerable<MoveRoot> GetMoveRoots();

    Move? GetMove(string id);
}

public class JsonMoveRepository : IMoveRepository
{
    private List<MoveRoot>? _moves;

    public Move? GetMove(string id)
    {
        return GetMoves().FirstOrDefault(o => o.Id == id);
    }

    public IEnumerable<MoveRoot> GetMoveRoots()
    {
        if (_moves == null)
        {
            _moves = new List<MoveRoot>();
            var files = new DirectoryInfo(Path.Combine("Data", "ironsworn")).GetFiles("*moves*.json").ToList();
            files.AddRange(new DirectoryInfo(Path.Combine("Data", "starforged")).GetFiles("*moves*.json").ToList());
            files.AddRange(new DirectoryInfo(Path.Combine("Data", "sundered")).GetFiles("*moves*.json").ToList());

            foreach (var file in files)
            {
                using var fileStream = file.OpenText();
                string text = fileStream.ReadToEnd();

                var root = JsonConvert.DeserializeObject<List<MoveRoot>>(text, new JsonSerializerSettings() { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });

                if (root != null) _moves.AddRange(root);
            }

            foreach (var node in _moves)
            {
                foreach (var Move in node.Moves)
                {
                    Move.Parent = node;
                }
            }
        }

        return _moves;
    }

    public IEnumerable<Move> GetMoves()
    {
        return GetMoveRoots().SelectMany(root => root.Moves);
    }
}
