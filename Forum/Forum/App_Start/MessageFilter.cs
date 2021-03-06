﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.App_Start
{
    public class MessageFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Controller.TempData.ContainsKey("message"))
            {
                filterContext.Controller.ViewBag.message = filterContext.Controller.TempData["message"].ToString();
                filterContext.Controller.TempData.Remove("message");
            }
        }
    }
}