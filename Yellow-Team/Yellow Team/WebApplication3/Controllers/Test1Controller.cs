using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class Test1Controller : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Test1
        public ActionResult Index()
        {
            return View(db.Test1.ToList());
        }

        // GET: Test1/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test1 test1 = db.Test1.Find(id);
            if (test1 == null)
            {
                return HttpNotFound();
            }
            return View(test1);
        }

        // GET: Test1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Test1/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,name")] Test1 test1)
        {
            if (ModelState.IsValid)
            {
                db.Test1.Add(test1);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(test1);
        }

        // GET: Test1/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test1 test1 = db.Test1.Find(id);
            if (test1 == null)
            {
                return HttpNotFound();
            }
            return View(test1);
        }

        // POST: Test1/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,name")] Test1 test1)
        {
            if (ModelState.IsValid)
            {
                db.Entry(test1).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(test1);
        }

        // GET: Test1/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test1 test1 = db.Test1.Find(id);
            if (test1 == null)
            {
                return HttpNotFound();
            }
            return View(test1);
        }

        // POST: Test1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Test1 test1 = db.Test1.Find(id);
            db.Test1.Remove(test1);
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
