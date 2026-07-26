namespace TheOracle2.Data;
public class Attribute
{
    [JsonProperty("Key")]
    public string Key { get; set; }

    [JsonProperty("Values")]
    public List<string> Values { get; set; }

    [JsonProperty("Value")]
    public string Value { get; set; }
}

public class OracleCategory
{
    [JsonProperty("Source")]
    public Source Source { get; set; }

    [JsonProperty("$id")]
    public string Id { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Display")]
    public Display Display { get; set; }

    [JsonProperty("Category")]
    public string Category { get; set; }

    [JsonProperty("Usage")]
    public OracleUsage Usage { get; set; }

    [JsonProperty("Oracles")]
    public List<Oracle> Oracles { get; set; }

    [JsonProperty("Description")]
    public string Description { get; set; }

    [JsonProperty("Aliases")]
    public List<string> Aliases { get; set; }

    [JsonProperty("Sample Names")]
    public List<string> SampleNames { get; set; }
}

public class Content
{
    [JsonProperty("Part of speech")]
    public List<string> PartOfSpeech { get; set; }

    [JsonProperty("Tags")]
    public List<string> Tags { get; set; }
}

public class GameObject
{
    [JsonProperty("Object type")]
    public string ObjectType { get; set; }

    [JsonProperty("Requires")]
    public Requires Requires { get; set; }
}

public class MultipleRolls
{
    [JsonProperty("Amount")]
    public int Amount { get; set; }

    [JsonProperty("Allow duplicates")]
    public bool AllowDuplicates { get; set; }

    [JsonProperty("Make it worse")]
    public bool MakeItWorse { get; set; }
}

public class Oracle
{
    public override string ToString()
    {
        return Id;
    }

    [JsonProperty("Source")]
    public Source Source { get; set; }

    [JsonProperty("$id")]
    public string Id { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Category")]
    public string Category { get; set; }

    [JsonProperty("Description")]
    public string Description { get; set; }

    [JsonProperty("Display")]
    public Display Display { get; set; }

    [JsonProperty("Content")]
    public Content Content { get; set; }

    [JsonProperty("Table")]
    public List<Table> Table { get; set; }

    [JsonProperty("Usage")]
    public OracleUsage Usage { get; set; }

    [JsonProperty("Oracles")]
    public List<Oracle> Oracles { get; set; }

    [JsonProperty("Aliases")]
    public List<string> Aliases { get; set; }

    [JsonProperty("Member of")]
    public string MemberOf { get; set; }

    // Base-game oracle ids this oracle supersedes (used by Sundered Isles to override
    // the Starforged tables it reskins). Null for base-game oracles.
    [JsonProperty("Replaces")]
    public List<string>? Replaces { get; set; }

    [JsonIgnore]
    public OracleRoot? Parent { get; set; }
}

public class Requires
{
    [JsonProperty("Attributes")]
    public List<Attribute> Attributes { get; set; }
}

public class ResultColumn
{
    [JsonProperty("Label")]
    public string Label { get; set; }

    [JsonProperty("Use content from")]
    public string UseContentFrom { get; set; }

    [JsonProperty("Key")]
    public string Key { get; set; }
}

public class RollColumn
{
    [JsonProperty("Label")]
    public string Label { get; set; }

    [JsonProperty("Use content from")]
    public string UseContentFrom { get; set; }
}

public class RollTemplate
{
    [JsonProperty("Result")]
    public string Result { get; set; }
}

public class OracleRoot
{
    [JsonProperty("Source")]
    public Source Source { get; set; }

    [JsonProperty("$id")]
    public string Id { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Aliases")]
    public List<string> Aliases { get; set; }

    [JsonProperty("Display")]
    public Display Display { get; set; }

    [JsonProperty("Oracles")]
    public List<Oracle> Oracles { get; set; }

    [JsonProperty("Description")]
    public string Description { get; set; }

    [JsonProperty("Categories")]
    public List<OracleCategory> Categories { get; set; }
}



public class Suggestions
{
    [JsonProperty("Assets")]
    public List<string> Assets { get; set; }

    [JsonProperty("Game objects")]
    public List<GameObject> GameObjects { get; set; }

    [JsonProperty("Oracle rolls")]
    public List<string> OracleRolls { get; set; }
}

public class Table : IComparable<int>
{
    [JsonProperty("Result columns")]
    public List<ResultColumn> ResultColumns { get; set; }

    [JsonProperty("Roll columns")]
    public List<RollColumn> RollColumns { get; set; }

    [JsonProperty("Floor")]
    public int? Floor { get; set; }

    [JsonProperty("Ceiling")]
    public int? Ceiling { get; set; }

    [JsonProperty("$id")]
    public string Id { get; set; }

    [JsonProperty("Result")]
    public string Result { get; set; }

    [JsonProperty("Summary")]
    public string Summary { get; set; }

    [JsonProperty("Suggestions")]
    public Suggestions Suggestions { get; set; }

    [JsonProperty("Oracle rolls")]
    public List<string> OracleRolls { get; set; }

    [JsonProperty("Multiple rolls")]
    public MultipleRolls MultipleRolls { get; set; }

    [JsonProperty("Attributes")]
    public List<Attribute> Attributes { get; set; }

    [JsonProperty("Roll template")]
    public RollTemplate RollTemplate { get; set; }

    [JsonProperty("Game objects")]
    public List<GameObject> GameObjects { get; set; }

    [JsonProperty("Display")]
    public Display Display { get; set; }

    [JsonProperty("Content")]
    public Content Content { get; set; }

    public int CompareTo(int other)
    {
        if (other > Ceiling) return -1;
        if (other >= Floor && other <= Ceiling) return 0;
        if (other < Ceiling) return 1;
        return -1;
    }
}

public class OracleUsage
{
    [JsonProperty("Max rolls")]
    public int MaxRolls { get; set; }

    [JsonProperty("Allow duplicates")]
    public bool AllowDuplicates { get; set; }

    [JsonProperty("Initial")]
    public bool? Initial { get; set; }

    [JsonProperty("Suggestions")]
    public Suggestions Suggestions { get; set; }

    [JsonProperty("Sets")]
    public List<Set> Sets { get; set; }

    [JsonProperty("Requires")]
    public Requires Requires { get; set; }

    [JsonProperty("Repeatable")]
    public bool? Repeatable { get; set; }
}

