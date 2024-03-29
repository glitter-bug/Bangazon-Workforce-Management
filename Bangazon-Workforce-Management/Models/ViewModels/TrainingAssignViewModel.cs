﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon_Workforce_Management.Models.ViewModels
{
    public class TrainingAssignViewModel
    {
        public int EmployeeId { get; set; }

        public int TrainingProgramId { get; set; }

        public List<SelectListItem> TrainingPrograms { get; set; }
    
    }
}
