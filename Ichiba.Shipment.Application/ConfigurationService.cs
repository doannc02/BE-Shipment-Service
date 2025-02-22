using Ichiba.Shipment.Application.Packages;
using Ichiba.Shipment.Application.Packages.Commands;
using Ichiba.Shipment.Application.Shipments.Commands;
using Ichiba.Shipment.Domain.Interfaces;
using Ichiba.Shipment.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;


namespace Ichiba.Shipment.Application;

public static class ConfigurationService
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection service)
    {
        service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(CreateMultiShipmentsCommandHandler).Assembly));
        service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(CreateShipmentCommandHandler).Assembly));
        service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(DeleteShipmentCommandHandler).Assembly));
        service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(UpdateShipmentCommandHandler).Assembly));
        service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(CreateMultiShipmentsCommand).Assembly));
        service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(CreatePackageCommandHandler).Assembly));
        service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(UpdatePackageCommandHandler).Assembly));
        service.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(DeletePackageCommandHandler).Assembly));
 
        service.AddScoped<IShipmentRepository, ShipmentRepository>();


        return service;
    }
}
