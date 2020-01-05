using Forum.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using Forum.App_Start;

namespace Forum.Controllers
{
    [MessageFilter]
    public class SubjectController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult Search(string word)
        {
            Debug.WriteLine(word);

            var subjectList = from subject in db.Subjects
                              where subject.Description.Contains(word) || subject.Title.Contains(word)
                              select subject;

            ViewBag.Subjects = subjectList.ToList<Subject>();
            ViewBag.Word = word;

            return View();
        }

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
        public ActionResult New(Subject subject, string categoryName)
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
                Debug.WriteLine(e.Message);
            }

            ViewBag.message = "Adding the subject failed. Please try again";
            ViewBag.CategoryName = categoryName;
            return View(subject);
        }





        public ActionResult Show(int id)
        {
            //if (TempData.ContainsKey("message"))
            //{
            //    ViewBag.message = TempData["message"].ToString();
            //}

            Subject subject = db.Subjects.Find(id);

            if (subject == null)
            {
                return RedirectToAction("ErrorWithMessage", "Error", new { message = "Invalid subject id" });
                //return RedirectToAction("Error", "Error");
            }
            
            ViewBag.Comments = subject.Comments.ToList<Comment>();
            ViewBag.UserCanDelete = false;
            if (Request.IsAuthenticated) {
                ViewBag.UserId = User.Identity.GetUserId();
                if (User.IsInRole("Administrator") || User.IsInRole("Moderator"))
                {
                    ViewBag.UserCanDelete = true;
                }
            }
            return View(subject);
        }


        public ActionResult CreateComment(int id)
        {
            Subject subject = db.Subjects.Find(id);
            return RedirectToAction("New", "Comment", new { subjectId = subject.SubjectId, subjectTitle = subject.Title });
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



        [NonAction]
        public bool DeleteSubject(ApplicationDbContext db, int id)
        {
            try
            {
                Subject subject = db.Subjects.Find(id);

                // remove the subject upvotes
                var upvotesToDelete = db.SubjectUpvotes.Where(u => u.SubjectId == id).ToList();
                foreach (var u in upvotesToDelete)
                {
                    db.SubjectUpvotes.Remove(u);
                }

                // remove the comments of this subject
                var commentController = DependencyResolver.Current.GetService<CommentController>(); // get a valid commentController to call its DeleteComment method
                commentController.ControllerContext = new ControllerContext(this.Request.RequestContext, commentController);
                foreach (var c in subject.Comments.ToList())
                {
                    commentController.DeleteComment(db, c.CommentId);
                }

                db.Subjects.Remove(subject);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

            return true;
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

            if (this.DeleteSubject(this.db, id))
            {
                db.SaveChanges();
                TempData["message"] = "The subject was removed";
                return RedirectToAction("Show", "Category", new { id = subject.CategoryId });
            }
            else
            {
                return RedirectToAction(
                    "ErrorWithMessage",
                    "Error",
                    new { message = "Failed to remove subject!" }
                );
            }

        }
    }
}