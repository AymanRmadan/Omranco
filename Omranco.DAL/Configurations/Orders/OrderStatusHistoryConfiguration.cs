using Omranco.Domain.Entities.Orders;

namespace Omranco.DAL.Configurations.Orders
{
    public sealed class OrderStatusHistoryConfiguration
        : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(
            EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("OrderStatusHistory");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OldStatus)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.NewStatus)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ChangedAt)
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}