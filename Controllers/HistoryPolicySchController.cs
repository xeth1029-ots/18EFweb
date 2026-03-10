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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using log4net;


namespace WDAIIP.WEB.Controllers
{
    public class HistoryPolicySchController : BaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: HistoryPolicySch

        /// <summary>
        /// 查詢條件頁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            //return View("Index", model);
            return base.SetPageNotFound();

            /*
            SessionModel sm = SessionModel.Get();
            ClassSearchViewModel model = new ClassSearchViewModel();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            model.Form.PlanType = "1"; //預設查詢計畫類別
            model.Form.IsFirst = "Y";

            DateTime now = DateTime.Now;
            string strRocSYr = (now.AddMonths(-6).Year - 1911).ToString(); //往前推半年
            string strSMon = now.AddMonths(-6).Month.ToString(); //往前推半年
            string strRocEYr = (now.Year - 1911).ToString(); //當日-年
            string strEMon = now.Month.ToString(); //當日-月

            model.Form.STDATE_YEAR_SHOW = strRocSYr;// (now.Year - 1911).ToString();
            model.Form.STDATE_MON = strSMon;//now.Month.ToString();
            model.Form.FTDATE_YEAR_SHOW = strRocEYr;//(now.AddMonths(1).Year - 1911).ToString();
            model.Form.FTDATE_MON = strEMon;//now.AddMonths(1).Month.ToString();

            return View("Index", model);
             */
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
            //ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };

            /* -------------------- 查詢條件檢核 start -------------------- */
            if (form == null) { return base.SetPageNotFound(); }
            //查詢計畫類別
            if (string.IsNullOrEmpty(form.PlanType))
            {
                //throw new ArgumentNullException("plantype 不可為 null");
                LOG.Warn("[ALERT] HistoryPolicySchController post Index (404):  plantype 不可為 null");
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }
            //!planTypeAry.Contains(form.PlanType)
            if (!"1".Equals(form.PlanType))
            {
                //throw new ArgumentException("plantype 格式錯誤");
                LOG.Warn("[ALERT] HistoryPolicySchController post Index (404):  plantype 格式錯誤");
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }

            Int32 iNum = 0;
            //上課地點
            if (form.CTID != null && !Int32.TryParse(form.CTID, out iNum))
            {
                LOG.Warn("[ALERT] HistoryPolicySchController post Index (404):  CTID 格式錯誤");
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
                        LOG.Warn("[ALERT] HistoryPolicySchController post Index (404):  DISTID 格式錯誤");
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
                LOG.Warn("[ALERT] HistoryPolicySchController post Index (404):  訓練期間（起日）格式錯誤");
                //return new HttpStatusCodeResult(404);
                return base.SetPageNotFound();
            }


            //訓練期間(迄)-必填(無請選擇)
            if (form.FTDATE_YEAR == null || !Int32.TryParse(form.FTDATE_YEAR, out iNum)
                || form.FTDATE_MON == null || !Int32.TryParse(form.FTDATE_MON, out iNum)
                || !DateTime.TryParseExact((form.FTDATE_YEAR + form.FTDATE_MON.PadLeft(2, '0') + "01"), "yyyyMMdd", new CultureInfo("en-US"), DateTimeStyles.None, out dt))
            {
                LOG.Debug("[ALERT] HistoryPolicySchController post Index (404):  訓練期間（迄日）格式錯誤");
                return new HttpStatusCodeResult(403);
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

            // 欄位檢核 OK, 處理查詢
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            rtn = View("Index", model);

            if (model.Form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);
                model.Form.IsHisSearch = "Y"; //註記為歷史查詢

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                switch (form.PlanType)
                {
                    case "1":
                        Session["ClassSearch.SortField.Form"] = form;
                        object field = Session["ClassSearch.SortField.FieldName"];
                        object desc = Session["ClassSearch.SortField.Desc"];
                        if (field != null) { form.ORDERBY = (string)field; }
                        if (desc != null) { form.ORDERDESC = (string)desc; }

                        // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                        model.Grid1 = dao.QueryHistoryPolicySch<ClassSearchGrid1Model>(form);
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
                    model.Form.PagingInfo = dao.PaginationInfo;
                }
                else
                {
                    rtn = View("SearchResults", model);

                    //有輸入關鍵字進行查詢時，要寫入關鍵字Log資料表
                    TblTB_KEYWORD_LOG keylog = null;
                    IList<TblTB_KEYWORD_LOG> keylogList = new List<TblTB_KEYWORD_LOG>();

                    if (!string.IsNullOrWhiteSpace(form.KEYWORDS))
                    {
                        foreach (var item in form.KEYWORDS_list)
                        {
                            keylog = new TblTB_KEYWORD_LOG
                            {
                                KWLTYPE = "3",
                                SUB_TYPEID = form.PlanType, //1,2,3,4,5,99,null
                                KEYWORD = item,
                                USERIP = Request.UserHostAddress,
                                SEARCHDATE = DateTime.Now
                            };

                            keylogList.Add(keylog);
                        }

                        //新增關鍵字查詢Log紀錄
                        dao.InsertKeyWordLog(keylogList);
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
            //現在是5月，則篩選開課日期區間前一年度12月~5月，以此類推
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

                Session["ClassSearch.SortField.Form"] = form;
                object field = Session["ClassSearch.SortField.FieldName"];
                object desc = Session["ClassSearch.SortField.Desc"];
                if (field != null) { form.ORDERBY = (string)field; }
                if (desc != null) { form.ORDERDESC = (string)desc; }

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                model.Grid1 = dao.QueryHistoryPolicySch<ClassSearchGrid1Model>(form);

                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                    //rtn = PartialView("_GridRows", model);
                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                    model.Form.PagingInfo = dao.PaginationInfo;
                }
                else
                {
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

        /// <summary>
        /// 更新查詢條件ajax 
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
            int mon = 0;

            switch (PlanType)
            {
                case "1": //產投
                    model.Form.STDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    model.Form.STDATE_MON = now.Month.ToString();
                    //model.Form.FTDATE_YEAR_SHOW = (now.AddMonths(1).Year - 1911).ToString();

                    mon = now.Month;

                    if (mon == 12)
                    {
                        model.Form.FTDATE_YEAR_SHOW = (now.AddYears(1).Year - 1911).ToString();
                    }
                    else
                    {
                        model.Form.FTDATE_YEAR_SHOW = (now.Year - 1911).ToString();
                    }
                    model.Form.FTDATE_MON = now.AddMonths(1).Month.ToString();
                    break;
            }

            return View("Index", model);
        }

        /// <summary>
        /// 檢視明細頁
        /// </summary>
        /// <param name="Detail"></param>
        /// <param name="ReplaceMsg">取代LastResultMessage的內容(for AddClassTrace 返回用)</param>
        /// <param name="IsReplace">是否取代LastResultMessage</param>
        /// <returns></returns>
        //[HttpPost]
        //[SecurityFilter]
        //[ValidateAntiForgeryToken]   // 避免CSRF攻擊
        public ActionResult Detail(string PlanType, string ProvideLocation, string OCID, string ReplaceMsg = "", bool IsReplace = false)
        {
            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };
            if (string.IsNullOrEmpty(OCID)) { return base.SetPageNotFound(); }
            if (string.IsNullOrEmpty(PlanType)) { return base.SetPageNotFound(); }
            if (!planTypeAry.Contains(PlanType)) { return base.SetPageNotFound(); }

            string str_Action_Name = "/HistoryPolicySch/Detail";
            LOG.Debug(str_Action_Name + " RequestParameter:plantype=[" + PlanType + "],providelocation=[" + ProvideLocation + "],ocid=[" + Convert.ToString(OCID) + "],ReplaceMsg=[" + ReplaceMsg + "]");

            Int64 i_OCID = -1;
            bool flag_OCID_ok = !string.IsNullOrEmpty(OCID) ? Int64.TryParse(OCID, out i_OCID) : false;
            if (flag_OCID_ok) { OCID = i_OCID.ToString(); }
            if (!flag_OCID_ok) { return base.SetPageNotFound(); }

            if (string.IsNullOrEmpty(ProvideLocation))
            {
                ProvideLocation = "N";
            }
            else
            {
                if (!"Y".Equals(ProvideLocation) && !"N".Equals(ProvideLocation)) { return base.SetPageNotFound(); }
            }

            ClassSearchDetailModel Detail = new ClassSearchDetailModel();
            Detail.PlanType = PlanType;
            Detail.ProvideLocation = ProvideLocation;
            Detail.OCID = i_OCID;

            //throw new ArgumentNullException("PlanType");
            if (string.IsNullOrEmpty(Detail.PlanType)) { return base.SetPageNotFound(); }
            //throw new ArgumentNullException("ProvideLocation");
            if (string.IsNullOrEmpty(Detail.ProvideLocation)) { return base.SetPageNotFound(); }
            //throw new ArgumentNullException("OCID");
            if (Detail.OCID == null) { return base.SetPageNotFound(); }

            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;

            TblCLASS_CLASSINFO where = new TblCLASS_CLASSINFO
            {
                OCID = Detail.OCID
            };

            //登入後導頁資訊
            TWJobsMemberDataModel redirectInfo = new TWJobsMemberDataModel();
            redirectInfo.PAGEURL = Url.Content(str_Action_Name);
            redirectInfo.OCID = i_OCID;
            redirectInfo.PLANTYPE = PlanType;
            sm.RedirectInfo = redirectInfo;
            sm.IsOnlineSignUp = "N"; //2019-01-07 add 是否從線上報名功能輸入課程代碼進行報名

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            MyKeyMapDAO KMdao = new MyKeyMapDAO();
            ClassSearchViewModel model = new ClassSearchViewModel();
            model.Detail = Detail;
            switch (Detail.PlanType)
            {
                case "1"://產投
                    ClassSearchDetail1Model detail1 = null;
                    try
                    {
                        detail1 = dao.GetClassSearchDetail<ClassSearchDetail1Model>(Detail.PlanType, where);
                    }
                    catch (Exception ex)
                    {
                        LOG.Warn(ex.Message, ex);
                        //return base.SetPageNotFound();//throw;
                    }
                    if (detail1 == null)
                    {
                        sm.LastErrorMessage = "找不到指定的資料!";
                        detail1 = new ClassSearchDetail1Model();
                    }
                    else
                    {
                        try
                        {
                            //取得報名人數
                            detail1.EnterCount = KMdao.GetEnterCount(detail1.OCID);
                            //判斷是否啟用報名鈕及訊息

                            //取得師資及課程訓練內容
                            detail1.TrainDesc = dao.GetClassSearchTrain_v2(Detail.PlanType, detail1.PLANID, detail1.COMIDNO, detail1.SEQNO);
                            detail1.TeacherInfo = dao.GetClassSearchTeacher(detail1.OCID);
                        }
                        catch (Exception ex)
                        {
                            LOG.Warn(ex.Message, ex);
                            //return base.SetPageNotFound();//throw;
                        }

                        //判斷按鈕狀態並決定提示訊息呈現方式
                        //1:反灰，訊息為點擊按鈕時才顯示
                        //2:隱藏，載入頁面時直接顯示訊息
                        //3:反灰，載入頁面時直接顯示訊息且點擊按鈕亦會顯示
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
                        (new WDAIIPWEBDAO()).AddClassViewRecord(detail1.TPLANID, detail1.OCID.Value);

                        //記錄瀏覽次數
                        detail1.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail1.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail1.ViewRecGrid = dao.QueryClassViewRecord(detail1.TPLANID, detail1.OCID.Value);

                        //系統時間（與報名時間比較）
                        //detail1.SERVERTIME = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
                        detail1.SERVERTIME = new MyKeyMapDAO().GetSysDateNow().ToString("yyyy/MM/dd HH:mm:ss.fff");
                    }

                    model.Detail.detail1 = detail1;

                    //記錄登入後導頁用的資訊
                    redirectInfo.PAGEURL = Url.Content(str_Action_Name);
                    redirectInfo.OCID = i_OCID;

                    rtn = View("Detail1", model);
                    break;

                case "2"://自辦在職
                    ClassSearchDetail2Model detail2 = dao.GetClassSearchDetail<ClassSearchDetail2Model>(Detail.PlanType, where);
                    if (detail2 == null)
                    {
                        sm.LastErrorMessage = "找不到指定的資料!";
                        detail2 = new ClassSearchDetail2Model();
                    }
                    else
                    {
                        //取得課程訓練內容
                        detail2.TrainDesc = dao.GetClassSearchTrain(Detail.PlanType, detail2.PLANID, detail2.COMIDNO, detail2.SEQNO);

                        //是否要取代提示訊息
                        if (IsReplace)
                            sm.LastResultMessage = ReplaceMsg;

                        //寫入瀏覽記錄(tb_viewrecord)
                        (new WDAIIPWEBDAO()).AddClassViewRecord(detail2.TPLANID, detail2.OCID.Value);

                        //記錄瀏覽次數
                        detail2.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail2.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail2.ViewRecGrid = dao.QueryClassViewRecord(detail2.TPLANID, detail2.OCID.Value);
                    }

                    model.Detail.detail2 = detail2;

                    rtn = View("Detail2", model);
                    break;

                case "3"://充電起飛
                    ClassSearchDetail1Model detail3 = dao.GetClassSearchDetail<ClassSearchDetail1Model>(Detail.PlanType, where);
                    if (detail3 == null)
                    {
                        sm.LastErrorMessage = "找不到指定的資料!";
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

                        //判斷按鈕狀態並決定提示訊息呈現方式
                        //1:反灰，訊息為點擊按鈕時才顯示
                        //2:隱藏，載入頁面時直接顯示訊息
                        //3:反灰，載入頁面時直接顯示訊息且點擊按鈕亦會顯示
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
                        (new WDAIIPWEBDAO()).AddClassViewRecord(detail3.TPLANID, detail3.OCID.Value);

                        //記錄瀏覽次數
                        detail3.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail3.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail3.ViewRecGrid = dao.QueryClassViewRecord(detail3.TPLANID, detail3.OCID.Value);
                    }

                    model.Detail.detail1 = detail3;

                    rtn = View("Detail1", model);
                    break;
                case "4"://接受企業委託訓練
                    ClassSearchDetail2Model detail4 = dao.GetClassSearchDetail<ClassSearchDetail2Model>(Detail.PlanType, where);
                    if (detail4 == null)
                    {
                        sm.LastErrorMessage = "找不到指定的資料!";
                        detail2 = new ClassSearchDetail2Model();
                    }
                    else
                    {
                        //取得課程訓練內容
                        detail4.TrainDesc = dao.GetClassSearchTrain(Detail.PlanType, detail4.PLANID, detail4.COMIDNO, detail4.SEQNO);

                        //是否要取代提示訊息
                        if (IsReplace)
                            sm.LastResultMessage = ReplaceMsg;

                        //寫入瀏覽記錄(tb_viewrecord)
                        (new WDAIIPWEBDAO()).AddClassViewRecord(detail4.TPLANID, detail4.OCID.Value);

                        //記錄瀏覽次數
                        detail4.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail4.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail4.ViewRecGrid = dao.QueryClassViewRecord(detail4.TPLANID, detail4.OCID.Value);
                    }

                    model.Detail.detail2 = detail4;

                    rtn = View("Detail2", model);
                    break;
                case "5"://區域據點
                    ClassSearchDetail3Model detail5 = dao.GetClassSearchDetail<ClassSearchDetail3Model>(Detail.PlanType, where);
                    if (detail5 == null)
                    {
                        sm.LastErrorMessage = "找不到指定的資料!";
                        detail2 = new ClassSearchDetail2Model();
                    }
                    else
                    {
                        //取得課程訓練內容
                        detail5.TrainDesc = dao.GetClassSearchTrain(Detail.PlanType, detail5.PLANID, detail5.COMIDNO, detail5.SEQNO);

                        //是否要取代提示訊息
                        if (IsReplace)
                            sm.LastResultMessage = ReplaceMsg;

                        //2018-12-25 因區域據點不涉及報名，所以不記瀏覽記錄
                        //寫入瀏覽記錄(tb_viewrecord)
                        //dao.AddClassViewRecord(detail5.TPLANID, detail5.OCID.Value);

                        //記錄瀏覽次數
                        detail5.BROWSECNT = (new WDAIIPWEBDAO()).AddBrowseCnt(detail5.OCID.Value);

                        //取得課程瀏覽記錄
                        //detail5.ViewRecGrid = dao.QueryClassViewRecord(detail5.TPLANID, detail5.OCID.Value);
                    }

                    model.Detail.detail3 = detail5;

                    rtn = View("Detail3", model);
                    break;
            }

            return rtn;
        }

        /// <summary>
        /// 關鍵字搜尋
        /// </summary>
        /// <param name="PlanType"></param>
        /// <param name="KEYWORDS"></param>
        /// <returns></returns>
        [HttpPost]
        //[ValidateAntiForgeryToken]   // 避免CSRF攻擊
        public ActionResult Search2(string PlanType, string KEYWORDS)
        {
            //提供課程查詢功能的計畫別（1 產投 , 2 在職 , 5 區域據點）
            ArrayList planTypeAry = new ArrayList() { "1", "2", "5" };
            string str_Action_Name = "/HistoryPolicySch/Search2";
            LOG.Debug(str_Action_Name + " RequestParameter:plantype=[" + PlanType + "],KEYWORDS=[" + KEYWORDS + "]");

            if (string.IsNullOrEmpty(PlanType))
            {
                return base.SetPageNotFound();
            }

            if (!string.IsNullOrEmpty(PlanType))
            {
                if (!planTypeAry.Contains(PlanType))
                {
                    return base.SetPageNotFound();
                }
            }

            //string PlanType = "1";
            SessionModel sm = SessionModel.Get();
            ClassSearchViewModel model = new ClassSearchViewModel();
            ClassSearchFormModel form = new ClassSearchFormModel();

            model.Form = form;
            model.Form.PlanType = PlanType;
            model.Form.IsPolicy = "Y";//政策性課程專區-Policy course area
            model.Form.IsHisSearch = "Y"; //註記為歷史查詢
            model.Form.KEYWORDS = KEYWORDS;

            //A.唯開課日期區間改為：往前推半年，
            //現在是6月，則篩選開課日期區間1月~6月，
            //現在是5月，則篩選開課日期區間前一年度12月~5月，以此類推
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
                model.Form.PlanType = PlanType;
                model.Form.IsPolicy = "Y";//政策性課程專區-Policy course area
                model.Form.IsHisSearch = "Y"; //註記為歷史查詢
                model.Form.KEYWORDS = KEYWORDS;

                Session["ClassSearch.SortField.Form"] = form;
                object field = Session["ClassSearch.SortField.FieldName"];
                object desc = Session["ClassSearch.SortField.Desc"];
                if (field != null) { form.ORDERBY = (string)field; }
                if (desc != null) { form.ORDERDESC = (string)desc; }

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                model.Grid1 = dao.QueryHistoryPolicySch<ClassSearchGrid1Model>(form);

                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                    //rtn = PartialView("_GridRows", model);
                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                    model.Form.PagingInfo = dao.PaginationInfo;
                }
                else
                {
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
        private Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);

            return obj;
        }

        public ActionResult LastResult()
        {
            string s_cacheBuster = Request.QueryString["cacheBuster"];
            if (!string.IsNullOrEmpty(s_cacheBuster)) { return base.SetPageNotFound(); }
            if (Session["LastModel"] == null) { return base.SetPageNotFound(); }
            string sessData = (string)Session["LastModel"] ?? null;
            if (sessData == null) { return base.SetPageNotFound(); }

            ClassSearchViewModel model = new ClassSearchViewModel();
            BaseDAO dao = new BaseDAO();
            if (sessData != null)
            {
                var data = Convert.FromBase64String(sessData);
                if (data != null)
                {
                    if (Session["rid"] == null) { return base.SetPageNotFound(); }
                    string rid = Session["rid"].ToString();
                    dao.ResultID = rid;
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
            return View("SearchResults", model);
        }

    }
}