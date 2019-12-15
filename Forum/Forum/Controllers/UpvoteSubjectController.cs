using Forum.Models;
using Newtonsoft.Json;
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

        private string GetVoteButtonStatus(string userId, int subjectId)
        {
            var rows = from upvote in db.Upvote
                       where upvote.UserId == userId
                       where upvote.SubjectId == subjectId
                       select upvote;

            if (rows.Count() == 0)
            {
                return "Vote";
            }
            return "Unvote";
        }

        private string GetVoteCount(int subjectId)
        {
            var rows = from upvote in db.Upvote
                       where upvote.SubjectId == subjectId
                       select upvote;

            return rows.Count().ToString();
        }
        
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
                return "Vote";
            }
            else
            {
                UpvoteSubject us = new UpvoteSubject();
                us.UserId = userId;
                us.SubjectId = subjectId;
                db.Upvote.Add(us);
                db.SaveChanges();
                return "Unvote";
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
            int subId = Convert.ToInt32(subjectId);
            var map = new Dictionary<string, string>();

            map.Add("VoteText", ChangeVote(subId));
            map.Add("VoteCount", GetVoteCount(subId));

            Debug.WriteLine(JsonConvert.SerializeObject(map));
            return JsonConvert.SerializeObject(map);
        }

        [WebMethod]
        [Authorize(Roles = "Administrator,User,Moderator")]
        public string Initialize(string subjectId)
        {
            string userId = User.Identity.Name;
            int subId = Convert.ToInt32(subjectId);

            var map = new Dictionary<string, string>();

            map.Add("VoteText", GetVoteButtonStatus(userId, subId));
            map.Add("VoteCount", GetVoteCount(subId));
            
            Debug.WriteLine(JsonConvert.SerializeObject(map));
            return JsonConvert.SerializeObject(map);
        }
    }
}