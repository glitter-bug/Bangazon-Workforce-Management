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
<<<<<<< HEAD
                     SELECT e.Id AS EmployeeId, e.FirstName, e.LastName, d.Name AS DepartmentName, c.PurchaseDate, c.Make AS ComputerMake, c.Manufacturer As ComputerManufacturer,
=======
                     SELECT e.Id AS EmployeeId, e.FirstName, e.LastName, d.Id AS DepartmentId, d.Name AS DepartmentName, c.Id AS ComputerId, c.PurchaseDate, c.Make AS ComputerMake, c.Manufacturer As ComputerManufacturer,
>>>>>>> master
                        tp.[Name] AS TrainingProgram, tp.StartDate, tp.EndDate, tp.MaxAttendees, d.Budget
                    FROM Department d 
                    LEFT JOIN Employee e ON d.Id = e.DepartmentId
                    LEFT JOIN ComputerEmployee ce ON ce.EmployeeId = e.Id
                    LEFT JOIN Computer c ON ce.ComputerId = c.Id
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
                            Computer computer = new Computer();
                            employee.Computer.Make = reader.GetString(reader.GetOrdinal("ComputerMake"));
                            employee.Computer.Manufacturer = reader.GetString(reader.GetOrdinal("ComputerManufacturer"));
                            employee.Computer.PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"));
                        
                        }


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

        // GET: Employees/Edit/5
        public ActionResult Edit(int id)
        {
            // new up a viewmodel for edit
            var viewModel = new EmployeeEditViewModel();
            // create a method that gets a single employee, assign it to a variable
            var employee = GetSingleEmployee(id);
            // create a method that gets a single employee, assign it to a variable 
            var oneComputer = GetSingleComputer(id);
            // create a method that gets all departments, assign it to a variable 
            var departments = GetAllDepartments();
            // create a method that gets all departments, assign it to a variable 
            var computers = GetAllComputers();

            // create  new SelectListItem instances for departments to create a drop down menu. Assign it to a variable
            var departmentSelects = departments
                //for every department put into the SelectList, Assign the default text of the drop down, Assign the value of each SelectListItem
                .Select(department => new SelectListItem
                {
                    Text = department.Name,
                    Value = department.Id.ToString()
                })
                .ToList();

            departmentSelects.Insert(0, new SelectListItem
            {
                Text = "Choose department...",
                Value = "0"
            });

            var computerSelects = computers
                .Select(computer => new SelectListItem
                {
                    Text = computer.Make,
                    Value = computer.Id.ToString()
                })
                .ToList();
            // If a single computer exists (GetSingleComputer(id), Insert a SelectListItem for a computer at index 0. Assign the default text of the drop down, assign the value of each SelectListItem
            if (oneComputer != null)
            {
                computerSelects.Insert(0, new SelectListItem
                {
                    Text = "Choose computer...",
                    Value = oneComputer.Id.ToString()
                });
            }
            else
            {

            }
            // Assign methods stored in variables to the viewModel
            viewModel.Computer = oneComputer;
            viewModel.Employee = employee;
            viewModel.Departments = departmentSelects;
            viewModel.Computers = computerSelects;

            return View(viewModel);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EmployeeEditViewModel model)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        if (model.Computer != null)
                        {
                            cmd.CommandText = @"
                                            UPDATE Employee 
                                            SET LastName = @lastName,
                                                DepartmentId = @departmentId
                                            WHERE Id = @id;

                                            UPDATE ComputerEmployee
                                                Set EmployeeId = @id,
                                                ComputerId = @computerId,
                                                AssignDate = GETDATE(),
                                                UnassignDate = null
                                            WHERE EmployeeId = @id
                                                ";
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@lastName", model.Employee.LastName);
                            cmd.Parameters.AddWithValue("@departmentId", model.Employee.DepartmentId);
                            cmd.Parameters.AddWithValue("@computerId", model.Computer.Id);
                        }
                        else
                        {
                            cmd.CommandText = @"
                                            UPDATE Employee 
                                            SET LastName = @lastName,
                                                DepartmentId = @departmentId
                                            WHERE Id = @id
                                              ";
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@lastName", model.Employee.LastName);
                            cmd.Parameters.AddWithValue("@departmentId", model.Employee.DepartmentId);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View();
            }
        }
<<<<<<< HEAD

        // GET: Employee/Assign/2
        public ActionResult Assign(int id)
        {
            var viewModel = new TrainingAssignViewModel();
            var allPrograms = new TrainingAssignViewModel();
            allPrograms.TrainingPrograms = CreateTrainingSelections(GetAllPrograms());
            viewModel.TrainingPrograms = CreateTrainingSelections(GetEligibleTrainingPrograms(id));
            viewModel.EmployeeId = id;
            allPrograms.EmployeeId = id;
            if (viewModel.TrainingPrograms.Count > 0)
            {
                return View(viewModel);
            }
            else
            {
                return View(allPrograms);
            }
            return View(allPrograms);

        }



        // POST: Employee/Assign/2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Assign(int id, TrainingAssignViewModel assign)
        {
=======
        private Employee GetSingleEmployee(int id)
        {
            using (SqlConnection conn = Connection)
            {
                Employee employee = null;
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT e.Id, e.FirstName, e.LastName, e.IsSuperVisor, e.DepartmentId, d.Name AS DepartmentName, c.PurchaseDate, c.Make AS ComputerMake, c.Manufacturer As ComputerManufacturer,
                        tp.[Name] AS TrainingProgram, tp.StartDate, tp.EndDate, tp.MaxAttendees, d.Budget, c.Id AS ComputerId
                    FROM Department d 
                    LEFT JOIN Employee e ON d.Id = e.DepartmentId
                    LEFT JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId
                    LEFT JOIN Computer c ON ce.ComputerId = c.Id
                    LEFT JOIN TrainingProgram tp ON c.Id = tp.Id
                    WHERE e.Id = @id
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
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("ComputerId")))
                        {
                            employee.ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId"));
                        }
                        else
                        {
                            employee.ComputerId = 0;
                        }
                    }
                }
                return employee;
            }
        }
        private Computer GetSingleComputer(int id)
        {
            Computer computer = null;
>>>>>>> master
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
<<<<<<< HEAD
                    cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
                                        VALUES (@employeeId, @trainingProgramId)";
                    cmd.Parameters.AddWithValue("@employeeId", id);
                    cmd.Parameters.AddWithValue("@trainingProgramId", assign.TrainingProgramId);

                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction(nameof(Details), new { id = id });
=======
                    cmd.CommandText = @"
                        SELECT c.Id, c.DecomissionDate, c.Make, c.Manufacturer, c.PurchaseDate
                        FROM ComputerEmployee ce
                        LEFT JOIN Computer c ON c.Id = ce.ComputerId 
                        WHERE ce.EmployeeId = @id
                    ";
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                    }
                    reader.Close();
                }
            }
            return computer;
>>>>>>> master
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
<<<<<<< HEAD
        private List<TrainingProgram> GetEligibleTrainingPrograms(int id)
        {
=======

        private List<Computer> GetAllComputers()
        {
            List<Computer> computers = new List<Computer>();
>>>>>>> master
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
<<<<<<< HEAD
                    cmd.CommandText = @"SELECT tp.Name AS [Name], tp.StartDate AS StartDate, tp.EndDate AS EndDate, e.Id AS EmployeeId, tp.Id AS TrainingProgramId, tp.MaxAttendees AS MaxAttendees 
                                        FROM Employee e 
                                        JOIN EmployeeTraining et ON e.Id = et.EmployeeId 
                                        JOIN TrainingProgram tp ON et.TrainingProgramId = tp.Id 
                                        WHERE CURRENT_TIMESTAMP < tp.StartDate AND @id != EmployeeId
                                        
                                        ";


                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();
                    while (reader.Read())
                    {
                        if (!trainingPrograms.Any(tp => tp.Id == reader.GetInt32(reader.GetOrdinal("TrainingProgramId"))))
                        {

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

        private List<TrainingProgram> GetAllPrograms()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram WHERE CURRENT_TIMESTAMP < StartDate";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<TrainingProgram> allPrograms = new List<TrainingProgram>();
                    while (reader.Read())
                    {
                        allPrograms.Add(new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        });
                    }

                    reader.Close();

                    return allPrograms;
                }
            }
=======
                    cmd.CommandText = @"
                                        SELECT Id, PurchaseDate, DecomissionDate, Make, Manufacturer 
                                        FROM Computer
                                      ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Computer computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }

                        computers.Add(computer);

                    }
                    reader.Close();
                }
            }
            return (computers);
>>>>>>> master
        }
    }
}




