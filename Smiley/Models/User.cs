using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class User
    {
        [Required(ErrorMessage = "Please enter User ID")]
        [StringLength(10, ErrorMessage = "Please make sure that User ID is 10 characters or less")]
        [Remote(action: "UniqueUserID", controller:"Account")]
        public string smiley_user_id { get; set; }

        [Required(ErrorMessage = "Please enter Password")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Password must be 8 characters or more")]
        public string smiley_user_pw { get; set; }

        [Compare("smiley_user_pw", ErrorMessage = "Passwords do not match")]
        public string smiley_user_pw2 { get; set; }

        [Required(ErrorMessage = "Please enter Full Name")]
        public string full_name { get; set; }

        public string smiley_user_picfile { get; set; }

        [Required(ErrorMessage = "Please enter Email")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string email { get; set; }

        [Required(ErrorMessage = "Please select User Role")]
        public string smiley_user_role { get; set; }

        public DateTime last_login { get; set; }

        public int face_id { get; set; }

        public string superior_id { get; set; }
    }
}
