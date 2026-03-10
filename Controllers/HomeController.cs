using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Commons.Filter;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Services;
using Turbo.Crypto;
using log4net;
using System.Collections;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// 首頁 Controller
    /// </summary>
    public class HomeController : BaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string strSID = ConfigModel.SSOSystemID;

        public HomeController()
        {
        }
        
        /// <summary>檢查是否為 SsoRedirect 進來, 若是則由 Token 中重建 Login Session SSO-重導</summary>
        private void checkSsoRedirect()
        {
            string Sid = Request["Sid"];
            string Rid = Request["Rid"];
            string Token = Request["Token"];

            if (String.IsNullOrEmpty(Sid)) { return; }
            if (String.IsNullOrEmpty(Rid)) { return; }
            if (String.IsNullOrEmpty(Token)) { return; }

            LOG.Info("#HomeController.checkSsoRedirect...");
            Hashtable tokens = null;
            try
            {
                tokens = new Commons.TokenPaser().parse(Token);
            }
            catch (Exception ex)
            {
                //ex.Message, ex
                LOG.Error("tokens 有誤!" + ex.Message, ex);
                return;
            }
            if (tokens["Sid"] == null)
            {
                LOG.Error("tokens.Sid 為空");
                return;
            }
            if (tokens["Mid"] == null)
            {
                LOG.Error("tokens.Mid 為空");
                return;
            }

            if (!strSID.Equals(Sid))
            {
                LOG.Error("Sid '" + Sid + "' 參數跟設定值不一致");
                return;
            }

            if (!Sid.Equals(tokens["Sid"]))
            {
                LOG.Error(string.Format("明碼 Sid 參數跟 Token 中的值不一致 tokens.Sid: {0}", tokens["Sid"]));
                return;
            }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            TWJobsMemberDataModel twMem = new TWJobsMemberDataModel();
            twMem = dao.QueryMember(Rid, Sid);
            if (twMem == null)
            {
                LOG.Error(string.Format("Rid '{0}' Sid '{1}' 對應的 member info 不存在", Rid, Sid));
                return;
            }
            if (!twMem.MEMBER_USER_ID.Equals(tokens["Mid"]))
            {
                LOG.Error(string.Format("Rid '{0}' 對應的 MEMBER_USER_ID '{1}' 跟 Token 中的值不一致", Rid, tokens["Mid"]));
                return;
            }
            SessionModel sm = SessionModel.Get();
            //Login Success
            //Session["Minfo"] = twMem;
            sm.RID = Convert.ToString(twMem.RID);
            sm.SID = Convert.ToString(twMem.SID);
            sm.ACID = twMem.ACID; //IDNO;
            if (!string.IsNullOrEmpty(twMem.BIRTHDAY)) { twMem.BIRTHDAY = twMem.BIRTHDAY.Replace("-", "/"); }
            sm.Birthday = twMem.BIRTHDAY; //"yyyy/MM/dd";             
            sm.MEMBER_USER_ID = Convert.ToString(twMem.MEMBER_USER_ID); //MEMBER_USER_ID;
        }

        /// <summary> 檢核資料庫連線狀態 </summary>
        public void Chk_dao_connection()
        {
            MyKeyMapDAO kdao = new MyKeyMapDAO();
            bool flag_error = false;
            try
            {
                DateTime sNow = kdao.GetSysDateNow();
            }
            catch (Exception ex)
            {
                flag_error = true;
                LOG.Error(ex.Message, ex);
            }
            if (flag_error)
            {
                string StopUrl = ConfigModel.StopUrl;
                var resp = HttpContext.Response;
                resp.Redirect(StopUrl);
                resp.End();
            }
            //return flag_error;
        }

        /// <summary> Index </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }

            // 檢查是否為 SsoRedirect 進來, 若是則由 Token 中重建 Login Session SSO-重導
            try
            {
                checkSsoRedirect();
            }
            catch (Exception ex)
            {
                LOG.Error("checkSsoRedirect 有誤!" + ex.Message, ex);
                return base.SetPageNotFound(); //throw;
            }            

            SessionModel sm = SessionModel.Get();
            HomeViewModel model = new HomeViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            TblTB_BANNER bannerWhere = null;
            TblTB_CONTENT contentWhere = null;

            //設定所在主功能表位置(清空)
            sm.ACTIVEFUNCTION = "";

            // 檢核資料庫連線狀態
            Chk_dao_connection();

            // 取得系統公告提示訊息
            model.Notice = dao.GetNotice();

            // 查詢首頁大Banner資料
            bannerWhere = new TblTB_BANNER { TYPEID = "B01" };
            //model.BigBanner = dao.GetBanner(bannerWhere);
            model.BigBannerGrid = dao.QueryBanner(bannerWhere);

            // 查詢首頁 頁尾Banner圖示清單資料
            bannerWhere = new TblTB_BANNER { TYPEID = "B02" };
            model.BannerGrid = dao.QueryBanner(bannerWhere);

            // 查詢最新焦點消息
            contentWhere = new TblTB_CONTENT { FUNID = "001", SUB_FUNID = "1" };
            model.TopNewsGrid = dao.QueryTopContent(contentWhere);

            // 查詢最新計畫公告
            contentWhere = new TblTB_CONTENT { FUNID = "001", SUB_FUNID = "2" };
            model.TopPlanGrid = dao.QueryTopContent(contentWhere);

            // 查詢宣導影片
            contentWhere = new TblTB_CONTENT { FUNID = "011" };
            model.Film = dao.GetTopContent(contentWhere);

            // 查詢最多分享課程(有被分享且未開訓的課程)
            model.TopShareClassGrid = dao.QueryTopShareClass();

            // 查詢-政策性課程專區-Policy course area
            model.PolicyClassGrid = dao.QueryPolicyClass();

            // 查詢-歷史政策性課程-Historical policy course
            model.PolicyClassHisGrid = dao.QueryPolicyClassHis();

            // 設定最新消息明細頁導回依據
            sm.NewsIndexController = @Url.Action("Index", "Home");

            return View("Index", model);
        }

        /// <summary> 關鍵字搜尋(google search) </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GoogleSearch()
        {
            ViewBag.keyWord = string.Empty;
            //LOG.Debug(string.Format("Request.QueryString[q]: {0}", Request.QueryString["q"]));
            //LOG.Debug(string.Format("Session[keyWordSearch]: {0}", Session["keyWordSearch"] ?? "[null]"));
            try
            {
                if (Session["keyWordSearch"] == null) { Session["keyWordSearch"] = Request.QueryString["q"]; }
                if (Session["keyWordSearch"] != null) { ViewBag.keyWord = Session["keyWordSearch"]; }
                Session["keyWordSearch"] = null;
            }
            catch (Exception ex)
            {
                LOG.Error("GoogleSearch 有誤!" + ex.Message, ex);
                return base.SetPageNotFound();
            }
            return View();
        }

        public ActionResult Detailv2()
        {
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }

            string subFunID = Request.QueryString["SUFU"];
            //空白-str_SEQ-異常
            if (string.IsNullOrEmpty(subFunID)) { return base.SetPageNotFound(); }
            string str_SEQ = Request.QueryString["SEQ"];
            //空白-str_SEQ-異常
            if (string.IsNullOrEmpty(str_SEQ)) { return base.SetPageNotFound(); }
            string str_WXR = Request.QueryString["WXR"];
            //空白-str_WXR-異常
            if (string.IsNullOrEmpty(str_WXR)) { return base.SetPageNotFound(); }
            int i_seqno; /*非數字的異常 1/2*/
            if (!int.TryParse(subFunID, out i_seqno)) { return base.SetPageNotFound(); }
            string ct_name = string.Empty;
            if (subFunID.Equals("1")) { ct_name = "News"; }
            if (subFunID.Equals("2")) { ct_name = "Plan"; }
            /*非數字的異常 1/2*/
            if (string.IsNullOrEmpty(subFunID)) { return base.SetPageNotFound(); }
            return RedirectToAction("Detailv2", ct_name, new { SEQ = str_SEQ, WXR = str_WXR });
        }

        /// <summary> 本次新冠肺炎點擊率統計邏輯 </summary>
        /// <returns></returns>
        public ActionResult Url1()
        {
            int i_clicktype = 1; //1:新冠肺炎點擊
            HttpContext context = this.HttpContext.ApplicationInstance.Context;
            string str_UserHostIp = MyCommonUtil.GetIpAddress(context);

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            dao.AddTBViewRecord2(str_UserHostIp, i_clicktype);

            //新冠肺炎 轉址 //string s_Url1 = "https://www.wda.gov.tw/cp.aspx?n=17C36B0EEE3BA5B8";
            //首頁  業務專區  防疫相關勞動權益與協助措施 疫情紓困
            string s_Url1 = "https://www.mol.gov.tw/topic/44761/48532/";
            return Redirect(s_Url1);
        }

        public ActionResult HNF()
        {
            return new HttpNotFoundResult();
        }

        public ActionResult MailTest22()
        {
            // xx http://localhost:2866//Home/MailTest22
            ExceptionHandler handler = new ExceptionHandler(new ErrorPageController());

            string sMailBody = string.Empty;
            string str_LocalAddr = MyCommonUtil.GetLocalAddr("2");
            sMailBody += string.Format("時間：{0}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")); //存入時間
            sMailBody += string.Format("LocalAddr：{0}\n", str_LocalAddr);

            string s_log1 = "Email OK!";
            try
            {
                s_log1 = string.Format("MailTest22({0}):\n\n{1}\n", "Email OK!", sMailBody);
                handler.SendMailTest(s_log1, handler.cst_EmailtoMe);
            }
            catch (Exception ex)
            {
                s_log1 = string.Format("MailTest22({0}):\n\n{1}\n ex.Message:\n{2}\n ex.StackTrace:\n{3}\n", "Email ERROR!", sMailBody, ex.Message, ex.StackTrace);
                s_log1 += string.Format("ex.ToString:{0}\n", ex.ToString());
            }
            s_log1 = s_log1.Replace("\n", "<br>\n");
            return Content(s_log1);
        }

    }
}