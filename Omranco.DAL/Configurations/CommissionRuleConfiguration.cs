using Omranco.Domain.Entities.Commissions;

namespace Omranco.DAL.Configurations.Commissions
{
    public sealed class CommissionRuleConfiguration
        : IEntityTypeConfiguration<CommissionRule>
    {
        public void Configure(
            EntityTypeBuilder<CommissionRule> builder)
        {
            builder.ToTable("CommissionRules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Percentage)
                .HasPrecision(5, 2)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.ValidFrom)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.IsActive,
                x.ValidFrom
            });
        }
    }
}