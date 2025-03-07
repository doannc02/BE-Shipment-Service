using Dapr.Client;
using Ichiba.Shipment.Infrastructure.Services.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ichiba.Shipment.Infrastructure.Connecter.CustomerService
{
    public class DaprCustomerService
    {
        private readonly DaprClient _daprClient;

        public DaprCustomerService(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async Task<CustomerEntityView> CallCustomerServiceAsync(string shipmentId)
        {
            try
            {
                var response = await _daprClient.InvokeMethodAsync<CustomerEntityView>(
                    httpMethod: HttpMethod.Get,
                    appId: "customer-api",
                    methodName: $"api/customer/{shipmentId}" 
                );

                Console.WriteLine($"Customer Data: {response}");
                 return response!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling CustomerService: {ex.Message}");
                throw;
            }
        }
    }
}
