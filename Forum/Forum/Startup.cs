using Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Forum.Startup))]
namespace Forum
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Se apeleaza o metoda in care se adauga contul de administrator si rolurile aplicatiei
            createAdminUserAndApplicationRoles();
        }

        //private void createAdminUserAndApplicationRoles()
        //{
        //    ApplicationDbContext context = new ApplicationDbContext();

        //    var roleManager = new RoleManager<IdentityRole>(new
        //    RoleStore<IdentityRole>(context));

        //    var UserManager = new UserManager<ApplicationUser>(new
        //    UserStore<ApplicationUser>(context));

        //    // Se adauga rolurile aplicatiei
        //    if (!roleManager.RoleExists("Administrator"))
        //    {
        //        // Se adauga rolul de administrator
        //        var role = new IdentityRole();
        //        role.Name = "Administrator";
        //        roleManager.Create(role);

        //        // se adauga utilizatorul administrator
        //        var user = new ApplicationUser();
        //        user.UserName = "admin@admin.com";
        //        user.Email = "admin@admin.com";

        //        var adminCreated = UserManager.Create(user, "Administrator1!");
        //        if (adminCreated.Succeeded)
        //        {
        //            UserManager.AddToRole(user.Id, "Administrator");
        //        }
        //    }

        //    if (!roleManager.RoleExists("Editor"))
        //    {
        //        var role = new IdentityRole();
        //        role.Name = "Editor";
        //        roleManager.Create(role);
        //    }

        //    if (!roleManager.RoleExists("User"))
        //    {
        //        var role = new IdentityRole();
        //        role.Name = "User";
        //        roleManager.Create(role);
        //    }
        //}

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

        private void createAdminUserAndApplicationRoles()
        {
            UserInfo[] info =
            {
                new UserInfo("User", "user@user.com", "User1!"),
                new UserInfo("Moderator", "mod@mod.com", "Moderator1!"),
                new UserInfo("Administrator", "admin@admin.com", "Administrator1!")
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
                    // Se adauga rolul de administrator
                    var role = new IdentityRole();
                    role.Name = info[i].role;
                    roleManager.Create(role);

                    // se adauga utilizatorul administrator
                    var user = new ApplicationUser();
                    user.UserName = info[i].email;
                    user.Email = info[i].email;

                    var adminCreated = UserManager.Create(user, info[i].parola);
                    if (adminCreated.Succeeded)
                    {
                        UserManager.AddToRole(user.Id, info[i].role);
                    }
                }
            }
        }
    }


}
