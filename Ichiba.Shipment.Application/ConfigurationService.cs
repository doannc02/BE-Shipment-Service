using Ichiba.Shipment.Application.Shipments.Commands;
using Ichiba.Shipment.Domain.Interfaces;
using Ichiba.Shipment.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ichiba.Shipment.Application
{
    public static class ConfigurationService
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection service)
        {
            service.AddAutoMapper(Assembly.GetExecutingAssembly());
            service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(CreateShipmentCommandHandler).Assembly));

            service.AddScoped<IShipmentRepository, ShipmentRepository>();


            return service;
        }
    }
}
