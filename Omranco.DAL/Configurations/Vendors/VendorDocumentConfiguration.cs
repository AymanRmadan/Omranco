namespace Omranco.DAL.Configurations.Vendors
{
    public sealed class VendorDocumentConfiguration
        : IEntityTypeConfiguration<VendorDocument>
    {
        public void Configure(EntityTypeBuilder<VendorDocument> builder)
        {
            builder.ToTable("VendorDocuments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.ReviewStatus)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ReviewNotes)
                .HasMaxLength(500);

            builder.Property(x => x.UploadedAt)
                .IsRequired();

            // المورد لا يرفع أكثر من مستند لنفس الـ Requirement
            builder.HasIndex(x => new
            {
                x.VendorId,
                x.RequirementId
            })
            .IsUnique();

            builder.HasOne(x => x.Vendor)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Requirement)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReviewedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}