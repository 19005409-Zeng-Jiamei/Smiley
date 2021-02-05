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
    public class FeedbackController : Controller
    {
        [Authorize(Roles ="admin, owner")]
        public IActionResult Index()
        {
            ViewData["Chart"] = "pie";
            ViewData["ShowLegend"] = "true";
            PrepareData(0);
            ViewData["Title0"] = "Gesture Feedback Summary";
            PrepareData(2);
            ViewData["Title2"] = "Emotion Feedback Summary";
            return View("Index");
        }

        public IActionResult MyFeedbacks()
        {
            TempData["Feedbacks"] = feedbacksGather("owner");

            return View();
        }

        [Authorize(Roles = "admin")]
        public IActionResult AllFeedbacks()
        {
            TempData["Feedbacks"] = feedbacksGather("admin");
            return View("MyFeedbacks");
        }

        private void PrepareData(int x)
        {
            int[] dataGesture = new int[] { 0, 0, 0 };
            int[] dataDoor = new int[] { 0, 0 };
            int[] dataEmotion = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            List<Gesture> gestList = DBUtl.GetList<Gesture>("SELECT * FROM Feedback");
            foreach (Gesture gest in gestList)
            {
                dataGesture[FeedbackCat(gest.feedback_gesture, "Gesture")]++;
            }

            List<Door> doorList = DBUtl.GetList<Door>("SELECT * FROM Door");
            foreach (Door door in doorList)
            {
                dataDoor[FeedbackCat(door.door_gesture, "Door")]++;
            }

            List<Emotion> emoList = DBUtl.GetList<Emotion>("SELECT * FROM Emotion");
            foreach (Emotion emo in emoList)
            {
                dataEmotion[FeedbackCat(emo.emotion_type, "Emotion")]++;
            }

            string[] colours = new[] { "#F1F0CC", "#C297AB", "#933E89", "#F64C71", "#FDCA40", "#86BBD8", "#563D67", "#BDBF09", "#99728D" };
            ViewData["Colors"] = colours;

            string[] grades = new[] { "A", "B", "C", "D", "F" };

            ViewData["Labels0"] = new string[] { "Good", "Neutral", "Bad" };
            ViewData["Data0"] = dataGesture;
            ViewData["Legend0"] = "Gestures";
            ViewData["Labels1"] = new string[] { "Open", "Close" };
            ViewData["Data1"] = dataDoor;
            ViewData["Legend1"] = "Door";
            ViewData["Labels2"] = new string[] { "Anger", "Anticipation", "Joy", "Trust", "Fear", "Surprise", "Sadness", "Disgust", "Others" };
            ViewData["Data2"] = dataEmotion;
            ViewData["Legend2"] = "Emotions";
            
        }

        private int FeedbackCat(string gesture, string modelname)
        {
            if (modelname.Equals("Gesture"))
            {
                if (gesture.Equals("Good")) return 0;
                else if (gesture.Equals("Bad")) return 2;
                else return 1;
            }
            else if (modelname.Equals("Door"))
            {
                if (gesture.Equals("Open")) return 0;
                else return 1;
            }
            else if (modelname.Equals("Emotion"))
            {
                if (gesture.Equals("anger")) return 0;
                else if (gesture.Equals("anticipation")) return 1;
                else if (gesture.Equals("joy")) return 2;
                else if (gesture.Equals("trust")) return 3;
                else if (gesture.Equals("fear")) return 4;
                else if (gesture.Equals("surprise")) return 5;
                else if (gesture.Equals("sadness")) return 6;
                else if (gesture.Equals("disgust")) return 7;
                else return 8;
            }
            else
            {
                return 0;
            }
        }

        private List<String[]> feedbacksGather(string userRole)
        {
            List<string[]> feedbackList = new List<string[]>();
            List<Gesture> gestList = new List<Gesture>();
            List<Door> doorList = new List<Door>();
            List<Emotion> emoList = new List<Emotion>();

            if (userRole.Equals("admin"))
            {
                gestList = DBUtl.GetList<Gesture>("SELECT * FROM Feedback");

                doorList = DBUtl.GetList<Door>("SELECT * FROM Door");

                emoList = DBUtl.GetList<Emotion>("SELECT * FROM Emotion");

            }
            else
            {
                string userid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                gestList = DBUtl.GetList<Gesture>("SELECT * FROM Feedback INNER JOIN Sensor ON Feedback.sensor_id = Sensor.sensor_id WHERE smiley_user_id = '{0}'", userid);

                doorList = DBUtl.GetList<Door>("SELECT * FROM Door INNER JOIN Sensor ON Door.sensor_id = Sensor.sensor_id WHERE smiley_user_id = '{0}'", userid);

                emoList = DBUtl.GetList<Emotion>("SELECT * FROM Emotion INNER JOIN Sensor ON Emotion.sensor_id = Sensor.sensor_id WHERE smiley_user_id = '{0}'", userid);
            }

            foreach (Gesture gest in gestList)
            {
                feedbackList.Add(new string[] { gest.feedback_gesture, gest.time_stamp.ToString("MM/dd/yyyy HH:mm:ss"), gest.sensor_id.ToString(), "Gesture" });
            }

            foreach (Door door in doorList)
            {
                feedbackList.Add(new string[] { door.door_gesture, door.time_stamp.ToString("MM/dd/yyyy HH:mm:ss"), door.sensor_id.ToString(), "Door" });
            }

            foreach (Emotion emo in emoList)
            {
                feedbackList.Add(new string[] { emo.emotion_type, emo.time_stamp.ToString("MM/dd/yyyy HH:mm:ss"), emo.sensor_id.ToString(), "Emotion" });
            }

            return feedbackList;
        }
    }
}