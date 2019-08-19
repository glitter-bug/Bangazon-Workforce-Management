using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon_Workforce_Management.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Department Id")]
        public int DepartmentId { get; set; }
        [Display(Name = "Computer Id")]
        public int ComputerId { get; set; }
        [Required]
        [Display(Name = "Supervisor")]
        public bool IsSuperVisor { get; set; }
        public Department Department { get; set; }
        public Computer Computer { get; set; }
        public List<TrainingProgram> TrainingPrograms { get; set; } = new List<TrainingProgram>();

        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
}