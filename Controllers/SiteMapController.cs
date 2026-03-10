using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Controllers
{
    public class SiteMapController : Controller
    {
        // GET: SiteMap
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = string.Empty;

            return View();
        }
    }
}