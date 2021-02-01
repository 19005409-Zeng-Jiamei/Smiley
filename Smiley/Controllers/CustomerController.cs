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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Smiley.Controllers
{
    public class CustomerController : Controller
    {

        [Authorize]
        public IActionResult ViewCustomers(string returnUrl = null)
        {

            List<User> list = DBUtl.GetList<User>("SELECT * FROM SmileyCustomer");

            return View(list);
        }

        [Authorize(Roles = "owner,admin")]
        public IActionResult Delete(string id)
        {
            string delete = "DELETE FROM SmileyCustomer WHERE customer_id='{0}'";
            int res = DBUtl.ExecSQL(delete, id);
            if (res == 1)
            {
                TempData["Message"] = "Customer Record Deleted";
                TempData["MsgType"] = "success";
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
            }

            return RedirectToAction("ViewCustomers");
        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult Update(int id)
        {
            List<Customer> editedCustomer = new List<Customer>();


            string sql = @"SELECT * FROM SmileyCustomer
                         WHERE customer_id={0}";
            editedCustomer = DBUtl.GetList<Customer>(sql, id);

            if (editedCustomer.Count != 1)
            {
                TempData["Message"] = "Customer Record does not exist";
                TempData["MsgType"] = "warning";

                return RedirectToAction("ViewCustomers");
            }
            else
            {
                Customer custom = editedCustomer[0];
                return View(custom);
            }
        }

        public IActionResult Update(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "danger";
                return View("Update", customer);
            }
            else
            {

                string sql = @"UPDATE SmileyCustomer SET customer_name='{1}', surname='{2}', email='{3}', membership='{4}', customer_picfile='{5}' WHERE customer_id={0} ";

                if (DBUtl.ExecSQL(sql, customer.customer_id, customer.customer_name, customer.surname, customer.email, customer.membership, customer.picfile) == 1)
                {
                    TempData["Message"] = "Customer Updated";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }

                return RedirectToAction("ViewCustomers");
            }

        }

        [Authorize(Roles = "owner, admin")]
        public IActionResult NewCustomer()
        {

            List<String> memberList = new List<String>();
            memberList.Add("Bronze");
            memberList.Add("Silver");
            memberList.Add("Gold");
            ViewData["membershipList"] = memberList;

            return View();
        }

        [HttpPost]
        public IActionResult NewSensor(Customer customer)
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
                   @"INSERT INTO SmileyCustomer(customer_name, surname, email, membership, signup_date) VALUES ('{0}', '{1}', '{2}', '{3}', CURDATE())";

                if (DBUtl.ExecSQL(insert, customer.customer_name, customer.surname, customer.email, customer.membership) == 1)
                {                 

                    String newCustEmail = customer.email;

                    string template = @"Hey {0} {1},<br/><br/>
                               Welcome to Smiley. You are now registered as a {2} Smiley Customer!
                               <br/><br/>Cheers!<br/>Smiley :)";
                    string title = "Welcome to Smiley!";

                    string message = String.Format(template, customer.customer_name, customer.surname, customer.membership);

                    string result = "";

                    bool outcome = false;

                    outcome = EmailUtl.SendEmail(newCustEmail, title, message, out result);
                    if (outcome)
                    {
                        TempData["Message"] = "Customer Successfully Added.";
                        TempData["MsgType"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = result;
                        TempData["MsgType"] = "warning";
                    }

                    return RedirectToAction("ViewCustomers");
                }
                else
                {
                    ViewData["Message"] = DBUtl.DB_Message;
                    ViewData["MsgType"] = "danger";
                    return View("NewCustomer");
                }
            }
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
