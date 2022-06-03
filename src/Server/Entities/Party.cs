using System.Text.Json.Serialization;
using Chambio.Server.Abstractions;

namespace Chambio.Server.Entities;

public class Party : Entity
{
    [JsonIgnore]
    public string Key { get; set; }

    public string Name { get; set; }

    public List<Ideology>? Ideologies { get; set; }

    public List<Member>? Members { get; set; }

    public int CountryId { get; set; }

    public Country? Country { get; set; }

    public Party(string key, string name)
    {
        Key = key;
        Name = name;
    }
}
