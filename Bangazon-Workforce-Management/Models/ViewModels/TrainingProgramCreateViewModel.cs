
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;


namespace Bangazon_Workforce_Management.Models.ViewModels
{
    public class TrainingProgramCreateViewModel
    {
        public List<SelectListItem> Employees { get; set; }
        public TrainingProgram TrainingProgram { get; set; }

        private readonly string _connectionString;

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_connectionString);
            }
        }

        public TrainingProgramCreateViewModel() { }

        public TrainingProgramCreateViewModel(string connectionString)
        {
            _connectionString = connectionString;

            Employees = GetAllEmployees()
                .Select(employee => new SelectListItem
                {
                    Text = employee.FirstName,
                    Value = employee.Id.ToString()
                })
                .ToList();

            Employees.Insert(0, new SelectListItem
            {
                Text = "Choose employee...",
                Value = "0"
            });
        }

        private List<Employee> GetAllEmployees()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FirstName FROM Employee";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        });
                    }

                    reader.Close();

                    return employees;
                }
            }
        }
    }
}

