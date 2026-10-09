
using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Entities.Catalog;
using Omranco.Domain.Entities.Geography;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Entities.Vendors;

namespace Omranco.DAL.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        DbSet<CustomerAccount> CustomerAccounts => Set<CustomerAccount>();

        DbSet<CustomerAccountUser> CustomerAccountUsers => Set<CustomerAccountUser>();
        public DbSet<Project> Projects => Set<Project>();

        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<Governorate> Governorates => Set<Governorate>();

        public DbSet<Zone> Zones => Set<Zone>();

        public DbSet<Vendor> Vendors => Set<Vendor>();

        public DbSet<VendorZone> VendorZones => Set<VendorZone>();

        public DbSet<VendorDocumentRequirement> VendorDocumentRequirements => Set<VendorDocumentRequirement>();

        public DbSet<VendorDocument> VendorDocuments => Set<VendorDocument>();

        public DbSet<VendorPayoutAccount> VendorPayoutAccounts =>
            Set<VendorPayoutAccount>();

        public DbSet<VendorStatusHistory> VendorStatusHistory =>
            Set<VendorStatusHistory>();
        public DbSet<Category> Categories => Set<Category>();

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<ProductAttribute> ProductAttributes =>
    Set<ProductAttribute>();

        public DbSet<ProductAttributeOption> ProductAttributeOptions =>
            Set<ProductAttributeOption>();

        public DbSet<ProductVariant> ProductVariants =>
            Set<ProductVariant>();

        public DbSet<ProductVariantValue> ProductVariantValues =>
            Set<ProductVariantValue>();

        public DbSet<ProductUnit> ProductUnits =>
            Set<ProductUnit>();
        public DbSet<VendorCategory> VendorCategories =>
    Set<VendorCategory>();

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}