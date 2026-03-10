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
    /// <summary>
    /// /DistClassSearch 課程查詢/分署課程列表
    /// </summary>
    public class DistClassSearchController : BaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: DistClass
        /// <summary>
        /// 查詢頁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;
            DistClassSearchViewModel model = new DistClassSearchViewModel();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            //第一次進入功能
            model.Form.IsFirst = "Y";

            //預設查詢條件
            model.Form.DISTID = "001"; //北分署
            model.Form.PLANTYPE = "2"; //在職進修

            ModelState.Clear();

            // 欄位檢核 OK, 處理查詢
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            // 設定查詢分頁資訊
            dao.SetPageInfo(model.Form.rid, model.Form.p);

            // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
            switch (model.Form.PLANTYPE)
            {
                case "1":
                    model.Grid1 = dao.QueryDistClassSearch<DistClassSearchGrid1Model>(model.Form);
                    break;
                case "2":
                    model.Grid2 = dao.QueryDistClassSearch<DistClassSearchGrid2Model>(model.Form);
                    break;
                case "5":
                    model.Grid5 = dao.QueryDistClassSearch<DistClassSearchGrid5Model>(model.Form);
                    break;
            }

            rtn = View("Index", model);

            if (!string.IsNullOrEmpty(model.Form.rid) && model.Form.useCache == 0)
            {
                //// 有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                //rtn = PartialView("_GridRows", model);

                // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                PagingViewModel pagingModel = new PagingViewModel();
                pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                pagingModel.PagingInfo = dao.PaginationInfo;
                rtn = Json(pagingModel);
            }

            // 設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model.Form, dao, "Index");

            return rtn;
        }

        /// <summary>查詢結果</summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(DistClassSearchFormModel form)
        {
            switch (form.PLANTYPE)
            {
                case "1"://產投
                case "2"://在職
                case "5"://區域產業據點
                    break;
                default:
                    //throw new ArgumentException("plantype 格式錯誤");
                    LOG.Error("DisClassSearchController post Index (403):  PLANTYPE 格式錯誤");
                    return new HttpStatusCodeResult(403);
                    //break;
            }

            if (string.IsNullOrEmpty(form.PLANTYPE))
            {
                //throw new ArgumentNullException("plantype 不可為 null");
                LOG.Error("DisClassSearchController post Index (403):  PLANTYPE 不可為 null");
                return new HttpStatusCodeResult(403);
            }
            if (string.IsNullOrEmpty(form.DISTID))
            {
                //throw new ArgumentNullException("distid 不可為 null");
                LOG.Error("DisClassSearchController post Index (403):  DISTID 格式錯誤");
                return new HttpStatusCodeResult(403);
            }
            if (!string.IsNullOrEmpty(form.ActiveMode))
            {
                switch (form.ActiveMode)
                {
                    case "1"://分署別
                        break;
                    case "2"://計畫別
                        break;
                    default:
                        LOG.Error("DisClassSearchController post Index (403):  ActiveMode 格式錯誤");
                        return new HttpStatusCodeResult(403);
                        //break;
                }
            }

            ActionResult rtn = null;
            DistClassSearchViewModel model = new DistClassSearchViewModel();
            model.Form = form;
            model.Form.IsFirst = "N";

            rtn = View("Index", model);

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                //欄位檢核 OK, 處理查詢
                WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                switch (form.PLANTYPE)
                {
                    case "1":
                        model.Grid1 = dao.QueryDistClassSearch<DistClassSearchGrid1Model>(form);
                        break;
                    case "2":
                        model.Grid2 = dao.QueryDistClassSearch<DistClassSearchGrid2Model>(form);
                        break;
                    case "5":
                        model.Grid5 = dao.QueryDistClassSearch<DistClassSearchGrid5Model>(form);
                        break;
                }

                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //// 有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                    //rtn = PartialView("_GridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }

            return rtn;
        }
    }
}