using Omranco.Domain.Entities.Rfqs;

namespace Omranco.DAL.Configurations.Rfqs
{
    public sealed class RfqAttachmentConfiguration
        : IEntityTypeConfiguration<RfqAttachment>
    {
        public void Configure(EntityTypeBuilder<RfqAttachment> builder)
        {
            builder.ToTable("RfqAttachments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.ContentType)
                .HasMaxLength(100);

            builder.Property(x => x.UploadedAt)
                .IsRequired();

            builder.HasOne(x => x.Rfq)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.RfqId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}