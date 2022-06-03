using Chambio.Server.Abstractions;

namespace Chambio.Server.Entities;

public class Ideology : Entity
{
    public string Name { get; set; }

    public List<Party>? Parties { get; set; }

    public Ideology(string name)
    {
        Name = name;
    }
}
