using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult ErrorWithMessage(string message)
        {
            ViewBag.message = message;
            return View("Error");
        }

        public ActionResult Error()
        {
            return ErrorWithMessage("Invalid Path");
        }
    }
}