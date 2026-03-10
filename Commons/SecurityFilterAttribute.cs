using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace WDAIIP.WEB.Commons
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SecurityFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            NameValueCollection param = filterContext.RequestContext.HttpContext.Request.Params;

            foreach (var key in param.AllKeys)
            {
                string value = param[key];
                Regex patterns3 = new Regex("(timeout|sleep|shutdown)");

                if (patterns3.IsMatch(value) || patterns3.IsMatch(key))
                {
                    throw new ArgumentException("SecurityFilterAttribute");
                }
            }
        }

    }
}