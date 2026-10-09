using Omranco.Domain.Entities;

namespace Omranco.DAL.Configurations.Customers
{
    public sealed class CustomerAccountUserConfiguration
        : IEntityTypeConfiguration<CustomerAccountUser>
    {
        public void Configure(EntityTypeBuilder<CustomerAccountUser> builder)
        {
            builder.ToTable("CustomerAccountUsers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // نفس المستخدم لا يتكرر في نفس الحساب
            builder.HasIndex(x => new
            {
                x.CustomerAccountId,
                x.UserId
            })
            .IsUnique();

            builder.HasOne(x => x.CustomerAccount)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.CustomerAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany(x => x.CustomerAccounts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}