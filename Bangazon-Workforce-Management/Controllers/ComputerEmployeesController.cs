using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bangazon_Workforce_Management.Controllers
{
    public class ComputerEmployeesController : Controller
    {
        // GET: ComputerEmployees
        public ActionResult Index()
        {
            return View();
        }

        // GET: ComputerEmployees/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ComputerEmployees/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ComputerEmployees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ComputerEmployees/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ComputerEmployees/Edit/5
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

        // GET: ComputerEmployees/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ComputerEmployees/Delete/5
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