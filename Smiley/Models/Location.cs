using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Location
    {
        public int location_id { get; set; }

        [Required(ErrorMessage = "Please enter Location Name")]
        [StringLength(50, ErrorMessage = "Please enter no more than 50 characters")]
        public string location_name { get; set; }

        [Required(ErrorMessage = "Please enter Location Type")]
        [StringLength(50, ErrorMessage = "Please enter no more than 50 characters")]
        public string location_type { get; set; }

        [Required(ErrorMessage = "Please enter Location Address")]
        [StringLength(50, ErrorMessage = "Please enter no more than 50 characters")]
        public string location_address { get; set; }

        [Required(ErrorMessage = "Please enter Building ID")]
        public int building_id { get; set; }

    }
}
