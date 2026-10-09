using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Enums;

namespace Omranco.Domain.Entities
{
    public sealed class CustomerAccountUser
    {
        public int Id { get; set; }

        public int CustomerAccountId { get; set; }

        public int UserId { get; set; }

        public CustomerAccountUserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties
        public CustomerAccount CustomerAccount { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;

        public ICollection<ProjectMember> ProjectMemberships { get; set; }
    = new List<ProjectMember>();

        public ICollection<ProjectMember> AssignedProjectMembers { get; set; }
            = new List<ProjectMember>();
    }
}
