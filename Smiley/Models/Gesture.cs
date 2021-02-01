using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Gesture
    {
        public int feedback_id { get; set; }

        public string feedback_gesture { get; set; }

        public DateTime time_stamp { get; set; }

        public int sensor_id { get; set; }

    }
}
