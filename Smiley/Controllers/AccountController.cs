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
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;


namespace Smiley.Controllers
{
    public class AccountController : Controller
    {
        private const string LOGIN_SQL =
           @"SELECT * FROM SmileyUser 
            WHERE smiley_user_id = '{0}' 
              AND smiley_user_pw = HASHBYTES('SHA1', '{1}')";

        private const string FACELOGIN_SQL =
           @"SELECT * FROM SmileyUser 
            WHERE smiley_user_id = '{0}'";

        private const string LASTLOGIN_SQL =
           @"UPDATE SmileyUser SET last_login=GETDATE() WHERE smiley_user_id ='{0}'";

        private const string ROLE_COL = "smiley_user_role";
        private const string NAME_COL = "full_name";

        private const string REDIRECT_CNTR = "Feedback";
        private const string REDIRECT_ACTN = "Index";

        private const string LOGIN_VIEW = "Login";

        private IWebHostEnvironment _env;

        //const string faceApiKey = "0d7af552cce6469999a42e0383d1edd9";
        //const faceApiEndPoint = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0";

        private readonly string FACEAPIKEY;
        private readonly string FACEAPIENDPOINT;
        private readonly IFaceClient faceClient;

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

        [AllowAnonymous]
        public IActionResult FaceIDLogin()
        {
            return View("FaceIDLogin");
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
                return View("Create");
            }
            else
            {
                string insert = "";
                if (user.smiley_user_role.Equals("user"))
                {
                    insert = @"INSERT INTO SmileyUser (smiley_user_id, smiley_user_pw , full_name, email, smiley_user_role, superior_id) VALUES
                   ('{0}', HASHBYTES('SHA1', '{1}'), '{2}', '{3}','{4}', '{5}')";

                }
                else
                {
                    insert = @"INSERT INTO SmileyUser (smiley_user_id, smiley_user_pw , full_name, email, smiley_user_role) VALUES
                   ('{0}', HASHBYTES('SHA1', '{1}'), '{2}', '{3}','{4}')";
                }

                int res = DBUtl.ExecSQL(insert, user.smiley_user_id, user.smiley_user_pw, user.full_name, user.email, user.smiley_user_role, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (res == 1)
                {
                    if (user.smiley_user_picfile != "" || user.smiley_user_picfile != null)
                    {
                        if (DBUtl.ExecSQL("INSERT INTO FaceId(face_picfile) VALUES ('{0}')", user.smiley_user_picfile) == 1)
                        {
                            List<FaceID> dt = DBUtl.GetList<FaceID>("SELECT * FROM FaceId WHERE face_picfile = '{0}'", user.smiley_user_picfile);
                            if (dt.Count == 1)
                            {
                                int faceID = dt[0].face_record_id;
                                string update = @"UPDATE SmileyUser SET face_id={1} WHERE smiley_user_id='{0}'";
                                int test = DBUtl.ExecSQL(update, faceID);
                                if (test == 1)
                                {
                                    TempData["Message"] = "User Updated";
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
                    }
                    else
                    {
                        TempData["Message"] = "User Created";
                        TempData["MsgType"] = "success";
                    }

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
        public IActionResult Update(string id)
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
                return View("Update");
            }
            else
            {
                string update =
                   @"UPDATE SmileyUser
                    SET smiley_user_pw='{1}', full_name='{2}', email='{3}', smiley_user_role='{4}' WHERE smiley_user_id='{0}'";
                int res = DBUtl.ExecSQL(update, user.smiley_user_id, user.smiley_user_pw, user.full_name, user.email, user.smiley_user_role);
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
        public IActionResult Delete(string id)
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

        public async Task<string> FaceLogin(IFormFile upimage)
        {
            string outcome = "no";
            string fullpath = Path.Combine(_env.WebRootPath, @"webcam\login.jpg");
            using (FileStream fs = new FileStream(fullpath, FileMode.Create))
            {
                upimage.CopyTo(fs);
                fs.Close();
            }
            var personGroupId = "c200team1";

            using (Stream fs = System.IO.File.OpenRead(fullpath))
            {
                var faces = await faceClient.Face.DetectWithStreamAsync(fs);
                if (faces.Count == 0)
                {
                    TempData["Message"] = "No person in the Webcam";
                }
                else if (faces.Count > 1)
                {
                    TempData["Message"] = "One person at a time please";
                }
                else
                {
                    var faceIds = faces.Select(face => face.FaceId).ToArray();
                    var results = await faceClient.Face.IdentifyAsync(faceIds.OfType<Guid?>().ToList(), personGroupId);
                    foreach (var identifyResult in results)
                    {
                        Debug.WriteLine("Result of face: {0}", identifyResult.FaceId);
                        if (identifyResult.Candidates.Count == 0)
                        {
                            Debug.WriteLine("No one identified");
                            TempData["Message"] = "No One Identified";
                        }
                        else
                        {
                            // Get top 1 among all candidates returned
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var confidence = identifyResult.Candidates[0].Confidence;
                            var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                            Debug.WriteLine("Identified as {0} ({1})", person.Name, confidence);

                            if (!AuthenticateFaceUser(person.Name, out ClaimsPrincipal principal))
                            {
                                TempData["Message"] = "Cannot find User Face ID";
                                ViewData["MsgType"] = "warning";
                            }
                            else
                            {
                                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                                // Update the Last Login Timestamp of the User
                                DBUtl.ExecSQL(LASTLOGIN_SQL, person.Name);
                                outcome = "yes";
                            }
                        }
                    }
                }

                return outcome;

            }
        }

        public string FaceLoginOrg(IFormFile upimage)
        {
            string fullpath = Path.Combine(_env.WebRootPath, "login/user.jpg");
            using (FileStream fs = new FileStream(fullpath, FileMode.Create))
            {
                upimage.CopyTo(fs);
                fs.Close();
            }

            string imagePath = @"/login/user.jpg";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FACEAPIKEY);
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";
            string uri = FACEAPIENDPOINT + "/detect?" + requestParameters;
            var fileInfo = _env.WebRootFileProvider.GetFileInfo(imagePath);
            var byteData = GetImageAsByteArray(fileInfo.PhysicalPath);
            string contentStringFace = string.Empty;
            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                var response = client.PostAsync(uri, content).Result;

                // Get the JSON response.
                contentStringFace = response.Content.ReadAsStringAsync().Result;
            }

            var expConverter = new ExpandoObjectConverter();
            dynamic faceList = JsonConvert.DeserializeObject<List<ExpandoObject>>(contentStringFace, expConverter);
            if (faceList.Count == 0)
            {
                TempData["json"] = "No Face detected";
            }
            else
            {
                TempData["json"] = JsonPrettyPrint(contentStringFace);
            }

            return contentStringFace;

        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            byte[] bytes = binaryReader.ReadBytes((int)fileStream.Length);
            binaryReader.Close();
            fileStream.Close();
            return bytes;
        }


        static string JsonPrettyPrint(string json)
        {
            string INDENT_STRING = "    ";
            int indentation = 0;
            int quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, --indentation)) + ch : ch.ToString()
                select lineBreak == null ? openChar.Length > 1 ? openChar : closeChar : lineBreak;
            return String.Concat(result);
        }

        public AccountController(IWebHostEnvironment environment, IConfiguration config)
        {
            _env = environment;
            FACEAPIKEY = config.GetSection("FaceConfig").GetValue<string>("SubscriptionKey");
            FACEAPIENDPOINT = config.GetSection("FaceConfig").GetValue<string>("EndPoint");
            faceClient = new FaceClient(
               new ApiKeyServiceClientCredentials(FACEAPIKEY),
               new System.Net.Http.DelegatingHandler[] { });
            //faceClient.Endpoint = FACEAPIENDPOINT;
            faceClient.Endpoint = "https://southeastasia.api.cognitive.microsoft.com";

        }

        private bool AuthenticateFaceUser(string uid, out ClaimsPrincipal principal)
        {
            principal = null;

            DataTable ds = DBUtl.GetTable(FACELOGIN_SQL, uid);
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

        public IActionResult UniqueUserID(string user_id)
        {
            List<User> list = DBUtl.GetList<User>("SELECT * FROM SmileyUser WHERE smiley_user_id='{0}'", user_id);
            if (list.Count == 1)
            {
                return Json($"User ID {user_id} is already in use.");
            }
            else
            {
                return Json(true);
            }
        }

    }
}