using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Bangazon_Workforce_Management.Models.ViewModels
{
    public class EmployeeEditViewModel
    {
        public Employee Employee { get; set; }

        public ComputerEmployee ComputerEmployee { get; set; }
        public List<SelectListItem> Departments { get; set; }
        public List<SelectListItem> Computers { get; set; }
        public EmployeeEditViewModel() { }


        public EmployeeEditViewModel(Employee employee, List<Department> departmentList, List<Computer> computerList)
        {
            Employee = employee;
            Departments = departmentList
                .Select(department => new SelectListItem
                {
                    Text = department.Name,
                    Value = department.Id.ToString()
                })
                .ToList();
            Departments.Insert(0, new SelectListItem
            {
                Text = "Choose Department...",
                Value = "0"
            });
            Computers = computerList
                .Select(computer => new SelectListItem
                {
                    Text = computer.Make,
                    Value = computer.Id.ToString()
                })
                .ToList();
            Computers.Insert(0, new SelectListItem
            {
                Text = "Assign Computer...",
                Value = "0"
            });
        }
    }
}