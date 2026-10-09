using Omranco.Domain.Entities.Vendors;

namespace Omranco.DAL.Configurations.Vendors
{
    public sealed class VendorStatusHistoryConfiguration
        : IEntityTypeConfiguration<VendorStatusHistory>
    {
        public void Configure(EntityTypeBuilder<VendorStatusHistory> builder)
        {
            builder.ToTable("VendorStatusHistory");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OldStatus)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.NewStatus)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.Reason)
                .HasMaxLength(500);

            builder.HasOne(x => x.Vendor)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}