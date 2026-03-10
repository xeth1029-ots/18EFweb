using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Controllers
{
    public class HeaderSearchController : BaseController
    {
        // GET: HeaderSearch
        public ActionResult Index()
        {
            HomeViewModel model = new HomeViewModel();
            model.Form.IsContainsOverEnter = "N"; //包含已截止報名課程(N:不含)
            model.Form.IsHomeSearch = "Y"; //是否為首頁查詢
            model.Form.PlanType = "2"; //預設查詢計畫類別

            return PartialView("Index", model);
        }

        /// <summary>
        /// 切換計畫別重取查詢條件
        /// </summary>
        /// <param name="PlanType"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetCondition(string PlanType, string keywords)
        {
            HomeViewModel model = new HomeViewModel();
            model.Form.IsContainsOverEnter = "N"; //包含已截止報名課程(N:不含)
            model.Form.IsHomeSearch = "Y"; //是否為首頁查詢
            model.Form.PlanType = PlanType; //查詢計畫類別
            model.Form.KEYWORDS = keywords; //查詢關鍵字搜尋

            return PartialView("_Condition", model);
        }
    }
}