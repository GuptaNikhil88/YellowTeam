﻿//Author﻿: Akash Jain (akashjain1205@gmail.com, jain2ar@mail.uc.edu, github: akash1205)
//        Nikhil Gupta (nikhil.damoh@gmail.com, guptan6@mail.uc.edu, github: guptanikhil88)
//      This controller is used to handle user data. This is not being used currently as it is beyond the scope of the project due to 
//      compliance issues.
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
    public class OccupanciesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Occupancies
        public ActionResult Index()
        {
            var occupancies = db.Occupancies.Include(o => o.room);
            return View(occupancies.ToList());
        }

        // GET: Occupancies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Occupancy occupancy = db.Occupancies.Find(id);
            if (occupancy == null)
            {
                return HttpNotFound();
            }
            return View(occupancy);
        }

        // GET: Occupancies/Create
        public ActionResult Create()
        {
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number");
            return View();
        }

        // POST: Occupancies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,StartDate,EndDate,RoomId")] Occupancy occupancy)
        {
            
            if (ModelState.IsValid)
            {

                db.Occupancies.Add(occupancy);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", occupancy.RoomId);
            return View(occupancy);
        }

        // GET: Occupancies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Occupancy occupancy = db.Occupancies.Find(id);
            if (occupancy == null)
            {
                return HttpNotFound();
            }
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", occupancy.RoomId);
            return View(occupancy);
        }

        // POST: Occupancies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,StartDate,EndDate,RoomId")] Occupancy occupancy)
        {
            if (ModelState.IsValid)
            {
                db.Entry(occupancy).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", occupancy.RoomId);
            return View(occupancy);
        }

        // GET: Occupancies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Occupancy occupancy = db.Occupancies.Find(id);
            if (occupancy == null)
            {
                return HttpNotFound();
            }
            return View(occupancy);
        }

        // POST: Occupancies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Occupancy occupancy = db.Occupancies.Find(id);
            db.Occupancies.Remove(occupancy);
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
