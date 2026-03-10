using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Controllers
{
    public class PrivacyController : Controller
    {
        // GET: Privacy
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            MyKeyMapDAO dao = new MyKeyMapDAO();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = string.Empty;

            // 查詢隱私權及安全政策設定資料
            ViewBag.Privacy = dao.GetFuncContent("004");

            return View();
        }
    }
}