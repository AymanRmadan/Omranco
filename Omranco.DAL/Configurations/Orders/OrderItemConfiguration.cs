using Omranco.Domain.Entities.Orders;

namespace Omranco.DAL.Configurations.Orders
{
    public sealed class OrderItemConfiguration
        : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                .HasPrecision(18, 3);

            builder.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalPrice)
                .HasPrecision(18, 2);

            builder.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.RfqItem)
                .WithMany()
                .HasForeignKey(x => x.RfqItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Unit)
                .WithMany()
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}