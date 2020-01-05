using Forum.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using Microsoft.AspNet.Identity;
using Forum.App_Start;

namespace Forum.Controllers
{
    [MessageFilter]
    public class UpvoteCommentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [NonAction]
        public bool GetVoteStatus(string userId, int commId)
        {
            Comment comment = db.Comments.Find(commId);
            List<ApplicationUser> users = comment.UpvotingUsers.Where(u => u.Id == userId).ToList();
            if (users.Count > 0)
            {
                return true;
            }

            return false;
        }

        [NonAction]
        public string GetVoteCount(int commId)
        {
            return db.Comments.Find(commId).UpvotingUsers.ToList().Count.ToString();
        }

        [NonAction]
        private bool ChangeVote(int commId)
        {
            try
            {
                Comment comment = db.Comments.Find(commId);
                if (comment == null)
                {
                    return false;
                }

                string userId = User.Identity.GetUserId();

                List<ApplicationUser> upvotingUsersList = db.Comments.Find(commId).UpvotingUsers.ToList();
                bool contains = false;
                foreach (ApplicationUser user in upvotingUsersList)
                {
                    if (user.Id == userId)
                    {
                        contains = true;
                        break;
                    }
                }

                if (contains)
                {
                    db.Comments.Find(commId).UpvotingUsers.Remove(upvotingUsersList.Find(u => u.Id == userId));
                    db.SaveChanges();
                    return false;
                }

                Debug.WriteLine("Found user");
                Debug.WriteLine(db.Users.Find(userId));
                db.Comments.Find(commId).UpvotingUsers.Add(db.Users.Find(userId));
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception thrown in ChanveVote()!: ${e}");
                return false;
            }
        }





        [WebMethod]
        public string Initialize(string CommentId)
        {
            try
            {
                var map = new Dictionary<string, string>();

                int commId = Convert.ToInt32(CommentId);
                map.Add("VoteCount", GetVoteCount(commId));

                if (Request.IsAuthenticated)
                {
                    string userId = User.Identity.GetUserId();
                    map.Add("VoteText", GetVoteStatus(userId, commId) ? "Unvote" : "Vote");
                }

                Debug.WriteLine(JsonConvert.SerializeObject(map));
                return JsonConvert.SerializeObject(map);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception thrown in UpvoteCommentController.Initialize()!: ${e}");

                var map = new Dictionary<string, string>();
                map.Add("VoteCount", "0");
                map.Add("VoteText", "Vote");

                return JsonConvert.SerializeObject(map);
            }
        }

        [WebMethod]
        [Authorize(Roles = "Administrator,User,Moderator")]
        public string Vote(string CommentId)
        {
            var map = new Dictionary<string, string>();

            int commId = Convert.ToInt32(CommentId);

            map.Add("VoteText", ChangeVote(commId) ? "Unvote" : "Vote");
            map.Add("VoteCount", GetVoteCount(commId));

            Debug.WriteLine(JsonConvert.SerializeObject(map));
            return JsonConvert.SerializeObject(map);
        }

        [WebMethod]
        [Authorize(Roles = "Administrator")]
        public string Clear(string CommentId)
        {
            try
            {
                int commId = Convert.ToInt32(CommentId);
                Comment comment = db.Comments.Find(commId);

                foreach (ApplicationUser user in comment.UpvotingUsers.ToList())
                {
                    comment.UpvotingUsers.Remove(user);
                }

                db.SaveChanges();


                var map = new Dictionary<string, string>();
                map.Add("VoteText", "Vote");
                map.Add("VoteCount", GetVoteCount(commId));

                Debug.WriteLine(JsonConvert.SerializeObject(map));
                return JsonConvert.SerializeObject(map);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception thrown in UpvoteCommentController.Clear()!: ${e}");

                var map = new Dictionary<string, string>();
                map.Add("VoteText", "Vote");
                map.Add("VoteCount", "0");

                return JsonConvert.SerializeObject(map);
            }
        }
    }
}