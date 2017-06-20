using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Occupancy
    {
        public int Id { get; set; }
        [DisplayFormat(DataFormatString ="{0:yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public virtual int RoomId { get; set; }
        public virtual Rooms room { get; set; }
    }
}