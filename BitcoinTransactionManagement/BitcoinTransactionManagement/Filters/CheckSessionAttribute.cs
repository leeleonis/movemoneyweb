using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BitcoinTransactionManagement.Filters
{
    public class CheckSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool islogin = (context.HttpContext.Session.Contents["IsLogin"] == null) ? false : (bool)context.HttpContext.Session.Contents["IsLogin"];//檢查是否為正常登入

            //沒通過
            if (!islogin)
            {
                context.HttpContext.Session.Add("IsLogin", false);

                RouteValueDictionary redirectTargetDictionary = new RouteValueDictionary
                {
                    { "action", "Login" },
                    { "controller", "Home" }
                };

                context.Result = new RedirectToRouteResult(redirectTargetDictionary);
                return;
            }

            base.OnActionExecuting(context);
        }

        //清暫存(?
        public class NoCacheAttribute : System.Web.Mvc.ActionFilterAttribute
        {
            public override void OnResultExecuting(System.Web.Mvc.ResultExecutingContext filterContext)
            {
                filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);

                filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                filterContext.HttpContext.Response.Cache.SetNoStore();

                base.OnResultExecuting(filterContext);
            }
        }
    }
}