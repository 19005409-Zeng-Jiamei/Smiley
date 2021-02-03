using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Smiley.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Smiley.Controllers
{
    public class AccountController : Controller
    {
        private const string LOGIN_SQL =
           @"SELECT * FROM SmileyUser 
            WHERE smiley_user_id = '{0}' 
              AND smiley_user_pw = HASHBYTES('SHA1', '{1}')";

        private const string LASTLOGIN_SQL =
           @"UPDATE SmileyUser SET last_login=GETDATE() WHERE smiley_user_id ='{0}'";

        private const string ROLE_COL = "smiley_user_role";
        private const string NAME_COL = "full_name";

        private const string REDIRECT_CNTR = "Feedback";
        private const string REDIRECT_ACTN = "Index";

        private const string LOGIN_VIEW = "Login";

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            TempData["ReturnUrl"] = returnUrl;
            return View(LOGIN_VIEW);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(UserLogin user)
        {
            if (!AuthenticateUser(user.UserID, user.Password, out ClaimsPrincipal principal))
            {
                ViewData["Message"] = "Incorrect User ID or Password";
                ViewData["MsgType"] = "warning";
                return View(LOGIN_VIEW);
            }
            else
            {
                HttpContext.SignInAsync(
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   principal);

                // Update the Last Login Timestamp of the User
                DBUtl.ExecSQL(LASTLOGIN_SQL, user.UserID);

                if (TempData["returnUrl"] != null)
                {
                    string returnUrl = TempData["returnUrl"].ToString();
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                }

                return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
            }
        }

        [Authorize]
        public IActionResult Logoff(string returnUrl = null)
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(LOGIN_VIEW);
        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View(LOGIN_VIEW);
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Users()
        {
            string role = User.FindFirst(ClaimTypes.Role).Value;
            List<User> list = new List<User>();
            if (role.Equals("admin"))
            {
                list = DBUtl.GetList<User>("SELECT * FROM SmileyUser");
            }
            else if (role.Equals("owner"))
            {
                list = DBUtl.GetList<User>("SELECT * FROM SmileyUser WHERE smiley_user_role='user'");
            }

            return View(list);

        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Create()
        {
            List<SelectListItem> rolelist = new List<SelectListItem>();
            if (User.IsInRole("admin"))
            {
                rolelist = new List<SelectListItem> { 
                    new SelectListItem("Admin","admin"),
                new SelectListItem("Owner","owner"),
                new SelectListItem("User","user"),};
            }
            else
            {
                rolelist = new List<SelectListItem> {
                new SelectListItem("Owner","owner"),
                new SelectListItem("User","user"),};
            }
            ViewData["UserRoleList"] = rolelist;
            return View();
        }

        [Authorize(Roles = "owner, admin")]
        [HttpPost]
        public IActionResult Create(User user)
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
                   @"INSERT INTO SmileyUser (smiley_user_id, smiley_user_pw , full_name, email, smiley_user_role) VALUES
                   ('{0}', HASHBYTES('SHA1', '{1}'), '{2}', '{3}','{4}')";

                int res = DBUtl.ExecSQL(insert, user.smiley_user_id, user.smiley_user_pw, user.full_name, user.email, user.smiley_user_role);
                if (res == 1)
                {
                    TempData["Message"] = "User Created";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }

                return RedirectToAction("Users");
            }
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Update(int id)
        {
            List<SelectListItem> rolelist = new List<SelectListItem>();
            if (User.IsInRole("admin"))
            {
                rolelist = new List<SelectListItem> { new SelectListItem("Admin","admin"),
                new SelectListItem("Owner","owner"),
                new SelectListItem("User","user"),};
            }
            else
            {
                rolelist = new List<SelectListItem> {
                new SelectListItem("Owner","owner"),
                new SelectListItem("User","user"),};
            }
            ViewData["UserRoleList"] = rolelist;


            string select = "SELECT * FROM SmileyUser WHERE smiley_user_id='{0}'";
            List<User> list = DBUtl.GetList<User>(select, id);
            if (list.Count == 1)
            {
                return View(list[0]);
            }
            else
            {
                TempData["Message"] = "User record does not exist";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "owner, admin")]
        [HttpPost]
        public IActionResult Update(User user)
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
                   @"UPDATE SmileyUser
                    SET smiley_user_pw='{1}', full_name='{2}', email='{3}', smiley_user_role='{4}' WHERE smiley_user_id='{0}'";
                int res = DBUtl.ExecSQL(update, user.smiley_user_id, user.smiley_user_pw, user.full_name, user.email,user.smiley_user_role);
                if (res == 1)
                {
                    TempData["Message"] = "User Updated";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("Users");
            }
        }

        [Authorize(Roles = "manager")]
        public IActionResult Delete(int id)
        {
            string select = @"SELECT * FROM SmileyUser WHERE smiley_user_id={0}";
            DataTable ds = DBUtl.GetTable(select, id);
            if (ds.Rows.Count != 1)
            {
                TempData["Message"] = "User does not exist";
                TempData["MsgType"] = "warning";
            }
            else
            {
                string delete = "DELETE FROM SmileyUser WHERE smiley_user_id={0}";
                int res = DBUtl.ExecSQL(delete, id);
                if (res == 1)
                {
                    TempData["Message"] = "User Deleted";
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

        private bool AuthenticateUser(string uid, string pw, out ClaimsPrincipal principal)
        {
            principal = null;

            DataTable ds = DBUtl.GetTable(LOGIN_SQL, uid, pw);
            if (ds.Rows.Count == 1)
            {
                principal =
                   new ClaimsPrincipal(
                      new ClaimsIdentity(
                         new Claim[] {
                        new Claim(ClaimTypes.NameIdentifier, uid),
                        new Claim(ClaimTypes.Name, ds.Rows[0][NAME_COL].ToString()),
                        new Claim(ClaimTypes.Role, ds.Rows[0][ROLE_COL].ToString())
                         }, "Basic"
                      )
                   );

                return true;
            }
            return false;
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult UniqueUserID (string user_id)
        {
            List<User> list = DBUtl.GetList<User>("SELECT * FROM SmileyUser WHERE smiley_user_id='{0}'", user_id);
            if (list.Count == 1)
            {
                return Json($"Email {user_id} is already in use.");
            } else
            {
                return Json(true);
            }
        }

    }
}