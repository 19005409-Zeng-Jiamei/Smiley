using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Building
    {
        public int building_id { get; set; }

        [Required(ErrorMessage = "Please enter Building Name")]
        [StringLength(50, ErrorMessage = "Please enter no more than 50 characters")]
        public string building_name { get; set; }

        [Required(ErrorMessage = "Please enter Building Type")]
        [StringLength(50, ErrorMessage = "Please enter no more than 50 characters")]
        public string building_type { get; set; }

        [Required(ErrorMessage = "Please enter Building Address")]
        [StringLength(200, ErrorMessage = "Please enter no more than 200 characters")]
        public string building_address { get; set; }

        [Required(ErrorMessage = "Please enter Building Postal Code")]
        [StringLength(6, MinimumLength =6, ErrorMessage = "A valid Postal Code")]
        public int building_postal_code { get; set; }

    }
}
