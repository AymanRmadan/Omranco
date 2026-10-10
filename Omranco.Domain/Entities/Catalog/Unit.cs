namespace Omranco.Domain.Entities.Catalog
{
    public sealed class Unit : BaseEntity<int>
    {

        public string Name { get; set; } = string.Empty;

        public string? Symbol { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProductUnit> Products { get; set; } = new List<ProductUnit>();
    }
}