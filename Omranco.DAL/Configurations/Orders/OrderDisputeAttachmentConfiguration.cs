using Omranco.Domain.Entities.Orders;

namespace Omranco.DAL.Configurations.Orders;

public sealed class OrderDisputeAttachmentConfiguration
    : IEntityTypeConfiguration<OrderDisputeAttachment>
{
    public void Configure(
        EntityTypeBuilder<OrderDisputeAttachment> builder)
    {
        builder.ToTable("OrderDisputeAttachments");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.FilePath)
            .HasMaxLength(1000)
            .IsRequired();


        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();


        builder.Property(x => x.ContentType)
            .HasMaxLength(100);


        builder.Property(x => x.UploadedAt)
            .IsRequired();


        builder.HasIndex(x => x.OrderDisputeId);


        builder.HasOne(x => x.OrderDispute)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.OrderDisputeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}