using Omranco.Domain.Entities.Rfqs;

namespace Omranco.DAL.Configurations.Rfqs
{
    public sealed class RfqInvitationConfiguration
        : IEntityTypeConfiguration<RfqInvitation>
    {
        public void Configure(EntityTypeBuilder<RfqInvitation> builder)
        {
            builder.ToTable("RfqInvitations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.SentAt)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.RfqId,
                x.VendorId
            })
            .IsUnique();

            builder.HasOne(x => x.Rfq)
                .WithMany(x => x.Invitations)
                .HasForeignKey(x => x.RfqId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Vendor)
                .WithMany(x => x.RfqInvitations)
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}