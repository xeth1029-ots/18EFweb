using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Commons.Filter
{
    /// <summary>
    /// 全域的 Action Filter 用來進行所有 action 執行的前置處理作業
    /// </summary>
    public class CustomActionFilter: ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            /*
            string actionPath = ControllerContextHelper.GetActionPath(filterContext);

            SessionModel sm = SessionModel.Get();
            sm.LastActionPath = actionPath;
            */

            base.OnActionExecuting(filterContext);
        }


    }
}