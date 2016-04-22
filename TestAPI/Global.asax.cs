using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using TestAPI.Repository;

namespace TestAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //GlobalConfiguration.Configure(WebApiConfig.Register);
            //ConfigureWebApiAutofac();
        }

        public void ConfigureWebApiAutofac()
        {

            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<TestRepository>().As<ITestRepository>().InstancePerRequest();
            containerBuilder.RegisterType<AuthRepository>().As<IAuthRepository>().InstancePerRequest();

            // REGISTER DEPENDENCIES
            //builder.RegisterType<ApplicationDbContext>().AsSelf().InstancePerRequest();
            //containerBuilder.RegisterType<UserRepository<ApplicationUser>>().As<IUserRepository<ApplicationUser>>().InstancePerRequest();
            //containerBuilder.RegisterType<ApplicationUserStore>().As<IUserStore<ApplicationUser>>().InstancePerRequest();
            //containerBuilder.RegisterType<ApplicationUserManager>().AsSelf().InstancePerRequest();
            //containerBuilder.RegisterType<ApplicationSignInManager>().AsSelf().InstancePerRequest();
            //containerBuilder.Register<IAuthenticationManager>(c => HttpContext.Current.GetOwinContext().Authentication).InstancePerRequest();
            //containerBuilder.Register<IDataProtectionProvider>(c => app.GetDataProtectionProvider()).InstancePerRequest();

            //containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerApiRequest();

            //containerBuilder.Register(c => new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ResourceManagerEntities())
            //{
            //    /*Avoids UserStore invoking SaveChanges on every actions.*/
            //    //AutoSaveChanges = false
            //})).As<UserManager<ApplicationUser>>().InstancePerApiRequest();

            containerBuilder.RegisterApiControllers(System.Reflection.Assembly.GetExecutingAssembly());
            IContainer container = containerBuilder.Build();
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }


}
