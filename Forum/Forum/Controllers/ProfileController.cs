using Forum.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class ProfileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult Index(string id)
        {
            Debug.WriteLine("Id is: " + id);
            Debug.WriteLine("User id is: " + User.Identity.GetUserId());
            // Debug.WriteLine(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory));
            
            try
            {
                ViewBag.Profile = db.Profiles.Find(id);
            } catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            return View();
        }

        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult Edit(string id)
        {
            Profile profile = db.Profiles.Find(id);
            return View(profile);
        }

        [Authorize(Roles = "User,Moderator,Administrator")]
        [HttpPut]
        public ActionResult Edit(string id, Profile requestProfile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Profile profile = db.Profiles.Find(id);
                    if (TryUpdateModel(profile))
                    {
                        profile.Name = requestProfile.Name;
                        profile.Birthday = requestProfile.Birthday;
                        TempData["message"] = "Profile changed!";
                        db.SaveChanges();
                    }

                    return RedirectToAction("Index", new { id = requestProfile.ProfileId } );
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return View(requestProfile);
        }
    }
}