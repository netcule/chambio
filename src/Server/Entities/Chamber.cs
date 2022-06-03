using System.Text.Json.Serialization;
using Chambio.Server.Abstractions;
using Chambio.Server.Enums;

namespace Chambio.Server.Entities;

public class Chamber : Entity
{
    [JsonIgnore]
    public string Key { get; set; }

    public string Name { get; set; }

    public HouseType? HouseType { get; set; }

    public List<Member>? Members { get; set; }

    public int CountryId { get; set; }

    public Country? Country { get; set; }

    public Chamber(string key, string name, HouseType? houseType)
    {
        Key = key;
        Name = name;
        HouseType = houseType;
    }
}
