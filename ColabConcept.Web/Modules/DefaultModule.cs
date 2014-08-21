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
using Nancy.Security;
using ColabConcept.Web.Infrastructure;
using Nancy.Authentication.Forms;
using Nancy.Owin;
using Microsoft.Owin.Security;
using System.Security.Claims;
using Nancy.Extensions;

namespace ColabConcept.Web.Modules
{
    public class DefaultModule : NancyModule
    {
        public DefaultModule(
            IUserStore userStore)
        {
            Get["/"] = (args) =>
                {
                    this.RequiresMSOwinAuthentication();

                    return View["index.html"];
                };

            Get["/login"] = (args) =>
                {
                    return View["login.html"];
                };

            Post["/login", true] = async (args, ct) =>
                {
                    string userName = Request.Form.userName;
                    if (string.IsNullOrEmpty(userName))
                    {
                        return View["login.html", new { Message = "User name can't be empty"  }];
                    }

                    if (await userStore.ExistsAsync(userName))
                    {
                        return View["login.html", new { Message = "User name already exists" }];
                    }

                    try
                    {
                        Guid id = await userStore.AddAsync(userName);
                        IAuthenticationManager authenticationManager = Context.GetAuthenticationManager();
                        authenticationManager.SignIn(
                            new ClaimsIdentity(
                                new Claim[] {
                                    new Claim(ClaimTypes.Name, userName)
                                },
                                "COOKIE"));

                        var returnUrl = Context.IsLocalUrl((string)Request.Query.ReturnUrl) ? (string)Request.Query.ReturnUrl : "/";

                        return Response.AsRedirect(returnUrl);
                    }
                    catch (Exception ex)
                    {
                        //not accurate but...
                        return View["login.html", new { Message = "User name already exists" }];
                    }
                };


        }
    }
}