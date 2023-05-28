using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DMS_WebApplication.Models
{
    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            // Check if session is expired or not
            if (session != null && session.IsNewSession)
            {
                var cookie = filterContext.HttpContext.Request.Headers["Cookie"];

                // Check if a session cookie is present
                if ((cookie != null) && (cookie.IndexOf("ASP.NET_SessionId") >= 0))
                {
                    // Redirect to login page
                    filterContext.Result = new RedirectResult("~/Account/SignIn");
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}