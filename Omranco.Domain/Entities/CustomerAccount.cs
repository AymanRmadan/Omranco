using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Enums;

namespace Omranco.Domain.Entities
{
    public sealed class CustomerAccount : BaseEntity<int>
    {

        public string Name { get; set; } = string.Empty;

        public CustomerAccountType Type { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties
        public ICollection<CustomerAccountUser> Users { get; set; }
            = new List<CustomerAccountUser>();


        public ICollection<Project> Projects { get; set; }
            = new List<Project>();
    }
}
