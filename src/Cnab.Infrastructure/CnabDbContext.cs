using Cnab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cnab.Infrastructure;

public class CnabDbContext : DbContext
{
    public CnabDbContext(DbContextOptions<CnabDbContext> options) : base(options) { }

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ImportFile> ImportFiles => Set<ImportFile>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Store>(e =>
        {
            e.HasIndex(x => new { x.Owner, x.Name }).IsUnique();
            e.Property(x => x.Owner).HasMaxLength(14).IsRequired();
            e.Property(x => x.Name).HasMaxLength(19).IsRequired();
        });

        b.Entity<Transaction>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasIndex(x => new { x.ImportFileId, x.SourceLine }).IsUnique();
        });

        b.Entity<ImportFile>(e =>
        {
            e.HasIndex(x => x.Sha256).IsUnique();
        });

        base.OnModelCreating(b);
    }
}