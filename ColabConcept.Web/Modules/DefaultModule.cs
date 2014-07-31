using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.ModelBinding;
using System.Web.Caching;
using ColabConcept.Web.Models;
using Microsoft.AspNet.SignalR;
using ColabConcept.Web.Hubs;

namespace ColabConcept.Web.Modules
{
    public class DefaultModule : NancyModule
    {
        public DefaultModule()
        {
            Get["/"] = (args) =>
                {
                    IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ProductHub>();
                    
                    
                   
                    return View["index.html"];
                };


            Get["/products"] =
                (args) =>
                {
                    return
                        HttpContext
                        .Current
                        .Cache
                        .OfType<Product>()
                        .ToList();
                };


            Put["/products/{id:guid}"] =
                (args) =>
                {
                    return HttpStatusCode.NotImplemented;
                };
        }
    }
}