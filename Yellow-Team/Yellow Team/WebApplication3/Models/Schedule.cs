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
        [CustomValidation(typeof(Schedule), "durationCheck")]
        [Display(Name = "Duration")]
        [Required(AllowEmptyStrings =false, ErrorMessage =("Please enter the appointment duration!"))]
        public int Length { get; set; }
        [Display(Name = "In")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString ="{0:HH:mm}")]
        [Required]
        public string CheckIn { get; set; }
        [Display(Name = "Out")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        public string CheckOut { get; set; }
        public int Priority { get; set; }
        public string created { get; set; }
        public int completed { get; set; }
        public bool CheckedIn { get; set; }
        //public bool finalized { get; set; }
        //public int late { get; set; }
        public virtual int RoomId { get; set; }
        public virtual Rooms room { get; set; }

        public static ValidationResult durationCheck(string length, ValidationContext context)
        {

            if (Convert.ToInt32(length) < 1)
            {
                return new ValidationResult("Duration should be greater than 0.");
            }
            else
                return ValidationResult.Success;
        }

    }
}