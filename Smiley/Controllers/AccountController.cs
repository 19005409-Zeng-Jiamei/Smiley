﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Security.Claims;
using Smiley.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


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

        [Authorize]
        public IActionResult Profile(string returnUrl = null)
        {

            return View();
        }

        [Authorize]
        public IActionResult Users(string returnUrl = null)
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
        public async Task<string> Test()
        {
            var personGroupId = "c200y2019";

            string fullpath = Path.Combine(_env.WebRootPath, "webcam\\FrankieDesmond.jpg");

            using (Stream fs = System.IO.File.OpenRead(fullpath))
            {
                var faces = await faceClient.Face.DetectWithStreamAsync(fs);
                var faceIds = faces.Select(face => face.FaceId).ToArray();
                var results = await faceClient.Face.IdentifyAsync(faceIds.OfType<Guid?>().ToList(), personGroupId);
                foreach (var identifyResult in results)
                {
                    Debug.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Count == 0)
                    {
                        Debug.WriteLine("No one identified");
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        //CHANGE FROM HERE TOO
                        var confidence = identifyResult.Candidates[0].Confidence;
                        var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                        Debug.WriteLine("Identified as {0} ({1})", person.Name, confidence);
                    }
                }
            }

            return "";
        }

        public IActionResult FaceLogin()
        {
            return View();
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
                            //CHANGE FROM HERE
                            var confidence = identifyResult.Candidates[0].Confidence;
                            var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                            Debug.WriteLine("Identified as {0} ({1})", person.Name, confidence);

                            TempData["Message"] = $"Identified as {person.Name} ({confidence})";
                            outcome = "yes";
                        }
                    }
                }
            }

            return outcome;

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

        public AccountController(IWebHostEnvironment environment,
                        IConfiguration config)
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

        private IWebHostEnvironment _env;

        //const string faceApiKey = "0d7af552cce6469999a42e0383d1edd9";
        //const faceApiEndPoint = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0";

        private readonly string FACEAPIKEY;
        private readonly string FACEAPIENDPOINT;
        private readonly IFaceClient faceClient;
    }
}
