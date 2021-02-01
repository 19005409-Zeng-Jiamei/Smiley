using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Emotion
    {
        public int emotion_record_id { get; set; }

        public string emotion_type { get; set; }

        public DateTime time_stamp { get; set; }

        public int sensor_id { get; set; }

    }
}
