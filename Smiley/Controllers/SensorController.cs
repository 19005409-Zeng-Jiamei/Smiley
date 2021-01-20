using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
        [AllowAnonymous]
        public IActionResult Index()
        {
            List<Facility> list = DBUtl.GetList<Facility>("SELECT * FROM Facility");
            return RedirectToAction("MySensors", list);
        }

        [Authorize(Roles = "owner")]
        public IActionResult MySensors()
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            string select = "SELECT * FROM Facility WHERE smiley_user_id = '{0}'";
            List<Facility> list = DBUtl.GetList<Facility>(select, userid);
            return View("MySensors", list);
        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }

        [Authorize(Roles = "owner")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "owner")]
        [HttpPost]
        public IActionResult Create(Facility sensor)
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
                   @"INSERT INTO FACILITY(facility_type, location_name, address, operation_hour, status, smiley_user_id) VALUES
('{0}', '{1}', '{2}', '{3}', {4}, '{5}')";
                if (DBUtl.ExecSQL(insert, sensor.facility_type, sensor.location_name, sensor.address, sensor.operation_hour, 1, sensor.smiley_user_id) == 1)
                {
                    string select = "SELECT * FROM SmileyUser WHERE smiley_user_id = '{0}'";
                    DataTable ds = DBUtl.GetTable(select, sensor.smiley_user_id);
                    String ownerEmail = ds.Rows[0]["email"].ToString();

                    string template = @"Hey {0},<br/><br/>
                               Now you are the owner of a new Smiley Sensor!
                               The following is the details of the sensor:<br/>Facility type: {1}<br/>Location Name: {2}<br/>Address: {3}<br/>Operation hours: {4}
                               <br/><br/>Cheers!<br/>Smiley :)";
                    string title = "Sensor Creation Successful";
                    string message = String.Format(template, User.FindFirst(ClaimTypes.Name).Value, sensor.facility_type, sensor.location_name, sensor.address, sensor.operation_hour);
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

        [Authorize(Roles = "owner")]
        public IActionResult Update(int id)
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            string sql = @"SELECT * FROM Facility 
                         WHERE sensor_id={0} AND smiley_user_id='{1}'";

            List<Facility> editedFacil = DBUtl.GetList<Facility>(sql, id, userid);
            if (editedFacil.Count == 1)
            {
                Facility facility = editedFacil[0];
                return View(facility);
            }
            else
            {
                TempData["Message"] = "Sensor Record does not exist";
                TempData["MsgType"] = "warning";
                return RedirectToAction("MySensors");
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Update(Facility facility)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "danger";
                return View("Update", facility);
            }
            else
            {
                string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                string sql = @"UPDATE Facility  
                              SET facility_type='{2}', location_name='{3}', address='{4}', operation_hour='{5}', 
                            WHERE sensor_id={0} AND smiley_user_id='{1}'";
                
                if (DBUtl.ExecSQL(sql, facility.sensor_id, userid, facility.facility_type, facility.location_name, facility.address, facility.operation_hour) == 1)
                {
                    TempData["Message"] = "Sensor Updated";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("MySensors");
            }
        }

        [Authorize(Roles = "owner")]
        public IActionResult Delete(int id)
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            string sql = @"SELECT * FROM Facility
                         WHERE sensor_id={0} AND smiley_user_id='{1}'";

            DataTable ds = DBUtl.GetTable(sql, id, userid);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Sensor Record does not exist";
                TempData["MsgType"] = "warning";
            }
            else
            {
                
                int res = DBUtl.ExecSQL("DELETE FROM Facility WHERE sensor_id ={0}", id);
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
            return RedirectToAction("MySensors");
        }

        [Authorize(Roles = "owner")]
        public IActionResult UpdateStatus(int id)
        {
            string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            string sql = @"SELECT * FROM Facility WHERE sensor_id={0} AND smiley_user_id='{1}'";

            List<Facility> onOffFacil = DBUtl.GetList<Facility>(sql, id, userid);
            if (onOffFacil.Count != 1)
            {
                TempData["Message"] = "Sensor Record does not exist";
                TempData["MsgType"] = "warning";
            }
            else
            {
                Facility facility = onOffFacil[0];
                int newStatus = 1;
                String msgOutput = "on";
                if (facility.status)
                {
                    newStatus = 0;
                    msgOutput = "off";
                }

                int res = DBUtl.ExecSQL("UPDATE Facility SET status = '{2}', WHERE sensor_id ={0} AND smiley_user_id = '{1}'", id, userid, newStatus);
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
            return RedirectToAction("MySensors");
        }


    }
}
