using Forum.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class CommentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.Comments = db.Comments.ToList<Comment>();
            return View();
        }


        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult New(int subjectId, string subjectTitle)
        {
            Comment c = new Comment();
            c.SubjectId = subjectId;
            ViewBag.SubjectTitle = subjectTitle;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View(c);
        }


        [Authorize(Roles = "User,Moderator,Administrator")]
        [HttpPost]
        public ActionResult New(Comment comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    comment.UserId = User.Identity.GetUserId();
                    comment.Date = DateTime.Now;
                    db.Comments.Add(comment);
                    db.SaveChanges();
                    return RedirectToAction("Show", "Subject", new { id = comment.SubjectId });
                }
            }
            catch (Exception e)
            {

            }
            
            return View(comment);
        }


        public ActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            Comment comment = db.Comments.Find(id);

            if (comment == null)
            {
                return RedirectToAction("ErrorWithMessage", "Error", new { message = "Invalid comment id" });
                //return RedirectToAction("Error", "Error");
            }

            return View(comment);
        }

        [Authorize(Roles = "User,Moderator,Administrator")]
        public ActionResult Edit(int id)
        {
            Comment comment = db.Comments.Find(id);

            if ((User.IsInRole("User") || User.IsInRole("Moderator")) && comment.UserId != User.Identity.GetUserId())
            {
                return RedirectToAction(
                    "ErrorWithMessage",
                    "Error",
                    new { message = "You don't have permission to edit this comment!" }
                );
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
                return RedirectToAction(
                    "ErrorWithMessage",
                    "Error",
                    new { message = "You don't have permission to edit that comment!" }
                );
            }

            try
            {
                if (ModelState.IsValid)
                {
                    if (TryUpdateModel(comment))
                    {
                        comment.Content = requestComment.Content;

                        TempData["message"] = "The comment was modified!";
                        db.SaveChanges();
                    }

                    return RedirectToAction("Show", "Subject", new { id = comment.SubjectId });
                }
            }
            catch (Exception e) { }

            TempData["message"] = "Editing the comment failed. Please try again";
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
                Debug.Write(e.Message);
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
                return RedirectToAction(
                    "ErrorWithMessage",
                    "Error",
                    new { message = "You don't have permission to delete that comment!" }
                );
            }

            if (this.DeleteComment(db, id))
            {
                db.SaveChanges();
                TempData["message"] = "The comment was removed";
                return RedirectToAction("Show", "Subject", new { id = comment.SubjectId });
            }
            else
            {
                return RedirectToAction(
                    "ErrorWithMessage",
                    "Error",
                    new { message = "Failed to delete comment!" }
                );
            }
        }
    }
}