using Bangazon_Workforce_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon_Workforce_Management.Models.ViewModels
{
    public class EmployeeEditViewModel
    {
        public Employee Employee { get; set; }

        public List<SelectListItem> Departments { get; set; }

        public List<SelectListItem> Computers { get; set; }

        public Computer Computer { get; set; }
    }
}