using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using Turbo.DataLayer;
using WDAIIP.WEB.DataLayers;
using Turbo.Commons;
using log4net;
using WDAIIP.WEB.Commons.Filter;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Web.Script.Serialization;

namespace WDAIIP.WEB.Controllers
{
    public class ClassSearchController : BaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string strSID = ConfigModel.SSOSystemID;

        // GET: ClassSearch
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }

            //[ValidateAntiForgeryToken]   // 搭配檢視畫面（View）的Razor表單裡面，「@Html.AntiForgeryToken()」這句話以避免CSRF攻擊！！
            SessionModel sm = SessionModel.Get();
            ClassSearchViewModel model = new ClassSearchViewModel();
            DateTime now = DateTime.Now;
            //DateTime dateS = now.AddMonths(-1);
            //經討論開訓日期區間起改為預設前一個月，才會查到資料(因報名迄日設比開始起日晚)
            //DateTime dateS = DateTime.Now;
            //DateTime dateE = DateTime.Now; 

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };

            //預設顯示計畫別查詢條件
            string strPlanType = "2";
            model.Form.PlanType = string.IsNullOrEmpty(model.Form.PlanType) ? strPlanType : model.Form.PlanType;

            //if (form.PlanType != "1" && form.PlanType != "2" )
            if (!planTypeAry.Contains(model.Form.PlanType))
            {
                //throw new ArgumentException("ClassSearchController.Index()");
                //return new HttpStatusCodeResult(404);
                LOG.Debug("[ALERT] 1-ClassSearchController.Index()");
                return base.SetPageNotFound();
            }

            string s_ErrorPage1 = MyCommonUtil.Utl_GetConfigSet("ErrorPage1");
            s_ErrorPage1 = string.Format(",{0},", s_ErrorPage1);
            if (s_ErrorPage1.Contains(",ClassSearch,")) { LOG.Warn("##[ErrorPage1] 1-ClassSearchController.Index() SetPageNotFound"); return base.SetPageNotFound(); }

            //經討論開訓日期區間起改為預設前一個月，才會查到資料(因報名迄日設比開始起日晚)
            //效能調整，只查3個月就好
            string dateSYr = (now.AddMonths(-1).Year - 1911).ToString();
            string dateSMon = now.AddMonths(-1).Month.ToString();
            string dateEYr = (now.AddMonths(3).Year - 1911).ToString();
            string dateEMon = now.AddMonths(3).Month.ToString();

            switch (model.Form.PlanType)
            {
                case "1":
                    //產投
                    //2019-01-30 問題7:開訓日期區間預設選取的邏輯（以當下日期為依據）
                    //當日介於１~６月為上半年：查詢區間預設->前1年12月 ~ 當年6月
                    //當日介於７～１２月為下半年：查詢區間預設->當年6月 ~ 當年12月
                    //經討論開訓日期區間起改為預設前一個月，才會查到資料(因報名迄日設比開始起日晚)
                    //效能調整，只查3個月就好

                    model.Form.STDATE_YEAR_SHOW = dateSYr;
                    model.Form.STDATE_MON = dateSMon;
                    model.Form.FTDATE_YEAR_SHOW = dateEYr;
                    model.Form.FTDATE_MON = dateEMon;
                    break;

                case "2": //在職
                    model.Form.IsContainsOverEnter = "N"; //搜尋結果調整為僅報名中及即將開放報名的課程 20241021
                    model.Form.STDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    model.Form.STDATE_MON = now.Month.ToString();
                    model.Form.FTDATE_YEAR_SHOW = (now.AddMonths(3).Year - 1911).ToString();
                    model.Form.FTDATE_MON = now.AddMonths(3).Month.ToString();
                    break;

                case "5": //區域據點
                    //model.Form.IsContainsOverEnter = "Y";//2019-05-01 add 是否包含已截止報名課程(預設是)
                    model.Form.IsContainsOverEnter = "N"; //搜尋結果調整為僅報名中及即將開放報名的課程 20241021
                    model.Form.STDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    model.Form.STDATE_MON = now.Month.ToString();
                    model.Form.FTDATE_YEAR_SHOW = (now.AddMonths(3).Year - 1911).ToString();
                    model.Form.FTDATE_MON = now.AddMonths(3).Month.ToString();
                    break;

            }

            model.Form.IsFirst = "Y"; //第1次查詢
            return View("Index", model);
        }

        /// <summary>修改查詢條件</summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModifyQuery(string qPlanType, string qProvideLocation, string qSELOCD28, string qIsContainsOverEnter, string qOCID, string qCJOBNO,
            string qKEYWORDS, string qCLASSCNAME, string qABILITYS, string qSTDATE_YEAR_SHOW, string qSTDATE_MON, string qFTDATE_YEAR_SHOW, string qFTDATE_MON)
        {
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }

            //[ValidateAntiForgeryToken]   // 搭配檢視畫面（View）的Razor表單裡面，「@Html.AntiForgeryToken()」這句話以避免CSRF攻擊！！
            SessionModel sm = SessionModel.Get();
            ClassSearchViewModel model = new ClassSearchViewModel();
            DateTime now = DateTime.Now;
            //DateTime dateS = now.AddMonths(-1);
            //經討論開訓日期區間起改為預設前一個月，才會查到資料(因報名迄日設比開始起日晚)
            //DateTime dateS = DateTime.Now; //DateTime dateE = DateTime.Now; 

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };

            //預設顯示計畫別查詢條件
            //string strPlanType = "2";
            model.Form.PlanType = qPlanType;//string.IsNullOrEmpty(model.Form.PlanType) ? qPlanType : model.Form.PlanType;
            //if (form.PlanType != "1" && form.PlanType != "2" )
            if (!planTypeAry.Contains(model.Form.PlanType))
            {
                LOG.Debug("[ALERT] 1-ClassSearchController.Index()");
                return base.SetPageNotFound();
            }

            //string s_ErrorPage1 = MyCommonUtil.Utl_GetConfigSet("ErrorPage1");//s_ErrorPage1 = string.Format(",{0},", s_ErrorPage1);
            //if (s_ErrorPage1.Contains(",ClassSearch,")) { LOG.Warn("##[ErrorPage1] 1-ClassSearchController.Index() SetPageNotFound"); return base.SetPageNotFound(); }

            model.Form.ProvideLocation = qProvideLocation;//是否提供上課距離("Y":是、"N":否)
            long o_OCID = 0;
            if (!string.IsNullOrEmpty(qOCID) && long.TryParse(qOCID, out o_OCID)) { model.Form.OCID = o_OCID; }
            model.Form.CJOBNO = string.IsNullOrEmpty(qCJOBNO) ? null : qCJOBNO;
            model.Form.KEYWORDS = string.IsNullOrEmpty(qKEYWORDS) ? null : qKEYWORDS;
            model.Form.CLASSCNAME = string.IsNullOrEmpty(qCLASSCNAME) ? null : qCLASSCNAME;
            model.Form.ABILITYS = string.IsNullOrEmpty(qABILITYS) ? null : qABILITYS;

            string sSTDATE_YEAR_SHOW = (now.Year - 1911).ToString();//Year
            string sSTDATE_MON = now.Month.ToString();//Month
            string sFTDATE_YEAR_SHOW = (now.AddMonths(3).Year - 1911).ToString();//Year
            string sFTDATE_MON = now.AddMonths(3).Month.ToString();//Month

            model.Form.STDATE_YEAR_SHOW = string.IsNullOrEmpty(qSTDATE_YEAR_SHOW) ? sSTDATE_YEAR_SHOW : qSTDATE_YEAR_SHOW;
            model.Form.STDATE_MON = string.IsNullOrEmpty(qSTDATE_MON) ? sSTDATE_MON : qSTDATE_MON;
            model.Form.FTDATE_YEAR_SHOW = string.IsNullOrEmpty(qFTDATE_YEAR_SHOW) ? sFTDATE_YEAR_SHOW : qFTDATE_YEAR_SHOW;
            model.Form.FTDATE_MON = string.IsNullOrEmpty(qFTDATE_MON) ? sFTDATE_MON : qFTDATE_MON;

            //經討論開訓日期區間起改為預設前一個月，才會查到資料(因報名迄日設比開始起日晚) //效能調整，只查3個月就好
            //string dateSYr = model.Form.STDATE_YEAR_SHOW;// (now.AddMonths(-1).Year - 1911).ToString();
            //string dateSMon = model.Form.STDATE_MON;// now.AddMonths(-1).Month.ToString();
            //string dateEYr = model.Form.FTDATE_YEAR_SHOW;// (now.AddMonths(3).Year - 1911).ToString();
            //string dateEMon = model.Form.FTDATE_MON;// now.AddMonths(3).Month.ToString();

            switch (model.Form.PlanType)
            {
                case "1":
                    //產投 //2019-01-30 問題7:開訓日期區間預設選取的邏輯（以當下日期為依據）
                    //當日介於１~６月為上半年：查詢區間預設->前1年12月 ~ 當年6月 //當日介於７～１２月為下半年：查詢區間預設->當年6月 ~ 當年12月
                    //經討論開訓日期區間起改為預設前一個月，才會查到資料(因報名迄日設比開始起日晚) //效能調整，只查3個月就好
                    model.Form.IsContainsOverEnter = qIsContainsOverEnter;
                    //model.Form.STDATE_YEAR_SHOW = dateSYr;
                    //model.Form.STDATE_MON = dateSMon;
                    //model.Form.FTDATE_YEAR_SHOW = dateEYr;
                    //model.Form.FTDATE_MON = dateEMon;
                    break;

                case "2": //在職
                    model.Form.IsContainsOverEnter = "N"; //搜尋結果調整為僅報名中及即將開放報名的課程 20241021
                    //model.Form.STDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    //model.Form.STDATE_MON = now.Month.ToString();
                    //model.Form.FTDATE_YEAR_SHOW = (now.AddMonths(3).Year - 1911).ToString();
                    //model.Form.FTDATE_MON = now.AddMonths(3).Month.ToString();
                    break;

                case "5": //區域據點
                    //model.Form.IsContainsOverEnter = "Y";//2019-05-01 add 是否包含已截止報名課程(預設是)
                    model.Form.IsContainsOverEnter = "N"; //搜尋結果調整為僅報名中及即將開放報名的課程 20241021
                    //model.Form.STDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    //model.Form.STDATE_MON = now.Month.ToString();
                    //model.Form.FTDATE_YEAR_SHOW = (now.AddMonths(3).Year - 1911).ToString();
                    //model.Form.FTDATE_MON = now.AddMonths(3).Month.ToString();
                    break;

            }
            model.Form.IsFirst = "Y"; //第1次查詢
            return View("Index", model);
        }

        /// <summary> 通俗職業代碼-2019-項目清單 </summary>
        /// <param name="cjob_no"></param>
        /// <returns></returns>
        public ActionResult GetJobSearchList(string cjob_no)
        {
            if (cjob_no == null) { return base.SetPageNotFound(); }
            if (cjob_no.Length != 6) { return base.SetPageNotFound(); }
            if (!MyCommonUtil.isUnsignedInt(cjob_no)) { return base.SetPageNotFound(); }

            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            string PlanType = "2";

            ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            ClassSearchFormModel form = new ClassSearchFormModel();

            SessionModel sm = SessionModel.Get();
            ClassSearchViewModel model = new ClassSearchViewModel();
            model.Form = form;

            model.Form.PlanType = PlanType;
            model.Form.IsFirst = "N"; //第1次查詢

            //model.Form.IsContainsOverEnter = "N";//2019-01-30 add 是否包含已截止報名課程(是Y/否N)
            model.Form.IsContainsOverEnter = "N"; //搜尋結果調整為僅報名中及即將開放報名的課程 20241021

            DateTime now = DateTime.Now;
            //經討論開訓日期區間起改為預設前一個月，才會查到資料(因報名迄日設比開始起日晚)
            //查6個月就好
            model.Form.STDATE_YEAR_SHOW = (now.Year - 1911).ToString();//Year
            model.Form.STDATE_MON = now.Month.ToString();//Month
            model.Form.FTDATE_YEAR_SHOW = (now.AddMonths(6).Year - 1911).ToString();//Year
            model.Form.FTDATE_MON = now.AddMonths(6).Month.ToString();//Month

            //通俗職業代碼-2019-項目清單
            model.Form.CJOBNO = cjob_no;

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            //queryClassSearch_2
            model.Grid2 = dao.QueryClassSearch<ClassSearchGrid2Model>(form);

            ActionResult rtn = View("SearchResults", model);

            //設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(form, dao, "Index");

            model.Form = form;
            byte[] byarr = ObjectToByteArray(model);
            Session["LastModel"] = Convert.ToBase64String(byarr);
            Session["rid"] = dao.ResultID;
            return rtn;
            //return View("Index", model);
        }

        /// <summary> 政策性課程專區-Policy course area-查詢結果清單頁 </summary>
        /// <returns></returns>
        public ActionResult ClassSch2(ClassSearchFormModel form)
        {

            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            ClassSearchViewModel model = new ClassSearchViewModel();
            //ClassSearchFormModel form = new ClassSearchFormModel();
            string PlanType = "1";
            model.Form = form;
            model.Form.PlanType = PlanType;
            //model.Form.IsFirst = "N"; //第1次查詢
            model.Form.IsPolicy = "Y";//政策性課程專區-Policy course area
            model.Form.CASETYPE = "0";
            //model.Form.IsContainsOverEnter = "Y";//2019-05-01 add 是否包含已截止報名課程(預設是)
            model.Form.IsContainsOverEnter = "N"; //搜尋結果調整為僅報名中及即將開放報名的課程 20241021
            //model.Form.IsHomeSearch = "N";

            //當日介於1月~6月 為上半年>> 撈取前一年12月~當年度6月
            //當日介於 7月~12月為下半年 >> 撈取當年度6月~當年度12月
            //Int64? iM64 = Convert.ToInt64(DateTime.Now.ToString("MM"));

            DateTime now = DateTime.Now;
            string strYears2 = now.ToString("yyyy"); //當年度
            //篩選開課日期：為 當年度
            string strSTDATE = string.Concat(strYears2, "/01/01"); //當年度
            string strFTDATE = string.Concat(strYears2, "/12/31"); //當年度

            model.Form.STDATE = strSTDATE;
            model.Form.FTDATE = strFTDATE;
            model.Form.STDATE_YEAR_SHOW = (now.Year - 1911).ToString();
            model.Form.STDATE_MON = "1";
            model.Form.FTDATE_YEAR_SHOW = (now.Year - 1911).ToString();
            model.Form.FTDATE_MON = "12";

            SessionModel sm = SessionModel.Get();
            //ClassSearchViewModel model = new ClassSearchViewModel();
            //model.Form = form;

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            ActionResult rtn = View("Index", model);
            //判斷是否為首頁進入
            if (!string.IsNullOrEmpty(form.IsHomeSearch) && form.IsHomeSearch != "Y")
            {
                ModelState.AddModelError("IsHomeSearch", "請由正確入口進入！");
            }

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 欄位檢核 OK, 處理查詢
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                switch (form.PlanType)
                {
                    case "1": //產投
                        Session["ClassSearch.SortField.Form"] = form;
                        object field = Session["ClassSearch.SortField.FieldName"];
                        object desc = Session["ClassSearch.SortField.Desc"];
                        if (field != null) { form.ORDERBY = (string)field; }
                        if (desc != null) { form.ORDERDESC = (string)desc; }

                        model.Grid1 = dao.QueryClassSearch<ClassSearchGrid1Model>(form);
                        break;
                }

                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                    // rtn = PartialView("_GridRows", model);
                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                    model.Form.PagingInfo = dao.PaginationInfo;
                }
                else
                {
                    //取得目前(DB)系統時間
                    MyKeyMapDAO keyDao = new MyKeyMapDAO();
                    DateTime nowTime = keyDao.GetSysDateNow();

                    //2019-02-11 問題9:add if ，修正執行批次課程收藏後仍會一直顯示下方提示訊息的問題 
                    if (string.IsNullOrEmpty(form.IsSearched))
                    {
                        //設置特殊提示訊息
                        sm.LastResultMessage = "「報名日期」會依訓練單位變更「預定訓練起迄日期」而提前或延後，請密切留意，以免錯過報名時間。<br/><br/>目前系統時間為(" + nowTime.ToString("yyyy/MM/dd HH:mm:ss") + ")";
                        form.IsSearched = "Y";
                    }

                    rtn = View("SearchResults", model);
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }

            model.Form = form;
            byte[] byarr = ObjectToByteArray(model);
            Session["LastModel"] = Convert.ToBase64String(byarr);
            Session["rid"] = dao.ResultID;
            return rtn;

        }

        /// <summary>查詢</summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index(ClassSearchFormModel form)
        {
            if (form == null) { return base.SetPageNotFound(); }

            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }

            //[ValidateAntiForgeryToken]   // 搭配檢視畫面（View）的Razor表單裡面，「@Html.AntiForgeryToken()」這句話以避免CSRF攻擊！！
            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };

            int i_PlanType = -1;
            bool flag_PlanType_ok = Int32.TryParse(form.PlanType, out i_PlanType);
            if (flag_PlanType_ok) { form.PlanType = i_PlanType.ToString(); }

            if (!flag_PlanType_ok)
            {
                //throw new ArgumentException("ClassSearchController.Index()");
                //return new HttpStatusCodeResult(404);
                LOG.Warn("[ALERT] 3-ClassSearchController.Index()");
                return base.SetPageNotFound();
            }
            if (!planTypeAry.Contains(form.PlanType))
            {
                //throw new ArgumentException("ClassSearchController.Index()");
                //return new HttpStatusCodeResult(404);
                LOG.Debug("[ALERT] 1-ClassSearchController.Index()");
                return base.SetPageNotFound();
            }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SessionModel sm = SessionModel.Get();
            ClassSearchViewModel model = new ClassSearchViewModel();
            //【就業通】職訓專長能力標籤斷字分詞
            form.ABILITYS = dao.GetSegmentkeyword(form.ABILITYS);

            model.Form = form;
            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            ActionResult rtn = View("Index", model);
            //判斷是否為首頁進入
            if (!string.IsNullOrEmpty(form.IsHomeSearch) && form.IsHomeSearch != "Y")
            {
                ModelState.AddModelError("IsHomeSearch", "請由正確入口進入！");
            }

            string st1 = "";
            string ft1 = "";
            switch (form.PlanType)
            {
                case "1": //產投(28)
                    if (string.IsNullOrEmpty(form.IsHomeSearch))
                    {
                        if (form.CASETYPE == "0")
                        {
                            if (string.IsNullOrEmpty(form.STDATE_YEAR))
                                ModelState.AddModelError("STYEAR", "開訓日期間-起日-年份 為必填欄位。");
                            if (string.IsNullOrEmpty(form.STDATE_MON))
                                ModelState.AddModelError("STMON", "開訓日期間-起日-月份 為必填欄位。");
                            if (string.IsNullOrEmpty(form.FTDATE_YEAR))
                                ModelState.AddModelError("FTYEAR", "開訓日期間-迄日-年份 為必填欄位。");
                            if (string.IsNullOrEmpty(form.FTDATE_MON))
                                ModelState.AddModelError("FTMON", "開訓日期間-迄日-月份 為必填欄位。");

                            if ((ModelState.IsValid) && !string.IsNullOrEmpty(form.STDATE_YEAR) && !string.IsNullOrEmpty(form.STDATE_MON))
                            {
                                st1 = $"{form.STDATE_YEAR}/{form.STDATE_MON}/01";
                                bool bl_st1 = MyCommonUtil.IsDate(st1);
                                if (!bl_st1) { ModelState.AddModelError("DATE", "訓練期間-起日有誤。" + st1); }
                            }
                            if ((ModelState.IsValid) && !string.IsNullOrEmpty(form.FTDATE_YEAR) && !string.IsNullOrEmpty(form.FTDATE_MON))
                            {
                                ft1 = $"{form.FTDATE_YEAR}/{form.FTDATE_MON}/01";
                                bool bl_ft1 = MyCommonUtil.IsDate(ft1);
                                if (!bl_ft1) { ModelState.AddModelError("DATE", "訓練期間-迄日有誤。" + ft1); }
                            }

                            if (ModelState.IsValid)
                            {
                                if (new TimeSpan(Convert.ToDateTime(ft1).Ticks - Convert.ToDateTime(st1).Ticks).Days < 0)
                                {
                                    ModelState.AddModelError("DATE", "開訓日期區間迄日不得小於起日。");
                                }
                            }
                        }
                    }
                    break;

                case "2": //在職(06)
                    if ((ModelState.IsValid) && !string.IsNullOrEmpty(form.STDATE_YEAR) && !string.IsNullOrEmpty(form.STDATE_MON))
                    {
                        st1 = $"{form.STDATE_YEAR}/{form.STDATE_MON}/01";
                        bool bl_st1 = MyCommonUtil.IsDate(st1);
                        if (!bl_st1) { ModelState.AddModelError("DATE", "訓練期間-起日有誤。" + st1); }
                    }
                    if ((ModelState.IsValid) && !string.IsNullOrEmpty(form.FTDATE_YEAR) && !string.IsNullOrEmpty(form.FTDATE_MON))
                    {
                        ft1 = $"{form.FTDATE_YEAR}/{form.FTDATE_MON}/01";
                        bool bl_ft1 = MyCommonUtil.IsDate(ft1);
                        if (!bl_ft1) { ModelState.AddModelError("DATE", "訓練期間-迄日有誤。" + ft1); }
                    }
                    break;

                case "5": //區域據點(70)
                    if (string.IsNullOrEmpty(form.IsHomeSearch))
                    {
                        if (string.IsNullOrEmpty(form.STDATE_YEAR))
                            ModelState.AddModelError("STYEAR", "訓練期間-起日-年份 為必填欄位。");
                        if (string.IsNullOrEmpty(form.STDATE_MON))
                            ModelState.AddModelError("STMON", "訓練期間-起日-月份 為必填欄位。");
                        if (string.IsNullOrEmpty(form.FTDATE_YEAR))
                            ModelState.AddModelError("FTYEAR", "訓練期間-迄日-年份 為必填欄位。");
                        if (string.IsNullOrEmpty(form.FTDATE_MON))
                            ModelState.AddModelError("FTMON", "訓練期間-迄日-月份 為必填欄位。");

                        if ((ModelState.IsValid) && !string.IsNullOrEmpty(form.STDATE_YEAR) && !string.IsNullOrEmpty(form.STDATE_MON))
                        {
                            st1 = $"{form.STDATE_YEAR}/{form.STDATE_MON}/01";
                            bool bl_st1 = MyCommonUtil.IsDate(st1);
                            if (!bl_st1) { ModelState.AddModelError("DATE", $"訓練期間-起日有誤。{st1}"); }
                        }
                        if ((ModelState.IsValid) && !string.IsNullOrEmpty(form.FTDATE_YEAR) && !string.IsNullOrEmpty(form.FTDATE_MON))
                        {
                            ft1 = $"{form.FTDATE_YEAR}/{form.FTDATE_MON}/01";
                            bool bl_ft1 = MyCommonUtil.IsDate(ft1);
                            if (!bl_ft1) { ModelState.AddModelError("DATE", $"訓練期間-迄日有誤。{ft1}"); }
                        }

                        if (ModelState.IsValid)
                        {
                            if (new TimeSpan(Convert.ToDateTime(ft1).Ticks - Convert.ToDateTime(st1).Ticks).Days < 0)
                            {
                                ModelState.AddModelError("DATE", "訓練期間迄日不得小於起日。");
                            }
                        }

                        if (form.DISTID_SHOW != null)
                        {
                            int iNum = 0;
                            foreach (string distid in form.DISTID_SHOW)
                            {
                                if (!Int32.TryParse(distid, out iNum))
                                {
                                    ModelState.AddModelError("DISTID", "上課地點輸入有誤。");
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }


            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 欄位檢核 OK, 處理查詢
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
                switch (form.PlanType)
                {
                    case "1":
                    case "2":
                    case "5":
                        Session["ClassSearch.SortField.Form"] = form;
                        object field = Session["ClassSearch.SortField.FieldName"];
                        object desc = Session["ClassSearch.SortField.Desc"];
                        if (field != null) { form.ORDERBY = (string)field; }
                        if (desc != null) { form.ORDERDESC = (string)desc; }
                        break;
                }

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                switch (form.PlanType)
                {
                    case "1": //產投
                        model.Grid1 = dao.QueryClassSearch<ClassSearchGrid1Model>(form);
                        break;

                    case "2": //在職
                        model.Grid2 = dao.QueryClassSearch<ClassSearchGrid2Model>(form);
                        break;

                    case "5": //區域據點
                        model.Grid3 = dao.QueryClassSearch<ClassSearchGrid3Model>(form);
                        break;
                }

                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                    // rtn = PartialView("_GridRows", model);
                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                    model.Form.PagingInfo = dao.PaginationInfo;
                }
                else
                {
                    if (form.PlanType == "1")
                    {
                        //取得目前(DB)系統時間
                        MyKeyMapDAO keyDao = new MyKeyMapDAO();
                        DateTime nowTime = keyDao.GetSysDateNow();

                        //2019-02-11 問題9:add if ，修正執行批次課程收藏後仍會一直顯示下方提示訊息的問題 
                        if (string.IsNullOrEmpty(form.IsSearched))
                        {
                            //設置特殊提示訊息
                            sm.LastResultMessage = "「報名日期」會依訓練單位變更「預定訓練起迄日期」而提前或延後，請密切留意，以免錯過報名時間。<br/><br/>目前系統時間為(" + nowTime.ToString("yyyy/MM/dd HH:mm:ss") + ")";
                            form.IsSearched = "Y";
                        }
                    }

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
                                KWLTYPE = "2",
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

            model.Form = form;
            byte[] byarr = ObjectToByteArray(model);
            Session["LastModel"] = Convert.ToBase64String(byarr);
            Session["rid"] = dao.ResultID;
            return rtn;
        }


        public ActionResult LastResult()
        {
            if (Session["LastModel"] == null) { return SetPageNotFound(); }
            string sessData = (string)Session["LastModel"] ?? null;
            if (sessData == null || string.IsNullOrEmpty(sessData)) { return SetPageNotFound(); }

            ClassSearchViewModel model = new ClassSearchViewModel();
            BaseDAO dao = new BaseDAO();
            //if (sessData != null) { }
            try
            {
                var data = Convert.FromBase64String(sessData);
                if (data != null)
                {
                    if (Session["rid"] == null) { return base.SetPageNotFound(); }
                    dao.ResultID = (string)Session["rid"]; //rid;
                    model = (ClassSearchViewModel)ByteArrayToObject(data);
                }
                else
                {
                    model = new ClassSearchViewModel();
                    model.Form = new ClassSearchFormModel();
                    base.SetPagingParams(model.Form, dao, "Index");
                    model.Form.action = Url.Action("Index");
                }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);
                return base.SetPageNotFound(); //throw;
            }

            return View("SearchResults", model);
        }

        /// <summary> 依學術科欄位排序 </summary>
        /// <param name="FieldName"></param>
        /// <param name="Desc"></param>
        /// <returns></returns>
        public ActionResult SortField(string FieldName = null, string Desc = null)
        {
            //if (Session["ClassSearch.SortField.Form"] == null) { return base.SetPageNotFound(); }
            ClassSearchFormModel form = null;
            try
            {
                form = (ClassSearchFormModel)Session["ClassSearch.SortField.Form"];
                Session["ClassSearch.SortField.FieldName"] = FieldName;
                Session["ClassSearch.SortField.Desc"] = Desc;
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);//form = null;
                return base.SetPageNotFound();//throw;
            }
            if (form == null) { return base.SetPageNotFound(); }
            if (FieldName == null) { return base.SetPageNotFound(); }
            form.ORDERBY = FieldName;
            form.ORDERDESC = Desc;

            ClassSearchViewModel model = new ClassSearchViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            dao.SetPageInfo(form.rid, form.p);
            dao.ResultID = null;  // 清除快取, 避免無法重新查詢
            model.Form = form;
            IList<ClassSearchGrid1Model> result = null;
            try
            {
                result = dao.QueryClassSearch<ClassSearchGrid1Model>(form);
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);//form = null;
                return base.SetPageNotFound();//throw;
            }
            model.Grid1 = result;

            PagingViewModel pagingModel = new PagingViewModel();
            pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
            pagingModel.PagingInfo = form.PagingInfo;
            pagingModel.Rid = dao.ResultID;
            pagingModel.IsResetRid = true;

            return Json(pagingModel);
        }

        /// <summary> 檢視明細頁 </summary>
        /// <param name="Detail"></param>
        /// <param name="ReplaceMsg">取代LastResultMessage的內容(for AddClassTrace 返回用)</param>
        /// <param name="IsReplace">是否取代LastResultMessage</param>
        /// <returns></returns>
        //[HttpPost]
        //[SecurityFilter]
        public ActionResult Detail(string plantype, string ProvideLocation, string OCID, string ReplaceMsg = "", bool IsReplace = false)
        {
            Int64? iOCID = null;
            if (MyCommonUtil.isUnsignedInt(OCID)) { iOCID = Convert.ToInt64(OCID); }
            if (!iOCID.HasValue)
            {
                //throw new ArgumentNullException("OCID");
                LOG.Error("##ClassSearch/Detail : OCID is null／!OCID.HasValue !!!");
                return RedirectToAction("Index", "Home"); //return base.SetPageNotFound();
            }

            //ProvideLocation 提供位置(超連結) //plantype 提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點） //string plantype = pty;
            ArrayList planTypeAry = new ArrayList() { "1", "2", "3", "4", "5" };
            string strLogMsg1 = string.Concat("##ClassSearch/Detail RequestParameter:plantype=[", plantype, "],providelocation=[", ProvideLocation, "],ocid=[", OCID, "],ReplaceMsg=[", ReplaceMsg, "]");
            LOG.Debug(strLogMsg1);

            //if (PlanType != "1" && PlanType != "2")
            if (string.IsNullOrEmpty(plantype))
            {
                LOG.Error("##ClassSearch/Detail IsNullOrEmpty(plantype) !!!");
                return base.SetPageNotFound();
            }
            else if (!string.IsNullOrEmpty(plantype) && !planTypeAry.Contains(plantype))
            {
                LOG.Error("##ClassSearch/Detail !planTypeAry.Contains(plantype) !!!");
                return base.SetPageNotFound();
            }

            if (string.IsNullOrEmpty(ProvideLocation)) { ProvideLocation = "N"; }

            //return new HttpStatusCodeResult(404);
            if (!"Y".Equals(ProvideLocation) && !"N".Equals(ProvideLocation))
            {
                LOG.Error("##ClassSearch/Detail IsNullOrEmpty(ProvideLocation) !!!");
                return base.SetPageNotFound();
            }

            ClassSearchDetailModel Detail = new ClassSearchDetailModel();
            Detail.PlanType = plantype;
            Detail.ProvideLocation = ProvideLocation;
            Detail.OCID = iOCID;
            if (string.IsNullOrEmpty(Detail.PlanType)) { throw new ArgumentNullException("PlanType"); }
            if (string.IsNullOrEmpty(Detail.ProvideLocation)) { throw new ArgumentNullException("ProvideLocation"); }
            if (Detail.OCID == null) { throw new ArgumentNullException("OCID"); }

            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;

            TblCLASS_CLASSINFO where = new TblCLASS_CLASSINFO { OCID = Detail.OCID };

            //登入後導頁資訊
            TWJobsMemberDataModel redirectInfo = new TWJobsMemberDataModel();
            redirectInfo.PAGEURL = Url.Content("/ClassSearch/Detail");
            redirectInfo.OCID = (iOCID.HasValue ? iOCID : null);
            redirectInfo.PLANTYPE = plantype;
            sm.RedirectInfo = redirectInfo;
            sm.IsOnlineSignUp = "N"; //2019-01-07 add 是否從線上報名功能輸入課程代碼進行報名

            //string cst_NODATA_Msg1 = "找不到指定的資料!(或資料已下架，請使用歷史課程查詢)";
            string cst_NODATA_Msg1 = "找不到指定的資料!(可能原因：資料已截止報名或資料已下架)";
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            MyKeyMapDAO KMdao = new MyKeyMapDAO();
            ClassSearchViewModel model = new ClassSearchViewModel();
            model.Detail = Detail;
            switch (Detail.PlanType)
            {
                case "1"://產投 Detail1
                    //班級
                    ClassSearchDetail1Model detail1 = dao.GetClassSearchDetail<ClassSearchDetail1Model>(Detail.PlanType, where);
                    if (detail1 == null)
                    {
                        sm.LastErrorMessage = cst_NODATA_Msg1;
                        detail1 = new ClassSearchDetail1Model();
                    }
                    else
                    {
                        //取得報名人數
                        detail1.EnterCount = KMdao.GetEnterCount(detail1.OCID);
                        //判斷是否啟用報名鈕及訊息

                        //取得師資及課程訓練內容
                        detail1.TrainDesc = dao.GetClassSearchTrain_v2(Detail.PlanType, detail1.PLANID, detail1.COMIDNO, detail1.SEQNO);
                        //取得師資
                        detail1.TeacherInfo = dao.GetClassSearchTeacher(detail1.OCID);
                        //專長能力標籤-PLAN_ABILITY
                        var wAbility = new TblPLAN_ABILITY() { PLANID = detail1.PLANID, COMIDNO = detail1.COMIDNO, SEQNO = detail1.SEQNO };
                        detail1.AbilityGrid = dao.GetRowList(wAbility);

                        //判斷按鈕狀態並決定提示訊息呈現方式
                        //1:反灰，訊息為點擊按鈕時才顯示 //2:隱藏，載入頁面時直接顯示訊息 //3:反灰，載入頁面時直接顯示訊息且點擊按鈕亦會顯示
                        switch (detail1.BtnStatus)
                        {
                            case "2":
                            case "3":
                                sm.LastResultMessage = detail1.ShowMsg;
                                break;
                        }

                        //是否要取代提示訊息
                        if (IsReplace) { sm.LastResultMessage = ReplaceMsg; }

                        //寫入瀏覽記錄(tb_viewrecord)
                        //(new WDAIIPWEBDAO()).AddClassViewRecord(detail1.TPLANID, detail1.OCID.Value);
                        dao.AddClassViewRecord(detail1.TPLANID, detail1.OCID.Value);

                        //記錄瀏覽次數
                        //detail1.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail1.OCID.Value);
                        detail1.BROWSECNT = dao.AddBrowseCnt(detail1.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail1.ViewRecGrid = dao.QueryClassViewRecord(detail1.TPLANID, detail1.OCID.Value);

                        //系統時間（與報名時間比較）
                        //detail1.SERVERTIME = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
                        //detail1.SERVERTIME = new MyKeyMapDAO().GetSysDateNow().ToString("yyyy/MM/dd HH:mm:ss.fff");
                        detail1.SERVERTIME = KMdao.GetSysDateNow().ToString("yyyy/MM/dd HH:mm:ss.fff");
                    }

                    model.Detail.detail1 = detail1;

                    //記錄登入後導頁用的資訊
                    redirectInfo.PAGEURL = Url.Content("/ClassSearch/Detail");
                    redirectInfo.OCID = iOCID;

                    rtn = View("Detail1", model);
                    break;

                case "2"://自辦在職 Detail2
                    ClassSearchDetail2Model detail2 = dao.GetClassSearchDetail<ClassSearchDetail2Model>(Detail.PlanType, where);
                    if (detail2 == null)
                    {
                        sm.LastErrorMessage = cst_NODATA_Msg1;
                        detail2 = new ClassSearchDetail2Model();
                    }
                    else
                    {
                        //取得課程訓練內容
                        detail2.TrainDesc = dao.GetClassSearchTrain(Detail.PlanType, detail2.PLANID, detail2.COMIDNO, detail2.SEQNO);
                        //專長能力標籤-PLAN_ABILITY
                        var wAbility = new TblPLAN_ABILITY() { PLANID = detail2.PLANID, COMIDNO = detail2.COMIDNO, SEQNO = detail2.SEQNO };
                        detail2.AbilityGrid = dao.GetRowList(wAbility);

                        //是否要取代提示訊息
                        if (IsReplace)
                            sm.LastResultMessage = ReplaceMsg;

                        //寫入瀏覽記錄(tb_viewrecord)
                        //(new WDAIIPWEBDAO()).AddClassViewRecord(detail2.TPLANID, detail2.OCID.Value);
                        dao.AddClassViewRecord(detail2.TPLANID, detail2.OCID.Value);

                        //記錄瀏覽次數
                        //detail2.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail2.OCID.Value);
                        detail2.BROWSECNT = dao.AddBrowseCnt(detail2.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail2.ViewRecGrid = dao.QueryClassViewRecord(detail2.TPLANID, detail2.OCID.Value);
                    }

                    model.Detail.detail2 = detail2;

                    rtn = View("Detail2", model);
                    break;

                case "3": //充電起飛 Detail1
                    ClassSearchDetail1Model detail3 = dao.GetClassSearchDetail<ClassSearchDetail1Model>(Detail.PlanType, where);
                    if (detail3 == null)
                    {
                        sm.LastErrorMessage = cst_NODATA_Msg1;
                        detail3 = new ClassSearchDetail1Model();
                    }
                    else
                    {
                        //取得報名人數
                        detail3.EnterCount = KMdao.GetEnterCount(detail3.OCID);
                        //判斷是否啟用報名鈕及訊息

                        //取得師資及課程訓練內容
                        detail3.TrainDesc = dao.GetClassSearchTrain_v2(Detail.PlanType, detail3.PLANID, detail3.COMIDNO, detail3.SEQNO);
                        detail3.TeacherInfo = dao.GetClassSearchTeacher(detail3.OCID);
                        //專長能力標籤-PLAN_ABILITY
                        var wAbility = new TblPLAN_ABILITY() { PLANID = detail3.PLANID, COMIDNO = detail3.COMIDNO, SEQNO = detail3.SEQNO };
                        detail3.AbilityGrid = dao.GetRowList(wAbility);

                        //判斷按鈕狀態並決定提示訊息呈現方式
                        //1:反灰，訊息為點擊按鈕時才顯示 //2:隱藏，載入頁面時直接顯示訊息 //3:反灰，載入頁面時直接顯示訊息且點擊按鈕亦會顯示
                        switch (detail3.BtnStatus)
                        {
                            case "2":
                            case "3":
                                sm.LastResultMessage = detail3.ShowMsg;
                                break;
                        }

                        //是否要取代提示訊息
                        if (IsReplace)
                            sm.LastResultMessage = ReplaceMsg;

                        //寫入瀏覽記錄(tb_viewrecord)
                        //(new WDAIIPWEBDAO()).AddClassViewRecord(detail3.TPLANID, detail3.OCID.Value);
                        dao.AddClassViewRecord(detail3.TPLANID, detail3.OCID.Value);

                        //記錄瀏覽次數
                        //detail3.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail3.OCID.Value);
                        detail3.BROWSECNT = dao.AddBrowseCnt(detail3.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail3.ViewRecGrid = dao.QueryClassViewRecord(detail3.TPLANID, detail3.OCID.Value);
                    }

                    model.Detail.detail1 = detail3;

                    rtn = View("Detail1", model);
                    break;

                case "4": //接受企業委託訓練 Detail2
                    ClassSearchDetail2Model detail4 = dao.GetClassSearchDetail<ClassSearchDetail2Model>(Detail.PlanType, where);
                    if (detail4 == null)
                    {
                        sm.LastErrorMessage = cst_NODATA_Msg1;
                        detail2 = new ClassSearchDetail2Model();
                    }
                    else
                    {
                        //取得課程訓練內容
                        detail4.TrainDesc = dao.GetClassSearchTrain(Detail.PlanType, detail4.PLANID, detail4.COMIDNO, detail4.SEQNO);
                        //專長能力標籤-PLAN_ABILITY
                        var wAbility = new TblPLAN_ABILITY() { PLANID = detail4.PLANID, COMIDNO = detail4.COMIDNO, SEQNO = detail4.SEQNO };
                        detail4.AbilityGrid = dao.GetRowList(wAbility);

                        //是否要取代提示訊息
                        if (IsReplace) { sm.LastResultMessage = ReplaceMsg; }

                        //寫入瀏覽記錄(tb_viewrecord)
                        //(new WDAIIPWEBDAO()).AddClassViewRecord(detail4.TPLANID, detail4.OCID.Value);
                        dao.AddClassViewRecord(detail4.TPLANID, detail4.OCID.Value);

                        //記錄瀏覽次數
                        //detail4.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail4.OCID.Value);
                        detail4.BROWSECNT = dao.AddBrowseCnt(detail4.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail4.ViewRecGrid = dao.QueryClassViewRecord(detail4.TPLANID, detail4.OCID.Value);
                    }

                    model.Detail.detail2 = detail4;

                    rtn = View("Detail2", model);
                    break;

                case "5": //區域據點 Detail3
                    ClassSearchDetail3Model detail5 = dao.GetClassSearchDetail<ClassSearchDetail3Model>(Detail.PlanType, where);

                    if (detail5 == null)
                    {
                        sm.LastErrorMessage = cst_NODATA_Msg1;
                        detail2 = new ClassSearchDetail2Model();
                    }
                    else
                    {
                        //取得課程訓練內容
                        detail5.TrainDesc = dao.GetClassSearchTrain(Detail.PlanType, detail5.PLANID, detail5.COMIDNO, detail5.SEQNO);
                        //專長能力標籤-PLAN_ABILITY
                        var wAbility = new TblPLAN_ABILITY() { PLANID = detail5.PLANID, COMIDNO = detail5.COMIDNO, SEQNO = detail5.SEQNO };
                        detail5.AbilityGrid = dao.GetRowList(wAbility);

                        //是否要取代提示訊息
                        if (IsReplace) { sm.LastResultMessage = ReplaceMsg; }

                        //2018-12-25 因區域據點不涉及報名，所以不記瀏覽記錄
                        //寫入瀏覽記錄(tb_viewrecord)
                        //dao.AddClassViewRecord(detail5.TPLANID, detail5.OCID.Value);

                        //記錄瀏覽次數
                        //detail5.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail5.OCID.Value);
                        detail5.BROWSECNT = dao.AddBrowseCnt(detail5.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail5.ViewRecGrid = dao.QueryClassViewRecord(detail5.TPLANID, detail5.OCID.Value);
                    }

                    model.Detail.detail3 = detail5;

                    rtn = View("Detail3", model);
                    break;

            }

            return rtn;
        }

        /// <summary> 課程比一比 </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Compare(string PlanType, string ProvideLocation, string OCID, string OCID2)
        {
            Int64 i_OCID = -1;
            if (!Int64.TryParse(OCID, out i_OCID)) { i_OCID = -1; }
            Int64 i_OCID2 = -1;
            if (!Int64.TryParse(OCID2, out i_OCID2)) { i_OCID2 = -1; }

            //Int64? iOCID2 = null;
            //if (i_OCID2 > -1 && MyCommonUtil.isUnsignedInt(OCID2)) { iOCID2 = Convert.ToInt64(OCID2); }
            //if (i_OCID2 == -1) { throw new ArgumentNullException("OCID2"); }
            if (i_OCID == -1 || i_OCID2 == -1)
            {
                LOG.Warn("ActionResult Compare: 傳入值有誤!OCID/OCID2"); //throw new ArgumentNullException("OCID"); 
                return base.SetPageNotFound();
            }
            if (string.IsNullOrEmpty(PlanType) || (PlanType != "1" && PlanType != "2"))
            {
                LOG.Warn("ActionResult Compare: 傳入值有誤!PlanType");
                return base.SetPageNotFound();
                //throw new ArgumentNullException("PlanType");
            }

            //是否提供上課位置距離
            if (string.IsNullOrEmpty(ProvideLocation))
            {
                LOG.Warn("ActionResult Compare: 傳入值有誤!ProvideLocation");
                return base.SetPageNotFound();
                //throw new ArgumentNullException("ProvideLocation");
            }
            //if (!iOCID.HasValue) { throw new ArgumentNullException("OCID"); }
            //if (!iOCID2.HasValue) { throw new ArgumentNullException("OCID2"); }

            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;

            IList<Int64?> ocid_list = new List<Int64?> { i_OCID, i_OCID2 };
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            MyKeyMapDAO KMdao = new MyKeyMapDAO();

            ClassSearchViewModel model = new ClassSearchViewModel();
            var compare = new ClassSearchCompareModel();

            switch (PlanType)
            {
                case "1":
                    for (int i = 0; i < ocid_list.Count; i++)
                    {
                        TblCLASS_CLASSINFO where = new TblCLASS_CLASSINFO
                        {
                            OCID = ocid_list[i]
                        };

                        ClassSearchDetail1Model detail = dao.GetClassSearchDetail<ClassSearchDetail1Model>(PlanType, where);
                        if (detail == null)
                        {
                            sm.LastErrorMessage += $"課程代碼：{ocid_list[i]}，找不到指定的資料!<br/>";
                            detail = new ClassSearchDetail1Model();
                        }
                        else
                        {
                            //取得報名人數
                            detail.EnterCount = KMdao.GetEnterCount(detail.OCID);

                            //取得師資及課程訓練內容
                            detail.TrainDesc = dao.GetClassSearchTrain_v2(PlanType, detail.PLANID, detail.COMIDNO, detail.SEQNO);
                            detail.TeacherInfo = dao.GetClassSearchTeacher(detail.OCID);
                        }

                        compare.Detail1.Add(detail);
                    }

                    rtn = View("Compare1", model);
                    break;

                case "2":
                    for (int i = 0; i < ocid_list.Count; i++)
                    {
                        TblCLASS_CLASSINFO where = new TblCLASS_CLASSINFO { OCID = ocid_list[i] };

                        ClassSearchDetail2Model detail = dao.GetClassSearchDetail<ClassSearchDetail2Model>(PlanType, where);
                        if (detail == null)
                        {
                            sm.LastErrorMessage += "課程代碼：" + ocid_list[i] + "，找不到指定的資料!<br/>";
                            detail = new ClassSearchDetail2Model();
                        }
                        else
                        {
                            //取得課程訓練內容
                            detail.TrainDesc = dao.GetClassSearchTrain(PlanType, detail.PLANID, detail.COMIDNO, detail.SEQNO);
                        }

                        compare.Detail2.Add(detail);
                    }

                    rtn = View("Compare2", model);
                    break;
            }


            model.Compare = compare;
            model.Compare.PlanType = PlanType;
            model.Compare.ProvideLocation = ProvideLocation;
            return rtn;
        }

        /// <summary> 多筆課程收藏 </summary>
        /// <returns></returns>
        public ActionResult MultiAddClassTrace(ClassSearchViewModel model)
        {
            //LOG.Debug("##MultiAddClassTrace ActionResult"); //未登入會員
            try
            {
                ClassSearchViewModel.CheckArgument(this.HttpContext);
            }
            catch (Exception ex)
            {
                var s_err = string.Format("#MultiAddClassTrace,ClassSearchViewModel.CheckArgument:{0} from {1}", ex.Message, Request.UserHostAddress);
                LOG.Error(s_err, ex);
                return base.SetPageNotFound();
                //return RedirectToAction("Login", "Member");
            }

            SessionModel sm = SessionModel.Get();
            //未登入會員
            if (sm.UserID == null)
            {
                return RedirectToAction("Login", "Member");
            }

            //ActionResult rtn = new RedirectResult("Index");
            //ActionResult rtn = View("SearchResults", model);
            ActionResult rtn = LastResult(); //2019-02-11 fix 問題9：修改點選完「課程收藏」後，都會跳到查詢頁一會，等按掉alert又跳回查詢清單頁，然後此時就又會跳出查詢清單頁一進入時的alert一次問題

            //增加檢核E_Menber MEM_SN(序號)
            if (sm.MemSN == null)
            {
                LOG.Warn("#Login Failed from " + Request.UserHostAddress + ": 未登入會員,mem_id:" + sm.UserID);
                rtn = RedirectToAction("Login", "Member");
            }
            else
            {
                IList<Int64?> classlist = new List<Int64?>();
                try
                {
                    //Model.Form.PlanType "1":產投 "2":分署自辦在職訓練 "5": 區域據點
                    switch (model.Form.PlanType)
                    {
                        case "1":
                            if (model.Form.SELOCD28 != "")
                            {
                                var SELOCD28_list = model.Form.SELOCD28.Split(',').ToList();
                                long i_OCD = 0;
                                foreach (var s_OCD in SELOCD28_list)
                                {
                                    if (long.TryParse(s_OCD, out i_OCD)) { classlist.Add(i_OCD); };
                                }
                            }
                            //classlist = model.Grid1.Where(m => m.SELECTIS == true).Select(m => m.OCID).ToList();
                            break;
                        case "2":
                            classlist = model.Grid2.Where(m => m.SELECTIS == true).Select(m => m.OCID).ToList();
                            break;
                        //case "5":
                        //classlist = model.Grid3.Where(m => m.SELECTIS == true).Select(m => m.OCID).ToList();
                        //break;
                        default:
                            sm.LastErrorMessage = "加入課程收藏失敗。";
                            sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassSearch", new { useCache = 1 });
                            return rtn; // break;
                    }

                }
                catch (Exception ex)
                {
                    LOG.Error(ex.Message, ex);
                    sm.LastErrorMessage = "加入課程收藏失敗。";
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassSearch", new { useCache = 1 });
                    return rtn; // break; //throw;
                }
                sm.LastResultMessage = "請至少勾選一項";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassSearch", new { useCache = 1 });

                if (classlist == null || classlist.Count == 0)
                {
                    sm.LastResultMessage = "請至少勾選一項";
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassSearch", new { useCache = 1 });
                    return rtn;
                }
                else
                {
                    try
                    {
                        string s_CNTMSG1 = string.Concat(classlist.Count, "筆");

                        WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
                        //批次加入課程收藏
                        dao.AddClassTrace(model.Form.PlanType, classlist, sm);

                        sm.LastResultMessage = string.Concat("已新增至課程收藏清單。", s_CNTMSG1);
                        sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassSearch", new { useCache = 2 });
                    }
                    catch (Exception ex)
                    {
                        LOG.Error(ex.Message, ex);
                        sm.LastErrorMessage = "加入課程收藏失敗。";
                        sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassSearch", new { useCache = 1 });
                        return rtn;
                    }
                }
            }
            return rtn;
        }

        /// <summary>單筆課程收藏</summary>
        /// <returns></returns>
        [LoginRequired]
        [HttpPost]
        public ActionResult AddClassTrace(string PlanType, string ProvideLocation, string OCID)
        {
            Int64? iOCID = null;
            if (MyCommonUtil.isUnsignedInt(OCID)) { iOCID = Convert.ToInt64(OCID); }

            if (iOCID == null) { LOG.Warn("ActionResult AddClassTrace: 傳入值有誤!OCID"); return base.SetPageNotFound(); }
            if (string.IsNullOrEmpty(PlanType)) { LOG.Warn("ActionResult AddClassTrace: 傳入值有誤!PlanType"); return base.SetPageNotFound(); }
            if (string.IsNullOrEmpty(ProvideLocation)) { LOG.Warn("ActionResult AddClassTrace: 傳入值有誤!ProvideLocation"); return base.SetPageNotFound(); }
            if (iOCID == null) { throw new ArgumentNullException("OCID"); }
            if (string.IsNullOrEmpty(PlanType)) { throw new ArgumentNullException("PlanType"); }
            if (string.IsNullOrEmpty(ProvideLocation)) { throw new ArgumentNullException("ProvideLocation"); }

            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;
            string alertMsg = string.Empty;

            //增加檢核E_Menber MEM_SN(序號)
            if (sm.MemSN == null)
            {
                LOG.Info("Login Failed from " + Request.UserHostAddress + ": 未登入會員(mem_id=" + sm.UserID);

                rtn = RedirectToAction("Login", "Member");
            }
            else
            {
                IList<Int64?> classlist = new List<Int64?>();
                classlist.Add(iOCID);

                try
                {
                    WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
                    bool isExists = dao.IsClassTraceExists(PlanType, iOCID.Value, sm);
                    if (isExists)
                    {
                        alertMsg = "課程收藏清單已有此課程！";
                    }
                    else
                    {
                        //批次加入課程收藏
                        dao.AddClassTrace(PlanType, classlist, sm);
                        alertMsg = "已新增至課程收藏清單。";
                    }

                    rtn = Detail(PlanType, ProvideLocation, Convert.ToString(iOCID), alertMsg, true);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.Message, ex);
                    rtn = Detail(PlanType, ProvideLocation, Convert.ToString(iOCID), "加入課程收藏失敗。", true);
                    sm.LastErrorMessage = "加入課程收藏失敗。";
                }
            }

            return rtn;
        }

        /// <summary> 更新查詢條件ajax-計畫別（1 產投 , 2 在職 , 5 區域據點）  </summary>
        /// <param name="PlanType">查詢類型</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PlanTypeChange(string PlanType)
        {
            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };
            Int32 i_PlanType = -1;
            if (!Int32.TryParse(PlanType, out i_PlanType)) { i_PlanType = -1; }

            //if (form == null) { return base.SetPageNotFound(); }

            if (i_PlanType == -1)
            {
                LOG.Warn("[ALERT] 3-ClassSearchController PlanTypeChange() plantype=[" + PlanType + "]");
                //throw new ArgumentException("ClassSearchController.Index()");
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }
            if (!string.IsNullOrEmpty(PlanType))
            {
                //if (form.PlanType != "1" && form.PlanType != "2" )
                if (!planTypeAry.Contains(PlanType))
                {
                    LOG.Warn("[ALERT] 1-ClassSearchController PlanTypeChange() plantype=[" + PlanType + "]");
                    //throw new ArgumentException("ClassSearchController.Index()");
                    //return new HttpStatusCodeResult(404);
                    return base.SetPageNotFound();
                }
            }
            else
            {
                LOG.Warn("[ALERT] 2-ClassSearchController PlanTypeChange() plantype is null");
                //throw new ArgumentException("ClassSearchController.Index()");
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }

            ClassSearchViewModel model = new ClassSearchViewModel();
            model.Form.PlanType = PlanType;
            model.Form.IsFirst = "N"; //第1次查詢

            DateTime now = DateTime.Now;

            switch (PlanType)
            {
                case "1":
                    //產投 //2019-01-30 問題7:開訓日期區間預設選取的邏輯（以當下日期為依據）
                    //當日介於１~６月為上半年：查詢區間預設->前1年12月 ~ 當年6月  //當日介於７～１２月為下半年：查詢區間預設->當年6月 ~ 當年12月
                    //經討論開訓日期區間起改為預設前一個月，才會查到資料(因報名迄日設比開始起日晚) //效能調整，只查3個月就好
                    string dateSYr = (now.AddMonths(-1).Year - 1911).ToString();
                    string dateSMon = now.AddMonths(-1).Month.ToString();
                    string dateEYr = (now.AddMonths(3).Year - 1911).ToString();
                    string dateEMon = now.AddMonths(3).Month.ToString();

                    model.Form.STDATE_YEAR_SHOW = dateSYr;
                    model.Form.STDATE_MON = dateSMon;
                    model.Form.FTDATE_YEAR_SHOW = dateEYr;
                    model.Form.FTDATE_MON = dateEMon;
                    break;
                case "2": //在職
                    //model.Form.IsContainsOverEnter = "Y";//2019-01-30 add 是否包含已截止報名課程(預設是)
                    model.Form.IsContainsOverEnter = "N"; //搜尋結果調整為僅報名中及即將開放報名的課程 20241021
                    model.Form.STDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    model.Form.STDATE_MON = now.Month.ToString();
                    model.Form.FTDATE_YEAR_SHOW = (now.AddMonths(3).Year - 1911).ToString();
                    model.Form.FTDATE_MON = now.AddMonths(3).Month.ToString();
                    break;
                case "5": //區域據點
                    //model.Form.IsContainsOverEnter = "Y";//2019-05-01 add 是否包含已截止報名課程(預設是)
                    model.Form.IsContainsOverEnter = "N"; //搜尋結果調整為僅報名中及即將開放報名的課程 20241021
                    model.Form.STDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    model.Form.STDATE_MON = now.Month.ToString();
                    model.Form.FTDATE_YEAR_SHOW = (now.AddMonths(3).Year - 1911).ToString();
                    model.Form.FTDATE_MON = now.AddMonths(3).Month.ToString();
                    break;
            }

            return View("Index", model);
        }

        /// <summary> Ajax 取得縣市代碼所對應的鄉鎮市區域郵遞區號清單 </summary>
        /// <param name="CTID">縣市代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetZipCode(string CTID)
        {
            IList<KeyMapModel> list = new List<KeyMapModel>();
            list.Insert(0, new KeyMapModel { CODE = "", TEXT = "請選擇" });
            Int64 i_CTID = -1;
            if (!Int64.TryParse(CTID, out i_CTID)) { return MyCommonUtil.BuildOptionHtmlAjaxResult(list, null, null); }

            MyKeyMapDAO dao = new MyKeyMapDAO();
            list = dao.GetCountyZipList(CTID);
            list.Insert(0, new KeyMapModel { CODE = "", TEXT = "請選擇" });
            return MyCommonUtil.BuildOptionHtmlAjaxResult(list, null, null);
        }

        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null) { return null; };
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        private object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = binForm.Deserialize(memStream);
            return obj;
        }

        //public ActionResult claspxUrl3()
        //{
        //    var s_Url1 = @"https://jobooks.taiwanjobs.gov.tw/cl.aspx?n=3";
        //    return Redirect(s_Url1);
        //}


    }
}