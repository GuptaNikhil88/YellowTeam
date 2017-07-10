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
    public class SchedulesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public static string firstCheckin;

        // GET: Schedules
        
        public ActionResult Index()
        {
            DateTime d1 = System.DateTime.Now;
            d1 = d1.AddDays(-1);
            string previousday = d1.ToShortDateString();
            var Curdate = System.DateTime.Now.ToShortDateString();
            var count = db.Schedules.Where(c=>c.created==Curdate).Count();
            if(count == 0) { 
            LoadSchedule(previousday);
            }
            var schedules = db.Schedules.Where(s => s.created == Curdate).OrderBy(s=>s.Priority);
            return View(schedules.ToList());
        }

        public ActionResult Finalize()
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedules = db.Schedules.Where(sc => sc.created == Curdate);
            foreach(Schedule sc in schedules)
            {
            //    sc.ScheduledCheckIn = sc.EstimatedCheckin;
              //  sc.ScheduledCheckout = sc.EstimatedCheckout;
            }
            db.SaveChanges();
            return RedirectToAction("Index", "Schedules");
        }

        public void LoadSchedule(string pd)
        {
            string previousday = pd;
            
            var scheduleid = db.Schedules.Max(sid => sid.Id);
            scheduleid = ++scheduleid;
            var previousschedule = db.Schedules.Where(ps => ps.created==previousday);
            
            foreach (Schedule ps in previousschedule){
                var NewSchedule = new Schedule();
                NewSchedule.Id = scheduleid;
                NewSchedule.Length = ps.Length;
              //  NewSchedule.Parent = false;
                NewSchedule.Priority = ps.Priority;
                NewSchedule.room = ps.room;
                NewSchedule.RoomId = ps.RoomId;
                //NewSchedule.EstimatedCheckin = ps.EstimatedCheckin;
                //NewSchedule.EstimatedCheckout = ps.EstimatedCheckout;
                NewSchedule.CheckIn = null;
                NewSchedule.CheckOut = null;
                NewSchedule.CheckedIn = false;
                NewSchedule.completed = 0;
                NewSchedule.created = System.DateTime.Now.ToShortDateString();
                
                scheduleid = ++scheduleid;
                db.Schedules.Add(NewSchedule);
                
            }
            db.SaveChanges();
            //updateschedule();
        }

        public void updateschedule()
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedules = db.Schedules.Where(s => s.created == Curdate);
            foreach(Schedule s in schedules)
            {
                TimeScheduling(s);
                db.Entry(s).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public ActionResult CheckIn(int? id)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedule = db.Schedules.Single(s => s.Id == id);
            
                if (validatePreviousCheckin(schedule))
                {
                    schedule.CheckIn = System.DateTime.Now.ToShortTimeString();
                    schedule.CheckedIn = true;
                    db.Entry(schedule).State = EntityState.Modified;
                    db.SaveChanges();
                    var updateschedule = db.Schedules.Where(up => up.Priority >= schedule.Priority && up.created == Curdate)
                                                  .OrderBy(up => up.Priority);
                    foreach (Schedule up in updateschedule)
                    {
                        TimeSchedulingIN(up);
                    }
                    db.SaveChanges();

                    
                }
            return RedirectToAction("Index");
        }

    public Boolean validatePreviousCheckin(Schedule schedule)
    {
        if(schedule.Priority > 1) { 
        var previousSchedule = db.Schedules.Single(ps => ps.Priority == schedule.Priority - 1);
        if (previousSchedule.CheckedIn == true)
        {
            if (previousSchedule.completed == 0)
            {
                previousSchedule.CheckOut = System.DateTime.Now.ToShortTimeString();
                previousSchedule.completed = 1;
                db.Entry(previousSchedule).State = EntityState.Modified;
                db.SaveChanges();
            }
                return true; 
            }
        }
        if(schedule.Priority == 1)
            {
                return true;
            }
            return false;
    }
        public ActionResult CheckOut(int? id)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();

            var schedule = db.Schedules.Single(s => s.Id == id);
            schedule.CheckOut = System.DateTime.Now.ToShortTimeString();
            schedule.completed = 1;
            
            DateTime temp = Convert.ToDateTime(schedule.CheckOut);
           DateTime temp1 = Convert.ToDateTime(schedule.CheckIn);
          TimeSpan length = temp.Subtract(temp1);
            schedule.Length = int.Parse(length.TotalMinutes.ToString());
          //schedule.late = span.Minutes;
          //temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
          //schedule.EstimatedCheckout = temp.ToShortTimeString();
            db.Entry(schedule).State = EntityState.Modified;
            db.SaveChanges();
            var updateschedule = db.Schedules.Where(up => up.Priority > schedule.Priority && up.created==Curdate);
            foreach(Schedule up in updateschedule)
            {
                TimeScheduling(up);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Clear(int? id)
        {

            var schedule = db.Schedules.Single(s => s.Id == id);
            schedule.CheckIn = "";
            schedule.CheckOut = "";
            schedule.completed = 0;
            db.Entry(schedule).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Moveup(int? id)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedule = db.Schedules.Single(s => s.Id == id);
            if (schedule.Priority != 1) { 
            var schedulepriority = db.Schedules.Single(sp => sp.Priority == schedule.Priority - 1 && sp.created==Curdate);
            if(schedulepriority.CheckedIn == false || schedulepriority.completed == 0) { 
            if (schedulepriority.Priority == 1)
                {
                    firstCheckin = schedulepriority.CheckIn;
                }
            if (schedule.Priority != 1 & schedulepriority.completed==0) { 
            
            schedule.Priority = schedulepriority.Priority + schedule.Priority;
            schedulepriority.Priority = schedule.Priority - schedulepriority.Priority;
            schedule.Priority = schedule.Priority - schedulepriority.Priority;
            priority = schedule.Priority;
            TimeScheduling(schedule);
            db.Entry(schedule).State = EntityState.Modified;
            db.Entry(schedulepriority).State = EntityState.Modified;
            db.SaveChanges();
            
            }
            TimeScheduling(schedulepriority);
            db.SaveChanges();
            }
            }
            return RedirectToAction("Index");
        }
        public ActionResult MoveDown(int? id)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedule = db.Schedules.Single(s => s.Id == id);
            var maxpriority = db.Schedules.Max(mp => mp.Priority);
            if (schedule.Priority != maxpriority) { 
            var schedulepriority = db.Schedules.Single(sp => sp.Priority == schedule.Priority + 1 && sp.created == Curdate);
            if (schedule.Priority != maxpriority & schedulepriority.completed==0)
            {
               
                schedule.Priority = schedulepriority.Priority + schedule.Priority;
                schedulepriority.Priority = schedule.Priority - schedulepriority.Priority;
                schedule.Priority = schedule.Priority - schedulepriority.Priority;
                    if (schedulepriority.Priority == 1)
                    {
                        firstCheckin = schedulepriority.CheckIn;
                    }
                    if (schedule.CheckIn != null | schedule.CheckOut != null) { 
                TimeScheduling(schedulepriority);
                       
                    }
                    db.Entry(schedule).State = EntityState.Modified;
                db.Entry(schedulepriority).State = EntityState.Modified;
                db.SaveChanges();
            }
                if (schedule.CheckIn != null | schedule.CheckOut != null)
                {
                    TimeScheduling(schedule);
                    TimeScheduling(schedulepriority);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        // GET: Schedules/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            return View(schedule);
        }

        // GET: Schedules/Create
        public ActionResult Create()
        {
            Schedule scheduleNew = new Schedule();
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedules = db.Schedules.Where(sc => sc.created == Curdate);
            List<Rooms> rooms = db.Rooms.OrderBy(rm=>rm.Room_Number).ToList();
            int countHit = 0;
            int id = 0;
            List<Rooms> finalRooms = new List<Rooms>();
             
            foreach (Rooms rm in rooms)
            {
                countHit = 0;
                foreach (Schedule sc  in schedules)
                {
                    
                    if(sc.RoomId == rm.Id)
                    {
                        countHit++;
                    }
                }
                if (countHit == 0)
                {
                    finalRooms.Add(rm);
                   
                }
            }
            ViewBag.RoomId = new SelectList(finalRooms, "Id", "Room_Number");
            
            var count = db.Schedules.Where(c => c.created == Curdate).Count();
            if(count==0)
            {
                priority = 1;
              
            }
            else
            { 
            var schedulepriority = db.Schedules.Where(s=>s.created==Curdate).Max(s=>s.Priority);
                priority = schedulepriority + 1;
                scheduleNew.CheckIn = (db.Schedules.Single(s => s.created == Curdate && s.Priority == schedulepriority).CheckOut);
            }
            
            return View(scheduleNew);
        }

        // POST: Schedules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Length,CheckIn,RoomId")] Schedule schedule)
        {
            
            schedule.created = System.DateTime.Now.ToShortDateString();
            schedule.completed = 0;
            schedule.Priority = priority;
            schedule.CheckedIn = false;
            DateTime temp = Convert.ToDateTime(schedule.CheckIn);
            temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
            schedule.CheckOut = temp.ToShortTimeString();
            //schedule.late = 0;
            //TimeScheduling(schedule);
            priority = 0;
            if (ModelState.IsValid)
            {
                db.Schedules.Add(schedule);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", schedule.RoomId);
            return View(schedule);
        }

        public void TimeScheduling(Schedule schedule)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            if (schedule.completed == 0)
            {
                if (schedule.Priority == 1)
                {
                    schedule.CheckIn = firstCheckin;
                }
                else
                {
                    var tempschedule = db.Schedules.Single(ts => ts.Priority == schedule.Priority - 1 && ts.created==Curdate);
                    //if (tempschedule.CheckOut == null)
                    //{
                    //    schedule.CheckIn = tempschedule.EstimatedCheckout;
                    //}
                    //else
                    //{
                    //    schedule.EstimatedCheckin = tempschedule.CheckOut;
                    //}
                    schedule.CheckIn = tempschedule.CheckOut;
                }
                DateTime temp = Convert.ToDateTime(schedule.CheckIn);
                temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
                schedule.CheckOut = temp.ToShortTimeString();
                //DateTime temp1 = Convert.ToDateTime(schedule.ScheduledCheckout);
                //DateTime temp2 = Convert.ToDateTime(schedule.EstimatedCheckout);
                //TimeSpan span = temp2.Subtract(temp1);
                //schedule.late = span.Minutes;
                //db.SaveChanges();
                priority = 0;
            }
        }

        public void TimeSchedulingIN(Schedule schedule)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            if (schedule.completed == 0)
            {
                if(schedule.CheckedIn ==true) { 
                DateTime temp = Convert.ToDateTime(schedule.CheckIn);
                temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
                schedule.CheckOut = temp.ToShortTimeString();
                }
                
                else
                {
                    var previousschedule = db.Schedules.Single(up => up.Priority == schedule.Priority - 1 & up.created == Curdate);
                    schedule.CheckIn = previousschedule.CheckOut;
                    DateTime temp = Convert.ToDateTime(schedule.CheckIn);
                    temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
                    schedule.CheckOut = temp.ToShortTimeString();
                }
                //db.SaveChanges();
            }
        }

        // GET: Schedules/Edit/5
        public static int priority;
        public static int completed;
        public static string created;
        public static int late;
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            priority = schedule.Priority;
            completed = schedule.completed;
            created = schedule.created;
            
           // late = schedule.late;
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", schedule.RoomId);
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Length,CheckIn,RoomId")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                schedule.Priority = priority;
                schedule.completed = completed;
                schedule.created = created;
                // schedule.late = late;
                DateTime temp = Convert.ToDateTime(schedule.CheckIn);
                temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
                schedule.CheckOut = temp.ToShortTimeString();
                db.Entry(schedule).State = EntityState.Modified;
                db.SaveChanges();
                priority = completed = late = 0;
                created = "";
                return RedirectToAction("Index");
            }
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", schedule.RoomId);
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Schedule schedule = db.Schedules.Find(id);
            db.Schedules.Remove(schedule);
            int intrim = schedule.Priority;
            db.SaveChanges();
            RefreshPriority(intrim);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public void RefreshPriority(int intrim)
        {
            var Maxp = db.Schedules.Max(mp => mp.Priority);
            if(intrim != Maxp)
            {
                var RemainingSchedule = db.Schedules.Where(rs => rs.Priority > intrim);
                foreach(Schedule sc in RemainingSchedule)
                {
                    sc.Priority--; 
                }
            }
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
