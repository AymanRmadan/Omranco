namespace Omranco.Domain.Entities.Geography
{
    public sealed class Governorate
    {
        public int Id { get; set; }

        public string NameAr { get; set; } = string.Empty;

        public string? NameEn { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Zone> Zones { get; set; }
            = new List<Zone>();
    }
}