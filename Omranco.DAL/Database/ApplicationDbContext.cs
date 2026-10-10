
using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Auths;
using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Entities.Catalog;
using Omranco.Domain.Entities.Geography;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Entities.Rfqs;
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
        public DbSet<VendorPayoutAccount> VendorPayoutAccounts => Set<VendorPayoutAccount>();
        public DbSet<VendorStatusHistory> VendorStatusHistory => Set<VendorStatusHistory>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
        public DbSet<ProductAttributeOption> ProductAttributeOptions => Set<ProductAttributeOption>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductVariantValue> ProductVariantValues => Set<ProductVariantValue>();
        public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();
        public DbSet<VendorCategory> VendorCategories => Set<VendorCategory>();
        public DbSet<Rfq> Rfqs => Set<Rfq>();
        public DbSet<RfqItem> RfqItems => Set<RfqItem>();
        public DbSet<RfqAttachment> RfqAttachments => Set<RfqAttachment>();
        public DbSet<RfqInvitation> RfqInvitations => Set<RfqInvitation>();
        public DbSet<RfqCategory> RfqCategories => Set<RfqCategory>();
        public DbSet<Bid> Bids => Set<Bid>();
        public DbSet<BidItem> BidItems => Set<BidItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}