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
                     SELECT e.FirstName, e.LastName, d.Name AS DepartmentName, c.PurchaseDate, c.Make AS ComputerMake, c.Manufacturer As ComputerManufacturer,
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
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
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

        // GET: Students/Edit/5
        public ActionResult Edit(int id)
        {
            //use GetSingleEmployee to get the Student you want to edit
            Employee employee = GetSingleEmployee(id);
            //Use GetAllDepartments to get a list of cohorts
            List<Department> departments = GetAllDepartments();
            //Use GetAllComputers to get a list of cohorts
            List<Computer> computers = GetAllComputers();
            //pass Employee, the List of Departments and the lits of Com[puters into an instance of the EmployeeEditViewModel
            var viewModel = new EmployeeEditViewModel(employee, departments, computers);
            //pass the instance of the viewModel into View()
            return View(viewModel);
        }


        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EmployeeEditViewModel model)

        {
            try
            {
                // TODO: Add update logic here
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                            UPDATE ComputerEmployee
                                            SET
                                                ComputerId = @computerId,
                                                UnassignDate = CURRENT_TIMESTAMP,
                                            Where EmployeeId = @id;

                                            INSERT INTO ComputerEmployee
                                            WHERE ComputerId = @computerId, EmployeeId = @employeeId, 

                                            UPDATE Employee
                                            SET
                                                FirstName = @firstName,
                                                LastName = @lastName,
                                                IsSuperVisor = @isSuperVisor,
                                                DepartmentId = @departmentId
                                            WHERE Id = @id";
                        cmd.Parameters.AddWithValue("@lastName", model.Employee.LastName);
                        cmd.Parameters.AddWithValue("@departmentId", model.Employee.DepartmentId);
                        cmd.Parameters.AddWithValue("@computerId", model.ComputerEmployee.ComputerId);
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));

                    }
                }
            }
            catch
            {
                return View();
            }
        }
        private Employee GetSingleEmployee(int id)
        {
            using (SqlConnection conn = Connection)
            {
                Employee employee = null;
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, IsSuperVisor, DepartmentId
                        FROM Employee
                        WHERE Id = @id
                    ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        employee = new Employee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"))
                        };
                    }
                }
                return employee;
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

        private List<Computer> GetAllComputers()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Make FROM Computer";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Computer> computers = new List<Computer>();
                    while (reader.Read())
                    {
                        computers.Add(new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                        });
                    }

                    reader.Close();

                    return computers;
                }
            }
        }
    }
}