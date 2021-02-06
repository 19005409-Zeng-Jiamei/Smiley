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
    public class BuildingController : Controller
    {

        [Authorize(Roles = "owner, admin")]
        public IActionResult ViewBuildings()
        {
            DataTable dt = DBUtl.GetTable("SELECT * FROM Building");
            return View("ViewBuildings", dt.Rows);

        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "owner, admin")]
        [HttpPost]
        public IActionResult Create(Building build)
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
                   @"INSERT INTO Building (building_name , building_type, building_address, building_postal_code) VALUES
                   ('{0}', '{1}', '{2}', {3})";

                int res = DBUtl.ExecSQL(insert, build.building_name, build.building_type, build.building_address, build.building_postal_code);
                if (res == 1)
                {
                    TempData["Message"] = "Building Created";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("ViewBuildings");
            }
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Update(int id)
        {
            List<SelectListItem> Typelist = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT
                building_type as Value,
                building_type as Text
                FROM Building 
                ORDER BY building_type"
                );
            ViewData["BuildTypeList"] = Typelist ;

            string select = "SELECT * FROM Building WHERE building_id='{0}'";
            List<Building> list = DBUtl.GetList<Building>(select, id);
            if (list.Count == 1)
            {
                return View(list[0]);
            }
            else
            {
                TempData["Message"] = "Building record does not exist";
                TempData["MsgType"] = "warning";
                return RedirectToAction("ViewBuildings");
            }
        }

        [Authorize(Roles = "owner, admin")]
        [HttpPost]
        public IActionResult Update(Building build)
        {
            if (!ModelState.IsValid)
            {
                List<SelectListItem> Typelist = DBUtl.GetList<SelectListItem>(
                @"SELECT DISTINCT
                building_type as Value,
                building_type as Text
                FROM Building 
                ORDER BY building_type"
                );
                ViewData["BuildTypeList"] = Typelist;
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("Update");
            }
            else
            {
                string update =
                   @"UPDATE Building
                    SET building_name='{1}', building_type='{2}', building_address='{3}', building_postal_code={4} WHERE building_id='{0}'";
                int res = DBUtl.ExecSQL(update, build.building_id, build.building_type, build.building_address, build.building_postal_code);
                if (res == 1)
                {
                    TempData["Message"] = "Building Updated";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("ViewBuildings");
            }
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Delete(int id)
        {
            string select = @"SELECT * FROM Building WHERE building_id={0}";
            DataTable ds = DBUtl.GetTable(select, id);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Building does not exist";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string delete = "DELETE FROM Building WHERE building_id={0}";
                int res = DBUtl.ExecSQL(delete, id);
                if (res == 1)
                {
                    TempData["Message"] = "Building Deleted";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
            }
            return RedirectToAction("ViewBuildings");
        }

    }
}