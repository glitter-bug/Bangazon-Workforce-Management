using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon_Workforce_Management.Models
{
    public class Order
    {
        public int Id { get; internal set; }
        public int CustomerId { get; internal set; }
        public int PaymentTypeId { get; internal set; }
        public List<Product> Products { get; internal set; }
        public Customer Customer { get; internal set; }
    }
}
