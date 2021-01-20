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
        //View
        [Authorize(Roles = "owner, admin")]
        public IActionResult ViewFeedBack()
        {
            return View();
        }

        //Delete
        [Authorize(Roles = "owner, admin")]
        public IActionResult DeleteFeedBack(List<int> id, List<int> sensor_id)
        {
            string sql = @"SELECT * FROM Prediction
                         WHERE prediction_id={0} AND sensor_id={1}";

            string message = "";
            int colour = 0;
            for (int count =0; count < id.Count; count++)
            {
                DataTable ds = DBUtl.GetTable(sql, id[count], sensor_id[count]);                
                if (ds.Rows.Count != 1)
                {
                    message += String.Format("/nPrediction Record {0} does not exist", id[count]);
                    if (colour != 2)
                        colour = 1;
                }
                else
                {
                    int res = DBUtl.ExecSQL("DELETE FROM Facility WHERE prediction_id ={0}", id[count]);
                    if (res == 1)
                    {
                        message += "/nFeedback Record Deleted";
                    }
                    else
                    {
                        message += String.Format("/n Prediction Record {0} : ", id[count]) + DBUtl.DB_Message;
                        colour = 2;
                    }
                }
                
            }
            TempData["Message"] = message;
            if (colour == 1)
                TempData["MsgType"] = "warning";
            else if (colour == 2)
                TempData["MsgType"] = "danger";
            else
                TempData["MsgType"] = "success";

            return RedirectToAction("ViewFeedBack");

        }

    }
}
