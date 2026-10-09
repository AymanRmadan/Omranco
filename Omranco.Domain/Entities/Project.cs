using Omranco.Domain.Entities.Geography;

namespace Omranco.Domain.Entities.Projects
{
    public sealed class Project
    {
        public int Id { get; set; }

        public int CustomerAccountId { get; set; }

        public int ZoneId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Address { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties

        public CustomerAccount CustomerAccount { get; set; } = null!;

        public Zone Zone { get; set; } = null!;

        public ICollection<ProjectMember> Members { get; set; }
            = new List<ProjectMember>();
    }
}