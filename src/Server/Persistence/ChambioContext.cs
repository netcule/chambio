using Chambio.Server.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Persistence;

public class ChambioContext : DbContext
{
    public DbSet<Country> Countries => Set<Country>();

    public DbSet<Chamber> Chambers => Set<Chamber>();

    public DbSet<Party> Parties => Set<Party>();

    public DbSet<Ideology> Ideologies => Set<Ideology>();

    public DbSet<Member> Members => Set<Member>();

    public ChambioContext(DbContextOptions<ChambioContext> options)
        : base(options) { }
}
