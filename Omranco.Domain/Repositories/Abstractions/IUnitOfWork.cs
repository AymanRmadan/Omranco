using Omranco.Domain.Entities;
using Omranco.Domain.Entities.Bids;
using Omranco.Domain.Entities.Catalog;
using Omranco.Domain.Entities.Commissions;
using Omranco.Domain.Entities.Geography;
using Omranco.Domain.Entities.Orders;
using Omranco.Domain.Entities.Payments;
using Omranco.Domain.Entities.Projects;
using Omranco.Domain.Entities.Rfqs;
using Omranco.Domain.Entities.Vendors;
using Omranco.Domain.Entities.Wallets;

namespace Omranco.Domain.Repositories.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        IGenaricRepository<AuditLog, int> AuditLogs { get; }

        IGenaricRepository<Rfq, int> Rfqs { get; }

        IGenaricRepository<Category, int> Categories { get; }

        IGenaricRepository<Vendor, int> Vendors { get; }

        IGenaricRepository<CustomerAccountUser, int> CustomerAccountUsers { get; }

        IGenaricRepository<ProjectMember, int> ProjectMembers { get; }

        IGenaricRepository<RfqInvitation, int> RfqInvitations { get; }

        IGenaricRepository<Bid, int> Bids { get; }
        IGenaricRepository<Order, int> Orders { get; }
        IGenaricRepository<OrderStatusHistory, int> OrderStatusHistory { get; }
        IGenaricRepository<Product, int> Products { get; }
        IGenaricRepository<ProductVariant, int> ProductVariants { get; }
        IGenaricRepository<Unit, int> Units { get; }
        IGenaricRepository<VendorRating, int> VendorRatings { get; }


        IGenaricRepository<CommissionRule, int> CommissionRules { get; }
        IGenaricRepository<VendorWallet, int> VendorWallets { get; }

        IGenaricRepository<VendorWalletTransaction, int> VendorWalletTransactions { get; }

        IGenaricRepository<Payment, int> Payments { get; }

        IGenaricRepository<PaymentAttempt, int> PaymentAttempts { get; }
        IGenaricRepository<PaymentRefund, int> PaymentRefunds { get; }

        IGenaricRepository<OrderCancellation, int> OrderCancellations { get; }

        IGenaricRepository<OrderDispute, int> OrderDisputes { get; }

        IGenaricRepository<OrderDisputeAttachment, int> OrderDisputeAttachments { get; }

        IGenaricRepository<VendorPayoutRequest, int> VendorPayoutRequests { get; }
        IGenaricRepository<VendorPayoutAccount, int> VendorPayoutAccounts { get; }
        IGenaricRepository<CustomerAccount, int> CustomerAccounts { get; }

        IGenaricRepository<Project, int> Projects { get; }
        IGenaricRepository<Zone, int> Zones { get; }

        IGenaricRepository<RfqItem, int> RfqItems { get; }

        IGenaricRepository<RfqAttachment, int> RfqAttachments { get; }


        IRfqCategoryRepository RfqCategories { get; }


        IGenaricRepository<BidItem, int> BidItems { get; }








        Task<int> SaveChangeAsync();
    }
}