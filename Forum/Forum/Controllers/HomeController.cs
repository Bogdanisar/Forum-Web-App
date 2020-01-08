using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Forum.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Category");
        }

        public ActionResult About()
        {
            return RedirectToAction("Index", "Category");
        }

        public ActionResult Contact()
        {
            return RedirectToAction("Index", "Category");
        }
    }
}