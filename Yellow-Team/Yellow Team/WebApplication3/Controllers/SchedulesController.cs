//Author﻿: Akash Jain (akashjain1205@gmail.com, jain2ar@mail.uc.edu, github: akash1205)
//        Nikhil Gupta (nikhil.damoh@gmail.com, guptan6@mail.uc.edu, github: guptanikhil88)
//      This controller deals with the creating, deletion, updation and manipulation of the schedules.
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
        public static int priority;
        // GET: Schedules
        //This Method is being used to load the schedules of the previous day if no schedules has been created by today.
        public ActionResult Index(int? successFlag)
        {
            DateTime d1 = System.DateTime.Now;
            d1 = d1.AddDays(-1);
            string previousday = d1.ToShortDateString();
            var Curdate = System.DateTime.Now.ToShortDateString();
            var countOfSchedules = db.Schedules.Where(c => c.created == Curdate).Count();
            if (countOfSchedules == 0)
            {
                //Call to copy the schedules of previous day. 
                LoadSchedule(previousday);
            }
            var schedules = db.Schedules.Where(s => s.created == Curdate).OrderBy(s => s.Priority);
            if (successFlag == 0)
            {
                ViewBag.message = "The previous schedule is not checked in yet!";
            }
            return View(schedules.ToList());
        }
        
        //This method gets called in the Index() Method above to load the schedules of the previous day. The created date, checkedIn
        // and checkedout status, completed flag will be refreshed.
        public void LoadSchedule(string pd)
        {
            string previousday = pd;

            var scheduleid = db.Schedules.Max(sid => sid.Id);
            scheduleid = ++scheduleid;
            var previousschedule = db.Schedules.Where(ps => ps.created == previousday).OrderBy(ps => ps.Priority);

            foreach (Schedule ps in previousschedule)
            {
                var NewSchedule = new Schedule();
                NewSchedule.Id = scheduleid;
                NewSchedule.Length = ps.Length;
                NewSchedule.Priority = ps.Priority;
                NewSchedule.room = ps.room;
                NewSchedule.RoomId = ps.RoomId;
                NewSchedule.CheckIn = ps.CheckIn;
                NewSchedule.CheckOut = ps.CheckOut;
                NewSchedule.CheckedIn = false;
                NewSchedule.completed = 0;
                NewSchedule.created = System.DateTime.Now.ToShortDateString();

                scheduleid = ++scheduleid;
                db.Schedules.Add(NewSchedule);

            }
            db.SaveChanges();
        }

        //This method is used to checkin the schedules. It updated the checkin and checkout time of the schedule and also
        //cascades the change to the remaning schedules.
        public ActionResult CheckIn(int? id)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedule = db.Schedules.Single(s => s.Id == id);
            var success = 0;
            //This condition validates whether previous schedule has been checked out or not.
            if (validatePreviousCheckin(schedule))
            {
                success = 1;
                schedule.CheckIn = System.DateTime.Now.ToShortTimeString();
                schedule.CheckedIn = true;
                db.Entry(schedule).State = EntityState.Modified;
                db.SaveChanges();
                var updateschedule = db.Schedules.Where(up => up.Priority >= schedule.Priority && up.created == Curdate)
                                              .OrderBy(up => up.Priority);
                foreach (Schedule up in updateschedule)
                {
                    //This call cascades the effect of checkIn to update the checkout and checkin time of the remaining schedules.
                    TimeSchedulingIN(up, false);
                }
                db.SaveChanges();


            }
            else
            {
                success = 0;
            }
            return RedirectToAction("Index", new { successFlag = success });
        }

        //This method return True if the previous schedule has been checked out. If it is not checked out but has been checked in,
        //it checks out the previous schedule and returns true. If the previous schedule is not checked in, it returns false.
        public Boolean validatePreviousCheckin(Schedule schedule)
        {
            var curDate = System.DateTime.Now.ToShortDateString();
            if (schedule.Priority > 1)
            {
                var previousSchedule = db.Schedules.Single(ps => ps.Priority == schedule.Priority - 1 & ps.created == curDate);
                if (previousSchedule.CheckedIn == true)
                {
                    if (previousSchedule.completed == 0)
                    {
                        previousSchedule.CheckOut = System.DateTime.Now.ToShortTimeString();
                        previousSchedule.Length = int.Parse((Convert.ToDateTime(previousSchedule.CheckOut).Subtract(Convert.ToDateTime(previousSchedule.CheckIn))).TotalMinutes.ToString());
                        previousSchedule.completed = 1;
                        db.Entry(previousSchedule).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            else if (schedule.Priority == 1)
            {
                return true;
            }
            return false;
        }

        //This method is used to checkin the schedules. It updated the checkin and checkout time of the schedule and also
        //cascades the change to the remaning schedules.
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
            db.Entry(schedule).State = EntityState.Modified;
            db.SaveChanges();
            var updateschedule = db.Schedules.Where(up => up.Priority > schedule.Priority && up.created == Curdate);
            foreach (Schedule up in updateschedule)
            {
                TimeScheduling(up);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //This method clears out the details of the schedules.
        //Only admin can access this method.
        [Authorize(Roles = "Admin")]
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

        //This method change the priority of the schedule by moving it up the ladder and updates the remaming schedules accordingly.
        //Only Admin can assess this method.
        [Authorize(Roles = "Admin")]
        public ActionResult Moveup(int? id)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedule = db.Schedules.Single(s => s.Id == id);
            if (schedule.Priority != 1)
            {
                var schedulepriority = db.Schedules.Single(sp => sp.Priority == schedule.Priority - 1 && sp.created == Curdate);
                if (schedulepriority.CheckedIn == false)
                {
                    if (schedulepriority.Priority == 1)
                    {
                        firstCheckin = schedulepriority.CheckIn;
                    }
                    if (schedule.Priority != 1 & schedulepriority.completed == 0)
                    {

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

        //This method change the priority of the schedule by moving it down the ladder and updates the remaming schedules accordingly.
        //Only Admin can assess this method.
        [Authorize(Roles = "Admin")]
        public ActionResult MoveDown(int? id)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedule = db.Schedules.Single(s => s.Id == id);
            var todaysSchedules = db.Schedules.Where(s => s.created == Curdate);
            var maxpriority = todaysSchedules.Max(mp => mp.Priority);
            if (schedule.Priority != maxpriority)
            {
                var schedulepriority = db.Schedules.Single(sp => sp.Priority == schedule.Priority + 1 && sp.created == Curdate);
                if (schedule.Priority != maxpriority & schedulepriority.completed == 0)
                {

                    schedule.Priority = schedulepriority.Priority + schedule.Priority;
                    schedulepriority.Priority = schedule.Priority - schedulepriority.Priority;
                    schedule.Priority = schedule.Priority - schedulepriority.Priority;
                    if (schedulepriority.Priority == 1)
                    {
                        firstCheckin = schedule.CheckIn;
                    }
                    if (schedule.CheckIn != null | schedule.CheckOut != null)
                    {
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
        //This method is used to see the details of the schedule.
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
        public static string maxCheckout;
        
        //This method initializes a new schedule. If it is the first schedule, the default time will be 9:00 AM, which is editable.
        //If the schedule is not the first schedule, it takes the checkout time of the previous schedule.
        // GET: Schedules/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            Schedule scheduleNew = new Schedule();
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedules = db.Schedules.Where(sc => sc.created == Curdate);
            List<Rooms> rooms = db.Rooms.OrderBy(rm => rm.Room_Number).ToList();
            int countHit = 0;
            
            List<Rooms> finalRooms = new List<Rooms>();

            foreach (Rooms rm in rooms)
            {
                countHit = 0;
                foreach (Schedule sc in schedules)
                {

                    if (sc.RoomId == rm.Id)
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
            if (count == 0)
            {
                priority = 1;
                scheduleNew.CheckIn = "09:00";

            }
            else
            {
                var schedulepriority = db.Schedules.Where(s => s.created == Curdate).Max(s => s.Priority);
                priority = schedulepriority + 1;
                maxCheckout = db.Schedules.Single(mc => mc.created == Curdate && mc.Priority == schedulepriority).CheckOut;
                scheduleNew.CheckIn = (db.Schedules.Single(s => s.created == Curdate && s.Priority == schedulepriority).CheckOut);
            }

            return View(scheduleNew);
        }

        // POST: Schedules/Create
        //This method saves the details of the schedule in the database.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Length,CheckIn,RoomId")] Schedule schedule)
        {

            schedule.created = System.DateTime.Now.ToShortDateString();
            schedule.completed = 0;
            schedule.Priority = priority;
            schedule.CheckedIn = false;
            DateTime temp = Convert.ToDateTime(schedule.CheckIn);
            temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
            schedule.CheckOut = temp.ToString("HH:mm");
          
            var allSchedules = db.Schedules.Where(all => all.created == schedule.created).OrderBy(all => all.Priority);
            if (ModelState.IsValid)
            {
                if (invalidCheckInAndCheckout(schedule, allSchedules))
                {
                    ModelState.AddModelError("CheckIn", "Time Period is overlapping with other schedules, adjust the Length or CheckIn of your Schedule.");
                }
                if (ModelState.IsValid)
                {
                    
                    db.Schedules.Add(schedule);
                    db.SaveChanges();
                    sortEditedItems(schedule, allSchedules);
                    maxCheckout = null;
                    priority = 0;
                    return RedirectToAction("Index");
                }
            }
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", schedule.RoomId);
            return View(schedule);
        }

        //This method gets called in checkout to support the cascading effect of checkout on the remaining schedules.
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
                    var tempschedule = db.Schedules.Single(ts => ts.Priority == schedule.Priority - 1 && ts.created == Curdate);
                    schedule.CheckIn = tempschedule.CheckOut;
                }
                DateTime temp = Convert.ToDateTime(schedule.CheckIn);
                temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
                schedule.CheckOut = temp.ToString("HH:mm");
                priority = 0;
            }
        }

        //This method gets called in checkout to support the cascading effect of checkout on the remaining schedules.
        public void TimeSchedulingIN(Schedule schedule, Boolean editFlag)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            if (schedule.completed == 0 && editFlag == false)
            {
                if (schedule.CheckedIn == true)
                {
                    DateTime temp = Convert.ToDateTime(schedule.CheckIn);
                    temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
                    schedule.CheckOut = temp.ToString("HH:mm");
                }

                else
                {
                    if (schedule.Priority != 1)
                    {
                        var previousschedule = db.Schedules.Single(up => up.Priority == schedule.Priority - 1 & up.created == Curdate);
                        schedule.CheckIn = previousschedule.CheckOut;
                    }
                    DateTime temp = Convert.ToDateTime(schedule.CheckIn);
                    temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
                    schedule.CheckOut = temp.ToString("HH:mm");
                }
                //db.SaveChanges();
            }
            if (editFlag == true)
            {
                schedule.CheckIn = firstCheckin;
                DateTime temp = Convert.ToDateTime(schedule.CheckIn);
                temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
                schedule.CheckOut = temp.ToString("HH:mm");
                editFlag = false;
                firstCheckin = schedule.CheckOut;
            }
        }

        // GET: Schedules/Edit/5
        //This method gets the schedule to be edited.
        public static Schedule interim;
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            interim = schedule;
            if (schedule == null)
            {
                return HttpNotFound();
            }
            // late = schedule.late;
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", schedule.RoomId);
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        //This method saves the changes made to the schedule.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Length,CheckIn,RoomId")] Schedule schedule)
        {
            schedule.RoomId = interim.RoomId;
            schedule.Priority = interim.Priority;
            schedule.completed = interim.completed;
            schedule.created = interim.created;
            // schedule.late = late;
            DateTime temp = Convert.ToDateTime(schedule.CheckIn);
            temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
            schedule.CheckOut = temp.ToString("HH:mm");
            var allSchedules = db.Schedules.Where(alls => alls.created == schedule.created).OrderBy(all => all.Priority);

            if (ModelState.IsValid)
            {
                db.Entry(schedule).State = EntityState.Modified;

                if (invalidCheckInAndCheckout(schedule, allSchedules))
                {
                    ModelState.AddModelError("CheckIn", "Time Period is overlapping with other schedules, adjust the Length or CheckIn of your Schedule.");
                }
                if (ModelState.IsValid)
                {
                    sortEditedItems(schedule, allSchedules);
                    db.SaveChanges();
                    var Curdate = System.DateTime.Now.ToShortDateString();
                    var trailingSchedule = db.Schedules.Where(up => up.Priority > schedule.Priority & up.created == Curdate);
                    firstCheckin = schedule.CheckOut;
                    
                    db.SaveChanges();
                    interim = null;
                    return RedirectToAction("Index");
                }
            }
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", schedule.RoomId);

            return View(schedule);

        }

        // GET: Schedules/Delete/5
        [Authorize(Roles = "Admin")]
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

        //This method updates the priorities of the schedules, if any schedule gets deleted.
        public void RefreshPriority(int intrim)
        {
            var Maxp = db.Schedules.Max(mp => mp.Priority);
            if (intrim != Maxp)
            {
                var RemainingSchedule = db.Schedules.Where(rs => rs.Priority > intrim);
                foreach (Schedule sc in RemainingSchedule)
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

        //This method handles the priority of schedules based on the checkin and checkout time.
        public void sortEditedItems(Schedule schedule, IOrderedQueryable<Schedule> allSchedules)
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            if (allSchedules.Count() != 0)
            {
                foreach (Schedule s in allSchedules)
                {
                    if (s.CheckedIn == true)
                    {
                        continue;
                    }
                    else if (s.Id == schedule.Id)
                    {
                        continue;
                    }
                    else
                    {
                        if ((Convert.ToDateTime(s.CheckIn) > Convert.ToDateTime(schedule.CheckIn) & s.Priority < schedule.Priority))
                        {
                            s.Priority = s.Priority + schedule.Priority;
                            schedule.Priority = s.Priority - schedule.Priority;
                            s.Priority = s.Priority - schedule.Priority;
                            schedule = s;
                        }
                        else if (Convert.ToDateTime(s.CheckIn) < Convert.ToDateTime(schedule.CheckIn) & s.Priority > schedule.Priority)
                        {
                            s.Priority = s.Priority + schedule.Priority;
                            schedule.Priority = s.Priority - schedule.Priority;
                            s.Priority = s.Priority - schedule.Priority;


                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                db.Entry(schedule).State = EntityState.Modified;
                db.SaveChanges();

            }
        }

        //This method doesn't allow user to create overlapping schedules.
        public Boolean invalidCheckInAndCheckout(Schedule schedule, IOrderedQueryable<Schedule> allSchedules)
        {
            if (allSchedules.Count() != 0)
            {
                foreach (Schedule s in allSchedules)
                {
                    if (s.Id == schedule.Id)
                    {
                        continue;
                    }
                    else if (Convert.ToDateTime(schedule.CheckIn) >= Convert.ToDateTime(s.CheckIn) & Convert.ToDateTime(schedule.CheckIn) < Convert.ToDateTime(s.CheckOut))
                    {
                        return true;
                    }
                    else if (Convert.ToDateTime(schedule.CheckOut) > Convert.ToDateTime(s.CheckIn) & Convert.ToDateTime(schedule.CheckOut) <= Convert.ToDateTime(s.CheckOut))
                    {
                        return true;
                    }
                    else{
                        if((Convert.ToDateTime(schedule.CheckIn)<Convert.ToDateTime(s.CheckIn))&& s.CheckedIn == true){
                            return true;
                        }
                        else{
                            continue;
                        }
                    }

                }

            }
            return false;
        }
        
    }
}
