using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Smiley.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace Smiley.Controllers
{
    public class CustomerController : Controller
    {

        [Authorize(Roles = "admin, owner")]
        public IActionResult ViewCustomers()
        {
            DataTable dt = DBUtl.GetTable("SELECT * FROM SmileyCustomer");
            return View("ViewCustomers", dt.Rows);

        }

        [Authorize(Roles = "admin, owner")]
        public IActionResult Create()
        {
            List<SelectListItem> rolelist = new List<SelectListItem> {
                new SelectListItem("Bronze","bronze"),
                new SelectListItem("Silver","silver"),
            new SelectListItem("Gold","gold"),};
            ViewData["MembershipList"] = rolelist;
            return View();
        }

        public async Task<string> SnapShot(IFormFile upimage)
        {
            string filename = Guid.NewGuid().ToString() + ".jpg";
            string fullpath = Path.Combine(_env.WebRootPath, @"customers\" + filename);
            using (FileStream fs = new FileStream(fullpath, FileMode.Create))
            {
                upimage.CopyTo(fs);
                fs.Close();
            }
            return filename;
        }

        [Authorize(Roles = "admin, owner")]
        [HttpPost]
        public IActionResult Create(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                List<SelectListItem> rolelist = new List<SelectListItem> {
                new SelectListItem("Bronze","bronze"),
                new SelectListItem("Silver","silver"),
            new SelectListItem("Gold","gold"),};
                ViewData["MembershipList"] = rolelist;
                return View("Create");
            }
            else
            {
                if (DBUtl.ExecSQL("INSERT INTO FaceId(face_picfile) VALUES ('{0}')", customer.picfile) == 1)
                {
                    List<FaceID> dt = DBUtl.GetList<FaceID>("SELECT * FROM FaceId WHERE face_picfile = '{0}'", customer.picfile);
                    if (dt.Count == 1)
                    {
                        int faceID = dt[0].face_record_id;
                        string custInsert =
                    @"INSERT INTO SmileyCustomer(customer_name, surname, email, membership, signup_date, face_id) VALUES
                   ('{0}', '{1}', '{2}', '{3}', CURDATE()), {4}";

                        int res = DBUtl.ExecSQL(custInsert, customer.customer_name, customer.surname, customer.email, customer.membership, faceID);
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
                    }
                    else
                    {
                        TempData["Message"] = DBUtl.DB_Message;
                        TempData["MsgType"] = "danger";

                    }
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
            ViewData["MembershipList"] = rolelist;


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
                List<SelectListItem> rolelist = new List<SelectListItem> {
                new SelectListItem("Bronze","bronze"),
                new SelectListItem("Silver","silver"),
            new SelectListItem("Gold","gold"),};
                ViewData["MembershipList"] = rolelist;
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

        private void UploadFile(IFormFile ufile, string fname)
        {
            string fullpath = Path.Combine(_env.WebRootPath, fname);
            using (var fileStream = new FileStream(fullpath, FileMode.Create))
            {
                ufile.CopyToAsync(fileStream);
            }
        }

        private IWebHostEnvironment _env;
        public CustomerController(IWebHostEnvironment environment)
        {
            _env = environment;
        }

    }
}