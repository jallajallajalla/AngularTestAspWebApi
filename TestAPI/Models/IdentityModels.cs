using AspNet.Identity.Dapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TestAPI.Repository.Providers;

namespace TestAPI.Models
{
    // You can add profile data for the AppMember by adding more properties to your AppMember class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class AppMember : IdentityMember
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<AppMember, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom AppMember claims here
            return userIdentity;
        }
    }

    /// <summary>
    /// Create and opens a connection to a MSSql database
    /// </summary>

    public class ApplicationDbContext : DbManager
    {
        public ApplicationDbContext(IConnectionStringProvider connectionStringProvider)
            : base(connectionStringProvider.DefaultConnection)
        {
        }

        //public static ApplicationDbContext Create()
        //{
        //    return new ApplicationDbContext(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        //}
    }

    public class ApplicationUserStore : UserStore<AppMember>
    {
        public ApplicationUserStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class ApplicationUserManager : UserManager<AppMember, int>
    {
        public ApplicationUserManager(IUserStore<AppMember, int> store)
            : base(store)
        {
        }

        //public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        //{
        //    var manager = new ApplicationUserManager(
        //       new UserStore<AppMember>(
        //           context.Get<ApplicationDbContext>() as DbManager));

        //    return manager;
        //}
    }
}
