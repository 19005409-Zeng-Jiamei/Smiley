﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class SmileyUser
    {
        [Required(ErrorMessage = "Please enter User ID")]
        [StringLength(10, ErrorMessage ="Please make sure that User ID is 10 characters or less")]
        [Remote(action: "VerifyUserID", controller: "Account", ErrorMessage ="User ID already in use")]
        public string smiley_user_id { get; set; }

        [Required(ErrorMessage = "Please enter Password")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Password must be 5 characters or more")]
        public string smiley_user_pw { get; set; }

        [Compare("smiley_user_pw", ErrorMessage = "Passwords do not match")]
        public string smiley_user_pw2 { get; set; }

        [Required(ErrorMessage = "Please enter Full Name")]
        public string full_name { get; set; }

        [Required(ErrorMessage = "Please enter Email")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        [Remote(action: "VerifyUserEmail", controller: "Account", ErrorMessage = "Email already in use")]
        public string email { get; set; }

        [Required(ErrorMessage = "Please select User Role")]
        public string smiley_user_role { get; set; }

        public DateTime last_login { get; set; }


    }
}