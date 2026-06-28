namespace Main_Structure.DAL.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.OldValue)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.NewValue)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.PerformedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.PerformedAt)
                .IsRequired();
        }
    }
}