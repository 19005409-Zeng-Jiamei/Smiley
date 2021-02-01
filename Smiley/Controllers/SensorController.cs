using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Data;
using System.Security.Claims;
using Smiley.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;


namespace Smiley.Controllers
{
    public class SensorController : Controller
    {
        [Authorize(Roles = "owner")]
        public IActionResult MySensors()
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            string select = "SELECT * FROM Sensor WHERE smiley_user_id = '{0}'";
            List<Sensor> list = DBUtl.GetList<Sensor>(select, userid);
            return View("MySensors", list);
        }

        [Authorize(Roles = "admin")]
        public IActionResult AllSensors()
        {

            string select = "SELECT * FROM Sensor";
            List<Sensor> list = DBUtl.GetList<Sensor>(select);
            return View("MySensors", list);
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult UpdateStatus(int id)
        {
            List<Sensor> onOffFacil = new List<Sensor>();
            string userRole = User.FindFirst(ClaimTypes.Role).Value;

            if (userRole.Equals("owner"))
            {
                string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                string sql = @"SELECT * FROM Sensor WHERE sensor_id={0} AND smiley_user_id='{1}'";
                onOffFacil = DBUtl.GetList<Sensor>(sql, id, userid);
            }
            else
            {
                string sql = @"SELECT * FROM Sensor WHERE sensor_id={0}";
                onOffFacil = DBUtl.GetList<Sensor>(sql, id);
            }


            if (onOffFacil.Count != 1)
            {
                TempData["Message"] = "Sensor Record does not exist";
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

                int res = DBUtl.ExecSQL("UPDATE Sensor SET sensor_status = '{1}' WHERE sensor_id ={0}", id, newStatus);
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
            if (userRole.Equals("admin"))
                return RedirectToAction("AllSensors");
            else
                return RedirectToAction("MySensors");
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult NewSensor()
        {
            List<SelectListItem> locaList = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT location_id As Value, location_name As Text From Exact_Location ORDER BY location_id");
            ViewData["LocationList"] = locaList;

            List<SelectListItem> userList = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT smiley_user_id As Value, smiley_user_id As Text From SmileyUser ORDER BY location_id");
            ViewData["UserList"] = userList;

            return View();
        }

        [HttpPost]
        public IActionResult NewSensor(Sensor sensor)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("NewSensor");
            }
            else
            {
                string insert =
                   @"INSERT INTO Sensor(start_time, end_time, sensor_status, smiley_user_id, location_id) VALUES ('{0}', '{1}', '{2}', '{3}', {4}, '{5}')";

                string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (User.FindFirst(ClaimTypes.Role).Value.Equals("admin"))
                    userId = sensor.smiley_user_id;

                if (DBUtl.ExecSQL(insert, sensor.start_time, sensor.end_time, 1, userId, sensor.location_id) == 1)
                {
                    string select = "SELECT * FROM SmileyUser WHERE smiley_user_id = '{0}'";
                    DataTable ds = DBUtl.GetTable(select, userId);

                    String ownerEmail = ds.Rows[0]["email"].ToString();

                    string template = @"Hey {0},<br/><br/>
                               Now you are the owner of a new Smiley Sensor!
                               The following is the details of the sensor:<br/>Location Name: {1}<br/>Address: {2}, {3} <br/>Building Name: {4} <br/>Operation hours: {5} - {6}
                               <br/><br/>Cheers!<br/>Smiley :)";
                    string title = "Sensor Creation Successful";

                    string getStuff = "SELECT Exact_Location.location_name, Building.building_address, Exact_Location.location_address, Building.building_name FROM Exact_Location INNER JOIN Building ON Exact_Location.building_id = Building.building_id WHERE location_id = '{0}'";

                    DataTable infoRow = DBUtl.GetTable(getStuff, sensor.location_id);
                    string locationName = infoRow.Rows[0]["location_name"].ToString();
                    string buildAdd = infoRow.Rows[0]["building_address"].ToString();
                    string locaAdd = infoRow.Rows[0]["location_address"].ToString();
                    string buildName = infoRow.Rows[0]["building_name"].ToString();

                    string message = String.Format(template, User.FindFirst(ClaimTypes.Name).Value, locationName, buildAdd, locaAdd, buildName, sensor.start_time, sensor.end_time);

                    string result = "";

                    bool outcome = false;

                    outcome = EmailUtl.SendEmail(ownerEmail, title, message, out result);
                    if (outcome)
                    {
                        TempData["Message"] = "Sensor Successfully Added.";
                        TempData["MsgType"] = "success";

                    }
                    else
                    {
                        TempData["Message"] = result;
                        TempData["MsgType"] = "warning";
                    }
                    if (User.FindFirst(ClaimTypes.Role).Value.Equals("admin"))
                        return RedirectToAction("AllSensors");
                    else
                        return RedirectToAction("MySensors");
                }
                else
                {
                    ViewData["Message"] = DBUtl.DB_Message;
                    ViewData["MsgType"] = "danger";
                    return View("NewSensor");
                }
            }
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Delete(int id)
        {
            DataTable ds = new DataTable();

            if (User.IsInRole("admin"))
            {
                string sql = @"SELECT * FROM Sensor
                         WHERE sensor_id={0}";
                ds = DBUtl.GetTable(sql, id);
            }
            else
            {
                string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                string sql = @"SELECT * FROM Sensor
                         WHERE sensor_id={0} AND smiley_user_id='{1}'";
                ds = DBUtl.GetTable(sql, id, userid);

            }

            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Sensor Record does not exist";
                TempData["MsgType"] = "warning";
            }
            else
            {

                int res = DBUtl.ExecSQL("DELETE FROM Sensor WHERE sensor_id ={0}", id);
                if (res == 1)
                {
                    TempData["Message"] = "Sensor Record Deleted";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
            }
            if (User.IsInRole("admin"))
                return RedirectToAction("AllSensors");
            else
                return RedirectToAction("MySensors");
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Update(int id)
        {
            List<Sensor> editedSensor = new List<Sensor>();

            if (User.IsInRole("admin"))
            {
                string sql = @"SELECT * FROM Sensor
                         WHERE sensor_id={0}";
                editedSensor = DBUtl.GetList<Sensor>(sql, id);
            }
            else
            {
                string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                string sql = @"SELECT * FROM Sensor
                         WHERE sensor_id={0} AND smiley_user_id='{1}'";
                editedSensor = DBUtl.GetList<Sensor>(sql, id, userid);

            }

            if (editedSensor.Count != 1)
            {
                TempData["Message"] = "Sensor Record does not exist";
                TempData["MsgType"] = "warning";
                if (User.IsInRole("admin"))
                    return RedirectToAction("AllSensors");
                else
                    return RedirectToAction("MySensors");
            }
            else
            {
                Sensor sense = editedSensor[0];
                return View(sense);
            }
        }

        public IActionResult Update(Sensor sensor)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "danger";
                return View("Update", sensor);
            }
            else
            {
                string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (User.IsInRole("admin"))
                    userid = sensor.smiley_user_id;

                string sql = @"UPDATE Sensor  
                              SET start_time='{2}', end_time='{2}', location_id='{3}' WHERE sensor_id={0} AND smiley_user_id='{1}'";

                if (DBUtl.ExecSQL(sql, sensor.sensor_id, userid, sensor.start_time, sensor.end_time, sensor.location_id) == 1)
                {
                    TempData["Message"] = "Sensor Updated";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                if (User.IsInRole("admin"))
                    return RedirectToAction("AllSensors");
                else
                    return RedirectToAction("MySensors");
            }

        }
    }
}
