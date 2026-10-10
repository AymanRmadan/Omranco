using Omranco.DAL.Database;
using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Entities.Catalog;
using Omranco.Domain.Entities.Commissions;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Entities.Ratings;
using Omranco.Domain.Entities.Rfqs;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Entities.Wallets;
using Omranco.Domain.Repositories.Abstractions;

namespace Omranco.DAL.GenaricBase.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        private IGenaricRepository<AuditLog, int>? _auditLogs;

        private IGenaricRepository<Rfq, int>? _rfqs;
        private IGenaricRepository<Category, int>? _categories;
        private IGenaricRepository<Vendor, int>? _vendors;

        private IGenaricRepository<CustomerAccountUser, int>? _customerAccountUsers;

        private IGenaricRepository<ProjectMember, int>? _projectMembers;

        private IGenaricRepository<RfqInvitation, int>? _rfqInvitations;

        private IGenaricRepository<Bid, int>? _bids;
        private IGenaricRepository<OrderStatusHistory, int>? _orderStatusHistory;

        private IGenaricRepository<Product, int>? _products;

        private IGenaricRepository<ProductVariant, int>? _productVariants;

        private IGenaricRepository<Unit, int>? _units;
        private IGenaricRepository<VendorRating, int>? _vendorRatings;


        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IGenaricRepository<AuditLog, int> AuditLogs =>
            _auditLogs ??=
                new GenaricRepository<AuditLog, int>(_dbContext);


        public IGenaricRepository<Rfq, int> Rfqs =>
            _rfqs ??=
                new GenaricRepository<Rfq, int>(_dbContext);


        public IGenaricRepository<Category, int> Categories =>
            _categories ??=
                new GenaricRepository<Category, int>(_dbContext);


        public IGenaricRepository<Vendor, int> Vendors =>
            _vendors ??=
                new GenaricRepository<Vendor, int>(_dbContext);


        public IGenaricRepository<CustomerAccountUser, int>
            CustomerAccountUsers =>
                _customerAccountUsers ??=
                    new GenaricRepository<CustomerAccountUser, int>(
                        _dbContext
                    );


        public IGenaricRepository<ProjectMember, int>
            ProjectMembers =>
                _projectMembers ??=
                    new GenaricRepository<ProjectMember, int>(
                        _dbContext
                    );


        public IGenaricRepository<RfqInvitation, int>
            RfqInvitations =>
                _rfqInvitations ??=
                    new GenaricRepository<RfqInvitation, int>(
                        _dbContext
                    );


        public IGenaricRepository<Bid, int> Bids =>
            _bids ??=
                new GenaricRepository<Bid, int>(_dbContext);

        private IGenaricRepository<Order, int>? _orders;

        public IGenaricRepository<Order, int> Orders =>
            _orders ??=
                new GenaricRepository<Order, int>(_dbContext);

        public IGenaricRepository<OrderStatusHistory, int>
    OrderStatusHistory =>
        _orderStatusHistory ??=
            new GenaricRepository<OrderStatusHistory, int>(
                _dbContext
            );


        public IGenaricRepository<Product, int> Products => _products ??=
                    new GenaricRepository<Product, int>(
                        _dbContext
                    );


        public IGenaricRepository<ProductVariant, int>
            ProductVariants =>
                _productVariants ??=
                    new GenaricRepository<ProductVariant, int>(
                        _dbContext
                    );


        public IGenaricRepository<Unit, int>
            Units =>
                _units ??=
                    new GenaricRepository<Unit, int>(
                        _dbContext
                    );

        public IGenaricRepository<VendorRating, int>
    VendorRatings =>
        _vendorRatings ??=
            new GenaricRepository<VendorRating, int>(
                _dbContext
            );


        private IGenaricRepository<CommissionRule, int>?
    _commissionRules;

        public IGenaricRepository<CommissionRule, int>
            CommissionRules =>
                _commissionRules ??=
                    new GenaricRepository<CommissionRule, int>(
                        _dbContext
                    );


        private IGenaricRepository<VendorWallet, int>? _vendorWallets;

        private IGenaricRepository<VendorWalletTransaction, int>? _vendorWalletTransactions;
        public IGenaricRepository<VendorWallet, int>
    VendorWallets =>
        _vendorWallets ??=
            new GenaricRepository<VendorWallet, int>(
                _dbContext
            );


        public IGenaricRepository<VendorWalletTransaction, int>
            VendorWalletTransactions =>
                _vendorWalletTransactions ??=
                    new GenaricRepository<VendorWalletTransaction, int>(
                        _dbContext
                    );


        public Task<int> SaveChangeAsync()
        {
            return _dbContext.SaveChangesAsync();
        }


        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}