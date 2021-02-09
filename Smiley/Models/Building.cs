using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Smiley.Models
{
    public class Building
    {
        public int building_id { get; set; }

        [Required(ErrorMessage = "Please enter Building Name")]
        [StringLength(50, ErrorMessage = "Please enter no more than 50 characters")]
        [Remote(action: "VerifyBuildingName", controller: "Building", ErrorMessage = "Building already exist")]
        public string building_name { get; set; }

        [Required(ErrorMessage = "Please enter Building Type")]
        [StringLength(50, ErrorMessage = "Please enter no more than 50 characters")]
        public string building_type { get; set; }

        [Required(ErrorMessage = "Please enter Building Address")]
        [StringLength(200, ErrorMessage = "Please enter no more than 200 characters")]
        [Remote(action: "VerifyBuildingAddress", controller: "Building", ErrorMessage = "Building Address already exist")]
        public string building_address { get; set; }

        [Required(ErrorMessage = "Please enter Building Postal Code")]
        [RegularExpression("[0-9]{6}", ErrorMessage = "A invalid Postal Code")]
        public int building_postal_code { get; set; }

    }
}
