/*using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omranco.Domain.Entities.Vendors;
using System.Xml.Linq;

namespace Omranco.DAL.Configurations.Vendors
{
    public sealed class DocumentTypeConfiguration
        : IEntityTypeConfiguration<DocumentType>
    {
        public void Configure(EntityTypeBuilder<DocumentType> builder)
        {
            builder.ToTable("DocumentTypes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.NameAr)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameEn)
                .HasMaxLength(200);

            builder.Property(x => x.VendorType)
                .HasConversion<string>()
                .HasMaxLength(30);
        }
    }
}*/