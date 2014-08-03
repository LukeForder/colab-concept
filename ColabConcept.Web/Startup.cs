using ColabConcept.Web.Hubs;
using ColabConcept.Web.Infrastructure;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

            app.MapSignalR();
            app.UseNancy();
            


        }
    }
}