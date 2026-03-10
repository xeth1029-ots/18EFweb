using System;
using System.Collections;
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
    public class HistoryClassSearchController : BaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: HistoryClassSearch
        /// <summary>
        /// 查詢條件頁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            ClassSearchViewModel model = new ClassSearchViewModel();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            model.Form.PlanType = "2"; //預設查詢計畫類別
            model.Form.IsFirst = "Y";

            DateTime now = DateTime.Now;
            model.Form.STDATE_YEAR_SHOW = (now.AddMonths(-6).Year - 1911).ToString();
            model.Form.STDATE_MON = now.AddMonths(-6).Month.ToString();
            model.Form.FTDATE_YEAR_SHOW = (now.Year - 1911).ToString();
            model.Form.FTDATE_MON = now.Month.ToString();
            return base.SetPageNotFound(); // 歷史課程查詢下架 return View("Index", model);
        }

        /// <summary>
        /// 查詢結果清單頁
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ClassSearchFormModel form)
        {
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            ClassSearchViewModel model = new ClassSearchViewModel();
            model.Form = form;

            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };

            /* -------------------- 查詢條件檢核 start -------------------- */
            //查詢計畫類別
            if (string.IsNullOrEmpty(form.PlanType))
            {
                //throw new ArgumentNullException("plantype 不可為 null");
                LOG.Error("HistoryClassSearchController post Index (404):  plantype 不可為 null");
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }
            else if (!planTypeAry.Contains(form.PlanType))
            {
                //throw new ArgumentException("plantype 格式錯誤");
                LOG.Error("HistoryClassSearchController post Index (404):  plantype 格式錯誤");
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }

            Int32 iNum = 0;

            //上課地點
            if (form.CTID != null && !Int32.TryParse(form.CTID, out iNum))
            {
                LOG.Error("HistoryClassSearchController post Index (404):  CTID 格式錯誤");
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }

            //上課地點(分署)
            if (form.DISTID_SHOW != null)
            {

                foreach (string distid in form.DISTID_SHOW)
                {
                    if (!Int32.TryParse(distid, out iNum))
                    {
                        LOG.Error("HistoryClassSearchController post Index (404):  DISTID 格式錯誤");
                        //return new HttpStatusCodeResult(404);
                        return base.SetPageNotFound();
                    }
                }
            }

            DateTime dt = DateTime.Now;

            //訓練期間(起)-必填(無請選擇)
            if (form.STDATE_YEAR == null || !Int32.TryParse(form.STDATE_YEAR, out iNum)
                || form.STDATE_MON == null || !Int32.TryParse(form.STDATE_MON, out iNum)
                || !DateTime.TryParseExact((form.STDATE_YEAR + form.STDATE_MON.PadLeft(2, '0') + "01"), "yyyyMMdd", new CultureInfo("en-US"), DateTimeStyles.None, out dt))
            {
                string str_err_msg = "HistoryClassSearchController post Index (404):  訓練期間（起日）格式錯誤";
                str_err_msg += ".form.STDATE_YEAR:" + form.STDATE_YEAR;
                str_err_msg += ".form.STDATE_MON:" + form.STDATE_MON;
                LOG.Error(str_err_msg);
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }


            //訓練期間(迄)-必填(無請選擇)
            if (form.FTDATE_YEAR == null || !Int32.TryParse(form.FTDATE_YEAR, out iNum)
                || form.FTDATE_MON == null || !Int32.TryParse(form.FTDATE_MON, out iNum)
                || !DateTime.TryParseExact((form.FTDATE_YEAR + form.FTDATE_MON.PadLeft(2, '0') + "01"), "yyyyMMdd", new CultureInfo("en-US"), DateTimeStyles.None, out dt))
            {
                string str_err_msg = "HistoryClassSearchController post Index (404):  訓練期間（迄日）格式錯誤";
                str_err_msg += ".form.FTDATE_YEAR:" + form.FTDATE_YEAR;
                str_err_msg += ".form.FTDATE_MON:" + form.FTDATE_MON;
                LOG.Error(str_err_msg);
                //return new HttpStatusCodeResult(403);
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }

            string msgtitle = ("1".Equals(form.PlanType) ? "開訓日期區間" : "訓練期間");

            if (!string.IsNullOrEmpty(form.STDATE_YEAR) && !string.IsNullOrEmpty(form.STDATE_MON)
                && !string.IsNullOrEmpty(form.FTDATE_YEAR) && !string.IsNullOrEmpty(form.FTDATE_MON))
            {
                if (new TimeSpan(Convert.ToDateTime(form.FTDATE_YEAR + "/" + form.FTDATE_MON.PadLeft(2, '0') + "/" + "01").Ticks - Convert.ToDateTime(form.STDATE_YEAR + "/" + form.STDATE_MON.PadLeft(2, '0') + "/" + "01").Ticks).Days < 0)
                {
                    ModelState.AddModelError("DATE", msgtitle + "迄日不得小於起日。");
                }
            }
            /* -------------------- 查詢條件檢核 End -------------------- */

            rtn = base.SetPageNotFound(); // 歷史課程查詢下架 View("Index", model);
            if (model.Form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 欄位檢核 OK, 處理查詢
                WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);
                model.Form.IsHisSearch = "Y"; //註記為歷史查詢

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                // 提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
                switch (form.PlanType)
                {
                    case "1":
                        model.Grid1 = dao.QueryHistoryClassSearch<ClassSearchGrid1Model>(form);
                        break;
                    case "2":
                        model.Grid2 = dao.QueryHistoryClassSearch<ClassSearchGrid2Model>(form);
                        break;
                    case "5":
                        model.Grid3 = dao.QueryHistoryClassSearch<ClassSearchGrid3Model>(form);
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
                else
                {
                    rtn = View("SearchResults", model);

                    //有輸入關鍵字進行查詢時，要寫入關鍵字Log資料表
                    TblTB_KEYWORD_LOG log = null;
                    IList<TblTB_KEYWORD_LOG> logList = new List<TblTB_KEYWORD_LOG>();

                    if (!string.IsNullOrWhiteSpace(form.KEYWORDS))
                    {
                        foreach (var item in form.KEYWORDS_list)
                        {
                            log = new TblTB_KEYWORD_LOG
                            {
                                KWLTYPE = "3",
                                SUB_TYPEID = form.PlanType,
                                KEYWORD = item,
                                USERIP = Request.UserHostAddress,
                                SEARCHDATE = DateTime.Now
                            };

                            logList.Add(log);
                        }

                        //新增關鍵字查詢Log紀錄
                        dao.InsertKeyWordLog(logList);
                    }
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }
            return base.SetPageNotFound();// 歷史課程查詢下架 return rtn;
        }

        /// <summary>
        /// 政策性課程專區-Policy course area-查詢結果清單頁
        /// </summary>
        /// <returns></returns>
        public ActionResult ClassSch2(ClassSearchFormModel form)
        {
            string PlanType = "1";
            SessionModel sm = SessionModel.Get();
            ClassSearchViewModel model = new ClassSearchViewModel();
            //ClassSearchFormModel form = new ClassSearchFormModel();
            model.Form = form;
            model.Form.PlanType = PlanType;
            model.Form.IsPolicy = "Y";//政策性課程專區-Policy course area
            model.Form.IsHisSearch = "Y"; //註記為歷史查詢

            //A.唯開課日期區間改為：往前推半年，
            //現在是6月，則篩選開課日期區間1月~6月，
            //現在是5月，則篩選開課日期區間前一年度12月~5月，
            //.以此類推

            DateTime now = DateTime.Now;

            string strSTDATE = now.AddMonths(-6).ToString("yyyy/MM/dd"); //往前推半年
            string strFTDATE = now.ToString("yyyy/MM/dd"); //當日
            string strRocSYr = (now.AddMonths(-6).Year - 1911).ToString(); //往前推半年
            string strSMon = now.AddMonths(-6).ToString("MM"); //往前推半年
            string strRocEYr = (now.Year - 1911).ToString(); //當日-年
            string strEMon = now.ToString("MM"); //當日-月

            model.Form.STDATE = strSTDATE;
            model.Form.FTDATE = strFTDATE;
            model.Form.STDATE_YEAR_SHOW = strRocSYr;
            model.Form.STDATE_MON = strSMon;
            model.Form.FTDATE_YEAR_SHOW = strRocEYr;
            model.Form.FTDATE_MON = strEMon;

            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            //ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };
            // 欄位檢核 OK, 處理查詢
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            ActionResult rtn = View("Index", model);

            if (model.Form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);
                model.Form.IsHisSearch = "Y"; //註記為歷史查詢

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                model.Grid1 = dao.QueryHistoryClassSearch<ClassSearchGrid1Model>(form);

                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                    //rtn = PartialView("_GridRows", model);
                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }
                else
                {
                    rtn = View("SearchResults", model);
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }
            return rtn;
        }

        /// <summary>
        /// 更新查詢條件ajax (1:產投/2:在職/5:區域據點)
        /// </summary>
        /// <param name="PlanType">查詢類型</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PlanTypeChange(string PlanType)
        {
            ClassSearchViewModel model = new ClassSearchViewModel();
            model.Form.IsFirst = "N";
            model.Form.PlanType = PlanType;
            DateTime now = DateTime.Now;

            switch (PlanType)
            {
                case "1": //產投
                    model.Form.STDATE_YEAR_SHOW = (now.AddMonths(-6).Year - 1911).ToString();
                    model.Form.STDATE_MON = now.AddMonths(-6).Month.ToString();
                    model.Form.FTDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    model.Form.FTDATE_MON = now.Month.ToString();
                    break;
                case "2": //在職
                case "5": //區域據點
                    model.Form.STDATE_YEAR_SHOW = (now.AddMonths(-6).Year - 1911).ToString();
                    model.Form.STDATE_MON = now.AddMonths(-6).Month.ToString();
                    model.Form.FTDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    model.Form.FTDATE_MON = now.Month.ToString();
                    break;
            }

            return View("Index", model);
        }

    }
}