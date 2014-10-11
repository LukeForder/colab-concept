using ColabConcept.Web.Hubs;
using ColabConcept.Web.Infrastructure;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;
using SimpleInjector;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using System;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace ColabConcept.Web
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            Container container = new Container();

            container.Register<IProductStore, ProductStore>();
            container.Register<ProductHub>();

            SimpleInjectorHubActivator activator = new SimpleInjectorHubActivator(container);

            GlobalHost.DependencyResolver.Register(typeof(IHubActivator), () => activator);
            CookieAuthenticationOptions options = new CookieAuthenticationOptions();
            options.AuthenticationType = "COOKIE";
            options.ExpireTimeSpan = new TimeSpan(1, 0, 0, 0);
            options.LoginPath = new PathString("/login");
            options.SlidingExpiration = true;

            app.UseCookieAuthentication(options);

            app.MapSignalR();
            app.UseNancy();
            


        }
    }
}