using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Prediction
    {
        public int prediction_id { get; set;}

        public string result { get; set;}

        public DateTime time_stamp { get; set; }

        public int sensor_id { get; set; }
    }
}
