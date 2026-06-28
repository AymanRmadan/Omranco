namespace Main_Structure.Domain.Entities
{
    public class AuditLog : BaseEntity<int>
    {

        public string EntityName { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateOnly PerformedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
