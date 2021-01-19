using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Smiley.Models;


namespace Smiley.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RetrievePrediction()
        {
            string sql = String.Format("SELECT * FROM Prediction WHERE ORDER BY prediction_id");

            DataTable dt = DBUtl.GetTable(sql);

            String xxx = DBUtl.DB_Message;

            return View(dt.Rows);
        }

        public IActionResult RetrieveFacility()
        {
            string sql = String.Format("SELECT * FROM Facility ORDER BY sensor_id");

            DataTable dt = DBUtl.GetTable(sql);

            String xxx = DBUtl.DB_Message;

            return View(dt.Rows);
        }

        public IActionResult RetrieveUser()
        {
            string sql = String.Format("SELECT * FROM SmileyUser ORDER BY smiley_user_id");

            DataTable dt = DBUtl.GetTable(sql);

            String xxx = DBUtl.DB_Message;

            return View(dt.Rows);
        }

        public IActionResult Performance()
        {
            

            return View();  
        }


    }
}
