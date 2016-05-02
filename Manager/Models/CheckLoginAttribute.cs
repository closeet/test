﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Manager.Models
{
    public class CheckLoginAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool pass = false;
            if (httpContext.Session["User"]==null)
            {
                httpContext.Response.StatusCode = 401;
            }
            else
            {
                pass = true;
            }
            return pass;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Response.StatusCode==401)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
        }
    }
}