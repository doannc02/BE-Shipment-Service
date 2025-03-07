using Ichiba.Shipment.Infrastructure.Connecter.CustomerService;
using Ichiba.Shipment.Infrastructure.Data;
using Ichiba.Shipment.Infrastructure.Services.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Ichiba.Shipment.Infrastructure
{
    public static class ConfigurationServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<ICustomerService, CustomerService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:8053");
            });

            services.AddHttpClient<ICustomerBatchLookupService, CustomerService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:8053");
            });
            services.AddScoped<DaprCustomerService>();
            services.AddDaprClient();
            services.AddDbContext<ShipmentDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
