using System.Text.Json.Serialization;
using Chambio.Server.Abstractions;
using Chambio.Server.Enums;

namespace Chambio.Server.Entities;

public class Country : Entity
{
    [JsonIgnore]
    public string Key { get; set; }

    public string Name { get; set; }

    public LegislatureType LegislatureType { get; set; }

    public string FlagUrl { get; set; }

    public string SymbolUrl { get; set; }

    public List<Chamber>? Chambers { get; set; }

    public List<Party>? Parties { get; set; }

    public List<Member>? Members { get; set; }

    public Country(string key, string name, LegislatureType legislatureType,
        string flagUrl, string symbolUrl)
    {
        Key = key;
        Name = name;
        LegislatureType = legislatureType;
        FlagUrl = flagUrl;
        SymbolUrl = symbolUrl;
    }
}
