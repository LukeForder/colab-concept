using ColabConcept.Web.Infrastructure;
using Microsoft.AspNet.SignalR;
using Nancy.Conventions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColabConcept.Web
{
    public class Bootstrapper : Nancy.DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(Nancy.Conventions.NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

           nancyConventions.StaticContentsConventions.AddDirectory("scripts");
           nancyConventions.StaticContentsConventions.AddDirectory("fonts");
        }

        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register(typeof(JsonSerializer), typeof(CamelCaseJsonSerializer));
        }

        protected override void ConfigureRequestContainer(Nancy.TinyIoc.TinyIoCContainer container, Nancy.NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);


        }
    }
}