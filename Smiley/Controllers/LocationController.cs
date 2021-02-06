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
    public class LocationController : Controller
    {

        [Authorize(Roles = "owner, admin")]
        public IActionResult ViewLocations()
        {
            DataTable dt = new DataTable();
            if (User.IsInRole("admin"))
                dt = DBUtl.GetTable("SELECT * FROM ((Exact_Location INNER JOIN Sensor ON Sensor.location_id = Exact_Location.location_id) INNER JOIN Building ON Building.building_id = Exact_Location.building_id)");
            else
                dt = DBUtl.GetTable("SELECT * FROM ((Exact_Location INNER JOIN Sensor ON Sensor.location_id = Exact_Location.location_id) INNER JOIN Building ON Building.building_id = Exact_Location.building_id) WHERE smiley_user_id='{0}'", User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return View("ViewLocations", dt.Rows);

        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Create()
        {
            string sql = @"SELECT building_id, building_name FROM Building";
            List<Building> buildList = DBUtl.GetList<Building>(sql);
            List<SelectListItem> BuildList = new List<SelectListItem>();
            foreach (var buildItem in buildList)
            {
                BuildList.Add(new SelectListItem(buildItem.building_name, buildItem.building_id.ToString()));
            }
            ViewData["BuildingList"] = BuildList;

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
                string sql = @"SELECT building_id, building_name FROM Building";
                List<Building> buildList = DBUtl.GetList<Building>(sql);
                List<SelectListItem> BuildList = new List<SelectListItem>();
                foreach (var buildItem in buildList)
                {
                    BuildList.Add(new SelectListItem(buildItem.building_name, buildItem.building_id.ToString()));
                }
                ViewData["BuildingList"] = BuildList;
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

        [Authorize(Roles = "owner, admin")]
        public IActionResult Update(int id)
        {
            string sql = @"SELECT building_id, building_name FROM Building";
            List<Building> buildList = DBUtl.GetList<Building>(sql);
            List<SelectListItem> BuildList = new List<SelectListItem>();
            foreach (var buildItem in buildList)
            {
                BuildList.Add(new SelectListItem(buildItem.building_name, buildItem.building_id.ToString()));
            }
            ViewData["BuildingList"] = BuildList;

            List<SelectListItem> LocaTypelist = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT
                location_type as Value,
                location_type as Text
                FROM Exact_Location 
                ORDER BY location_type"
                );
            ViewData["LocationTypeList"] = LocaTypelist;


            string select = "SELECT * FROM Exact_Location WHERE location_id='{0}'";
            List<Location> list = DBUtl.GetList<Location>(select, id);
            if (list.Count == 1)
            {
                return View(list[0]);
            }
            else
            {
                TempData["Message"] = "Location record does not exist";
                TempData["MsgType"] = "warning";
                return RedirectToAction("ViewLocations");
            }
        }

        [Authorize(Roles = "owner, admin")]
        [HttpPost]
        public IActionResult Update(Location loca)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                string sql = @"SELECT building_id, building_name FROM Building";
                List<Building> buildList = DBUtl.GetList<Building>(sql);
                List<SelectListItem> BuildList = new List<SelectListItem>();
                foreach (var buildItem in buildList)
                {
                    BuildList.Add(new SelectListItem(buildItem.building_name, buildItem.building_id.ToString()));
                }
                ViewData["BuildingList"] = BuildList;
                List<SelectListItem> LocaTypelist = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT
                location_type as Value,
                location_type as Text
                FROM Exact_Location 
                ORDER BY location_type"
                );
                ViewData["LocationTypeList"] = LocaTypelist;
                return View("Update");
            }
            else
            {
                string update =
                   @"UPDATE Exact_Location
                    SET location_name='{1}', location_type='{2}', location_address='{3}', building_id={4} WHERE Pid='{0}'";
                int res = DBUtl.ExecSQL(update, loca.location_id, loca.location_name, loca.location_type, loca.location_address, loca.building_id);
                if (res == 1)
                {
                    TempData["Message"] = "Location Updated";
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

        [Authorize(Roles = "owner, admin")]
        public IActionResult Delete(int id)
        {
            string select = @"SELECT * FROM Exact_Location WHERE location_id={0}";
            DataTable ds = DBUtl.GetTable(select, id);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Location does not exist";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string delete = "DELETE FROM Exact_Location WHERE location_id={0}";
                int res = DBUtl.ExecSQL(delete, id);
                if (res == 1)
                {
                    TempData["Message"] = "Location Deleted";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
            }
            return RedirectToAction("ViewLocations");
        }

    }
}