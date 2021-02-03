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
    public class CustomerController : Controller
    {

        [Authorize(Roles = "admin, owner")]
        public IActionResult ViewAll()
        {
            DataTable dt = DBUtl.GetTable("SELECT * FROM SmileyCustomer");
            return View("Index", dt.Rows);

        }

        [Authorize(Roles = "admin, owner")]
        public IActionResult Create()
        {
            List<SelectListItem> rolelist = new List<SelectListItem> {
                new SelectListItem("Bronze","bronze"),
                new SelectListItem("Silver","silver"),
            new SelectListItem("Gold","gold"),};
            ViewData["UserRoleList"] = rolelist;
            return View();
        }

        [Authorize(Roles = "admin, owner")]
        [HttpPost]
        public IActionResult Create(Customer customer)
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
                   @"INSERT INTO SmileyCustomer(customer_name, surname, email, membership, signup_date) VALUES
                   ('{0}', '{1}', '{2}', '{3}', CURDATE())";

                int res = DBUtl.ExecSQL(insert, customer.customer_name, customer.surname, customer.email, customer.membership);
                if (res == 1)
                {
                    TempData["Message"] = "Customer Created";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("ViewAll");
            }
        }

        [Authorize(Roles = "admin, owner")]
        public IActionResult Update(int id)
        {
            List<SelectListItem> rolelist = new List<SelectListItem> {
                new SelectListItem("Bronze","bronze"),
                new SelectListItem("Silver","silver"),
            new SelectListItem("Gold","gold"),};
            ViewData["UserRoleList"] = rolelist;


            string select = "SELECT * FROM SmileyCustomer WHERE customer_id='{0}'";
            List<Customer> list = DBUtl.GetList<Customer>(select, id);
            if (list.Count == 1)
            {
                return View(list[0]);
            }
            else
            {
                TempData["Message"] = "Customer record does not exist";
                TempData["MsgType"] = "warning";
                return RedirectToAction("ViewAll");
            }
        }

        [Authorize(Roles = "admin, owner")]
        [HttpPost]
        public IActionResult Update(Customer customer)
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
                   @"UPDATE SmileyCustomer
                    SET customer_name='{1}', surname='{2}', email='{3}', membership={4} WHERE customer_id={0}";
                int res = DBUtl.ExecSQL(update, customer.customer_id, customer.customer_name, customer.surname, customer.email, customer.membership);
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
                return RedirectToAction("ViewAll");
            }
        }

        [Authorize(Roles = "manager")]
        public IActionResult Delete(int id)
        {
            string select = @"SELECT * FROM SmileyCustomer WHERE customer_id={0}";
            DataTable ds = DBUtl.GetTable(select, id);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "Customer does not exist";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string delete = "DELETE FROM SmileyCustomer WHERE customer_id={0}";
                int res = DBUtl.ExecSQL(delete, id);
                if (res == 1)
                {
                    TempData["Message"] = "Customer Deleted";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
            }
            return RedirectToAction("ViewAll");
        }

    }
}