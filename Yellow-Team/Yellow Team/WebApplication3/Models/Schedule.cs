using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        [Display(Name ="Duration")]
        public int Length { get; set; }
        //[Display(Name ="Parent Present")]
        //public bool Parent { get; set; }
        //public string ScheduledCheckIn { get; set; }
        //public string ScheduledCheckout { get; set; }
        //public string EstimatedCheckin { get; set; }
        //public string EstimatedCheckout { get; set; }
        [Display(Name = "In")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString ="{0:hh:mm tt}")]
        public string CheckIn { get; set; }
        [Display(Name = "Out")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public string CheckOut { get; set; }
        public int Priority { get; set; }
        public string created { get; set; }
        public int completed { get; set; }
        public bool CheckedIn { get; set; }
        //public bool finalized { get; set; }
        //public int late { get; set; }
        public virtual int RoomId { get; set; }
        public virtual Rooms room { get; set; }

    }
}