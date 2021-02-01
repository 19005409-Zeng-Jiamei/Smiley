using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Door
    {
        public int door_record_id { get; set; }

        public string door_gesture { get; set; }

        public DateTime time_stamp { get; set; }

        public int sensor_id { get; set; }

    }
}
