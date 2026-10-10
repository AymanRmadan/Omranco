using Omranco.Domain.Entities.Rfqs;

namespace Omranco.DAL.Configurations.Rfqs
{
    public sealed class RfqCategoryConfiguration
        : IEntityTypeConfiguration<RfqCategory>
    {
        public void Configure(EntityTypeBuilder<RfqCategory> builder)
        {
            builder.ToTable("RfqCategories");

            builder.HasKey(x => new
            {
                x.RfqId,
                x.CategoryId
            });

            builder.HasOne(x => x.Rfq)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.RfqId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}