using Omranco.DAL.Database;
using Omranco.DAL.Repositories.Implementations;
using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Entities.Catalog;
using Omranco.Domain.Entities.Commissions;
using Omranco.Domain.Entities.Geography;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Payments;
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

        private IGenaricRepository<Payment, int>? _payments;

        private IGenaricRepository<PaymentAttempt, int>? _paymentAttempts;


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

        public IGenaricRepository<Payment, int>
    Payments =>
        _payments ??=
            new GenaricRepository<Payment, int>(
                _dbContext
            );


        public IGenaricRepository<PaymentAttempt, int>
            PaymentAttempts =>
                _paymentAttempts ??=
                    new GenaricRepository<PaymentAttempt, int>(
                        _dbContext
                    );

        private IGenaricRepository<PaymentRefund, int>? _paymentRefunds;
        public IGenaricRepository<PaymentRefund, int> PaymentRefunds => _paymentRefunds ??=
            new GenaricRepository<PaymentRefund, int>(_dbContext);

        private IGenaricRepository<OrderCancellation, int>?
    _orderCancellations;

        public IGenaricRepository<OrderCancellation, int>
            OrderCancellations =>
                _orderCancellations ??=
                    new GenaricRepository<OrderCancellation, int>(
                        _dbContext);

        private IGenaricRepository<OrderDispute, int>?
    _orderDisputes;

        private IGenaricRepository<OrderDisputeAttachment, int>?
            _orderDisputeAttachments;

        public IGenaricRepository<OrderDispute, int> OrderDisputes =>
                _orderDisputes ??=
                    new GenaricRepository<OrderDispute, int>(
                        _dbContext
                    );


        public IGenaricRepository<OrderDisputeAttachment, int>
            OrderDisputeAttachments =>
                _orderDisputeAttachments ??=
                    new GenaricRepository<OrderDisputeAttachment, int>(
                        _dbContext
                    );



        private IGenaricRepository<VendorPayoutRequest, int>?
    _vendorPayoutRequests;
        public IGenaricRepository<VendorPayoutRequest, int>
            VendorPayoutRequests =>
                _vendorPayoutRequests ??=
                    new GenaricRepository<VendorPayoutRequest, int>(
                        _dbContext
                    );


        private IGenaricRepository<VendorPayoutAccount, int>? _vendorPayoutAccount;
        public IGenaricRepository<VendorPayoutAccount, int>
            VendorPayoutAccounts =>
                _vendorPayoutAccount ??=
                    new GenaricRepository<VendorPayoutAccount, int>(
                        _dbContext
                    );


        private IGenaricRepository<CustomerAccount, int>?
    _customerAccounts;

        private IGenaricRepository<Project, int>?
            _projects;

        private IGenaricRepository<Zone, int>?
            _zones;

        public IGenaricRepository<CustomerAccount, int>
    CustomerAccounts =>
        _customerAccounts ??=
            new GenaricRepository<CustomerAccount, int>(
                _dbContext
            );

        public IGenaricRepository<Project, int>
            Projects =>
                _projects ??=
                    new GenaricRepository<Project, int>(
                        _dbContext
                    );

        public IGenaricRepository<Zone, int>
            Zones =>
                _zones ??=
                    new GenaricRepository<Zone, int>(
                        _dbContext);




        private IGenaricRepository<RfqItem, int>? _rfqItems;

        private IGenaricRepository<RfqAttachment, int>? _rfqAttachments;

        private IRfqCategoryRepository? _rfqCategories;

        public IGenaricRepository<RfqItem, int>
    RfqItems =>
        _rfqItems ??=
            new GenaricRepository<RfqItem, int>(
                _dbContext
            );


        public IGenaricRepository<RfqAttachment, int>
            RfqAttachments =>
                _rfqAttachments ??=
                    new GenaricRepository<RfqAttachment, int>(
                        _dbContext
                    );


        public IRfqCategoryRepository
            RfqCategories =>
                _rfqCategories ??=
                    new RfqCategoryRepository(
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