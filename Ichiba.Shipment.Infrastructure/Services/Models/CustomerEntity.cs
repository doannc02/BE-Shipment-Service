using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ichiba.Shipment.Infrastructure.Services.Models
{
    public class CustomerEntityView
    {

        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? isDelete { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<CustomerAddress> Addresses { get; set; }

    }

    public class CustomerAddressDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string Address { get; set; }
    }
}
