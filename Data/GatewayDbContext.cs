using Microsoft.EntityFrameworkCore;

namespace BinyanAv.PublicGateway.Data;

public class GatewayDbContext : DbContext
{
    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options)
    {
    }

    public DbSet<InboundRegistration> InboundRegistrations => Set<InboundRegistration>();
    public DbSet<InboundRegistrationDocument> InboundRegistrationDocuments => Set<InboundRegistrationDocument>();
    public DbSet<InboundNedarimCallback> InboundNedarimCallbacks => Set<InboundNedarimCallback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboundRegistration>(e =>
        {
            e.Property(x => x.PayloadJson).HasColumnType("longtext");
            e.HasIndex(x => x.ReceivedAtUtc);
            e.HasIndex(x => x.ImportedAtUtc);
            e.HasMany(x => x.Documents)
                .WithOne(x => x.InboundRegistration!)
                .HasForeignKey(x => x.InboundRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InboundRegistrationDocument>(e =>
        {
            e.HasIndex(x => x.InboundRegistrationId);
        });

        modelBuilder.Entity<InboundNedarimCallback>(e =>
        {
            e.Property(x => x.PayloadJson).HasColumnType("longtext");
            e.Property(x => x.TransactionId).HasMaxLength(80);
            e.Property(x => x.KevaId).HasMaxLength(80);
            e.HasIndex(x => x.ReceivedAtUtc);
            e.HasIndex(x => x.TransactionId);
            e.HasIndex(x => x.KevaId);
        });
    }
}
