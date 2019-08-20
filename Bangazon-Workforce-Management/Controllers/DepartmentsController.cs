using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Bangazon_Workforce_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Bangazon_Workforce_Management.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly IConfiguration _config;
        public DepartmentsController(IConfiguration config)
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


        // GET: Departments
        public ActionResult Index()
        {
            var departments = new List<Department>();
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT d.Id as DepartmentId, d.Name, d.Budget,
                                COUNT(e.Id) as NumberOfEmployees
                                FROM Department d
                                LEFT JOIN Employee e ON d.ID = e.DepartmentId
                        GROUP BY d.Id, d.Name, d.Budget
                    ";
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        departments.Add(new Department()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            NumberOfEmployees = reader.GetInt32(reader.GetOrdinal("NumberOfEmployees"))
                        });
                    }
                    reader.Close();
                }
            }
            return View(departments);
        }
        //GET: Departments/Details/5
        public ActionResult Details(int id)
        {
            Department department = null;
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT d.Id as DepartmentId, d.Name, d.Budget, e.FirstName, e.LastName, e.Id as EmployeeId, e.DepartmentId AS EmployeeDepartmentId, e.IsSuperVisor
                        FROM Department d
                        LEFT JOIN Employee e ON e.DepartmentId = d.Id
                        WHERE d.Id = @id
                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (department == null)
                        {
                            department = new Department()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                               
                            };
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))                        
                        {
                            department.Employees.Add(new Employee()

                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("EmployeeDepartmentId"))
                            });
                        }
                        else
                        {
                            department.Employees = new List<Employee>();
                        }
                    }
                }
            }
            return View(department);
        }


        // GET: Departments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            INSERT INTO Department (
                                Name, Budget
                            ) VALUES (
                                @Name, @Budget
                            )
                        ";
                        cmd.Parameters.AddWithValue("@Name", department.Name);
                        cmd.Parameters.AddWithValue("@Budget", department.Budget);

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


        // GET: Departments/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Departments/Edit/5
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

        // GET: Departments/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Departments/Delete/5
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
    }
}