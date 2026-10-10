namespace Omranco.Domain.Entities.Geography
{
    public sealed class Zone : BaseEntity<int>
    {

        public int GovernorateId { get; set; }

        public string NameAr { get; set; } = string.Empty;

        public string? NameEn { get; set; }

        public bool IsActive { get; set; } = true;

        public Governorate Governorate { get; set; } = null!;
    }
}