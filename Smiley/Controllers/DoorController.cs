using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Smiley.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering; //for SelectListItem

namespace Smiley.Controllers
{
    public class DoorController : Controller
    {

        [Authorize(Roles = "owner, admin")]
        public IActionResult ViewDoors()
        {
            DataTable dt = DBUtl.GetTable("SELECT * FROM Door");

            return View("ViewDoors", dt.Rows);

        }

    }
}