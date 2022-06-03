using System.Text.Json.Serialization;
using Chambio.Server.Abstractions;
using Chambio.Server.Enums;

namespace Chambio.Server.Entities;

public class Member : Entity
{
    [JsonIgnore]
    public string Key { get; set; }

    public string Name { get; set; }

    public Gender? Gender { get; set; }

    public int? PartyId { get; set; }

    public Party? Party { get; set; }

    public int ChamberId { get; set; }

    public Chamber? Chamber { get; set; }

    public int CountryId { get; set; }

    public Country? Country { get; set; }

    public Member(string key, string name, Gender? gender)
    {
        Key = key;
        Name = name;
        Gender = gender;
    }
}
