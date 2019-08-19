using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Bangazon_Workforce_Management.Models;
using Bangazon_Workforce_Management.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace Bangazon_Workforce_Management.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IConfiguration _config;

        public EmployeesController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Employees
        public ActionResult Index()
        {
            var employees = new List<Employee>();
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, DepartmentId, IsSuperVisor
                        FROM Employee
                    ";

                    SqlDataReader reader = cmd.ExecuteReader();


                    while (reader.Read())
                    {
                        employees.Add(new Employee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor"))
                        });
                    }
                    reader.Close();
                }
            }

            return View(employees);
        }

        // GET: Employees/Details/5
        public ActionResult Details(int id)
        {
            Employee employee = null;
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                     SELECT e.Id AS EmployeeId, e.FirstName, e.LastName, d.Name AS DepartmentName, c.PurchaseDate, c.Make AS ComputerMake, c.Manufacturer As ComputerManufacturer,
                        tp.[Name] AS TrainingProgram, tp.StartDate, tp.EndDate, tp.MaxAttendees, d.Budget
                    FROM Department d 
                    LEFT JOIN Employee e ON d.Id = e.DepartmentId
                    LEFT JOIN Computer c ON e.Id = c.Id
                    LEFT JOIN TrainingProgram tp ON c.Id = tp.Id
                    WHERE e.Id = @id
                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        employee = new Employee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        };

                        employee.Computer = new Computer();
                        if (!reader.IsDBNull(reader.GetOrdinal("ComputerMake")))
                        {
                            employee.Computer.Make = reader.GetString(reader.GetOrdinal("ComputerMake"));
                            employee.Computer.Manufacturer = reader.GetString(reader.GetOrdinal("ComputerManufacturer"));
                            employee.Computer.PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"));
                        };

                        employee.Department = new Department()
                        {
                            Name = reader.GetString(reader.GetOrdinal("DepartmentName")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget"))

                        };
                        var trainingProgram = new TrainingProgram();
                        if (!reader.IsDBNull(reader.GetOrdinal("StartDate")))
                        {

                            trainingProgram.Name = reader.GetString(reader.GetOrdinal("TrainingProgram"));
                            trainingProgram.StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate"));
                            trainingProgram.EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"));
                            trainingProgram.MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"));
                        }
                        employee.TrainingPrograms.Add(trainingProgram);
                    };
                };
            }

            return View(employee);
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            var viewModel = new EmployeeCreateViewModel();
            var departments = GetAllDepartments();
            var selectItems = departments
           .Select(department => new SelectListItem
           {
               Text = department.Name,
               Value = department.Id.ToString()

           })
           .ToList();
            selectItems.Insert(0, new SelectListItem
            {
                Text = "Choose Department...",
                Value = "0"
            });
            viewModel.Departments = selectItems;
            return View(viewModel);
        }

        // POST: Studenst/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            INSERT INTO Employee (
                                FirstName, 
                                LastName, 
                                IsSuperVisor, 
                                DepartmentId
                            ) VALUES (
                                @firstName,
                                @lastName,
                                @isSuperVisor,
                                @departmentId
                            )
                        ";

                        cmd.Parameters.AddWithValue("@firstName", employee.FirstName);
                        cmd.Parameters.AddWithValue("@lastName", employee.LastName);
                        cmd.Parameters.AddWithValue("@isSuperVisor", employee.IsSuperVisor);
                        cmd.Parameters.AddWithValue("@departmentId", employee.DepartmentId);

                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Employee/Assign/2
        public ActionResult Assign(int id)
        {
            var viewModel = new TrainingAssignViewModel();
            viewModel.TrainingPrograms = CreateTrainingSelections(GetEligibleTrainingPrograms(id));
            viewModel.EmployeeId = id;
            if (viewModel.TrainingPrograms.Count > 0)
            {
                return View(viewModel);
            }
            else return RedirectToAction(nameof(Details), new { id = id });

        }



        // POST: Employee/Assign/2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Assign(int id, TrainingAssignViewModel assign)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
                                        VALUES (@employeeId, @trainingProgramId)";
                    cmd.Parameters.AddWithValue("@employeeId", id);
                    cmd.Parameters.AddWithValue("@trainingProgramId", assign.TrainingProgramId);

                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }
        private List<Department> GetAllDepartments()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Department";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Department> departments = new List<Department>();
                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        });
                    }

                    reader.Close();

                    return departments;
                }
            }
        }
        private List<TrainingProgram> GetEligibleTrainingPrograms(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT tp.Name AS [Name], tp.StartDate AS StartDate, tp.EndDate AS EndDate, e.Id AS EmployeeId, tp.Id AS TrainingProgramId, tp.MaxAttendees AS MaxAttendees 
                                        FROM Employee e 
                                        JOIN EmployeeTraining et ON e.Id = et.EmployeeId 
                                        JOIN TrainingProgram tp ON et.TrainingProgramId = tp.Id 
                                        WHERE CURRENT_TIMESTAMP < tp.StartDate AND e.Id != @id
                                        
                                        SELECT COUNT(Id) AS AttendeeCount
                                        FROM EmployeeTraining
                                        GROUP BY TrainingProgramId
                                        ";

                            
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();
                    while (reader.Read())
                    {
                        if (!trainingPrograms.Any(tp => tp.Id == reader.GetInt32(reader.GetOrdinal("TrainingProgramId")))) {

                            trainingPrograms.Add(new TrainingProgram
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("TrainingProgramId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))

                            });
                        }

                        
                    }
                    return trainingPrograms;


                }
            }
        }
        private List<SelectListItem> CreateTrainingSelections(List<TrainingProgram> programz)
        {
            return programz.Select(prog => new SelectListItem()
            {
                Text = prog.Name,
                Value = prog.Id.ToString()
            }).ToList();
        }
    }
}