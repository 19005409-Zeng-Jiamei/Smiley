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
    public class LocationController : Controller
    {

        [Authorize(Roles = "owner, admin")]
        public IActionResult ViewLocations()
        {
            DataTable dt = DBUtl.GetTable("SELECT * FROM Exact_Location");
            return View(dt.Rows);

        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Create()
        {
            List<SelectListItem> Buildlist = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT
                Chamber as Value,
                Chamber as Text
                FROM Performance 
                ORDER BY Chamber"
                );
            ViewData["ChamberList"] = Clist;

            return View();
        }

        [Authorize(Roles = "owner, admin")]
        [HttpPost]
        public IActionResult Create(Location loca)
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
                   @"INSERT INTO Exact_Location(location_name, location_type, location_address, building_id) VALUES
('{0}','{1}','{2}', {3})";

                int res = DBUtl.ExecSQL(insert, loca.location_name, loca.location_type, loca.location_address, loca.building_id);
                if (res == 1)
                {
                    TempData["Message"] = "Location Created";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("ViewLocations");
            }
        }

        [Authorize(Roles = "manager")]
        public IActionResult Update(int id)
        {
            List<SelectListItem> Clist = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT
                Chamber as Value,
                Chamber as Text
                FROM Performance 
                ORDER BY Chamber"
                );
            ViewData["ChamberList"] = Clist;

            List<SelectListItem> Dlist = new List<SelectListItem>
            {
                new SelectListItem("0.5","0.5"),new SelectListItem("1","1"),
                new SelectListItem("1.5","1.5"),new SelectListItem("2","2"),
                new SelectListItem("2.5","2.5"),new SelectListItem("3","3"),
                new SelectListItem("3.5","3.5"),new SelectListItem("4","4"),
            };
            ViewData["DurationList"] = Dlist;


            string select = "SELECT * FROM Performance WHERE Pid='{0}'";
            List<Performance> list = DBUtl.GetList<Performance>(select, id);
            if (list.Count == 1)
            {
                return View(list[0]);
            }
            else
            {
                TempData["Message"] = "Performance record does not exist";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "manager")]
        [HttpPost]
        public IActionResult Update(Performance perform)
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
                   @"UPDATE Performance
                    SET Title='{1}', Artist='{2}', PerformDT='{3:yyyy-MM-dd HH:mm}', Duration={4}, Price={5}, Chamber='{6}' WHERE Pid='{0}'";
                int res = DBUtl.ExecSQL(update, perform.Pid, perform.Title, perform.Artist, perform.PerformDT, perform.Duration, perform.Price, perform.Chamber);
                if (res == 1)
                {
                    TempData["Message"] = "Performance Updated";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "manager")]
        public IActionResult Delete(int id)
        {
            string select = @"SELECT * FROM Performance WHERE Pid={0}";
            DataTable ds = DBUtl.GetTable(select, id);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Performance does not exist";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string delete = "DELETE FROM Performance WHERE Pid={0}";
                int res = DBUtl.ExecSQL(delete, id);
                if (res == 1)
                {
                    TempData["Message"] = "Performance Deleted";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
            }
            return RedirectToAction("Index");
        }

    }
}