namespace Omranco.Domain.Entities.Projects
{
    public sealed class ProjectMember : BaseEntity<int>
    {

        public int ProjectId { get; set; }

        public int CustomerAccountUserId { get; set; }

        public int? AssignedByAccountUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties
        public Project Project { get; set; } = null!;

        public CustomerAccountUser CustomerAccountUser { get; set; } = null!;

        public CustomerAccountUser? AssignedByAccountUser { get; set; }
    }
}