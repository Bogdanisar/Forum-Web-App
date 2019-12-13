using Forum.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;

namespace Forum.Controllers
{
    public class UpvoteSubjectController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        [Authorize(Roles = "Administrator,User,Moderator")]
        private string ChangeVote(int subjectId)
        {
            string userId = User.Identity.Name;

            var rows = from upvote in db.Upvote
                       where upvote.UserId == userId
                       where upvote.SubjectId == subjectId
                       select upvote;
            

            if (rows.Count() != 0)
            {
                foreach (var up in rows)
                {
                    db.Upvote.Remove(up);
                }
                db.SaveChanges();
                return "Vote removed :(";
            }
            else
            {
                UpvoteSubject us = new UpvoteSubject();
                us.UserId = userId;
                us.SubjectId = subjectId;
                db.Upvote.Add(us);
                db.SaveChanges();
                Debug.WriteLine("Succes");
                return "Vote added :)";
            }
        }

        [WebMethod]
        [Authorize(Roles = "Administrator,User,Moderator")]
        public string Clear()
        {
            var rows = from upvote in db.Upvote select upvote;
            foreach (var up in rows)
            {
                db.Upvote.Remove(up);
            }
            db.SaveChanges();

            return "Clear";
        }

        [WebMethod]
        [Authorize(Roles = "Administrator,User,Moderator")]
        public string Vote(string subjectId)
        {
            return ChangeVote(Convert.ToInt32(subjectId));
        }

        [WebMethod]
        [Authorize(Roles = "Administrator,User,Moderator")]
        public string Initialize(string subjectId)
        {
            string userId = User.Identity.Name;
            int subId = Convert.ToInt32(subjectId);

            var rows = from upvote in db.Upvote
                       where upvote.UserId == userId
                       where upvote.SubjectId == subId
                       select upvote;

            if (rows.Count() == 0)
            {
                return "Vote";
            }
            else
            {
                return "Unvote";
            }
        }
    }
}