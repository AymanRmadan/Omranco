using Omranco.Domain.Enums.Vendors;

namespace Omranco.Domain.Entities.Vendors
{
    public sealed class VendorDocumentRequirement : BaseEntity<int>
    {

        public string Name { get; set; } = string.Empty;

        public VendorType? VendorType { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        public ICollection<VendorDocument> Documents { get; set; }
            = new List<VendorDocument>();
    }
}