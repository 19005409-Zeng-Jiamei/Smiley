using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Sensor
    {
        public int sensor_id { get; set; }

        public DateTime start_time { get; set; }

        public DateTime end_time { get; set; }

        [Required(ErrorMessage = "Please enter Status")]
        public byte sensor_status { get; set; }

        [Required(ErrorMessage = "Please enter Owner User ID")]
        public string smiley_user_id { get; set; }

        [Required(ErrorMessage = "Please enter Location ID")]
        public int location_id { get; set; }
    }
}
