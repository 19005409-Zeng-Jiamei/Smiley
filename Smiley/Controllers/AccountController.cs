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
    public class AccountController : Controller
    {
        private const string LOGIN_SQL =
           @"SELECT * FROM SmileyUser 
            WHERE smiley_user_id = '{0}' 
              AND smiley_user_pw = HASHBYTES('SHA1', '{1}')";

        private const string LASTLOGIN_SQL =
           @"UPDATE SmileyUser SET last_login=GETDATE() WHERE smiley_user_id='{0}'";

        private const string ROLE_COL = "smiley_user_role";
        private const string NAME_COL = "full_name";
        

        private const string REDIRECT_CNTR = "Home";
        private const string REDIRECT_ACTN = "Index";

        private const string LOGIN_VIEW = "UserLogin";

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            TempData["ReturnUrl"] = returnUrl;
            return View(LOGIN_VIEW);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(UserLogin user)
        {
            if (!AuthenticateUser(user.smiley_user_id, user.smiley_user_pw, out ClaimsPrincipal principal))
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
                DBUtl.ExecSQL(LASTLOGIN_SQL, user.smiley_user_id);

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
            return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }

        [Authorize(Roles = "owner,admin")]
        public IActionResult Users()
        {
            List<SmileyUser> list = DBUtl.GetList<SmileyUser>("SELECT * FROM SmileyUser WHERE smiley_user_role='user' ");
            return View(list);
        }

        [Authorize(Roles = "owner,admin")]
        public IActionResult Delete(string id)
        {
            string delete = "DELETE FROM SmileyUser WHERE smiley_user_id='{0}'";
            int res = DBUtl.ExecSQL(delete, id);
            if (res == 1)
            {
                TempData["Message"] = "User Record Deleted";
                TempData["MsgType"] = "success";
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
            }

            return RedirectToAction("Users");
        }

        [Authorize(Roles = "owner,admin")]
        public IActionResult Register()
        {
            return View("UserRegister");
        }

        [Authorize(Roles = "owner,admin")]
        [HttpPost]
        public IActionResult Register(SmileyUser usr)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("UserRegister");
            }
            else
            {
                string insert =
                   @"INSERT INTO SmileyUser(smiley_user_id, smiley_user_pw , full_name, email, smiley_user_role) 
                        VALUES('{0}',   HASHBYTES('SHA1', '{1}'), '{2}', '{3}','{4}')";
                if (DBUtl.ExecSQL(insert, usr.smiley_user_id, usr.smiley_user_pw, usr.full_name, usr.email, usr.smiley_user_role) == 1)
                {
                    string template = @"Hi {0},<br/><br/>
                               Welcome to Smiley! :)
                               Your userid is <b>{1}</b> and password is <b>{2}</b>.
                               <br/><br/>{3} {4}";
                    string title = "Registration Successful - Welcome";
                    string message = String.Format(template, usr.full_name, usr.smiley_user_id, usr.smiley_user_pw, User.FindFirst(ClaimTypes.Role).Value, User.FindFirst(ClaimTypes.Name).Value);
                    string result = "";

                    bool outcome = false;

                    outcome = EmailUtl.SendEmail(usr.email, title, message, out result);
                    if (outcome)
                    {
                        ViewData["Message"] = "User Successfully Registered";
                        ViewData["MsgType"] = "success";
                    }
                    else
                    {
                        ViewData["Message"] = result;
                        ViewData["MsgType"] = "warning";
                    }
                }
                else
                {
                    ViewData["Message"] = DBUtl.DB_Message;
                    ViewData["MsgType"] = "danger";
                }
                return View("UserRegister");
            }
        }

        [AllowAnonymous]
        public IActionResult VerifyUserID(string userId)
        {
            string select = $"SELECT * FROM SmileyUser WHERE smiley_user_id='{userId}'";
            if (DBUtl.GetTable(select).Rows.Count > 0)
            {
                return Json($"[{userId}] already in use");
            }
            return Json(true);
        }

        [AllowAnonymous]
        public IActionResult VerifyUserEmail(string email)
        {
            string select = $"SELECT * FROM SmileyUser WHERE email='{email}'";
            if (DBUtl.GetTable(select).Rows.Count > 0)
            {
                return Json($"[{email}] already in use");
            }
            return Json(true);
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
    }
}
