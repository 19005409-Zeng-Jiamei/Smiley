using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class Customer
    {
        public int customer_id { get; set; }

        [Required(ErrorMessage = "Please enter Name")]
        [StringLength(50, ErrorMessage = "Max 50 characters")]
        public string customer_name { get; set; }

        [Required(ErrorMessage = "Please enter Surname")]
        [StringLength(50, ErrorMessage = "Max 50 characters")]
        public string surname { get; set; }

        [Required(ErrorMessage = "Please enter Email")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        [Remote(action: "VerifyUserEmail", controller: "Customer", ErrorMessage = "Email already in use")]
        public string email { get; set; }

        [Required(ErrorMessage = "Please enter membership")]
        public string membership { get; set; }

        [Required(ErrorMessage = "Please take a Picture")]
        public string picfile { get; set; }

        public DateTime signup_date { get; set; }

        public int face_id { get; set; }

    }
}
