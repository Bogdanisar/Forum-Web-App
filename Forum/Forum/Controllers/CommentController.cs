using Forum.App_Start;
using Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    [MessageFilter]
    public class CommentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            ViewBag.Comments = db.Comments.ToList<Comment>();
            return View();
        }


        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult New(int subjectId, string subjectTitle)
        {
            Comment c = new Comment();
            c.SubjectId = subjectId;
            ViewBag.SubjectTitle = subjectTitle;

            return View(c);
        }


        [Authorize(Roles = "User,Moderator,Administrator")]
        [HttpPost]
        public ActionResult New(Comment comment, string subjectTitle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    comment.UserId = User.Identity.GetUserId();
                    comment.Date = DateTime.Now;
                    comment.Content = Sanitizer.GetSafeHtmlFragment(comment.Content);
                    db.Comments.Add(comment);
                    db.SaveChanges();
                    return RedirectToAction("Show", "Subject", new { id = comment.SubjectId });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            ViewBag.message = "Adding the comment failed. Please try again";
            ViewBag.SubjectTitle = subjectTitle;
            return View(comment);
        }

        

        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult Edit(int id)
        {
            Comment comment = db.Comments.Find(id);

            if ((User.IsInRole("User") || User.IsInRole("Moderator")) && comment.UserId != User.Identity.GetUserId())
            {
                TempData["message"] = "You don't have permission to edit that comment!";
                return RedirectToAction("Show", "Subject", new { @id = comment.SubjectId });
            }
            
            return View(comment);
        }


        [Authorize(Roles = "User,Moderator,Administrator")]
        [HttpPut]
        public ActionResult Edit(int id, Comment requestComment)
        {
            Comment comment = db.Comments.Find(id);
            if ((User.IsInRole("User") || User.IsInRole("Moderator")) && comment.UserId != User.Identity.GetUserId())
            {
                TempData["message"] = "You don't have permission to edit that comment!";
                return RedirectToAction("Show", "Subject", new { @id = comment.SubjectId });
            }

            try
            {
                if (ModelState.IsValid)
                {
                    if (TryUpdateModel(comment))
                    {
                        requestComment.Content = Sanitizer.GetSafeHtmlFragment(requestComment.Content);

                        comment.Content = requestComment.Content;
                        db.SaveChanges();

                        TempData["message"] = "The comment was modified!";
                        return RedirectToAction("Show", "Subject", new { id = comment.SubjectId });
                    }

                }
            }
            catch (Exception e) { }

            ViewBag.message = "Editing the comment failed. Please try again";
            return View(requestComment);
        }

        [NonAction]
        public bool DeleteComment(ApplicationDbContext db, int id)
        {
            try
            {
                Comment comment = db.Comments.Find(id);

                comment.UpvotingUsers.Clear(); // delete all the upvotes from the db;
                db.Comments.Remove(comment);
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
            Comment comment = db.Comments.Find(id);
            if (User.IsInRole("User") && comment.UserId != User.Identity.GetUserId())
            {
                TempData["message"] = "You don't have permission to delete that comment!";
                return RedirectToAction("Show", "Subject", new { @id = comment.SubjectId });
            }

            if (this.DeleteComment(db, id))
            {
                db.SaveChanges();
                TempData["message"] = "The comment was removed";
                return RedirectToAction("Show", "Subject", new { id = comment.SubjectId });
            }
            else
            {
                TempData["message"] = "Failed to delete comment :(";
                return RedirectToAction("Show", "Subject", new { @id = comment.SubjectId });
            }
        }
    }
}