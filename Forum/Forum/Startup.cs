using Forum.Controllers;
using Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System;
using System.Diagnostics;

[assembly: OwinStartupAttribute(typeof(Forum.Startup))]
namespace Forum
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Se apeleaza o metoda in care se adauga conturile si rolurile aplicatiei
            createAdminUserAndApplicationRoles();
        }

        public struct UserInfo
        {
            public string role, email, parola;
            public UserInfo(string _role, string _email, string _parola)
            {
                role = _role;
                email = _email;
                parola = _parola;
            }
        }

        private ApplicationDbContext db = new ApplicationDbContext(); //Add user information (name + bd) to db table
        private void createAdminUserAndApplicationRoles()
        {
            UserInfo[] info =
            {
                new UserInfo("User", "user@user.com", "User1!"),
                new UserInfo("Moderator", "mod@mod.com", "Moderator1!"),
                new UserInfo("Administrator", "admin@admin.com", "Administrator1!"),
                new UserInfo("Deleted", UsersController.EmailDeleted, "Deleted1!")
            };

            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new
            RoleStore<IdentityRole>(context));

            var UserManager = new UserManager<ApplicationUser>(new
            UserStore<ApplicationUser>(context));

            for (int i = 0; i < info.Length; ++i)
            {
                if (!roleManager.RoleExists(info[i].role))
                {
                    // se adauga rolul
                    var role = new IdentityRole();
                    role.Name = info[i].role;
                    roleManager.Create(role);
                    
                    // se adauga utilizatorul
                    var user = new ApplicationUser();
                    user.UserName = info[i].email;
                    user.Email = info[i].email;

                    var userCreated = UserManager.Create(user, info[i].parola);
                    if (userCreated.Succeeded)
                    {
                        UserManager.AddToRole(user.Id, info[i].role);
                    }

                    try
                    {
                        Profile tmp = new Profile();
                        tmp.ProfileId = user.Id;
                        tmp.Name = "Default";
                        tmp.Birthday = DateTime.Now;
                        db.Profiles.Add(tmp);
                        db.SaveChanges();
                    } catch(Exception e)
                    {
                        // User is already in DB
                    }
                }
            }
        }
    }


}
