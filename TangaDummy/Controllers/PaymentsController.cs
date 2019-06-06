using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TangaDummy;
using TangaDummy.Models;

namespace TangaDummy.Controllers
{
    public class PaymentsController : Controller
    {
        private Tanga_City_CouncilEntities db = new Tanga_City_CouncilEntities();

        // GET: Payments
        public ActionResult Index()
        {
            // Create manager
            var manager = new UserManager<ApplicationUser>(
               new UserStore<ApplicationUser>(
                   new ApplicationDbContext()));

            // Find user
            var user = manager.FindById(User.Identity.GetUserId());
            
            //Find ID
            var nationalID = user.IDNumber;

            var payments = db.Payments.Include(p => p.PermitType1).Where(v => v.PersonTable.IDNumber.Equals(nationalID)).Include(p => p.PersonTable);
            return View(payments.ToList());
        }

        // GET: Payments/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // GET: Payments/Create
        public ActionResult Create()
        {
            ViewBag.PermitType = new SelectList(db.PermitTypes, "PermitTypeID", "Permit_Name");
            ViewBag.PersonID = new SelectList(db.PersonTables, "PersonID", "Person_Name");
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PaymentID,PaymentMade,PersonID,PermitType,PaymentDate")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Payments.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PermitType = new SelectList(db.PermitTypes, "PermitTypeID", "Permit_Name", payment.PermitType);
            ViewBag.PersonID = new SelectList(db.PersonTables, "PersonID", "Person_Name", payment.PersonID);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            ViewBag.PermitType = new SelectList(db.PermitTypes, "PermitTypeID", "Permit_Name", payment.PermitType);
            ViewBag.PersonID = new SelectList(db.PersonTables, "PersonID", "Person_Name", payment.PersonID);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PaymentID,PaymentMade,PersonID,PermitType,PaymentDate")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PermitType = new SelectList(db.PermitTypes, "PermitTypeID", "Permit_Name", payment.PermitType);
            ViewBag.PersonID = new SelectList(db.PersonTables, "PersonID", "Person_Name", payment.PersonID);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Payment payment = db.Payments.Find(id);
            db.Payments.Remove(payment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
