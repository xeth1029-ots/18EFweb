using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using log4net;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// /GovClassSearch 其他政府單位職訓課程
    /// </summary>
    public class GovClassSearchController : BaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: GovClassSearch
        /// <summary>
        /// 查詢頁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            GovClassSearchViewModel model = new GovClassSearchViewModel();
            DateTime aNow = DateTime.Now;
            DateTime defaultSDate = aNow;
            DateTime defaultEDate = aNow.AddMonths(4); //預設查五個月內的課程

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            //預設查詢條件
            model.Form.STDATE_YEAR = defaultSDate.Year.ToString();
            model.Form.STDATE_MON = string.Format("{0:0#}", defaultSDate.Month);
            model.Form.FTDATE_YEAR = defaultEDate.Year.ToString();
            model.Form.FTDATE_MON = string.Format("{0:0#}", defaultEDate.Month);

            return View(model);
        }

        /// <summary>
        /// 查詢結果
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(GovClassSearchFormModel form)
        {
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            GovClassSearchViewModel model = new GovClassSearchViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO("sqlMapETrainTrans.config");
            model.Form = form;

            Int32 iNum = 0;
            DateTime dt = DateTime.Now;

            if (form.STDATE_YEAR == null || !Int32.TryParse(form.STDATE_YEAR, out iNum)
                || form.STDATE_MON == null || !Int32.TryParse(form.STDATE_MON, out iNum)
                || !DateTime.TryParseExact((form.STDATE_YEAR + form.STDATE_MON + "01"), "yyyyMMdd", new CultureInfo("en-US"), DateTimeStyles.None, out dt))
            {
                LOG.Error("GovClassSearchController Index STDATE_YEAR("+ Convert.ToString(form.STDATE_YEAR) + ") or STDATE_MON("+ Convert.ToString(form.STDATE_MON) + ") is not valid!!");
                return new HttpStatusCodeResult(403);
            }

            if (form.FTDATE_YEAR == null || !Int32.TryParse(form.FTDATE_YEAR, out iNum)
                || form.FTDATE_MON == null || !Int32.TryParse(form.FTDATE_MON, out iNum)
                || !DateTime.TryParseExact((form.FTDATE_YEAR + form.FTDATE_MON + "01"), "yyyyMMdd", new CultureInfo("en-US"), DateTimeStyles.None, out dt))
            {
                LOG.Error("GovClassSearchController Index FTDATE_YEAR(" + Convert.ToString(form.FTDATE_YEAR) + ") or FTDATE_MON(" + Convert.ToString(form.FTDATE_MON) + ") is not valid!!");
                return new HttpStatusCodeResult(403);
            }

            model.Valid(ModelState);
            if (!ModelState.IsValid)
            {
                rtn = View("Index", model);
            }
            else
            {
                rtn = View("SearchResults", model);
                if (model.Form.useCache > 0 || ModelState.IsValid)
                {
                    ModelState.Clear();

                    // 欄位檢核 OK, 處理查詢
                    // 設定查詢分頁資訊
                    dao.SetPageInfo(form.rid, form.p);

                    model.Grid = dao.QueryGovClassSearch(form);

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
            }
            
            return rtn;
        }

        /// <summary>
        /// 課程明細頁
        /// </summary>
        /// <param name="cpid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Detail(Int64? cpid)
        {
            if (!cpid.HasValue)
            {
                LOG.Error("GovClassSearch Detail ex： cpid is null");
                return new HttpStatusCodeResult(403);
            }

            SessionModel sm = SessionModel.Get();
            GovClassSearchViewModel model = new GovClassSearchViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO("sqlMapETrainTrans.config");

            GovClassSearchDetailModel detail = dao.GetGovClassSearch(cpid.Value);          
            if (detail == null)
            {
                sm.LastErrorMessage = "找不到指定的資料";
                detail = new GovClassSearchDetailModel();
            }

            model.Detail = detail;

            return View("Detail", model);
        }
    }
}