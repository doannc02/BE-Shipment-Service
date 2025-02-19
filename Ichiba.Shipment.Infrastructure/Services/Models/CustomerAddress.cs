using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ichiba.Shipment.Infrastructure.Services.Models
{
    public class CustomerAddress
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public virtual CustomerEntityView? Customer { get; set; }


        //public Guid Id { get; set; } = Guid.NewGuid();
        //public Guid CustomerId { get; set; }
        //public CustomerEntityView Customer { get; set; }

        //public string City { get; set; } = string.Empty;
        //public string District { get; set; } = string.Empty;
        //public string Ward { get; set; } = string.Empty;
        //public string Address { get; set; } = string.Empty;
    }
}
