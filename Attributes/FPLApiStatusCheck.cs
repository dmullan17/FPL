using FPL.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPL.Attributes
{
    public class FPLApiStatusCheck : ActionFilterAttribute
    {
        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            //response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    action = "Index",
                    controller = "Login"
                }));

                return;
            }
            //if (filterContext.HttpContext.Request.Cookies.Count == 0)
            //{
            //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            //    {
            //        action = "Index",
            //        controller = "Login"
            //    }));

            //    return;
            //}
            //else
            //{
            //    var plProfileCookie = filterContext.HttpContext.Request.Cookies["pl_profile"];

            //    if (plProfileCookie == null)
            //    {
            //        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            //        {
            //            action = "Index",
            //            controller = "Login"
            //        }));

            //        return;
            //    }
            //}
        }
    }
}
