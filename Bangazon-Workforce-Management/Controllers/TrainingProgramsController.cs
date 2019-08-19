﻿using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Bangazon_Workforce_Management.Models;
using System;

namespace Bangazon_Workforce_Management.Controllers
{
    public class TrainingProgramsController : Controller
    {

        private readonly IConfiguration _config;

        public TrainingProgramsController(IConfiguration config)
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
        // GET: TrainingPrograms
        public ActionResult Index()
        {
            var trainingPrograms = new List<TrainingProgram>();
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT  Id, Name, StartDate, EndDate,MaxAttendees
                        FROM TrainingProgram 
                        WHERE CURRENT_TIMESTAMP < StartDate
                         
                        
                    ";

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        trainingPrograms.Add(new TrainingProgram()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        });
                    }
                    reader.Close();
                }
            }

            return View(trainingPrograms);
        }

        // GET: TrainingPrograms/Details/5
        public ActionResult Details(int id)
        {
            TrainingProgram trainingProgram = null;
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                       SELECT tp.Id, tp.Name, tp.StartDate, tp.EndDate, tp.MaxAttendees,e.Id AS EmployeeId, e.FirstName, e.LastName
                        FROM TrainingProgram tp
                        LEFT JOIN EmployeeTraining et ON tp.Id = et.TrainingProgramId
                        LEFT JOIN Employee e ON et.EmployeeId = e.Id 
                        WHERE tp.Id = @id
                        
                    ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if (trainingProgram == null)
                        {
                            trainingProgram = new TrainingProgram()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),


                            };
                        }
                       
                        
                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))
                        {
                            trainingProgram.Attendees.Add(
                                new Employee
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),

                                }
                            );
                            
                        }
                    }

                    return View(trainingProgram);
                }
            }
        }

        // GET: TrainingPrograms/Create
        [HttpGet]
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: TrainingPrograms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TrainingProgram trainingProgram)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            INSERT INTO TrainingProgram (
                                Name,
                                StartDate,
                                EndDate,
                                MaxAttendees
                                ) VALUES (
                                @name,
                                @startDate,
                                @endDate,
                                @maxAttendees
                            
                            
                        ";

                        cmd.Parameters.AddWithValue("@name", trainingProgram.Name);
                        cmd.Parameters.AddWithValue("@startDate", trainingProgram.StartDate);
                        cmd.Parameters.AddWithValue("@endDate", trainingProgram.EndDate);
                        cmd.Parameters.AddWithValue("@maxAttendees", trainingProgram.MaxAttendees);

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



        // GET: TrainingPrograms/Edit/5
        public ActionResult Edit(int id)
        {
            TrainingProgram trainingProgram = GetSingleTrainingProgram(id);
            return View(trainingProgram);
        }


        // POST: TrainingPrograms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, TrainingProgram trainingProgram)
        {
            try
            {
                // TODO: Add update logic here
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE TrainingProgram
                                            SET
                                                Name = @name,
                                                StartDate = @startDate,
                                                EndDate = @endDate,
                                                MaxAttendees = @maxAttendees
                                     
                                            WHERE Id = @id";
                        cmd.Parameters.AddWithValue("@name",trainingProgram.Name);
                        cmd.Parameters.AddWithValue("@startDate",trainingProgram.StartDate);
                        cmd.Parameters.AddWithValue("@endDate",trainingProgram.EndDate);
                        cmd.Parameters.AddWithValue("@maxAttendees",trainingProgram.MaxAttendees);
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));

                    }
                }
            }
            catch (Exception byebye)
            {
                return View();
            }
        }


        // GET: TrainingPrograms/Delete/5
        public ActionResult Delete(int id)
        {
            TrainingProgram trainingProgram = GetSingleTrainingProgram(id);
            return View(trainingProgram);
        }

        // POST: TrainingPrograms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTrainingProgram(int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM EmployeeTraining
                                                WHERE TrainingProgramId = @id;
                                            DELETE FROM TrainingProgram
                                                WHERE Id = @id";
                        cmd.Parameters.AddWithValue("@id", id);

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

        private TrainingProgram GetSingleTrainingProgram(int id)
        {
            using (SqlConnection conn = Connection)
            {
                TrainingProgram trainingProgram = null;
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, StartDate, EndDate, MaxAttendees
                        FROM TrainingProgram
                        WHERE Id = @id
                    ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        trainingProgram = new TrainingProgram()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                        };
                    }
                }
                return trainingProgram;
            }
        }
    }

}