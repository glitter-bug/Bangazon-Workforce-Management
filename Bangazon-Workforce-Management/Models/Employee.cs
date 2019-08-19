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
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public int DepartmentId { get; set; }
        [Required]
        public bool IsSuperVisor { get; set; }
        public Department Department { get; set; }
        public Computer Computer { get; set; }
        public List<TrainingProgram> TrainingPrograms { get; set; } = new List<TrainingProgram>();

        public static implicit operator Employee(int v)
        {
            throw new NotImplementedException();
        }
    }
}