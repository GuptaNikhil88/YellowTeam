using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Rooms
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Room#")]
        public string Room_Number { get; set; }

        public virtual List<Occupancy> occupancy { get; set; }
        public virtual List<Schedule> schedule { get; set; }
    }
}