using Omranco.BLL.Services.Abstractions.AuthServices;
using Omranco.BLL.Services.Abstractions.BidServices;
using Omranco.BLL.Services.Abstractions.CommissionServices;
using Omranco.BLL.Services.Abstractions.OrderServices;
using Omranco.BLL.Services.Abstractions.PaymentServices;
using Omranco.BLL.Services.Abstractions.RatingServices;
using Omranco.BLL.Services.Abstractions.RfqServices;
using Omranco.BLL.Services.Abstractions.WalletServices;
using Omranco.BLL.Services.Implementations.AuthServices;
using Omranco.BLL.Services.Implementations.AuthServices.Emails;
using Omranco.BLL.Services.Implementations.BidServices;
using Omranco.BLL.Services.Implementations.CommissionServices;
using Omranco.BLL.Services.Implementations.OrderServices;
using Omranco.BLL.Services.Implementations.PaymentServices;
using Omranco.BLL.Services.Implementations.RatingServices;
using Omranco.BLL.Services.Implementations.RfqServices;
using Omranco.BLL.Services.Implementations.WalletServices;
using Omranco.DAL.Database;
using Omranco.Domain.Entities.Auths;

namespace Omranco.BLL
{
    public static class BLLDependencyInjection
    {

        public static IServiceCollection AddBLL(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IAuthService, AuthService>();
            services.AddSingleton<IJwtProvider, JwtProvider>();
            services.AddScoped<IEmailSender, EmailService>();



            services.AddScoped<IRfqMatchingService, RfqMatchingService>();
            services.AddScoped<IRfqService, RfqService>();
            services.AddScoped<IRfqInvitationService, RfqInvitationService>();
            services.AddScoped<IBidService, BidService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<ICommissionService, CommissionService>();
            services.AddScoped<IVendorWalletService, VendorWalletService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymobHmacService, PaymobHmacService>();
            services.AddScoped<IOrderSettlementService, OrderSettlementService>();
            services.AddScoped<IOrderCancellationService, OrderCancellationService>();
            services.AddScoped<IOrderDisputeService, OrderDisputeService>();



            services.AddHttpContextAccessor();

            services.AddAuthConfig(configuration);
            services.AddMapsterConfig();
            services.AddFluentValidationConfig();

            // To Handle Exceptions
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();


            // services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
            services.AddOptions<MailSettings>()
                 .BindConfiguration(nameof(MailSettings))
                 .ValidateDataAnnotations()
                 .ValidateOnStart();


            services.Configure<JwtOptions>(
                configuration.GetSection("Jwt"));

            services
                .AddOptions<PaymobOptions>()
                .Bind(configuration.GetSection(PaymobOptions.SectionName))
                .ValidateDataAnnotations();



            //======================================== Paymob =============
            services.AddHttpClient<IPaymobService, PaymobService>((serviceProvider, httpClient) =>
            {
                var paymobOptions = serviceProvider.GetRequiredService<IOptions<PaymobOptions>>().Value;
                httpClient.BaseAddress = new Uri(paymobOptions.BaseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(30);
            }
        );


            return services;
        }




        private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddOptions<JwtOptions>()
                .BindConfiguration("Jwt")
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var jwtSetting = configuration.GetSection("Jwt").Get<JwtOptions>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSetting?.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSetting?.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting?.Key!)),
                    RoleClaimType = ClaimTypes.Role
                };
            });

            return services;
        }
        private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton<IMapper>(new Mapper(config));
            return services;
        }

        private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation()
                   .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }


}
