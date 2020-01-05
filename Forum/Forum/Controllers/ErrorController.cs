using Forum.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    [MessageFilter]
    public class ErrorController : Controller
    {
        public ActionResult ErrorWithMessage(string message)
        {
            ViewBag.errorMessage = message;
            return View("Error");
        }

        public ActionResult Error()
        {
            return ErrorWithMessage("Invalid Path");
        }
    }
}