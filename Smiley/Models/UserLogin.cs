using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Smiley.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "Please enter User ID")]
        public string smiley_user_id { get; set; }

        [Required(ErrorMessage = "Please enter Password")]
        [DataType(DataType.Password)]
        public string smiley_user_pw { get; set; }

    }
}


