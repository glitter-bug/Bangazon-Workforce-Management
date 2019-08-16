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
        public ActionResult Details(int id, Computer computer, Department department, TrainingProgram trainingProgram)
        {
            Employee employee = null;
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                       SELECT e.FirstName, e.LastName, d.Name AS DepartmentName, c.Make AS ComputerMake, c.Manufacturer As ComputerManufacturer, tp.Name AS TrainingProgram
                    FROM Department d 
                    JOIN Employee e ON d.Id = e.DepartmentId
                    JOIN Computer c ON e.Id = c.Id
                    JOIN TrainingProgram tp ON c.Id = tp.Id
                    WHERE e.Id = @id
                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        employee = new Employee()
                        {
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                        };
                        computer = new Computer()
                        {
                            Make = reader.GetString(reader.GetOrdinal("ComputerMake")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("ComputerManufacturer")),
                        };
                        department = new Department()
                        {
                            Name = reader.GetString(reader.GetOrdinal("DepartmentName")),
                        };
                        trainingProgram = new TrainingProgram()
                        {
                            Name = reader.GetString(reader.GetOrdinal("TrainingProgram")),
                        };
                    }
                }
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
                            INSERT INTO Student (
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
                        cmd.Parameters.AddWithValue("@slackHandle", employee.IsSuperVisor);
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
    }
}