using Forum.App_Start;
using Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
    [MessageFilter]
    public class ProfileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        
        public ActionResult Index(string id)
        {
            Debug.WriteLine("Id is: " + id);
            Debug.WriteLine("User id is: " + User.Identity.GetUserId());
            // Debug.WriteLine(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory));
            
            try
            {
                ViewBag.Profile = db.Profiles.Find(id);
                ViewBag.SubjectNumber = (from subject in db.Subjects
                                        where subject.UserId == id
                                        select subject).Count();
                ViewBag.CommentNumber = (from comment in db.Comments
                                         where comment.UserId == id
                                         select comment).Count();

                var UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = UserManager.FindById(id);

                ViewBag.UserEmail = user.Email;
                ViewBag.UserId = User.Identity.GetUserId();
            } catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            return View();
        }

        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult Edit(string id)
        {
            if ( !(User.IsInRole("Administrator") || User.IsInRole("Moderator") || id == User.Identity.GetUserId()) )
            {
                TempData["message"] = "You can't edit that profile!";
                return RedirectToAction("Index", "Category");
            }

            Profile profile = db.Profiles.Find(id);
            return View(profile);
        }

        [Authorize(Roles = "User,Moderator,Administrator")]
        [HttpPut]
        public ActionResult Edit(string id, Profile requestProfile)
        {
            if (!(User.IsInRole("Administrator") || User.IsInRole("Moderator") || id == User.Identity.GetUserId()))
            {
                TempData["message"] = "You can't edit that profile!";
                return RedirectToAction("Index", "Category");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    Profile profile = db.Profiles.Find(id);
                    if (TryUpdateModel(profile))
                    {
                        profile.Name = requestProfile.Name;
                        profile.Birthday = requestProfile.Birthday;
                        profile.City = requestProfile.City;
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