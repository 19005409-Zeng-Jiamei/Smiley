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
    public class FeedbackController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            PrepareData(0);
            PrepareData(2);
            ViewData["Chart"] = "pie";
            ViewData["Title1"] = "Gesture Feedback Summary";
            ViewData["Title2"] = "Emotion Feedback Summary";
            ViewData["ShowLegend"] = true;
            return View();
        }

        [Authorize(Roles = "owner,user")]
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

            ViewData["Colours"] = colours;

            string[] feedbackGest = new string[] { };
            if (x == 0)
            {
                feedbackGest = new string[] { "Good", "Neutral", "Bad" };
                ViewData["Data"] = dataGesture;
                ViewData["Legend"] = "Gestures";
            }
            else if (x == 1)
            {
                feedbackGest = new string[] { "Open", "Close" };
                ViewData["Data"] = dataDoor;
                ViewData["Legend"] = "Door";
            }
            else
            {
                feedbackGest = new string[] { "Anger", "Anticipation", "Joy", "Trust", "Fear", "Surprise", "Sadness", "Disgust", "Others" };
                ViewData["Data"] = dataEmotion;
                ViewData["Legend"] = "Emotions";
            }
            ViewData["Labels"] = feedbackGest;
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
