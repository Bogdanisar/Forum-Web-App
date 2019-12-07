using Forum.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace Forum.Controllers
{
    public class SubjectController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            
            ViewBag.Subjects = db.Subjects.ToList<Subject>();
            return View();
        }

        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult New(int categoryId, string categoryName)
        {
            Subject s = new Subject();
            s.CategoryId = categoryId;
            ViewBag.CategoryName = categoryName;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View(s);
        }

        [Authorize(Roles = "User,Moderator,Administrator")]
        [HttpPost]
        public ActionResult New(Subject subject)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    subject.UserId = User.Identity.GetUserId();
                    subject.Date = DateTime.Now;
                    db.Subjects.Add(subject);
                    db.SaveChanges();
                    TempData["message"] = "The subject was added!";
                    return RedirectToAction("Show", new { id = subject.SubjectId });
                }
            }
            catch (Exception e)
            {

            }

            TempData["message"] = "Adding the subject failed. Please try again";
            return View(subject);
        }


        public ActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            Subject subject = db.Subjects.Find(id);

            if (subject == null)
            {
                return RedirectToAction("ErrorWithMessage", "Error", new { message = "Invalid subject id" });
                //return RedirectToAction("Error", "Error");
            }

            return View(subject);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();
            var categories = db.Categories.ToList();
            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName
                });
            }

            return selectList;
        }

        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult Edit(int id)
        {
            Subject subject = db.Subjects.Find(id);

            if ((User.IsInRole("User") || User.IsInRole("Moderator")) && subject.UserId != User.Identity.GetUserId())
            {
                return RedirectToAction(
                    "ErrorWithMessage", 
                    "Error", 
                    new { message = "You don't have permission to edit this subject!" }
                );
            }

            ViewBag.CategoryList = GetAllCategories();
            return View(subject);
        }


        [Authorize(Roles = "User,Moderator,Administrator")]
        [HttpPut]
        public ActionResult Edit(int id, Subject requestSubject)
        {
            Subject subject = db.Subjects.Find(id);
            if ((User.IsInRole("User") || User.IsInRole("Moderator")) && subject.UserId != User.Identity.GetUserId())
            {
                return RedirectToAction(
                    "ErrorWithMessage", 
                    "Error", 
                    new { message = "You don't have permission to edit that subject!" }
                );
            }

            try
            {
                if (ModelState.IsValid)
                {
                    if (TryUpdateModel(subject))
                    {
                        subject.Title = requestSubject.Title;
                        subject.Description = requestSubject.Description;
                        subject.CategoryId = requestSubject.CategoryId;
                        TempData["message"] = "Subiectul a fost modificat!";
                        db.SaveChanges();
                    }

                    return RedirectToAction("Show", new { id = subject.SubjectId });
                }
            }
            catch (Exception e) { }
            
            TempData["message"] = "Editing the subject failed. Please try again";
            return View(requestSubject);
        }

        [Authorize(Roles = "User,Moderator,Administrator")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Subject subject = db.Subjects.Find(id);
            if (User.IsInRole("User") && subject.UserId != User.Identity.GetUserId())
            {
                return RedirectToAction(
                    "ErrorWithMessage", 
                    "Error", 
                    new { message = "You don't have permission to delete that subject!" }
                );
            }

            db.Subjects.Remove(subject);
            db.SaveChanges();

            TempData["message"] = "The subject was removed";
            return RedirectToAction("Index", "Category", null);
        }
    }
}