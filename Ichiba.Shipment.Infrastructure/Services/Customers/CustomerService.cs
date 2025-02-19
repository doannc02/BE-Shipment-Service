using Ichiba.Shipment.Infrastructure.Services.Models;
using Newtonsoft.Json;

namespace Ichiba.Shipment.Infrastructure.Services.Customers;

public class CustomerService : ICustomerService, ICustomerBatchLookupService
{
    private readonly HttpClient _httpClient;
    public CustomerService(HttpClient dbContext)
    {
        _httpClient = dbContext;
    }

    public async Task<CustomerEntityView> GetDetailCustomer(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/customer/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error retrieving customer: {response.StatusCode} - {errorContent}");
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"JSON Response: {jsonString}");

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return new CustomerEntityView();
        }

        var responseJson = JsonConvert.DeserializeObject<CustomerEntityView>(jsonString);

        if (responseJson == null)
        {
            throw new Exception("Deserialization returned null. Check the JSON structure and CustomerEntity definition.");
        }

        return responseJson;
    }

    public async Task<Dictionary<Guid, CustomerEntityView>> GetListCustomerByIds(List<Guid> customerIds)
    {
        if (customerIds == null || !customerIds.Any())
        {
            return new Dictionary<Guid, CustomerEntityView>();
        }

        try
        {
            string queryString = string.Join("&", customerIds.Select(id => $"customerIds={id}"));
            var response = await _httpClient.GetAsync($"/api/customer/by-ids?{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error retrieving customer: {response.StatusCode} - {errorContent}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"JSON Response: {jsonString}");

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new Dictionary<Guid, CustomerEntityView>(); 
            }

            var responseJson = JsonConvert.DeserializeObject<Dictionary<Guid, CustomerEntityView>>(jsonString);

            if (responseJson == null)
            {
                throw new Exception("Deserialization returned null. Check the JSON structure and CustomerEntityView definition.");
            }

            return responseJson;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching customer list: {ex.Message}");
            return new Dictionary<Guid, CustomerEntityView>(); 
        }
    }

}


