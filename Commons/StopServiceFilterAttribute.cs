using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Commons
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class StopServiceFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string chkTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string stopTimeS = ConfigModel.StopTimeS;
            string stopTimeE = ConfigModel.StopTimeE;
            string StopUrl = ConfigModel.StopUrl;

            var req = filterContext.RequestContext.HttpContext.Request;
            var resp = filterContext.RequestContext.HttpContext.Response;

            if (!string.IsNullOrEmpty(stopTimeS) && !string.IsNullOrEmpty(stopTimeE))
            {
                if ((string.Compare(stopTimeS, chkTime) <= 0 && string.Compare(stopTimeE, chkTime) >= 0))
                {
                    if (req.IsAjaxRequest())
                    {
                        //for ajax process
                        resp.StatusCode = 404;
                        resp.End();
                    }
                    else
                    {
                        resp.Redirect(StopUrl);
                        resp.End();
                    }
                }
            }
        }
    }
}