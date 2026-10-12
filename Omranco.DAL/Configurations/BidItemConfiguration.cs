using Omranco.Domain.Entities.Bids;

namespace Omranco.DAL.Configurations.Bids
{
    public sealed class BidItemConfiguration
        : IEntityTypeConfiguration<BidItem>
    {
        public void Configure(EntityTypeBuilder<BidItem> builder)
        {
            builder.ToTable("BidItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.TotalPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.HasIndex(x => new
            {
                x.BidId,
                x.RfqItemId
            })
            .IsUnique();


            builder.HasOne(x => x.Bid)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.BidId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.RfqItem)
                .WithMany()
                .HasForeignKey(x => x.RfqItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}