﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon_Workforce_Management.Models
{
    public class Department
    {
        public int Id { get; set; }
        [Required]

        [Display(Name = "Department")]
        public string Name { get; set; }
        [Required]
        public int Budget { get; set; }

        [Display(Name ="Number Of Employees")]
        public int NumberOfEmployees { get; set; }


        public List<Employee> Employees { get; set; } = new List<Employee>();
    }
}