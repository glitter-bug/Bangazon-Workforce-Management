﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon_Workforce_Management.Models
{

    public class TrainingProgram
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Training Program")]

        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int MaxAttendees { get; set; }

        public List<Employee> Attendees { get; set; } = new List<Employee>();
       
    }
}