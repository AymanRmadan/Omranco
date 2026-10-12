using Microsoft.OpenApi.Models;
using Omranco.BLL.Services.Abstractions.FileServices;
using Omranco.PL.Services;

namespace Omranco.PL
{
    public static class PLDependencyInjection
    {
        public static IServiceCollection AddPL(this IServiceCollection services, IConfiguration config)
        {
            services.AddSwaggerConfig();

            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            return services;
        }



        private static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {

            // Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Main_Structure Platform API",
                    Version = "v1"
                });


                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your JWT token (e.g., Bearer eyJhbGciOi...)."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
            });


            return services;
        }


    }
}
