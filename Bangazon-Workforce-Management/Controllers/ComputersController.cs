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
    public class ComputersController : Controller
    {
        private readonly IConfiguration _config;

        public ComputersController(IConfiguration config)
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

        // GET: Computers
        public ActionResult Index()
        {
            var computers = new List<Computer>();
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Manufacturer, ce.EmployeeId AS CEEmployeeId, e.Id AS EmployeeId, e.FirstName, e.LastName, e.DepartmentId
                        FROM Computer c
                        LEFT JOIN ComputerEmployee ce ON ce.ComputerId = c.Id
                        LEFT JOIN Employee e ON e.Id = ce.EmployeeId
                        ORDER BY Id
                    ";

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Computer computer = new Computer()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                        else
                        {
                            computer.DecomissionDate = null;
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))
                        {
                            Employee employee = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"))
                            };
                            computer.Employee = employee;
                        }
                        else
                        {
                            computer.Employee = null;
                        };
                        computers.Add(computer);
                    }
                    reader.Close();
                }
            }
            return View(computers);
        }

        // GET: Computers/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id AS ComputerId, c.Make, c.Manufacturer, c.PurchaseDate, c.DecomissionDate, e.FirstName, e.LastName, e.Id AS EmployeeId, e.IsSuperVisor, e.DepartmentId
                                        FROM Computer c
                                        LEFT JOIN ComputerEmployee ce on ce.ComputerId = @id
                                        LEFT JOIN Employee e on ce.EmployeeId = e.Id
                                        WHERE ComputerId = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee employee = new Employee();
                    Computer computer = new Computer();
                    if (reader.Read())
                    {
                        employee.Id = reader.GetInt32(reader.GetOrdinal("EmployeeId"));
                        employee.FirstName = reader.GetString(reader.GetOrdinal("FirstName"));
                        employee.LastName = reader.GetString(reader.GetOrdinal("LastName"));
                        employee.IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor"));
                        employee.DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"));
                        computer.Id = reader.GetInt32(reader.GetOrdinal("ComputerId"));
                        computer.Make = reader.GetString(reader.GetOrdinal("Make"));
                        computer.Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"));
                        computer.PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"));
                        computer.Employee = employee;
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                        else
                        {
                            computer.DecomissionDate = null;
                        };
                    };
                    reader.Close();
                    return View(computer);
                }
            }
        }

        // GET: Computers/Create
        public ActionResult Create()
        {
            var viewModel = new ComputerCreateViewModel();
            var employees = GetAllEmployees();
            var selectItems = employees
                .Select(employee => new SelectListItem
                {
                    Text = $"{employee.FirstName} {employee.LastName}",
                    Value = employee.Id.ToString()
                })
                .ToList();

            selectItems.Insert(0, new SelectListItem
            {
                Text = "Assign to employee...",
                Value = "0"
            });
            viewModel.Employees = selectItems;
            return View(viewModel);
        }

        // POST: Computers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                             INSERT INTO Computer (
                                PurchaseDate,  
                                Make,
                                Manufacturer
                            ) VALUES (
                                @purchaseDate,
                                @make,
                                @manufacturer
                            );

                            INSERT INTO ComputerEmployee (
                                ComputerId,
                                EmployeeId,
                                AssignDate
                            ) VALUES (
                                (SELECT MAX(Id) FROM Computer),
                                @employeeId,
                                @assignDate
                            )                           
                        ";

                        cmd.Parameters.AddWithValue("@purchaseDate", computer.PurchaseDate);
                        //cmd.Parameters.AddWithValue("@decomissionDate", computer.DecomissionDate);                     
                        cmd.Parameters.AddWithValue("@make", computer.Make);
                        cmd.Parameters.AddWithValue("@manufacturer", computer.Manufacturer);
                        cmd.Parameters.AddWithValue("@employeeId", computer.EmployeeId);
                        cmd.Parameters.AddWithValue("@assignDate", DateTime.Now);

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

        // GET: Computers/Delete/5
        public ActionResult Delete(int id)
        {
            var computer = GetOneComputer(id);
            return View(computer);
        }

        // POST: Computers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM ComputerEmployee
                                            WHERE ComputerId = @id;

                                            DELETE FROM Computer
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
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

        private List<Employee> GetAllEmployees()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FirstName, LastName, DepartmentId, IsSuperVisor FROM Employee";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor"))
                        });
                    }

                    reader.Close();

                    return employees;
                }
            }
        }

        private Computer GetOneComputer(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id AS ComputerId, c.Make, c.Manufacturer, c.PurchaseDate, c.DecomissionDate, e.FirstName, e.LastName, e.Id AS EmployeeId, e.IsSuperVisor, e.DepartmentId
                                        FROM Computer c
                                        LEFT JOIN ComputerEmployee ce on ce.ComputerId = @id
                                        LEFT JOIN Employee e on ce.EmployeeId = e.Id
                                        WHERE ComputerId = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee employee = new Employee();
                    Computer computer = new Computer();
                    if (reader.Read())
                    {
                        employee.Id = reader.GetInt32(reader.GetOrdinal("EmployeeId"));
                        employee.FirstName = reader.GetString(reader.GetOrdinal("FirstName"));
                        employee.LastName = reader.GetString(reader.GetOrdinal("LastName"));
                        employee.IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor"));
                        employee.DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"));
                        computer.Id = reader.GetInt32(reader.GetOrdinal("ComputerId"));
                        computer.Make = reader.GetString(reader.GetOrdinal("Make"));
                        computer.Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"));
                        computer.PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"));
                        computer.Employee = employee;
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                        else
                        {
                            computer.DecomissionDate = null;
                        };
                    };
                    reader.Close();
                    return computer;
                }
            }
        }
    }
}