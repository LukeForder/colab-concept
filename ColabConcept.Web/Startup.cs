using ColabConcept.Web.Infrastructure;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Owin;
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
            app.MapSignalR();
            app.UseNancy();
            


        }
    }
}