using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bangazon_Workforce_Management.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastActiveTime { get; set; }

        public List<PaymentType> PaymentTypes { get; set; } = new List<PaymentType>();

        public List<Product> Products { get; set; } = new List<Product>();
    }
}