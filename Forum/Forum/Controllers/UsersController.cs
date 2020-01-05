using Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Users
        public ActionResult Index()
        {
            //var users = from user in db.Users
            //            orderby user.UserName
            //            select user;
            //ViewBag.UsersList = users;

            var list = db.Users.ToList();
            var deletedUser = list.Find(u => u.Id == UsersController.GetDeletedUser().Id);
            list.Remove(deletedUser);

            ViewBag.UsersList = list;
            return View();
        }



        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles select role;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        public ActionResult Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
            ViewBag.DeletedUserId = UsersController.GetDeletedUser().Id;

            var roles = db.Roles.ToList();
            var roleName = roles.Where(j => j.Id ==
               user.Roles.FirstOrDefault().RoleId).
               Select(a => a.Name).FirstOrDefault();
            ViewBag.roleName = roleName;;

            return View(user);
        }

        public ActionResult Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            ViewBag.AllRoles = GetAllRoles();
            ViewBag.currentUserId = User.Identity.GetUserId();

            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRoleId = userRole.RoleId;

            return View(user);
        }

        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = db.Users.Find(id);
            ViewBag.AllRoles = GetAllRoles();

            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


                if (TryUpdateModel(user))
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.PhoneNumber = newData.PhoneNumber;

                    var roles = db.Roles.ToList();
                    foreach (var role in roles)
                    {
                        UserManager.RemoveFromRole(id, role.Name);
                    }
                    
                    var selectedRole = db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                    UserManager.AddToRole(id, selectedRole.Name);

                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                return View(user);
            }
        }

        [HttpDelete]
        public ActionResult Delete(string id)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var user = UserManager.Users.FirstOrDefault(u => u.Id == id);
            var deletedUserId = UsersController.GetDeletedUser().Id;
            
            // remove the user, but keep their subjects: Change the subject's user from this one to the "deleted" user;
            var subjectsToRemove = db.Subjects.Where(s => s.UserId == id).ToList();
            foreach (var subject in subjectsToRemove)
            {
                subject.UserId = deletedUserId;
            }

            // the same as with subjects;
            var commentsToRemove = db.Comments.Where(c => c.UserId == id).ToList();
            foreach (var comment in commentsToRemove)
            {
                comment.UserId = deletedUserId;
            }

            // remove the subject upvotes of this user;
            var subjectUpvotesToRemove = db.SubjectUpvotes.Where(su => su.UserId == id).ToList();
            foreach (var subjectUpvote in subjectUpvotesToRemove)
            {
                db.SubjectUpvotes.Remove(subjectUpvote);
            }

            // remove the comment upvotes of this user;
            foreach (var comment in db.Comments.ToList())
            {
                comment.UpvotingUsers.Remove(user);
            }

            db.SaveChanges();
            UserManager.Delete(user);

            return RedirectToAction("Index");
        }






        public static string EmailDeleted
        {
            get { return "Deleted"; }
        }
        public static ApplicationUser GetDeletedUser()
        {
            ApplicationUser deletedUser = (new ApplicationDbContext()).Users.Where(u => u.Email == EmailDeleted).FirstOrDefault();
            return deletedUser;
        }
    }
}