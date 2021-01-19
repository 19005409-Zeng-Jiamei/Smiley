using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Facility
    {
        public int sensor_id { get; set; }

        public string facility_type { get; set; }

        public string location_name { get; set; }

        public string address { get; set; }

        public string operation_hour { get; set; }

        public bool status { get; set; }

        public string smiley_user_id { get; set; }

    }
}


