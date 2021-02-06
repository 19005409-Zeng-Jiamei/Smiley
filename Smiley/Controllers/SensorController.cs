using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Smiley.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Smiley.Controllers
{
    public class SensorController : Controller
    {
        [Authorize(Roles = "owner, admin")]
        public IActionResult ViewSensors()
        {
            DataTable dt = new DataTable();
            if (User.IsInRole("admin"))
                dt = DBUtl.GetTable("SELECT * FROM Sensor INNER JOIN Exact_Location ON Exact_Location.location_id = Sensor.location_id");
            else
                dt = DBUtl.GetTable("SELECT * FROM Sensor INNER JOIN Exact_Location ON Exact_Location.location_id = Sensor.location_id WHERE smiley_user_id = '{0}'", User.FindFirst(ClaimTypes.NameIdentifier).Value);

            return View(dt.Rows);

        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Create()
        {
            string sql = @"SELECT location_id, location_name FROM Exact_Location";
            List<Location> locaList = DBUtl.GetList<Location>(sql);

            List<SelectListItem> localist = new List<SelectListItem>();
            foreach (var locaItem in locaList)
            {
                localist.Add(new SelectListItem(locaItem.location_name, locaItem.location_id.ToString()));
            }

            ViewData["LocationList"] = localist;

            string smilesql = @"SELECT smiley_user_id, full_name FROM SmileyUser";
            List<User> useList = DBUtl.GetList<User>(smilesql);

            List<SelectListItem> UserList = new List<SelectListItem>();
            foreach (var userItem in useList)
            {
                UserList.Add(new SelectListItem(userItem.full_name, userItem.smiley_user_id.ToString()));
            }

            ViewData["UserList"] = UserList;

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
                string sql = @"SELECT location_id, location_name FROM Exact_Location";
                List<Location> locaList = DBUtl.GetList<Location>(sql);

                List<SelectListItem> localist = new List<SelectListItem>();
                foreach (var locaItem in locaList)
                {
                    localist.Add(new SelectListItem(locaItem.location_name, locaItem.location_id.ToString()));
                }

                ViewData["LocationList"] = localist;
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
            string sql = @"SELECT location_id, location_name FROM Exact_Location";
            List<Location> locaList = DBUtl.GetList<Location>(sql);

            List<SelectListItem> localist = new List<SelectListItem>();
            foreach (var locaItem in locaList)
            {
                localist.Add(new SelectListItem(locaItem.location_name, locaItem.location_id.ToString()));
            }

            ViewData["LocationList"] = localist;

            string smilesql = @"SELECT smiley_user_id, full_name FROM SmileyUser";
            List<User> useList = DBUtl.GetList<User>(smilesql);

            List<SelectListItem> UserList = new List<SelectListItem>();
            foreach (var userItem in useList)
            {
                UserList.Add(new SelectListItem(userItem.full_name, userItem.smiley_user_id.ToString()));
            }

            ViewData["UserList"] = UserList;


            string select = "SELECT * FROM Sensor WHERE sensor_id ={0}";
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
                string sql = @"SELECT location_id, location_name FROM Exact_Location";
                List<Location> locaList = DBUtl.GetList<Location>(sql);

                List<SelectListItem> localist = new List<SelectListItem>();
                foreach (var locaItem in locaList)
                {
                    localist.Add(new SelectListItem(locaItem.location_name, locaItem.location_id.ToString()));
                }

                ViewData["LocationList"] = localist;

                string smilesql = @"SELECT smiley_user_id, full_name FROM SmileyUser";
                List<User> useList = DBUtl.GetList<User>(smilesql);

                List<SelectListItem> UserList = new List<SelectListItem>();
                foreach (var userItem in useList)
                {
                    UserList.Add(new SelectListItem(userItem.full_name, userItem.smiley_user_id.ToString()));
                }

                ViewData["UserList"] = UserList;
                return View("Update");
            }
            else
            {
                string update =
                   @"UPDATE Sensor SET start_time='{1: HH:mm}', end_time='{2: HH:mm}', smiley_user_id='{3}', location_id={4} WHERE sensor_id={0}";
                int res = DBUtl.ExecSQL(update, sense.sensor_id, sense.start_time, sense.end_time, sense.smiley_user_id, sense.location_id);
                if (res == 1)
                {
                    TempData["Message"] = "Sensor Updated";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message + res;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("ViewSensors");
            }
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult UpdateStatus(int id)
        {

            string sql = @"SELECT * FROM Sensor WHERE sensor_id={0}";

            List<Sensor> onOffFacil = DBUtl.GetList<Sensor>(sql, id);
            if (onOffFacil.Count != 1)
            {
                TempData["Message"] = "Sensor Record does not exist" + sql;
                TempData["MsgType"] = "warning";
            }
            else
            {
                Sensor facility = onOffFacil[0];
                int newStatus = 1;
                String msgOutput = "on";

                if (facility.sensor_status == 1)
                {
                    newStatus = 0;
                    msgOutput = "off";
                }

                int res = DBUtl.ExecSQL("UPDATE Sensor SET sensor_status = {1} WHERE sensor_id = {0}", id, newStatus);
                if (res == 1)
                {
                    TempData["Message"] = "Sensor has been turned " + msgOutput;
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