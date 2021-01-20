using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Facility
    {
        public int sensor_id { get; set; }

        [Required(ErrorMessage = "Please enter Facility Type")]
        [StringLength(50, ErrorMessage ="Please enter no more than 50 characters")]
        public string facility_type { get; set; }

        [Required(ErrorMessage = "Please enter Location Name")]
        [StringLength(50, ErrorMessage = "Please enter no more than 50 characters")]
        public string location_name { get; set; }

        [Required(ErrorMessage = "Please enter address")]
        public string address { get; set; }

        public string operation_hour { get; set; }

        public bool status { get; set; }

        [Required(ErrorMessage = "Please enter Owner User ID")]
        public string smiley_user_id { get; set; }

    }
}


