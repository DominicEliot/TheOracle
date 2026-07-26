namespace TheOracle2.Data;

public class Move
{
    [JsonProperty("Source")]
    public Source Source { get; set; }

    [JsonProperty("Category")]
    public string Category { get; set; }

    [JsonProperty("$id")]
    public string Id { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Optional")]
    public bool Optional { get; set; }

    [JsonProperty("Display")]
    public Display Display { get; set; }

    [JsonProperty("Trigger")]
    public Trigger Trigger { get; set; }

    [JsonProperty("Text")]
    public string Text { get; set; }

    [JsonProperty("Oracles")]
    public List<string> Oracles { get; set; }

    [JsonProperty("Outcomes")]
    public Outcomes Outcomes { get; set; }

    [JsonProperty("Progress Move")]
    public bool? ProgressMove { get; set; }

    [JsonProperty("Variant of")]
    public string VariantOf { get; set; }

    // Base-game move ids this move supersedes (used by Sundered Isles to override
    // the Starforged moves it reskins). Null for base-game moves.
    [JsonProperty("Replaces")]
    public List<string>? Replaces { get; set; }

    [JsonIgnore]
    public MoveRoot? Parent { get; set; }
}

public class MoveRoot
{
    [JsonProperty("$id")]
    public string Id { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Description")]
    public string Description { get; set; }

    [JsonProperty("Source")]
    public Source Source { get; set; }

    [JsonProperty("Display")]
    public Display Display { get; set; }

    [JsonProperty("Optional")]
    public bool Optional { get; set; }

    [JsonProperty("Moves")]
    public List<Move> Moves { get; set; }
}

