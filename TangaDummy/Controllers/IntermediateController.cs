using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using TangaDummy.Models;

namespace TangaDummy.Controllers
{
    [HandleError]
    public class IntermediateController : Controller
    {
        // GET: Intermediate
        public ActionResult Index()
        {
            return View();
        }

        // GET: Intermediate/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Intermediate/Create
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult PaymentConfirmation()
        {
            // Create manager
            var manager = new UserManager<ApplicationUser>(
               new UserStore<ApplicationUser>(
                   new ApplicationDbContext()));

            // Find user
            var user = manager.FindById(User.Identity.GetUserId());

            //Find ID
            ViewData["ApplicationUser"] = user.IDNumber;

            //Get Checkout ID
            string varTempData = TempData["CheckOutID"].ToString();

            //Pass to Payment Confirmation View
            ViewData["CheckOutID"] = varTempData;

            return View();
        }

        public ActionResult MeterPaymentConfirmation()
        {
            // Create manager
            var manager = new UserManager<ApplicationUser>(
               new UserStore<ApplicationUser>(
                   new ApplicationDbContext()));

            // Find user
            var user = manager.FindById(User.Identity.GetUserId());

            //Find ID
            ViewData["ApplicationUser"] = user.IDNumber;

            //Get Checkout ID
            string varTempData = TempData["CheckOutID"].ToString();

            //Pass to Payment Confirmation View
            ViewData["CheckOutID"] = varTempData;

            return View();
        }

        // POST: Intermediate/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Intermediate/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Intermediate/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Intermediate/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Intermediate/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
