using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using log4net;

namespace WDAIIP.WEB.Controllers
{
    public class KeepUseClassSearchController : BaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // 顯示「適用留用外國中階技術工作人力」課程清單 GET: KeepUseClassSearch 
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            KeepUseClassSearchViewModel model = new KeepUseClassSearchViewModel();

            //設定所在主功能表位置- 所屬主功能表：課程查詢報名
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            //是否為第一次進入此功能 (Y 是, N 否)
            model.Form.IsFirst = "Y";
            //條件類別 (1:適用留用外國中階技術工作人力 )'
            model.Form.KeepUseType = "1"; //預設查詢-條件類別 
            //計畫類別（1 產投 , 2 在職 , 5 區域據點）
            model.Form.PlanType = "1"; //預設查詢-計畫類別

            //return View("Index", model);
            return Index(model.Form);
        }

        [HttpPost]
        public ActionResult Index(KeepUseClassSearchFormModel form)
        {
            if (form != null && !form.CheckArgument())
            {
                return new HttpStatusCodeResult(403);
            }

            KeepUseClassSearchViewModel model = new KeepUseClassSearchViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            model.Form = form;
            model.Form.IsFirst = "N";

            //查詢類別 (1:適用留用外國中階技術工作人力 )'
            //string x = model.Form.KeepUseType;
            //string x = model.Form.PlanType;

            ActionResult rtn = View(model);

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(model.Form.rid, model.Form.p);

                switch (model.Form.PlanType)
                {
                    case "1":
                        model.Form.TPLANID = "28";
                        model.Grid1 = dao.QueryKeepUseClassSearch(model.Form);
                        break;
                }

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //rtn = PartialView("_GridRows", model);
                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                //設定分頁元件(_PagingLink partial view)所需的資訊
                //base.SetPagingParams(model.Form, dao, "Index", "ajaxCallback");
                base.SetPagingParams(form, dao, "Index");
            }
            return rtn;
        }

       
    }
}