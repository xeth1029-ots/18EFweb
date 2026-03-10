using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Controllers
{
    public class GovAnnounceController : Controller
    {
        // GET: GovAnnounce
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            MyKeyMapDAO dao = new MyKeyMapDAO();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = string.Empty;

            // 查詢政府網站資訊開放宣告設定資訊
            ViewBag.GovAnnounce = dao.GetFuncContent("005");

            return View();
        }
    }
}