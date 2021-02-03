using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Smiley.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Smiley.Controllers
{
    public class SensorController : Controller
    {
        [Authorize(Roles = "owner, admin")]
        public IActionResult ViewSensors()
        {
            DataTable dt = DBUtl.GetTable("SELECT * FROM Sensors");
            return View(dt.Rows);

        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Create()
        {
            List<SelectListItem> localist = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT
                location_id as Value,
                location_name as Text
                FROM Exact_Location 
                ORDER BY location_name"
                );
            ViewData["LocationList"] = localist;

            return View();
        }

        [Authorize(Roles = "owner, admin")]
        [HttpPost]
        public IActionResult Create(Sensor sense)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("Create");
            }
            else
            {
                string insert =
                   @"INSERT INTO Sensor(start_time, end_time, sensor_status, smiley_user_id, location_id) VALUES
                   ('{0: HH:mm}', '{1: HH:mm}', 1, '{2}', '{3}')";

                int res = DBUtl.ExecSQL(insert, sense.start_time, sense.end_time, sense.smiley_user_id, sense.location_id);
                if (res == 1)
                {
                    TempData["Message"] = "Sensor Created";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("ViewSensors");
            }
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Update(int id)
        {
            List<SelectListItem> localist = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT
                location_id as Value,
                location_name as Text
                FROM Exact_Location 
                ORDER BY location_name"
                );
            ViewData["LocationList"] = localist;

            List<SelectListItem> UserList = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT
                smiley_user_id as Value,
                full_name as Text
                FROM SmileyUser 
                WHERE smiley_user_role = 'owner'
                ORDER BY full_name"
                );
            ViewData["OwnerList"] = UserList;


            string select = "SELECT * FROM Sensor WHERE sensor_id ='{0}'";
            List<Sensor> list = DBUtl.GetList<Sensor>(select, id);
            if (list.Count == 1)
            {
                return View(list[0]);
            }
            else
            {
                TempData["Message"] = "Sensor record does not exist";
                TempData["MsgType"] = "warning";
                return RedirectToAction("ViewSensors");
            }
        }

        [Authorize(Roles = "owner, admin")]
        [HttpPost]
        public IActionResult Update(Sensor sense)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("Update");
            }
            else
            {
                string update =
                   @"UPDATE Sensor
                    SET start_time='{1: HH:mm}', end_time='{2: HH:mm}', smiley_user_id='{3}', location_id={4} WHERE sensor_id='{0}'";
                int res = DBUtl.ExecSQL(update, sense.sensor_id, sense.start_time, sense.end_time, sense.smiley_user_id, sense.location_id);
                if (res == 1)
                {
                    TempData["Message"] = "Sensor Updated";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("ViewSensors");
            }
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Delete(int id)
        {
            string select = @"SELECT * FROM Sensor WHERE sensor_id={0}";
            DataTable ds = DBUtl.GetTable(select, id);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Sensor does not exist";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string delete = "DELETE FROM Sensor WHERE sensor_id={0}";
                int res = DBUtl.ExecSQL(delete, id);
                if (res == 1)
                {
                    TempData["Message"] = "Sensor Deleted";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
            }
            return RedirectToAction("ViewSensors");
        }

    }
}