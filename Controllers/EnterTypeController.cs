using Geo.Grid.Common.Models;
using log4net;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Tesseract;
using Turbo.Commons;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Services;
//using Point = OpenCvSharp.Point;
//using System.IO.Compression;//using System.Data.SqlClient;//using System.Configuration;
//using System.Drawing.Imaging;
//using System.Web.UI;

namespace WDAIIP.WEB.Controllers
{
    public class EnterTypeController : LoginBaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 報名資料維護-首頁
        /// </summary>
        /// <returns></returns>
        // GET: EnterType
        public ActionResult Index()
        {
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();

            if (string.IsNullOrEmpty(sm.ACID))
            {
                rtn = View();
            }
            else
            {
                rtn = CheckEdit();
            }

            return rtn;
        }

        /// <summary>
        /// 報名資料維護-
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckEdit()
        {
            SessionModel sm = SessionModel.Get();
            SignUpViewModel model = new SignUpViewModel();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu07;

            model.Form.NAME = sm.UserName;
            model.Form.BIRTHDAY = sm.Birthday;
            model.Form.IDNO = sm.ACID;

            return View("CheckEdit", model);
        }

        /// <summary> 報名資料維護-資料維護頁 </summary>
        /// <returns></returns>
        public ActionResult Detail(SignUpViewModel model)
        {
            //LOG.Debug("GET EnterType/Detail");
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBService serv = new WDAIIPWEBService();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            model.Detail = new SignUpDetailModel();

            string s_IDNO = (model != null && model.Form != null) ? model.Form.IDNO : string.Empty;

            string strMsg = "";

            strMsg = CheckPage(model.Form);

            if (!string.IsNullOrEmpty(strMsg))
            {
                sm.LastResultMessage = strMsg;
                sm.RedirectUrlAfterBlock = "";
                rtn = View("CheckEdit", model);
            }
            else
            {
                // 檢測是否停止報名(僅顯示提示訊息用)
                serv.StopEnterTempMsg();

                #region 黑名單判斷
                IList<Hashtable> listblock = dao.GetBlackList(s_IDNO);
                //IList<Hashtable> listblock = (new WDAIIPWEBDAO()).GetBlackList2(s_IDNO, s_tplanID);
                strMsg = MyCommonUtil.Show_BlockMsg1(listblock);
                #endregion

                if (string.IsNullOrEmpty(strMsg))
                {
                    //1.使用前次報名資料 STUD_STUDENTINFO / STUD_ENTERTEMP2 //this.LoadTemp2(model);
                    //2.使用Stud_EnterTemp3 報名資料維護。 //this.LoadTemp3(model);
                    //3.登入成功 '採用 Member 資料 //this.LoadMember(model);

                    //載入會員報名資料
                    serv.LoadEnterData(model);

                    //判斷是否需顯示「是否維護產投報名資料」選項的flag
                    model.Detail.IsShowSelEditPlan28 = (string.IsNullOrEmpty(model.Detail.ISPLAN28) || "Y".Equals(model.Detail.ISPLAN28) ? "Y" : "N");
                }

                MyCommonUtil.HtmlDecode(model.Detail); //2019-01-18 fix 中文編碼轉置問題（&#XXXXX;）

                rtn = View("Detail", model);
            }

            return rtn;
        }

        public ActionResult Detail2(string Epi, string Epb, string Ept)
        {
            //LOG.Debug("GET EnterType/Detail");
            ActionResult rtn = null;
            string strMsg = "";
            SessionModel sm = SessionModel.Get();
            var model = new SignUpViewModel();
            var form = new SignUpFormModel();
            var detail = new SignUpDetailModel();
            model.Form = form;
            model.Detail = detail;

            string sm_IDNO = null, sm_BIRTHDAY = null; DateTime? stamptime1 = null;
            //LOG.Debug(string.Concat("#SU7.Epi:", Epi, ",Epb:", Epb, ",Ept:", Ept));
            try
            {
                model.Form.NAME = sm.UserName;
                model.Form.BIRTHDAY = sm.Birthday;
                model.Form.IDNO = sm.ACID;
                //Console.WriteLine(((string)null).Length);
                var deEpi = MyCommonUtil.TkDecrypt(Epi);
                var deEpb = MyCommonUtil.TkDecrypt(Epb);
                var deEpt = MyCommonUtil.TkDecrypt(Ept);
                stamptime1 = UserInfoVM.GetDateTimeFmt1(deEpt).Value; sm_IDNO = sm.ACID; sm_BIRTHDAY = sm.Birthday;
                if (string.IsNullOrEmpty(sm_IDNO)) { throw new ArgumentNullException("ACID"); }
                if (string.IsNullOrEmpty(sm_BIRTHDAY)) { throw new ArgumentNullException("BIRTHDAY"); }
                //LOG.Debug(string.Concat("#SU7.deEpi:", deEpi, ",deEpb:", deEpb, ",deEpt:", deEpt));
            }
            catch (Exception ex)
            {
                strMsg = string.Concat("儲存資訊有誤，", ex.Message);
                strMsg = strMsg.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
                LOG.Error(strMsg, ex); //return Json(new { result = new { Success = false, Message = message, } });
                sm.LastErrorMessage = strMsg;
                sm.RedirectUrlAfterBlock = "";
                rtn = View("CheckEdit", model);
                return rtn;
            }

            WDAIIPWEBService serv = new WDAIIPWEBService();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            //string s_IDNO = (model != null && model.Form != null) ? model.Form.IDNO : string.Empty;
            strMsg = CheckPage(model.Form);
            if (!string.IsNullOrEmpty(strMsg))
            {
                sm.LastErrorMessage = strMsg;
                sm.RedirectUrlAfterBlock = "";
                rtn = View("CheckEdit", model);
                return rtn;
            }
            else
            {
                // 檢測是否停止報名(僅顯示提示訊息用)
                serv.StopEnterTempMsg();

                #region 黑名單判斷
                IList<Hashtable> listblock = dao.GetBlackList(sm_IDNO);
                //IList<Hashtable> listblock = (new WDAIIPWEBDAO()).GetBlackList2(s_IDNO, s_tplanID);
                strMsg = MyCommonUtil.Show_BlockMsg1(listblock);
                #endregion

                if (string.IsNullOrEmpty(strMsg))
                {
                    //1.使用前次報名資料 STUD_STUDENTINFO / STUD_ENTERTEMP2 //this.LoadTemp2(model);
                    //2.使用Stud_EnterTemp3 報名資料維護。 //this.LoadTemp3(model);
                    //3.登入成功 '採用 Member 資料 //this.LoadMember(model);

                    //載入會員報名資料
                    serv.LoadEnterData(model);

                    //判斷是否需顯示「是否維護產投報名資料」選項的flag
                    model.Detail.IsShowSelEditPlan28 = (string.IsNullOrEmpty(model.Detail.ISPLAN28) || "Y".Equals(model.Detail.ISPLAN28) ? "Y" : "N");
                }

                MyCommonUtil.HtmlDecode(model.Detail); //2019-01-18 fix 中文編碼轉置問題（&#XXXXX;）

                rtn = View("Detail", model);
            }
            return rtn;
        }

        /// <summary> 報名資料維護-儲存 </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(SignUpViewModel model)
        {
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            rtn = View("Detail", model);

            try
            {
                // 表單欄位檢核
                model.Valid(ModelState);

                if (ModelState.IsValid)
                {
                    ModelState.Clear();

                    // 查詢產投報名暫存資料(stud_entertemp3)
                    IList<TblSTUD_ENTERTEMP3> enterTemp3List = dao.GeteSETID3(model.Detail.IDNO);

                    // 報名資料維護
                    model.Detail.DB_ACTION = (enterTemp3List != null && enterTemp3List.Count > 0) ? "UPDATE" : "CREATE";

                    dao.SaveCaseData("StudEnterTemp3", model.Detail, true);

                    sm.LastResultMessage = "資料儲存成功";
                    sm.RedirectUrlAfterBlock = "";
                    rtn = RedirectToAction("Index", "ClassSearch");
                }
            }
            catch (Exception ex)
            {
                string s_ErrorMessage = string.Concat("儲存失敗，該學員報名資料異常 或 資料庫異常，請重試!!<br>", "請再試一次，造成您不便之處，還請見諒。<br>", "(若持續出現此問題，請聯絡系統管理者)!!!");
                sm.LastErrorMessage = s_ErrorMessage;
                LOG.Error(string.Concat("#EnterType Save ex:", ex.Message, "\n", s_ErrorMessage), ex);
            }

            return rtn;
        }

        /// <summary>儲存-上傳身分證</summary>
        /// <param name="Epi"></param>
        /// <param name="Epb"></param>
        /// <param name="Ept"></param>
        /// <param name="ACID"></param>
        /// <param name="TwBIRTH"></param>
        /// <param name="TwISSUEDATE"></param>
        /// <param name="ISSUEPLACE"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveUploadDBBC(string Epi, string Epb, string Ept,
            string ACID, string TwBIRTH, string TwISSUEDATE, string ISSUEPLACE)
        {
            SessionModel sm = SessionModel.Get();
            string sm_IDNO = null, sm_BIRTHDAY = null; DateTime? stamptime1 = null;
            //LOG.Debug(string.Concat("#SU7.Epi:", Epi, ",Epb:", Epb, ",Ept:", Ept));
            try
            {
                //Console.WriteLine(((string)null).Length);
                var deEpi = MyCommonUtil.TkDecrypt(Epi);
                var deEpb = MyCommonUtil.TkDecrypt(Epb);
                var deEpt = MyCommonUtil.TkDecrypt(Ept);
                stamptime1 = UserInfoVM.GetDateTimeFmt1(deEpt).Value; sm_IDNO = sm.ACID; sm_BIRTHDAY = sm.Birthday;
                if (string.IsNullOrEmpty(sm_IDNO)) { throw new ArgumentNullException("ACID"); }
                if (string.IsNullOrEmpty(sm_BIRTHDAY)) { throw new ArgumentNullException("BIRTHDAY"); }
                //LOG.Debug(string.Concat("#SU7.deEpi:", deEpi, ",deEpb:", deEpb, ",deEpt:", deEpt));
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);
                var message = string.Concat("儲存資訊有誤，", ex.Message);
                return Json(new { result = new { Success = false, Message = message, } });
            }

            DateTime? dBIRTH = null;
            DateTime? dISSUEDATE = null;
            try { dBIRTH = MyCommonUtil.IsDate(MyCommonUtil.DateRoc2Ad(TwBIRTH)) ? HelperUtil.TransTwToDateTime(TwBIRTH) : null; }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("TwBIRTH: ", TwBIRTH, ",ex.Message: ", ex.Message), ex);
            }
            try { dISSUEDATE = MyCommonUtil.IsDate(MyCommonUtil.DateRoc2Ad(TwISSUEDATE)) ? HelperUtil.TransTwToDateTime(TwISSUEDATE) : null; }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("TwISSUEDATE: ", TwISSUEDATE, ",ex.Message: ", ex.Message), ex);
            }
            //[ACID] [varchar] (15)  NULL , //[BIRTH] [datetime] NULL ,//[ISSUEDATE] [datetime] NULL ,	//[ISSUEPLACE] [nvarchar] (100)  NULL ,
            int max_length = 12;
            ACID = MyHelperUtil.GetEnglishLettersAndNumbers(MyHelperUtil.ChangeIDNO(ACID));
            if (!string.IsNullOrEmpty(ACID) && ACID.Length > max_length) { ACID = ACID.Substring(0, max_length); }
            max_length = 100;
            if (!string.IsNullOrEmpty(ISSUEPLACE) && ISSUEPLACE.Length > max_length) { ISSUEPLACE = ISSUEPLACE.Substring(0, max_length); }

            var orgData = new TblE_IMG2
            {
                CREATEDATE = stamptime1,
                ACID = ACID,
                BIRTH = dBIRTH,
                ISSUEDATE = dISSUEDATE,
                ISSUEPLACE = ISSUEPLACE
            };
            SaveExIMG2(orgData, sm, "save1");
            //return CheckEdit(); 
            return Detail2(Epi, Epb, Ept);
        }

        /// <summary>取得系統時間(民國年時間 yyyy) 檢核錯誤</summary>
        /// <param name="ACID"></param>
        /// <param name="TwBIRTH"></param>
        /// <param name="TwISSUEDATE"></param>
        /// <param name="ISSUEPLACE"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSysDate2(string ACID, string TwBIRTH, string TwISSUEDATE, string ISSUEPLACE, string Epi, string Epb, string Ept)
        {
            var result = new AjaxResultStruct();

            SessionModel sm = SessionModel.Get();
            string sm_IDNO = null, sm_BIRTHDAY = null; DateTime? stamptime1 = null;
            //LOG.Debug(string.Concat("#SU7.Epi:", Epi, ",Epb:", Epb, ",Ept:", Ept));
            try
            {
                //Console.WriteLine(((string)null).Length);
                var deEpi = MyCommonUtil.TkDecrypt(Epi);
                var deEpb = MyCommonUtil.TkDecrypt(Epb);
                var deEpt = MyCommonUtil.TkDecrypt(Ept);
                stamptime1 = UserInfoVM.GetDateTimeFmt1(deEpt).Value; sm_IDNO = sm.ACID; sm_BIRTHDAY = sm.Birthday;
                if (string.IsNullOrEmpty(sm_IDNO)) { throw new ArgumentNullException("ACID"); }
                if (string.IsNullOrEmpty(sm_BIRTHDAY)) { throw new ArgumentNullException("BIRTHDAY"); }
                //LOG.Debug(string.Concat("#SU7.deEpi:", deEpi, ",deEpb:", deEpb, ",deEpt:", deEpt));
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);
                result.data = string.Concat("儲存資訊有誤，", ex.Message);
                result.status = false;
                return Content(result.Serialize(), "application/json");
                //var message = string.Concat("儲存資訊有誤，", ex.Message);
                //return Json(new { result = new { Success = false, Message = message, } });
            }

            DateTime? dBIRTH = null;
            try { dBIRTH = MyCommonUtil.IsDate(MyCommonUtil.DateRoc2Ad(TwBIRTH)) ? HelperUtil.TransTwToDateTime(TwBIRTH) : null; }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("TwBIRTH: ", TwBIRTH, ",ex.Message: ", ex.Message), ex);
            }
            DateTime? dISSUEDATE = null;
            try { dISSUEDATE = MyCommonUtil.IsDate(MyCommonUtil.DateRoc2Ad(TwISSUEDATE)) ? HelperUtil.TransTwToDateTime(TwISSUEDATE) : null; }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("TwISSUEDATE: ", TwISSUEDATE, ",ex.Message: ", ex.Message), ex);
            }

            if (string.IsNullOrEmpty(ACID) || ACID != sm.ACID)
            {
                result.data = "比對身分證字號有誤! " + (ACID ?? "");
                result.status = false;
                return Content(result.Serialize(), "application/json");
            }
            else if (!dBIRTH.HasValue || dBIRTH.Value.ToString("yyyy/MM/dd") != sm.Birthday)
            {
                result.data = "比對出生年月日有誤! " + (dBIRTH.HasValue ? dBIRTH.Value.ToString("yyyy-MM-dd") : "");
                result.status = false;
                return Content(result.Serialize(), "application/json");
            }
            else if (!dISSUEDATE.HasValue)
            {
                result.data = "發證日期有誤!" + (string.IsNullOrEmpty(TwISSUEDATE) ? "(不可為空)" : "(日期錯誤)");
                result.status = false;
                return Content(result.Serialize(), "application/json");
            }
            else if (string.IsNullOrEmpty(ISSUEPLACE))
            {
                result.data = "發證地有誤!(不可為空)";
                result.status = false;
                return Content(result.Serialize(), "application/json");
            }
            var dao = new WDAIIPWEBDAO();
            var getData1 = dao.GetRow(new TblE_IMG2() { IDNO = sm.ACID, CREATEDATE = stamptime1 });
            if (getData1 == null)
            {
                result.data = "上傳資料有誤!(請重新查詢操作)";
                result.status = false;
                return Content(result.Serialize(), "application/json");
            }
            else
            {
                if (string.IsNullOrEmpty(getData1.FILEPATH1) || string.IsNullOrEmpty(getData1.FILENAME1) || string.IsNullOrEmpty(getData1.FILENAME1W))
                {
                    result.data = "上傳資料有誤!(正面未上傳)";
                    result.status = false;
                    return Content(result.Serialize(), "application/json");
                }
                if (string.IsNullOrEmpty(getData1.FILEPATH2) || string.IsNullOrEmpty(getData1.FILENAME2) || string.IsNullOrEmpty(getData1.FILENAME2W))
                {
                    result.data = "上傳資料有誤!(背面未上傳)";
                    result.status = false;
                    return Content(result.Serialize(), "application/json");
                }
            }
            result.data = MyHelperUtil.DateTimeToTwFormatLongString((new MyKeyMapDAO()).GetSysDateNow());
            result.status = true;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>儲存-上傳銀行存摺 BankVM</summary>
        /// <param name="Epi"></param>
        /// <param name="Epb"></param>
        /// <param name="Ept"></param>
        /// <param name="ACCTHEADNO"></param>
        /// <param name="BANKNAME"></param>
        /// <param name="ACCTEXNO"></param>
        /// <param name="EXBANKNAME"></param>
        /// <param name="ACCTNO"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveUpload70643D(string Epi, string Epb, string Ept,
            string ACCTHEADNO, string BANKNAME, string ACCTEXNO, string EXBANKNAME, string ACCTNO)
        {
            SessionModel sm = SessionModel.Get();
            string sm_IDNO = null, sm_BIRTHDAY = null; DateTime? stamptime1 = null;
            //LOG.Debug(string.Concat("#SU7.Epi:", Epi, ",Epb:", Epb, ",Ept:", Ept));
            try
            {
                var deEpi = MyCommonUtil.TkDecrypt(Epi);
                var deEpb = MyCommonUtil.TkDecrypt(Epb);
                var deEpt = MyCommonUtil.TkDecrypt(Ept);
                stamptime1 = UserInfoVM.GetDateTimeFmt1(deEpt).Value; sm_IDNO = sm.ACID; sm_BIRTHDAY = sm.Birthday;
                if (string.IsNullOrEmpty(sm_IDNO)) { throw new ArgumentNullException("ACID"); }
                if (string.IsNullOrEmpty(sm_BIRTHDAY)) { throw new ArgumentNullException("BIRTHDAY"); }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);
                var message = string.Concat("儲存資訊有誤，", ex.Message);
                return Json(new { result = new { Success = false, Message = message, } });
            }

            ACCTHEADNO = MyHelperUtil.SafeTrim(ACCTHEADNO);
            string postno = null;
            int iAcctmode = (ACCTHEADNO == "700" ? 0 : 1);
            if (iAcctmode == 0) { postno = ACCTEXNO; }
            //[ACCTMODE] [numeric] (10,0)  NULL ,//[POSTNO] [nvarchar] (50)  NULL ,//[ACCTHEADNO] [nvarchar] (50)  NULL ,
            //[BANKNAME] [nvarchar] (100)  NULL ,//[ACCTEXNO] [nvarchar] (50)  NULL ,//[EXBANKNAME] [nvarchar] (100)  NULL ,//[ACCTNO] [nvarchar] (50)  NULL ,
            int max_length = 30;
            if (!string.IsNullOrEmpty(postno) && postno.Length > max_length) { postno = postno.Substring(0, max_length); }
            max_length = 50;
            if (!string.IsNullOrEmpty(ACCTHEADNO) && ACCTHEADNO.Length > max_length) { ACCTHEADNO = ACCTHEADNO.Substring(0, max_length); }
            if (!string.IsNullOrEmpty(ACCTEXNO) && ACCTEXNO.Length > max_length) { ACCTEXNO = ACCTEXNO.Substring(0, max_length); }
            if (!string.IsNullOrEmpty(ACCTNO) && ACCTNO.Length > max_length) { ACCTNO = ACCTNO.Substring(0, max_length); }
            max_length = 100;
            if (!string.IsNullOrEmpty(BANKNAME) && BANKNAME.Length > max_length) { BANKNAME = BANKNAME.Substring(0, max_length); }
            if (!string.IsNullOrEmpty(EXBANKNAME) && EXBANKNAME.Length > max_length) { EXBANKNAME = EXBANKNAME.Substring(0, max_length); }

            var orgData = new TblE_IMG1
            {
                CREATEDATE = stamptime1,
                IDNO = sm.ACID,
                ACCTMODE = iAcctmode,
                POSTNO = postno,
                ACCTHEADNO = ACCTHEADNO,
                BANKNAME = BANKNAME,
                ACCTEXNO = ACCTEXNO,
                EXBANKNAME = EXBANKNAME,
                ACCTNO = ACCTNO,
                MODIFYACCT = sm.ACID
            };
            SaveExIMG1(orgData, sm, "save1");
            //return CheckEdit(); //return View("Upload70643D");
            return Detail2(Epi, Epb, Ept);
        }

        /// <summary>儲存-刪除／上傳銀行存摺</summary>
        /// <param name="orgData"></param>
        /// <param name="sm"></param>
        /// <param name="saveType"></param>
        /// <returns></returns>
        private string SaveExIMG1(TblE_IMG1 orgData, SessionModel sm, string saveType)
        {
            if (orgData == null) { return null; }
            var dao = new WDAIIPWEBDAO();
            if (saveType == "deldata1")
            {
                //原資料修改
                var wData = new TblE_IMG1 { IDNO = sm.ACID, ISUSE = "Y" };
                var wEIMG1 = dao.GetRow(wData); if (wEIMG1 == null) { return null; } //(使用中)查無資料異常

                wData = new TblE_IMG1 { IDNO = sm.ACID, ISUSE = "Y" };
                var wEIMGlist = dao.GetRowList(wData); //(多筆修正)
                foreach (var item1 in wEIMGlist)
                {
                    if (!string.IsNullOrEmpty(item1.FILEPATH1))
                    {
                        var v_webPath = item1.FILEPATH1;
                        var v_savePath = HttpContext.Server.MapPath(v_webPath);//原
                        var v_webPathDel = item1.FILEPATH1.Replace("/ExIMG", "/ExIMGDEL");//刪
                        var v_savePathDel = HttpContext.Server.MapPath(v_webPathDel);//刪MapPath
                        if (!string.IsNullOrEmpty(item1.FILENAME1))
                        {
                            var v_fileNM = Path.GetFileName(item1.FILENAME1);
                            var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                            MoveFile(v_DELFILENM, v_savePathDel);
                        }
                        if (!string.IsNullOrEmpty(item1.FILENAME1W))
                        {
                            var v_fileNMW = Path.GetFileName(item1.FILENAME1W);
                            var v_DELFILENMW = string.Concat(v_savePath, v_fileNMW);
                            MoveFile(v_DELFILENMW, v_savePathDel);
                        }
                        ClearFieldMap clearFields = new ClearFieldMap();
                        clearFields.Add("ISUSE");
                        clearFields.Add("ISDEL");
                        clearFields.Add("FILEPATH1");
                        var wData2 = new TblE_IMG1 { IDNO = sm.ACID, EMID1 = item1.EMID1 };
                        var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                        orgData.ISUSE = null;//清除使用註記
                        orgData.ISDEL = "Y"; //刪除註記
                        orgData.FILEPATH1 = v_webPathDel; //刪除PATH
                        orgData.MODIFYACCT = sm.ACID;
                        orgData.MODIFYDATE = SysDateNow;
                        dao.Update(orgData, wData2, clearFields);
                    }
                    else
                    {
                        ClearFieldMap clearFields = new ClearFieldMap();
                        clearFields.Add("ISUSE");
                        clearFields.Add("ISDEL");
                        var wData2 = new TblE_IMG1 { IDNO = sm.ACID, EMID1 = item1.EMID1 };
                        var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                        orgData.ISUSE = null;//清除使用註記
                        orgData.ISDEL = "Y"; //刪除註記
                        orgData.MODIFYACCT = sm.ACID;
                        orgData.MODIFYDATE = SysDateNow;
                        dao.Update(orgData, wData2, clearFields);
                    }

                }
            }

            //執行刪除時沒有 CREATEDATE (其他任何情況都應該有 CREATEDATE)
            if (!orgData.CREATEDATE.HasValue) { return null; }
            var getData1 = dao.GetRow(new TblE_IMG1() { IDNO = sm.ACID, CREATEDATE = orgData.CREATEDATE });
            if (getData1 == null) //查無資料新增 且有建立日期
            {
                //刪除無效資料 MODIFYDATE is null
                var ChkDelDatalist1 = dao.GetRowList(new TblE_IMG1() { IDNO = sm.ACID });
                foreach (var item in ChkDelDatalist1)
                {
                    //刪除無效資料 MODIFYDATE is null
                    if (!item.MODIFYDATE.HasValue)
                    {
                        if (!string.IsNullOrEmpty(item.FILEPATH1))
                        {
                            if (!string.IsNullOrEmpty(item.FILENAME1))
                            {
                                //刪除無效資料 MODIFYDATE is null
                                var v_webPath = item.FILEPATH1;
                                var v_savePath = HttpContext.Server.MapPath(v_webPath);
                                var v_fileNM = Path.GetFileName(item.FILENAME1);
                                var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                                DeleteTemporaryFiles(v_DELFILENM);
                            }
                            if (!string.IsNullOrEmpty(item.FILENAME1W))
                            {
                                //刪除無效資料 MODIFYDATE is null
                                var v_webPath = item.FILEPATH1;
                                var v_savePath = HttpContext.Server.MapPath(v_webPath);
                                var v_fileNM = Path.GetFileName(item.FILENAME1W);
                                var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                                DeleteTemporaryFiles(v_DELFILENM);
                            }
                        }
                        dao.Delete(new TblE_IMG1() { EMID1 = item.EMID1 });
                    }
                }

                //新增1筆基礎序號 //var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                orgData.EMID1 = dao.GetNewId("E_IMG1_EMID1_SEQ,E_IMG1,EMID1").Value; //dao.GetAutoNum()
                orgData.IDNO = sm.ACID;
                orgData.MODIFYACCT = sm.ACID;
                //orgData.MODIFYDATE = SysDateNow; //暫不用 modifydate
                dao.Insert(orgData);
                return null;
            }
            if (saveType == "up1")
            {
                //原資料修改
                ClearFieldMap clearFields = new ClearFieldMap();
                clearFields.Add("FILEPATH1");
                clearFields.Add("FILENAME1");
                clearFields.Add("FILENAME1W");
                clearFields.Add("SRCFILENAME1");
                var wData = new TblE_IMG1 { EMID1 = getData1.EMID1, IDNO = sm.ACID };
                var wEIMG1 = dao.GetRow(wData);
                if (wEIMG1 == null) { return null; } //查無資料異常

                if (!string.IsNullOrEmpty(wEIMG1.FILEPATH1))
                {
                    if (!string.IsNullOrEmpty(wEIMG1.FILENAME1))
                    {
                        //刪除無效資料 MODIFYDATE is null
                        var v_webPath = wEIMG1.FILEPATH1;
                        var v_savePath = HttpContext.Server.MapPath(v_webPath);
                        var v_fileNM = Path.GetFileName(wEIMG1.FILENAME1);
                        var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                        DeleteTemporaryFiles(v_DELFILENM);
                    }
                    if (!string.IsNullOrEmpty(wEIMG1.FILENAME1W))
                    {
                        //刪除無效資料 MODIFYDATE is null
                        var v_webPath = wEIMG1.FILEPATH1;
                        var v_savePath = HttpContext.Server.MapPath(v_webPath);
                        var v_fileNM = Path.GetFileName(wEIMG1.FILENAME1W);
                        var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                        DeleteTemporaryFiles(v_DELFILENM);
                    }
                }

                //var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                orgData.MODIFYACCT = sm.ACID;
                //orgData.MODIFYDATE = SysDateNow; //暫不用 modifydate
                dao.Update(orgData, wData, clearFields);
            }
            else if (saveType == "save1")
            {
                {
                    var ChkUptDatalist1 = dao.GetRowList(new TblE_IMG1() { IDNO = sm.ACID });
                    foreach (var item in ChkUptDatalist1)
                    {
                        var clearFields = new ClearFieldMap().Add("ISUSE");
                        var wData = new TblE_IMG1 { EMID1 = item.EMID1 };
                        var uData = new TblE_IMG1 { ISUSE = null };
                        dao.Update(uData, wData, clearFields);
                    }
                }
                {
                    //原資料修改
                    ClearFieldMap clearFields = new ClearFieldMap();
                    clearFields.Add("ACCTMODE");
                    clearFields.Add("POSTNO");
                    clearFields.Add("ACCTHEADNO");
                    clearFields.Add("BANKNAME");
                    clearFields.Add("ACCTEXNO");
                    clearFields.Add("EXBANKNAME");
                    clearFields.Add("ACCTNO");
                    var wData = new TblE_IMG1 { EMID1 = getData1.EMID1, IDNO = sm.ACID };
                    var wEIMG1 = dao.GetRow(wData);
                    if (wEIMG1 == null) { return null; } //查無資料異常
                    var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                    orgData.ISUSE = "Y";
                    orgData.MODIFYACCT = sm.ACID;
                    orgData.MODIFYDATE = SysDateNow; //增加 modifydate
                    dao.Update(orgData, wData, clearFields);
                }
                {
                    orgData.POSTNO = string.IsNullOrEmpty(orgData.POSTNO) ? orgData.POSTNO : orgData.POSTNO.Replace("-", "");
                    orgData.ACCTEXNO = string.IsNullOrEmpty(orgData.ACCTEXNO) ? orgData.ACCTEXNO : orgData.ACCTEXNO.Replace("-", "");
                    orgData.ACCTNO = string.IsNullOrEmpty(orgData.ACCTNO) ? orgData.ACCTNO : orgData.ACCTNO.Replace("-", "");
                    var newTmp3 = new TblSTUD_ENTERTEMP3
                    {
                        ACCTMODE = orgData.ACCTMODE,
                        POSTNO = orgData.POSTNO,
                        ACCTHEADNO = orgData.ACCTHEADNO,
                        BANKNAME = orgData.BANKNAME,
                        ACCTEXNO = orgData.ACCTEXNO,
                        EXBANKNAME = orgData.EXBANKNAME,
                        ACCTNO = orgData.ACCTNO,
                        MODIFYACCT = sm.ACID
                    };
                    // 可被清空的欄位
                    ClearFieldMap cfm = new ClearFieldMap();
                    cfm.Add("ACCTMODE");
                    cfm.Add("POSTNO");
                    cfm.Add("ACCTHEADNO");
                    cfm.Add("BANKNAME");
                    cfm.Add("ACCTEXNO");
                    cfm.Add("EXBANKNAME");
                    cfm.Add("ACCTNO");
                    var whereTmp3 = new TblSTUD_ENTERTEMP3 { IDNO = sm.ACID };
                    var oldTmp3 = dao.GetRow(whereTmp3);
                    if (oldTmp3 == null) { return null; }
                    whereTmp3 = new TblSTUD_ENTERTEMP3 { IDNO = sm.ACID, ESETID3 = oldTmp3.ESETID3 };
                    dao.Update(newTmp3, whereTmp3, cfm);
                }
            }
            return null;
        }

        /// <summary>儲存-刪除／上傳身分證</summary>
        /// <param name="orgData"></param>
        /// <param name="sm"></param>
        /// <param name="saveType"></param>
        /// <returns></returns>
        private string SaveExIMG2(TblE_IMG2 orgData, SessionModel sm, string saveType)
        {
            if (orgData == null) { return null; }
            var dao = new WDAIIPWEBDAO();
            if (saveType == "deldata1")
            {
                //原資料修改
                var wData = new TblE_IMG2 { IDNO = sm.ACID, ISUSE = "Y" };
                var wEIMG1 = dao.GetRow(wData); if (wEIMG1 == null) { return null; } //查無資料異常

                wData = new TblE_IMG2 { IDNO = sm.ACID, ISUSE = "Y" };
                var wEIMGlist = dao.GetRowList(wData); //(多筆修正)
                foreach (var item1 in wEIMGlist)
                {
                    var fgf1NG = true;//FILEPATH1 無資料
                    var fgf2NG = true;//FILEPATH2 無資料
                    if (!string.IsNullOrEmpty(item1.FILEPATH1))
                    {
                        fgf1NG = false;
                        var v_webPath = item1.FILEPATH1;
                        var v_savePath = HttpContext.Server.MapPath(v_webPath);//原
                        var v_webPathDel = item1.FILEPATH1.Replace("/ExIMG", "/ExIMGDEL");//刪
                        var v_savePathDel = HttpContext.Server.MapPath(v_webPathDel);//刪MapPath
                        if (!string.IsNullOrEmpty(item1.FILENAME1))
                        {
                            var v_fileNM = Path.GetFileName(item1.FILENAME1);
                            var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                            MoveFile(v_DELFILENM, v_savePathDel);
                        }
                        if (!string.IsNullOrEmpty(item1.FILENAME1W))
                        {
                            var v_fileNMW = Path.GetFileName(item1.FILENAME1W);
                            var v_DELFILENMW = string.Concat(v_savePath, v_fileNMW);
                            MoveFile(v_DELFILENMW, v_savePathDel);
                        }
                        ClearFieldMap clearFields = new ClearFieldMap();
                        clearFields.Add("ISUSE");
                        clearFields.Add("ISDEL");
                        clearFields.Add("FILEPATH1");
                        var wData2 = new TblE_IMG2 { IDNO = sm.ACID, EMID2 = item1.EMID2 };
                        var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                        orgData.ISUSE = null;//清除使用註記
                        orgData.ISDEL = "Y"; //刪除註記
                        orgData.FILEPATH1 = v_webPathDel; //刪除PATH
                        orgData.MODIFYACCT = sm.ACID;
                        orgData.MODIFYDATE = SysDateNow;
                        dao.Update(orgData, wData2, clearFields);
                    }
                    if (!string.IsNullOrEmpty(item1.FILEPATH2))
                    {
                        fgf2NG = false;
                        var v_webPath = item1.FILEPATH2;
                        var v_savePath = HttpContext.Server.MapPath(v_webPath);//原
                        var v_webPathDel = item1.FILEPATH2.Replace("/ExIMG", "/ExIMGDEL");//刪
                        var v_savePathDel = HttpContext.Server.MapPath(v_webPathDel);//刪MapPath
                        if (!string.IsNullOrEmpty(item1.FILENAME2))
                        {
                            var v_fileNM = Path.GetFileName(item1.FILENAME2);
                            var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                            MoveFile(v_DELFILENM, v_savePathDel);
                        }
                        if (!string.IsNullOrEmpty(item1.FILENAME2W))
                        {
                            var v_fileNMW = Path.GetFileName(item1.FILENAME2W);
                            var v_DELFILENMW = string.Concat(v_savePath, v_fileNMW);
                            MoveFile(v_DELFILENMW, v_savePathDel);
                        }
                        ClearFieldMap clearFields = new ClearFieldMap();
                        clearFields.Add("ISUSE");
                        clearFields.Add("ISDEL");
                        clearFields.Add("FILEPATH2");
                        var wData2 = new TblE_IMG2 { IDNO = sm.ACID, EMID2 = item1.EMID2 };
                        var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                        orgData.ISUSE = null;//清除使用註記
                        orgData.ISDEL = "Y"; //刪除註記
                        orgData.FILEPATH2 = v_webPathDel; //刪除PATH
                        orgData.MODIFYACCT = sm.ACID;
                        orgData.MODIFYDATE = SysDateNow;
                        dao.Update(orgData, wData2, clearFields);
                    }
                    //FILEPATH1/FILEPATH2都沒資料?
                    if (fgf1NG && fgf2NG)
                    {
                        ClearFieldMap clearFields = new ClearFieldMap();
                        clearFields.Add("ISUSE");
                        clearFields.Add("ISDEL");
                        var wData2 = new TblE_IMG2 { IDNO = sm.ACID, EMID2 = item1.EMID2 };
                        var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                        orgData.ISUSE = null;//清除使用註記
                        orgData.ISDEL = "Y"; //刪除註記
                        orgData.MODIFYACCT = sm.ACID;
                        orgData.MODIFYDATE = SysDateNow;
                        dao.Update(orgData, wData2, clearFields);
                    }
                }
            }

            //執行刪除時沒有 CREATEDATE (其他任何情況都應該有 CREATEDATE)
            if (!orgData.CREATEDATE.HasValue) { return null; }
            var getData1 = dao.GetRow(new TblE_IMG2() { IDNO = sm.ACID, CREATEDATE = orgData.CREATEDATE });
            if (getData1 == null)
            {
                //刪除無效資料
                var ChkDelDatalist1 = dao.GetRowList(new TblE_IMG2() { IDNO = sm.ACID });
                foreach (var item in ChkDelDatalist1)
                {
                    if (!item.MODIFYDATE.HasValue)
                    {
                        if (!string.IsNullOrEmpty(item.FILEPATH1))
                        {
                            if (!string.IsNullOrEmpty(item.FILENAME1))
                            {
                                //刪除無效資料 MODIFYDATE is null
                                var v_webPath = item.FILEPATH1;
                                var v_savePath = HttpContext.Server.MapPath(v_webPath);
                                var v_fileNM = Path.GetFileName(item.FILENAME1);
                                var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                                DeleteTemporaryFiles(v_DELFILENM);
                            }
                            if (!string.IsNullOrEmpty(item.FILENAME1W))
                            {
                                //刪除無效資料 MODIFYDATE is null
                                var v_webPath = item.FILEPATH1;
                                var v_savePath = HttpContext.Server.MapPath(v_webPath);
                                var v_fileNM = Path.GetFileName(item.FILENAME1W);
                                var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                                DeleteTemporaryFiles(v_DELFILENM);
                            }
                        }
                        if (!string.IsNullOrEmpty(item.FILEPATH2))
                        {
                            if (!string.IsNullOrEmpty(item.FILENAME2))
                            {
                                //刪除無效資料 MODIFYDATE is null
                                var v_webPath = item.FILEPATH2;
                                var v_savePath = HttpContext.Server.MapPath(v_webPath);
                                var v_fileNM = Path.GetFileName(item.FILENAME2);
                                var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                                DeleteTemporaryFiles(v_DELFILENM);
                            }
                            if (!string.IsNullOrEmpty(item.FILENAME2W))
                            {
                                //刪除無效資料 MODIFYDATE is null
                                var v_webPath = item.FILEPATH2;
                                var v_savePath = HttpContext.Server.MapPath(v_webPath);
                                var v_fileNM = Path.GetFileName(item.FILENAME2W);
                                var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                                DeleteTemporaryFiles(v_DELFILENM);
                            }
                        }
                        dao.Delete(new TblE_IMG2() { EMID2 = item.EMID2 });
                    }
                }

                //var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                orgData.EMID2 = dao.GetNewId("E_IMG2_EMID2_SEQ,E_IMG2,EMID2").Value; //dao.GetAutoNum()
                orgData.IDNO = sm.ACID;
                orgData.MODIFYACCT = sm.ACID;
                //orgData.MODIFYDATE = SysDateNow;
                dao.Insert(orgData);
                return null;
            }
            if (saveType == "up1")
            {
                //原資料修改
                ClearFieldMap clearFields = new ClearFieldMap();
                clearFields.Add("FILEPATH1");
                clearFields.Add("FILENAME1");
                clearFields.Add("FILENAME1W");
                clearFields.Add("SRCFILENAME1");
                var wData = new TblE_IMG2 { EMID2 = getData1.EMID2, IDNO = sm.ACID };
                var wEIMG2 = dao.GetRow(wData); if (wEIMG2 == null) { return null; } //查無資料異常

                if (!string.IsNullOrEmpty(wEIMG2.FILEPATH1))
                {
                    if (!string.IsNullOrEmpty(wEIMG2.FILENAME1))
                    {
                        //刪除無效資料 MODIFYDATE is null
                        var v_webPath = wEIMG2.FILEPATH1;
                        var v_savePath = HttpContext.Server.MapPath(v_webPath);
                        var v_fileNM = Path.GetFileName(wEIMG2.FILENAME1);
                        var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                        DeleteTemporaryFiles(v_DELFILENM);
                    }
                    if (!string.IsNullOrEmpty(wEIMG2.FILENAME1W))
                    {
                        //刪除無效資料 MODIFYDATE is null
                        var v_webPath = wEIMG2.FILEPATH1;
                        var v_savePath = HttpContext.Server.MapPath(v_webPath);
                        var v_fileNM = Path.GetFileName(wEIMG2.FILENAME1W);
                        var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                        DeleteTemporaryFiles(v_DELFILENM);
                    }
                }

                //var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                orgData.MODIFYACCT = sm.ACID;
                //orgData.MODIFYDATE = SysDateNow;
                dao.Update(orgData, wData, clearFields);
            }
            if (saveType == "up2")
            {
                //原資料修改
                ClearFieldMap clearFields = new ClearFieldMap();
                clearFields.Add("FILEPATH2");
                clearFields.Add("FILENAME2");
                clearFields.Add("FILENAME2W");
                clearFields.Add("SRCFILENAME2");
                var wData = new TblE_IMG2 { EMID2 = getData1.EMID2, IDNO = sm.ACID };
                var wEIMG2 = dao.GetRow(wData); if (wEIMG2 == null) { return null; } //查無資料異常

                if (!string.IsNullOrEmpty(wEIMG2.FILEPATH2))
                {
                    if (!string.IsNullOrEmpty(wEIMG2.FILENAME2))
                    {
                        //刪除無效資料 MODIFYDATE is null
                        var v_webPath = wEIMG2.FILEPATH2;
                        var v_savePath = HttpContext.Server.MapPath(v_webPath);
                        var v_fileNM = Path.GetFileName(wEIMG2.FILENAME2);
                        var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                        DeleteTemporaryFiles(v_DELFILENM);
                    }
                    if (!string.IsNullOrEmpty(wEIMG2.FILENAME2W))
                    {
                        //刪除無效資料 MODIFYDATE is null
                        var v_webPath = wEIMG2.FILEPATH1;
                        var v_savePath = HttpContext.Server.MapPath(v_webPath);
                        var v_fileNM = Path.GetFileName(wEIMG2.FILENAME2W);
                        var v_DELFILENM = string.Concat(v_savePath, v_fileNM);
                        DeleteTemporaryFiles(v_DELFILENM);
                    }
                }
                //var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                orgData.MODIFYACCT = sm.ACID;
                //orgData.MODIFYDATE = SysDateNow;
                dao.Update(orgData, wData, clearFields);
            }
            else if (saveType == "save1")
            {
                {
                    var ChkUptDatalist1 = dao.GetRowList(new TblE_IMG2() { IDNO = sm.ACID });
                    foreach (var item in ChkUptDatalist1)
                    {
                        var clearFields = new ClearFieldMap().Add("ISUSE");
                        var wData = new TblE_IMG2 { EMID2 = item.EMID2 };
                        var uData = new TblE_IMG2 { ISUSE = null };
                        dao.Update(uData, wData, clearFields);
                    }
                }
                {
                    //原資料修改 ,ACID,BIRTH,ISSUEDATE,ISSUEPLACE
                    ClearFieldMap clearFields = new ClearFieldMap();
                    clearFields.Add("ACID");
                    clearFields.Add("BIRTH");
                    clearFields.Add("ISSUEDATE");
                    clearFields.Add("ISSUEPLACE");
                    var wData = new TblE_IMG2 { EMID2 = getData1.EMID2, IDNO = sm.ACID };
                    var wEIMG2 = dao.GetRow(wData); if (wEIMG2 == null) { return null; } //查無資料異常

                    var SysDateNow = (new MyKeyMapDAO()).GetSysDateNow();
                    orgData.ISUSE = "Y";
                    orgData.MODIFYACCT = sm.ACID;
                    orgData.MODIFYDATE = SysDateNow;
                    dao.Update(orgData, wData, clearFields);
                }
            }
            return null;
        }

        /// <summary>
        /// 將指定的檔案從來源路徑移動到目標目錄。
        /// </summary>
        /// <param name="sourceFilePath">要移動的檔案的完整來源路徑 (例如: "c:\\temp1\\happy1.jpg")。</param>
        /// <param name="destinationDirectory">檔案要移動到的目標目錄路徑 (例如: "c:\\temp2")。</param>
        /// <returns>如果檔案移動成功，則為 true；否則為 false。</returns>
        public bool MoveFile(string sourceFilePath, string destinationDirectory)
        {
            // 檢查來源檔案是否存在 //Console.WriteLine($"錯誤：來源檔案 '{sourceFilePath}' 不存在。");
            if (!System.IO.File.Exists(sourceFilePath)) { return false; }

            // 檢查目標目錄是否存在，如果不存在則建立 //Console.WriteLine($"已建立目標目錄：'{destinationDirectory}'");
            if (!Directory.Exists(destinationDirectory))
            {
                try
                {
                    Directory.CreateDirectory(destinationDirectory);
                }
                catch (Exception ex)
                {
                    LOG.Error(ex.Message, ex);//Console.WriteLine($"錯誤：無法建立目標目錄 '{destinationDirectory}'。錯誤訊息：{ex.Message}");
                    return false;
                }
            }

            // 構建目標檔案的完整路徑
            string fileName = Path.GetFileName(sourceFilePath);
            string destinationFilePath = Path.Combine(destinationDirectory, fileName);
            try
            {
                // 如果目標位置已存在同名檔案，您可以選擇覆蓋它或跳過
                // 例如：如果要覆蓋現有檔案，請使用 File.Move(sourceFilePath, destinationFilePath, true); (適用於 .NET 5+)
                // 對於舊版 .NET 或不覆蓋，您可能需要先刪除目標檔案
                if (System.IO.File.Exists(destinationFilePath))
                {
                    //Console.WriteLine($"警告：目標位置已存在檔案 '{destinationFilePath}'，將覆蓋它。");
                    System.IO.File.Delete(destinationFilePath); // 刪除現有檔案以便移動
                }
                //Console.WriteLine($"檔案已成功從 '{sourceFilePath}' 移動到 '{destinationFilePath}'。");
                System.IO.File.Move(sourceFilePath, destinationFilePath);
                return true;
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);//Console.WriteLine($"錯誤：移動檔案時發生錯誤。錯誤訊息：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 資料載入時檢核欄位
        /// </summary>
        /// <returns></returns>
        private string CheckPage(SignUpFormModel form)
        {
            string strMsg = "";

            if (string.IsNullOrEmpty(form.NAME))
            {
                if (!string.IsNullOrEmpty(strMsg)) strMsg += "<br />";
                strMsg += "請輸入姓名";
            }

            if (string.IsNullOrEmpty(form.BIRTH_YEAR))
            {
                if (!string.IsNullOrEmpty(strMsg)) strMsg += "<br />";
                strMsg += "請選擇出生日期－年份";
            }
            if (string.IsNullOrEmpty(form.BIRTH_MON))
            {
                if (!string.IsNullOrEmpty(strMsg)) strMsg += "<br />";
                strMsg += "請選擇出生日期－月份";
            }
            if (string.IsNullOrEmpty(form.BIRTH_DAY))
            {
                if (!string.IsNullOrEmpty(strMsg)) strMsg += "<br />";
                strMsg += "請選擇出生日期－天數";
            }

            if (string.IsNullOrEmpty(form.IDNO))
            {
                if (!string.IsNullOrEmpty(strMsg)) strMsg += "<br />";
                strMsg += "請輸入身分證號";
            }
            else
            {
                if (!MyHelperUtil.IsIDNO(form.IDNO))
                {
                    /*if (!string.IsNullOrEmpty(strMsg)) strMsg += "<br />";
                    strMsg += "身分證號碼錯誤(如果有此身分證號碼，請聯絡系統管理者)!";*/
                }
            }

            return strMsg;
        }

        /// <summary>
        /// 載入在職進修報名資料(使用前次報名資料 Stud_StudentInfo / Stud_EnterTemp2)
        /// SHOW_Stud_EnterTemp12
        /// </summary>
        /// <param name="model"></param>
        private void LoadTemp2(SignUpViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SignUpDetailModel temp2Info = dao.GetStudEnterTmp12();
            LoadMemberBase(model, temp2Info);
        }

        /// <summary>
        /// 載入產投報名資料(使用 Stud_EnterTemp3 報名資料維護)
        /// </summary>
        /// <param name="model"></param>
        private void LoadTemp3(SignUpViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SignUpDetailModel temp3Info = dao.GetStudEnterTmp3();

            if (temp3Info == null)
            {
                model.Detail.ISPLAN28 = "N";
            }
            else
            {
                LoadMemberBase(model, temp3Info);

                model.Detail.ESETID3 = temp3Info.ESETID3;
                /*開始載入產投報名用相關欄位*/
                model.Detail.ZIPCODE2 = temp3Info.ZIPCODE2;
                model.Detail.ZIPCODE2_6W = temp3Info.ZIPCODE2_6W;
                model.Detail.ZIPCODE2_2W = MyCommonUtil.GET_ZIPCODE2W(temp3Info.ZIPCODE2_6W, null);
                model.Detail.HOUSEHOLDADDRESS = temp3Info.HOUSEHOLDADDRESS;
                model.Detail.MIDENTITYID = temp3Info.MIDENTITYID;
                //受訓前薪資
                model.Detail.PRIORWORKPAY = temp3Info.PRIORWORKPAY;

                //郵政/銀行帳號資訊
                model.Detail.ACCTMODE = temp3Info.ACCTMODE;
                model.Detail.POSTNO = "";
                model.Detail.BANKNAME = "";
                model.Detail.ACCTHEADNO = "";
                model.Detail.EXBANKNAME = "";
                model.Detail.ACCTEXNO = "";
                model.Detail.ACCTNO = "";

                //0:郵局帳號 1:銀行帳號 2:訓練單位代轉現金
                switch (Convert.ToInt64(temp3Info.ACCTMODE))
                {
                    case 0:
                        //郵局 //局號
                        model.Detail.POSTNO = (!string.IsNullOrEmpty(temp3Info.POSTNO)) ? temp3Info.POSTNO.Replace("-", "") : temp3Info.POSTNO;
                        //帳號
                        model.Detail.POST_ACCTNO = (!string.IsNullOrEmpty(temp3Info.ACCTNO)) ? temp3Info.ACCTNO.Replace("-", "") : temp3Info.ACCTNO;
                        break;
                    case 1: //銀行
                        model.Detail.BANKNAME = temp3Info.BANKNAME;
                        model.Detail.ACCTHEADNO = temp3Info.ACCTHEADNO;
                        model.Detail.EXBANKNAME = temp3Info.EXBANKNAME;
                        model.Detail.ACCTEXNO = temp3Info.ACCTEXNO;
                        model.Detail.BANK_ACCTNO = temp3Info.ACCTNO;
                        break;
                }

                model.Detail.ISAGREE = temp3Info.ISAGREE;
                model.Detail.ISEMAIL = temp3Info.ISEMAIL;

                //服務單位資料
                model.Detail.UNAME = temp3Info.UNAME;
                model.Detail.INTAXNO = temp3Info.INTAXNO;
                model.Detail.SERVDEPTID = temp3Info.SERVDEPTID;
                model.Detail.ACTNAME = temp3Info.ACTNAME;
                model.Detail.ACTTYPE = temp3Info.ACTTYPE;
                model.Detail.ACTNO = temp3Info.ACTNO;
                model.Detail.ACTTEL = temp3Info.ACTTEL;
                model.Detail.ZIPCODE3 = temp3Info.ZIPCODE3;
                model.Detail.ZIPCODE3_6W = temp3Info.ZIPCODE3_6W;
                model.Detail.ZIPCODE3_2W = MyCommonUtil.GET_ZIPCODE2W(temp3Info.ZIPCODE_6W, null);
                model.Detail.ACTADDRESS = temp3Info.ACTADDRESS;
                model.Detail.JOBTITLEID = temp3Info.JOBTITLEID;

                //參訓背景資料
                model.Detail.Q1 = temp3Info.Q1;
                model.Detail.Q2_1 = temp3Info.Q2_1;
                model.Detail.Q2_1_CHECKED = ("1".Equals(temp3Info.Q2_1));
                model.Detail.Q2_2 = temp3Info.Q2_2;
                model.Detail.Q2_2_CHECKED = ("1".Equals(temp3Info.Q2_2));
                model.Detail.Q2_3 = temp3Info.Q2_3;
                model.Detail.Q2_3_CHECKED = ("1".Equals(temp3Info.Q2_3));
                model.Detail.Q2_4 = temp3Info.Q2_4;
                model.Detail.Q2_4_CHECKED = ("1".Equals(temp3Info.Q2_4));
                model.Detail.Q3 = temp3Info.Q3;
                model.Detail.Q3_OTHER = temp3Info.Q3_OTHER;
                model.Detail.Q4 = temp3Info.Q4;
                model.Detail.Q5 = temp3Info.Q5;
                model.Detail.Q61 = temp3Info.Q61;
                model.Detail.Q62 = temp3Info.Q62;
                model.Detail.Q63 = temp3Info.Q63;
                model.Detail.Q64 = temp3Info.Q64;

                model.Detail.ISPLAN28 = temp3Info.ISPLAN28;
            }
        }

        /// <summary>
        /// 載入就業通單一簽入傳送的會員資料
        /// </summary>
        /// <param name="model"></param>
        private void LoadMember(SignUpViewModel model)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SignUpDetailModel memInfo = dao.GetEMember();
            LoadMemberBase(model, memInfo);
            model.Detail.IDNO = memInfo.IDNO;
        }

        /// <summary> 載入會員基本資料 member-model.Detail </summary>
        /// <param name="model"></param>
        /// <param name="newDetail"></param>
        private void LoadMemberBase(SignUpViewModel model, SignUpDetailModel newDetail)
        {
            SignUpDetailModel detail = model.Detail;

            if (model.Detail == null) { model.Detail = new SignUpDetailModel(); }

            if (newDetail != null)
            {
                model.Detail.NAME = newDetail.NAME;
                model.Detail.PASSPORTNO = newDetail.PASSPORTNO;
                model.Detail.IDNO = newDetail.IDNO;
                model.Detail.SEX = newDetail.SEX;
                model.Detail.BIRTHDAY = newDetail.BIRTHDAY;
                model.Detail.DEGREEID = newDetail.DEGREEID;
                model.Detail.GRADID = newDetail.GRADID.PadLeft(2, '0');
                model.Detail.SCHOOLNAME = !string.IsNullOrEmpty(newDetail.SCHOOLNAME) ? newDetail.SCHOOLNAME : model.Detail.SCHOOLNAME;
                model.Detail.DEPARTMENT = !string.IsNullOrEmpty(newDetail.DEPARTMENT) ? newDetail.DEPARTMENT : model.Detail.DEPARTMENT;  //newDetail.DEPARTMENT;

                //婚姻狀況  (1.已;2.未 3.暫不提供(預設))
                switch (Convert.ToInt64(newDetail.MARITALSTATUS))
                {
                    case 1:
                    case 2:
                        model.Detail.MARITALSTATUS = newDetail.MARITALSTATUS;
                        break;
                    default:
                        model.Detail.MARITALSTATUS = 3;
                        break;
                }

                model.Detail.PHONED = newDetail.PHONED;
                model.Detail.PHONEN = newDetail.PHONEN;
                model.Detail.CELLPHONE = newDetail.CELLPHONE;
                model.Detail.HASMOBILE = (string.IsNullOrWhiteSpace(newDetail.CELLPHONE) ? "N" : "Y");

                model.Detail.ZIPCODE = newDetail.ZIPCODE;// (!newDetail.ZIPCODE.HasValue || newDetail.ZIPCODE== 0 ? null : newDetail.ZIPCODE);
                model.Detail.ZIPCODE_6W = newDetail.ZIPCODE_6W;
                model.Detail.ZIPCODE_2W = MyCommonUtil.GET_ZIPCODE2W(newDetail.ZIPCODE_6W, null);
                model.Detail.ADDRESS = newDetail.ADDRESS;
                model.Detail.HASEMAIL = (string.IsNullOrWhiteSpace(newDetail.EMAIL) || "無".Equals(newDetail.EMAIL) ? "N" : "Y");
                model.Detail.EMAIL = newDetail.EMAIL;
            }
        }

        /// <summary>
        /// 身分證件驗證
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadDBBC()
        {
            //中華民國身分證模板 //ActionResult rtn = View(); return rtn;
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SignUpDetailModel memInfo = dao.GetEMember();
            if (memInfo == null)
            {
                sm.LastResultMessage = "資料有誤，請重新操作";
                return Index(); //base.SetPageNotFound(); //throw new ArgumentException("plantype");
            }
            SignUpViewModel model = new SignUpViewModel();
            LoadMemberBase(model, memInfo);
            //model.Detail.IDNO = memInfo.IDNO; //model.Detail.Epi = EncodeString(memInfo.IDNO);
            //string formattedString = now.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
            DateTime createDt = DateTime.Now;
            var userinfoVM = new UserInfoVM
            {
                Epi = MyCommonUtil.TkEncrypt(sm.ACID),
                Epb = MyCommonUtil.TkEncrypt(sm.Birthday),
                Ept = MyCommonUtil.TkEncrypt(createDt.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture))
            };
            ViewBag.EditData = userinfoVM; // applyUnitVM;
            var orgData = new TblE_IMG2 { CREATEDATE = createDt };
            SaveExIMG2(orgData, sm, "create1");
            return View(model); //ActionResult rtn = View(model);
        }

        /// <summary>
        /// 上傳銀行存摺
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Upload70643D()
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SignUpDetailModel memInfo = dao.GetEMember();
            if (memInfo == null)
            {
                sm.LastResultMessage = "資料有誤，請重新操作";
                //rtn = Index(); sm.RedirectUrlAfterBlock = ""; rtn = View("CheckEdit", model); LOG.Error("memInfo 不可為空值");
                return Index(); //base.SetPageNotFound(); //throw new ArgumentException("plantype");
            }
            SignUpViewModel model = new SignUpViewModel();
            LoadMemberBase(model, memInfo);
            //model.Detail.IDNO = memInfo.IDNO; //model.Detail.Epi = EncodeString(memInfo.IDNO);

            var createDt = DateTime.Now;
            var userinfoVM = new UserInfoVM();
            userinfoVM.Epi = MyCommonUtil.TkEncrypt(sm.ACID);
            userinfoVM.Epb = MyCommonUtil.TkEncrypt(sm.Birthday);
            userinfoVM.Ept = MyCommonUtil.TkEncrypt(createDt.ToString("yyyyMMddHHmmssfff"));
            ViewBag.EditData = userinfoVM; // applyUnitVM; //ActionResult rtn = View(model);
            var orgData = new TblE_IMG1 { CREATEDATE = createDt };
            SaveExIMG1(orgData, sm, "create1");
            return View(model);
        }

        /// <summary>
        /// 刪除-上傳銀行存摺
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Upload70643DDEL()
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SignUpDetailModel memInfo = dao.GetEMember();
            if (memInfo == null)
            {
                sm.LastResultMessage = "資料有誤，請重新操作";
                return Index(); //base.SetPageNotFound(); //throw new ArgumentException("plantype");
            }
            SaveExIMG1(new TblE_IMG1(), sm, "deldata1");

            DateTime createDt = DateTime.Now;
            var userinfoVM = new UserInfoVM
            {
                Epi = MyCommonUtil.TkEncrypt(sm.ACID),
                Epb = MyCommonUtil.TkEncrypt(sm.Birthday),
                Ept = MyCommonUtil.TkEncrypt(createDt.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture))
            };
            ViewBag.EditData = userinfoVM; // applyUnitVM;
            return Detail2(userinfoVM.Epi, userinfoVM.Epb, userinfoVM.Ept); //return Upload70643D(); //return CheckEdit();
        }

        /// <summary>
        /// 刪除-上傳身分證
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadDBBCDEL()
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SignUpDetailModel memInfo = dao.GetEMember();
            if (memInfo == null)
            {
                sm.LastResultMessage = "資料有誤，請重新操作";
                return Index(); //base.SetPageNotFound(); //throw new ArgumentException("plantype");
            }
            SaveExIMG2(new TblE_IMG2(), sm, "deldata1");

            DateTime createDt = DateTime.Now;
            var userinfoVM = new UserInfoVM
            {
                Epi = MyCommonUtil.TkEncrypt(sm.ACID),
                Epb = MyCommonUtil.TkEncrypt(sm.Birthday),
                Ept = MyCommonUtil.TkEncrypt(createDt.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture))
            };
            ViewBag.EditData = userinfoVM; // applyUnitVM;
            return Detail2(userinfoVM.Epi, userinfoVM.Epb, userinfoVM.Ept); //return UploadDBBC(); //return CheckEdit();
        }

        /// <summary>
        /// 上傳的圖片 身分證反面
        /// </summary>
        /// <param name="epi"></param>
        /// <param name="epb"></param>
        /// <param name="ept"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public JsonResult UploadAndParseImageB(string epi, string epb, string ept,
            HttpPostedFileBase file, int frontEndImageWidth, int frontEndImageHeight, string scanMode)
        {
            SessionModel sm = SessionModel.Get();
            string sm_IDNO = null, sm_BIRTHDAY = null; DateTime? stamptime1 = null;
            //LOG.Debug(string.Concat("#UAPIB.epi:", epi, ",epb:", epb, ",ept:", ept));
            try
            {
                //Console.WriteLine(((string)null).Length);
                var deEpi = MyCommonUtil.TkDecrypt(epi);
                var deEpb = MyCommonUtil.TkDecrypt(epb);
                var deEpt = MyCommonUtil.TkDecrypt(ept);
                stamptime1 = UserInfoVM.GetDateTimeFmt1(deEpt).Value; sm_IDNO = sm.ACID; sm_BIRTHDAY = sm.Birthday;
                if (string.IsNullOrEmpty(sm_IDNO)) { throw new ArgumentNullException("ACID"); }
                if (string.IsNullOrEmpty(sm_BIRTHDAY)) { throw new ArgumentNullException("BIRTHDAY"); }
                //LOG.Debug(string.Concat("#UAPI.deEpi:", deEpi, ",deEpb:", deEpb, ",deEpt:", deEpt));
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);
                return Json(new { result = new { Success = false, Message = "傳入資訊有誤", } });
            }
            if (epi != MyCommonUtil.TkEncrypt(sm.ACID)) { return Json(new { result = new { Success = false, Message = "登入資訊有誤，請重新操作!", } }); }
            if (file == null || file.ContentLength == 0) { return Json(new { result = new { Success = false, Message = "請上傳圖片", } }); }

            //上傳儲存圖片檔的路徑。 "~/upojt/web" "ExIMG2" "~/Uploads"
            string UploadRootPath = ConfigModel.UploadOJTWEBPath;
            string sExIMG2xPath = ConfigModel.ExIMG2xPath;
            string webPath = string.Concat(UploadRootPath, "/", sExIMG2xPath, "/", DateTime.Now.Year, "/", DateTime.Now.Month, "/");
            string savePath = HttpContext.Server.MapPath(webPath);

            //yyyyMMddHHmmssfff //file.FileName //file.e.FileName
            var EPRS10 = string.Concat("f", RandomStringGenerator.GetRS10());
            string srcFileName = Path.GetFileName(file.FileName);
            string s_fileExt = Path.GetExtension(file.FileName);
            string s_ServerTime = DateTime.Now.ToString("ddHHmmssfff"); //系統時間2
            string s_fileN = Path.GetFileName(string.Concat(EPRS10, "x", s_ServerTime, "xB"));
            string s_fileNM = string.Concat(s_fileN, s_fileExt);
            //LOG.Debug(string.Concat("#UAPI.s_fileExt:", s_fileExt)); //LOG.Debug(string.Concat("#UAPI.s_fileN:", s_fileN)); //LOG.Debug(string.Concat("#UAPI.s_fileNM:", s_fileNM));

            //如果路徑不存在，就建立 
            try
            {
                if (!Directory.Exists(savePath)) { Directory.CreateDirectory(savePath); }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);//Console.WriteLine($"錯誤：無法建立目標目錄 '{destinationDirectory}'。錯誤訊息：{ex.Message}");
                throw ex;
            }
            //file.FileName
            string croppedPath = null; //裁切的圖片
            string processedImage = null; //影像處理 & OCR
            string filePath = Path.Combine(savePath, s_fileNM);
            LOG.Debug(string.Concat("#UAPIB.filePath:", filePath));
            file.SaveAs(filePath);
            //添加文字浮水印
            var s_fileNMW = Path.GetFileName(ADDWatermarkImage(filePath, savePath, EPRS10));
            LOG.Debug(string.Concat("#UAPIB.fileNMw:", s_fileNMW));

            var orgData = new TblE_IMG2
            {
                CREATEDATE = stamptime1,
                FILEPATH2 = webPath,
                FILENAME2 = s_fileNM,
                FILENAME2W = s_fileNMW,
                SRCFILENAME2 = srcFileName
            };
            SaveExIMG2(orgData, sm, "up2");

            Bitmap originalImage = new Bitmap(filePath);
            List<string> ocrResults = new List<string>();
            if (scanMode == "full")
            {
                //完整掃描
                int canvasWidth = 600;
                int canvasHeight = 400;

                float scaleX = (float)frontEndImageWidth / canvasWidth;
                float scaleY = (float)frontEndImageHeight / canvasHeight;
                float scale = Math.Max(scaleX, scaleY);

                Bitmap croppedImage = null;
                if (scaleX > 1 || scaleY > 1)
                {
                    Rectangle cropArea = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
                    croppedImage = originalImage.Clone(cropArea, originalImage.PixelFormat);
                }
                else
                {
                    scaleX = (float)canvasWidth / originalImage.Width;
                    scaleY = (float)canvasHeight / originalImage.Height;
                    scale = Math.Min(scaleX, scaleY);

                    int scaledWidth = (int)(originalImage.Width * scale);
                    int scaledHeight = (int)(originalImage.Height * scale);

                    using (var scaledImage = new Bitmap(originalImage, scaledWidth, scaledHeight))
                    {
                        Rectangle cropArea = new Rectangle(0, 0, scaledWidth, scaledHeight);
                        croppedImage = scaledImage.Clone(cropArea, originalImage.PixelFormat);
                    }
                }

                //裁切的圖片
                croppedPath = Path.Combine(savePath, "crop_" + s_fileNM);
                if (croppedImage != null)
                {
                    croppedImage.Save(croppedPath);
                    croppedImage?.Dispose();
                }

                //影像處理 & OCR string processedImage
                processedImage = ProcessImage(croppedPath, savePath, EPRS10);
                string ocrText = RecognizeText(processedImage);
                ocrResults.Add(ocrText);
                List<string> extractedData = ExtractRocPassPortCode(ocrText);
                if (extractedData != null)
                {
                    foreach (var data in extractedData)
                    {
                        var dataN = "";
                        if (!string.IsNullOrEmpty(data)) { dataN = data?.Replace(" ", "").Replace("\n", "") ?? ""; }
                        ocrResults.Add(dataN);
                    }
                }
            }
            if (originalImage != null) { originalImage?.Dispose(); }
            //限制精準掃描每日使用2次 //if (scanMode == "precise" && ocrUseCount < 2)
            //originalImage?.Dispose(); //DeleteTemporaryFiles(filePath); //清除檔案
            DeleteTemporaryFiles(croppedPath); //清除檔案 裁切的圖片
            DeleteTemporaryFiles(processedImage); //清除檔案 影像處理 & OCR

            bool success = true; //ocrResults.Any(r => !string.IsNullOrEmpty(r));

            string message = success ? string.Concat("已成功上傳", s_ServerTime) : string.Concat("未成功擷取", s_ServerTime);
            //LOG.Debug(string.Concat("#UAPI.success:", success));
            //LOG.Debug(string.Concat("#UAPI.ocrResults[oRes]:", oRes));
            //LOG.Debug(string.Concat("#UAPI.message:", message));
            //精準掃描每日限制使用2次
            //string message = success ? "已成功擷取存摺號碼" : (ocrUseCount >= 2) ? "已達使用上限" : "未偵測到任何有效的文字";
            //var result = new { Success = success, Data = ocrResults, Message = message, };
            //return new JsonResult() { Data = new { result }, };
            return Json(new { result = new { Success = success, Data = ocrResults, Message = message, } });
            //var result = new Result<List<string>>() { Success = success, Data = ocrResults, Message = message, }; return new JsonExtResult(result);
        }

        /// <summary>
        /// 解析上傳的圖片 身分證正面圖像上傳
        /// </summary>
        /// <param name="epi"></param>
        /// <param name="epb"></param>
        /// <param name="ept"></param>
        /// <param name="file"></param>
        /// <param name="frontEndImageWidth"></param>
        /// <param name="frontEndImageHeight"></param>
        /// <param name="scanMode"></param>
        /// <param name="boxX1"></param>
        /// <param name="boxY1"></param>
        /// <param name="boxWidth1"></param>
        /// <param name="boxHeight1"></param>
        /// <param name="boxX2"></param>
        /// <param name="boxY2"></param>
        /// <param name="boxWidth2"></param>
        /// <param name="boxHeight2"></param>
        /// <param name="boxX3"></param>
        /// <param name="boxY3"></param>
        /// <param name="boxWidth3"></param>
        /// <param name="boxHeight3"></param>
        /// <param name="boxX4"></param>
        /// <param name="boxY4"></param>
        /// <param name="boxWidth4"></param>
        /// <param name="boxHeight4"></param>
        /// <returns></returns>
        public JsonResult UploadAndParseImageF(string epi, string epb, string ept,
            HttpPostedFileBase file, int frontEndImageWidth, int frontEndImageHeight, string scanMode,
            int boxX1, int boxY1, int boxWidth1, int boxHeight1, int boxX2, int boxY2, int boxWidth2, int boxHeight2,
            int boxX3, int boxY3, int boxWidth3, int boxHeight3, int boxX4, int boxY4, int boxWidth4, int boxHeight4)
        {
            SessionModel sm = SessionModel.Get();
            string sm_IDNO = null, sm_BIRTHDAY = null; DateTime? stamptime1 = null;
            //LOG.Debug(string.Concat("#UAPIF.epi:", epi, ",epb:", epb, ",ept:", ept));
            try
            {
                //Console.WriteLine(((string)null).Length);
                var deEpi = MyCommonUtil.TkDecrypt(epi);
                var deEpb = MyCommonUtil.TkDecrypt(epb);
                var deEpt = MyCommonUtil.TkDecrypt(ept);
                stamptime1 = UserInfoVM.GetDateTimeFmt1(deEpt).Value; sm_IDNO = sm.ACID; sm_BIRTHDAY = sm.Birthday;
                if (string.IsNullOrEmpty(sm_IDNO)) { throw new ArgumentNullException("ACID"); }
                if (string.IsNullOrEmpty(sm_BIRTHDAY)) { throw new ArgumentNullException("BIRTHDAY"); }
                //LOG.Debug(string.Concat("#UAPI.deEpi:", deEpi, ",deEpb:", deEpb, ",deEpt:", deEpt));
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);
                return Json(new { result = new { Success = false, Message = "傳入資訊有誤", } });
            }
            if (epi != MyCommonUtil.TkEncrypt(sm.ACID)) { return Json(new { result = new { Success = false, Message = "登入資訊有誤，請重新操作!", } }); }
            if (file == null || file.ContentLength == 0) { return Json(new { result = new { Success = false, Message = "請上傳圖片", } }); }

            //上傳儲存簽名圖片檔的路徑。 "~/upojt/web" "ExIMG2" "~/Uploads"
            string UploadRootPath = ConfigModel.UploadOJTWEBPath;
            string sExIMG2xPath = ConfigModel.ExIMG2xPath;
            string webPath = string.Concat(UploadRootPath, "/", sExIMG2xPath, "/", DateTime.Now.Year, "/", DateTime.Now.Month, "/");
            string savePath = HttpContext.Server.MapPath(webPath);

            //yyyyMMddHHmmssfff //file.FileName //file.e.FileName
            var EPRS10 = string.Concat("f", RandomStringGenerator.GetRS10());
            string srcFileName = Path.GetFileName(file.FileName);
            string s_fileExt = Path.GetExtension(file.FileName);
            string s_ServerTime = DateTime.Now.ToString("ddHHmmssfff"); //系統時間2
            string s_fileN = Path.GetFileName(string.Concat(EPRS10, "x", s_ServerTime));
            string s_fileNM = string.Concat(s_fileN, s_fileExt);
            //LOG.Debug(string.Concat("#UAPI.s_fileExt:", s_fileExt)); //LOG.Debug(string.Concat("#UAPI.s_fileN:", s_fileN)); //LOG.Debug(string.Concat("#UAPI.s_fileNM:", s_fileNM));

            //如果路徑不存在，就建立 
            try
            {
                if (!Directory.Exists(savePath)) { Directory.CreateDirectory(savePath); }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);//Console.WriteLine($"錯誤：無法建立目標目錄 '{destinationDirectory}'。錯誤訊息：{ex.Message}");
                throw ex;
            }
            //file.FileName
            string croppedPath = null; //裁切的圖片
            string processedImage = null; //影像處理 & OCR
            string filePath = Path.Combine(savePath, s_fileNM);
            LOG.Debug(string.Concat("#UAPIF.filePath:", filePath));
            file.SaveAs(filePath);
            //添加文字浮水印
            var s_fileNMW = Path.GetFileName(ADDWatermarkImage(filePath, savePath, EPRS10));
            LOG.Debug(string.Concat("#UAPIF.fileNMw:", s_fileNMW));

            var orgData = new TblE_IMG2
            {
                CREATEDATE = stamptime1,
                FILEPATH1 = webPath,
                FILENAME1 = s_fileNM,
                FILENAME1W = s_fileNMW,
                SRCFILENAME1 = srcFileName
            };
            SaveExIMG2(orgData, sm, "up1");

            Bitmap originalImage = new Bitmap(filePath);
            List<string> ocrResults = new List<string>();
            var boxes = new[]
            {
                new { X = boxX1, Y = boxY1, Width = boxWidth1, Height = boxHeight1 },
                new { X = boxX2, Y = boxY2, Width = boxWidth2, Height = boxHeight2 },
                new { X = boxX3, Y = boxY3, Width = boxWidth3, Height = boxHeight3 },
                new { X = boxX4, Y = boxY4, Width = boxWidth4, Height = boxHeight4 }
            };

            //限制精準掃描每日使用2次 //if (scanMode == "precise" && ocrUseCount < 2)
            if (scanMode == "precise")
            {
                //1:身分證字號/2:出生年月日/3:發證日期/4:發證地
                int index = 1;
                List<string> rawOcrResults = new List<string>();
                foreach (var box in boxes)
                {
                    s_ServerTime = DateTime.Now.ToString("ddHHmmssfff"); //系統時間2
                    s_fileN = Path.GetFileName(string.Concat(EPRS10, "x", "p", index, "x", s_ServerTime));
                    s_fileNM = string.Concat(s_fileN, s_fileExt);

                    int canvasWidth = 600;
                    int canvasHeight = 400;

                    float scaleX = (float)frontEndImageWidth / canvasWidth;
                    float scaleY = (float)frontEndImageHeight / canvasHeight;
                    float scale = Math.Max(scaleX, scaleY);

                    int cropX = (int)Math.Round(box.X * scale);
                    int cropY = (int)Math.Round(box.Y * scale);
                    int cropWidth = (int)Math.Floor(box.Width * scale);
                    int cropHeight = (int)Math.Floor(box.Height * scale);

                    Bitmap croppedImage = null;
                    if (scaleX > 1 || scaleY > 1)
                    {
                        cropX = Math.Max(0, cropX);
                        cropY = Math.Max(0, cropY);
                        cropWidth = Math.Min(originalImage.Width - cropX, cropWidth);
                        cropHeight = Math.Min(originalImage.Height - cropY, cropHeight);
                        Rectangle cropArea = new Rectangle(cropX, cropY, cropWidth, cropHeight);
                        croppedImage = originalImage.Clone(cropArea, originalImage.PixelFormat);
                    }
                    else
                    {
                        scaleX = (float)canvasWidth / originalImage.Width;
                        scaleY = (float)canvasHeight / originalImage.Height;
                        scale = Math.Min(scaleX, scaleY);

                        int scaledWidth = (int)(originalImage.Width * scale);
                        int scaledHeight = (int)(originalImage.Height * scale);

                        using (var scaledImage = new Bitmap(originalImage, scaledWidth, scaledHeight))
                        {
                            cropX = Math.Max(0, box.X);
                            cropY = Math.Max(0, box.Y);
                            cropWidth = Math.Min(scaledWidth - cropX, box.Width);
                            cropHeight = Math.Min(scaledHeight - cropY, box.Height);
                            Rectangle cropArea = new Rectangle(cropX, cropY, cropWidth, cropHeight);
                            croppedImage = scaledImage.Clone(cropArea, originalImage.PixelFormat);
                        }
                    }

                    //裁切的圖片
                    croppedPath = Path.Combine(savePath, "crop_" + s_fileNM);
                    if (croppedImage != null)
                    {
                        croppedImage.Save(croppedPath);
                        croppedImage?.Dispose();
                    }

                    // 影像處理 & OCR
                    processedImage = ProcessImage(croppedPath, savePath, EPRS10, index);
                    string ocrText; //掃瞄出的文字
                    //身分證字號/出生年月日/發證日期/發證地
                    if (index == 1) //身分證字號
                    {
                        ocrText = RecognizeText(processedImage);
                    }
                    else if (index == 2) //出生年月日
                    {
                        ocrText = ExtractRocDate(RecognizeText(processedImage));
                        //parseText = GetBankCodeByCodeOrName(ocrText);
                    }
                    else if (index == 3) //發證日期
                    {
                        //ocrText = ExtractRocDate(RecognizeText(processedImage));
                        ocrText = ExtractRocDate(RecognizeText(processedImage));
                        //ocrText = ExtractChineseOrNumber(RecognizeText(processedImage));
                        //parseText = GetBankCodeByCodeOrName(ocrText);
                    }
                    else //發證地
                    {
                        ocrText = ExtractChineseOrNumber(RecognizeText(processedImage));
                        //parseText = ocrResults.Count >= 2 ? GetBranchCodeByCodeOrName(ocrResults[1], ocrText) : "";
                    }
                    //parseText = parseText?.Replace(" ", "").Replace("\n", "") ?? "";
                    //LOG.Debug(string.Concat("#uapf:[", index, "]:p:", parseText));
                    LOG.Debug(string.Concat("#uapf:[", index, "]:o:", ocrText));
                    //ocrResults.Add(ocrText);
                    rawOcrResults.Add(ocrText ?? "");
                    DeleteTemporaryFiles(croppedPath); //清除檔案 裁切的圖片
                    DeleteTemporaryFiles(processedImage); //清除檔案 影像處理 & OCR
                    index++;
                }
                ocrResults.AddRange(rawOcrResults);
                //紀錄使用1次 //UpdateOcrUseCount(companyId, ocrUseCount + 1);
            }
            else if (scanMode == "full")
            {
                //完整掃描
                int canvasWidth = 600;
                int canvasHeight = 400;

                float scaleX = (float)frontEndImageWidth / canvasWidth;
                float scaleY = (float)frontEndImageHeight / canvasHeight;
                float scale = Math.Max(scaleX, scaleY);

                Bitmap croppedImage = null;
                if (scaleX > 1 || scaleY > 1)
                {
                    Rectangle cropArea = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
                    croppedImage = originalImage.Clone(cropArea, originalImage.PixelFormat);
                }
                else
                {
                    scaleX = (float)canvasWidth / originalImage.Width;
                    scaleY = (float)canvasHeight / originalImage.Height;
                    scale = Math.Min(scaleX, scaleY);

                    int scaledWidth = (int)(originalImage.Width * scale);
                    int scaledHeight = (int)(originalImage.Height * scale);

                    using (var scaledImage = new Bitmap(originalImage, scaledWidth, scaledHeight))
                    {
                        Rectangle cropArea = new Rectangle(0, 0, scaledWidth, scaledHeight);
                        croppedImage = scaledImage.Clone(cropArea, originalImage.PixelFormat);
                    }
                }

                //裁切的圖片
                croppedPath = Path.Combine(savePath, "crop_" + s_fileNM);
                if (croppedImage != null)
                {
                    croppedImage.Save(croppedPath);
                    croppedImage?.Dispose();
                }

                //影像處理 & OCR string processedImage
                processedImage = ProcessImage(croppedPath, savePath, epi);
                string ocrText = RecognizeText(processedImage);
                ocrResults.Add(ocrText);
                List<string> extractedData = ExtractRocPassPortCode(ocrText);
                if (extractedData != null)
                {
                    foreach (var data in extractedData)
                    {
                        var dataN = "";
                        if (!string.IsNullOrEmpty(data)) { dataN = data?.Replace(" ", "").Replace("\n", "") ?? ""; }
                        ocrResults.Add(dataN);
                    }
                }
            }

            if (originalImage != null) { originalImage?.Dispose(); }
            //DeleteTemporaryFiles(filePath); //清除檔案
            DeleteTemporaryFiles(croppedPath); //清除檔案 裁切的圖片
            DeleteTemporaryFiles(processedImage); //清除檔案 影像處理 & OCR

            bool success = ocrResults.Any(r => !string.IsNullOrEmpty(r));
            string oRes = "";
            if (ocrResults != null)
            {
                int irow = 0;
                foreach (var ocr1 in ocrResults)
                {
                    irow += 1;
                    oRes += string.Concat((!string.IsNullOrEmpty(oRes) ? "," : ""), irow, ":", ocr1);
                }
            }
            string message = success ? string.Concat("已成功擷取", s_ServerTime) : string.Concat("未偵測到任何有效的文字", s_ServerTime);

            //LOG.Debug(string.Concat("#UAPIF.success:", success));
            LOG.Debug(string.Concat("#UAPIF.ocrResults[oRes]:", oRes));
            LOG.Debug(string.Concat("#UAPIF.message:", message));
            //精準掃描每日限制使用2次
            //string message = success ? "已成功擷取存摺號碼" : (ocrUseCount >= 2) ? "已達使用上限" : "未偵測到任何有效的文字";
            //var result = new { Success = success, Data = ocrResults, Message = message, };
            //return new JsonResult() { Data = new { result }, };
            return Json(new { result = new { Success = success, Data = ocrResults, Message = message, } });
            //var result = new Result<List<string>>() { Success = success, Data = ocrResults, Message = message, }; return new JsonExtResult(result);
        }

        /// <summary>
        /// 解析上傳的圖片
        /// </summary>
        /// <param name="file"></param>
        /// <param name="frontEndImageWidth"></param>
        /// <param name="frontEndImageHeight"></param>
        /// <param name="boxX1"></param>
        /// <param name="boxY1"></param>
        /// <param name="boxWidth1"></param>
        /// <param name="boxHeight1"></param>
        /// <param name="boxX2"></param>
        /// <param name="boxY2"></param>
        /// <param name="boxWidth2"></param>
        /// <param name="boxHeight2"></param>
        /// <param name="boxX3"></param>
        /// <param name="boxY3"></param>
        /// <param name="boxWidth3"></param>
        /// <param name="boxHeight3"></param>
        /// <returns></returns>
        public JsonResult UploadAndParseImage(string epi, string epb, string ept,
            HttpPostedFileBase file, int frontEndImageWidth, int frontEndImageHeight, string scanMode,
            int boxX1, int boxY1, int boxWidth1, int boxHeight1, int boxX2, int boxY2, int boxWidth2, int boxHeight2,
            int boxX3, int boxY3, int boxWidth3, int boxHeight3)
        {
            SessionModel sm = SessionModel.Get();
            string sm_IDNO = null, sm_BIRTHDAY = null; DateTime? stamptime1 = null;
            //LOG.Debug(string.Concat("#UAPI.epi:", epi, ",epb:", epb, ",ept:", ept));
            try
            {
                //Console.WriteLine(((string)null).Length);
                var deEpi = MyCommonUtil.TkDecrypt(epi);
                var deEpb = MyCommonUtil.TkDecrypt(epb);
                var deEpt = MyCommonUtil.TkDecrypt(ept);
                stamptime1 = UserInfoVM.GetDateTimeFmt1(deEpt).Value; sm_IDNO = sm.ACID; sm_BIRTHDAY = sm.Birthday;
                if (string.IsNullOrEmpty(sm_IDNO)) { throw new ArgumentNullException("ACID"); }
                if (string.IsNullOrEmpty(sm_BIRTHDAY)) { throw new ArgumentNullException("BIRTHDAY"); }
                //LOG.Debug(string.Concat("#UAPI.deEpi:", deEpi, ",deEpb:", deEpb, ",deEpt:", deEpt));
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);
                return Json(new { result = new { Success = false, Message = "傳入資訊有誤,請重新操作,報名資料維護", } });
            }
            //LOG.Debug(string.Concat("#UAPI.int frontEndImageWidth,", frontEndImageWidth, "  int frontEndImageHeight,", frontEndImageHeight, " string scanMode:", scanMode, "."));
            //LOG.Debug(string.Concat("#UAPI.int boxX1,", boxX1, " int boxY1,", boxY1, " int boxWidth1,", boxWidth1, " int boxHeight1:", boxHeight1, "."));
            //LOG.Debug(string.Concat("#UAPI.int boxX2,", boxX2, " int boxY2,", boxY2, " int boxWidth2,", boxWidth2, " int boxHeight2:", boxHeight2, "."));
            //LOG.Debug(string.Concat("#UAPI.int boxX3,", boxX3, " int boxY3,", boxY3, " int boxWidth3,", boxWidth3, " int boxHeight3:", boxHeight3, "."));
            if (epi != MyCommonUtil.TkEncrypt(sm.ACID)) { return Json(new { result = new { Success = false, Message = "登入資訊有誤，請重新操作!", } }); }
            if (file == null || file.ContentLength == 0) { return Json(new { result = new { Success = false, Message = "請上傳圖片", } }); }

            //上傳儲存簽名圖片檔的路徑。 "~/upojt/web" "ExIMG1" "~/Uploads"
            string UploadRootPath = ConfigModel.UploadOJTWEBPath;
            string sExIMG1xPath = ConfigModel.ExIMG1xPath;
            string webPath = string.Concat(UploadRootPath, "/", sExIMG1xPath, "/", DateTime.Now.Year, "/", DateTime.Now.Month, "/");
            string savePath = HttpContext.Server.MapPath(webPath);

            //yyyyMMddHHmmssfff //file.FileName //file.e.FileName
            var EPRS10 = string.Concat("f", RandomStringGenerator.GetRS10());
            string srcFileName = Path.GetFileName(file.FileName);
            string s_fileExt = Path.GetExtension(file.FileName);
            string s_ServerTime = DateTime.Now.ToString("ddHHmmssfff"); //系統時間2
            string s_fileN = Path.GetFileName(string.Concat(EPRS10, "x", s_ServerTime));
            string s_fileNM = string.Concat(s_fileN, s_fileExt);
            //LOG.Debug(string.Concat("#UAPI.s_fileExt:", s_fileExt)); //LOG.Debug(string.Concat("#UAPI.s_fileN:", s_fileN)); //LOG.Debug(string.Concat("#UAPI.s_fileNM:", s_fileNM));

            //如果路徑不存在，就建立 
            try
            {
                if (!Directory.Exists(savePath)) { Directory.CreateDirectory(savePath); }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);//Console.WriteLine($"錯誤：無法建立目標目錄 '{destinationDirectory}'。錯誤訊息：{ex.Message}");
                throw ex;
            }
            //file.FileName
            string croppedPath = null; //裁切的圖片
            string processedImage = null; //影像處理 & OCR
            string filePath = Path.Combine(savePath, s_fileNM);
            LOG.Debug(string.Concat("#UAPI.filePath:", filePath));
            file.SaveAs(filePath);
            //添加文字浮水印
            var s_fileNMW = Path.GetFileName(ADDWatermarkImage(filePath, savePath, EPRS10));
            LOG.Debug(string.Concat("#UAPI.fileNMw:", s_fileNMW));

            var orgData = new TblE_IMG1
            {
                CREATEDATE = stamptime1,
                FILEPATH1 = webPath,
                FILENAME1 = s_fileNM,
                FILENAME1W = s_fileNMW,
                SRCFILENAME1 = srcFileName
            };
            SaveExIMG1(orgData, sm, "up1");

            Bitmap originalImage = new Bitmap(filePath);
            List<string> ocrResults = new List<string>();
            var boxes = new[]
            {
                new { X = boxX1, Y = boxY1, Width = boxWidth1, Height = boxHeight1 },
                new { X = boxX2, Y = boxY2, Width = boxWidth2, Height = boxHeight2 },
                new { X = boxX3, Y = boxY3, Width = boxWidth3, Height = boxHeight3 }
            };

            //限制精準掃描每日使用2次 //if (scanMode == "precise" && ocrUseCount < 2)
            if (scanMode == "precise")
            {
                // 1:帳號, 2:銀行, 3:分行
                int index = 1;
                List<string> rawOcrResults = new List<string>();
                foreach (var box in boxes)
                {
                    s_ServerTime = DateTime.Now.ToString("ddHHmmssfff"); //系統時間2
                    s_fileN = Path.GetFileName(string.Concat(EPRS10, "x", "p", index, "x", s_ServerTime));
                    s_fileNM = string.Concat(s_fileN, s_fileExt);

                    int canvasWidth = 600;
                    int canvasHeight = 400;

                    float scaleX = (float)frontEndImageWidth / canvasWidth;
                    float scaleY = (float)frontEndImageHeight / canvasHeight;
                    float scale = Math.Max(scaleX, scaleY);

                    int cropX = (int)Math.Round(box.X * scale);
                    int cropY = (int)Math.Round(box.Y * scale);
                    int cropWidth = (int)Math.Floor(box.Width * scale);
                    int cropHeight = (int)Math.Floor(box.Height * scale);

                    Bitmap croppedImage = null;
                    if (scaleX > 1 || scaleY > 1)
                    {
                        cropX = Math.Max(0, cropX);
                        cropY = Math.Max(0, cropY);
                        cropWidth = Math.Min(originalImage.Width - cropX, cropWidth);
                        cropHeight = Math.Min(originalImage.Height - cropY, cropHeight);
                        Rectangle cropArea = new Rectangle(cropX, cropY, cropWidth, cropHeight);
                        croppedImage = originalImage.Clone(cropArea, originalImage.PixelFormat);
                    }
                    else
                    {
                        scaleX = (float)canvasWidth / originalImage.Width;
                        scaleY = (float)canvasHeight / originalImage.Height;
                        scale = Math.Min(scaleX, scaleY);

                        int scaledWidth = (int)(originalImage.Width * scale);
                        int scaledHeight = (int)(originalImage.Height * scale);

                        using (var scaledImage = new Bitmap(originalImage, scaledWidth, scaledHeight))
                        {
                            cropX = Math.Max(0, box.X);
                            cropY = Math.Max(0, box.Y);
                            cropWidth = Math.Min(scaledWidth - cropX, box.Width);
                            cropHeight = Math.Min(scaledHeight - cropY, box.Height);
                            Rectangle cropArea = new Rectangle(cropX, cropY, cropWidth, cropHeight);
                            croppedImage = scaledImage.Clone(cropArea, originalImage.PixelFormat);
                        }
                    }

                    //裁切的圖片
                    croppedPath = Path.Combine(savePath, "crop_" + s_fileNM);
                    if (croppedImage != null)
                    {
                        croppedImage.Save(croppedPath);
                        croppedImage?.Dispose();
                    }

                    // 影像處理 & OCR
                    processedImage = ProcessImage(croppedPath, savePath, EPRS10, index);
                    string ocrText; //掃瞄出的文字
                    string parseText; //解析過後的帳號或銀行代碼或分行代碼
                    if (index == 1) //帳號
                    {
                        ocrText = ExtractNumber(RecognizeText(processedImage, textType: "number"));
                        parseText = ocrText;
                    }
                    else if (index == 2) //金融機構
                    {
                        ocrText = ExtractChineseOrNumber(RecognizeText(processedImage));
                        parseText = GetBankCodeByCodeOrName(ocrText);
                    }
                    else //分行
                    {
                        ocrText = ExtractChineseOrNumber(RecognizeText(processedImage));
                        parseText = ocrResults.Count >= 2 ? GetBranchCodeByCodeOrName(ocrResults[1], ocrText) : "";
                    }
                    parseText = parseText?.Replace(" ", "").Replace("\n", "") ?? "";
                    ocrResults.Add(parseText);
                    rawOcrResults.Add(ocrText ?? "");
                    DeleteTemporaryFiles(croppedPath); //清除檔案 裁切的圖片
                    DeleteTemporaryFiles(processedImage); //清除檔案 影像處理 & OCR
                    index++;
                }
                ocrResults.AddRange(rawOcrResults);
                //紀錄使用1次 //UpdateOcrUseCount(companyId, ocrUseCount + 1);
            }
            else if (scanMode == "full")
            {
                //完整掃描
                int canvasWidth = 600;
                int canvasHeight = 400;

                float scaleX = (float)frontEndImageWidth / canvasWidth;
                float scaleY = (float)frontEndImageHeight / canvasHeight;
                float scale = Math.Max(scaleX, scaleY);

                Bitmap croppedImage = null;
                if (scaleX > 1 || scaleY > 1)
                {
                    Rectangle cropArea = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
                    croppedImage = originalImage.Clone(cropArea, originalImage.PixelFormat);
                }
                else
                {
                    scaleX = (float)canvasWidth / originalImage.Width;
                    scaleY = (float)canvasHeight / originalImage.Height;
                    scale = Math.Min(scaleX, scaleY);

                    int scaledWidth = (int)(originalImage.Width * scale);
                    int scaledHeight = (int)(originalImage.Height * scale);

                    using (var scaledImage = new Bitmap(originalImage, scaledWidth, scaledHeight))
                    {
                        Rectangle cropArea = new Rectangle(0, 0, scaledWidth, scaledHeight);
                        croppedImage = scaledImage.Clone(cropArea, originalImage.PixelFormat);
                    }
                }

                //裁切的圖片
                croppedPath = Path.Combine(savePath, "crop_" + s_fileNM);
                if (croppedImage != null)
                {
                    croppedImage.Save(croppedPath);
                    croppedImage?.Dispose();
                }

                //影像處理 & OCR string processedImage
                processedImage = ProcessImage(croppedPath, savePath, EPRS10);
                string ocrText = RecognizeText(processedImage);
                ocrResults.Add(ocrText);
                List<string> extractedData = ExtractBankCode(ocrText);
                if (extractedData != null)
                {
                    foreach (var data in extractedData)
                    {
                        var dataN = data?.Replace(" ", "").Replace("\n", "") ?? "";
                        ocrResults.Add(dataN);
                    }
                }
            }

            if (originalImage != null) { originalImage?.Dispose(); }
            //DeleteTemporaryFiles(filePath); //清除檔案
            DeleteTemporaryFiles(croppedPath); //清除檔案 裁切的圖片
            DeleteTemporaryFiles(processedImage); //清除檔案 影像處理 & OCR

            bool success = ocrResults.Any(r => !string.IsNullOrEmpty(r));
            string oRes = "";
            if (ocrResults != null)
            {
                int irow = 0;
                foreach (var ocr1 in ocrResults)
                {
                    irow += 1;
                    oRes += string.Concat((!string.IsNullOrEmpty(oRes) ? "," : ""), irow, ":", ocr1);
                }
            }
            //存摺號碼
            string message = success ? string.Concat("已成功擷取", s_ServerTime) : string.Concat("未偵測到任何有效的文字", s_ServerTime);
            //LOG.Debug(string.Concat("#UAPI.success:", success));
            LOG.Debug(string.Concat("#UAPI.ocrResults[oRes]:", oRes));
            LOG.Debug(string.Concat("#UAPI.message:", message));
            //精準掃描每日限制使用2次
            //string message = success ? "已成功擷取存摺號碼" : (ocrUseCount >= 2) ? "已達使用上限" : "未偵測到任何有效的文字";
            //var result = new { Success = success, Data = ocrResults, Message = message, };
            //return new JsonResult() { Data = new { result }, };
            return Json(new { result = new { Success = success, Data = ocrResults, Message = message, } });
            //var result = new Result<List<string>>() { Success = success, Data = ocrResults, Message = message, }; return new JsonExtResult(result);
        }

        private double EstimateScaleFactor(Mat gray)
        {
            using (Mat edges = new Mat())
            {
                Cv2.Canny(gray, edges, 50, 150);

                OpenCvSharp.Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                double avgHeight = 0;
                int count = 0;

                foreach (var cnt in contours)
                {
                    var rect = Cv2.BoundingRect(cnt);
                    if (rect.Height > 5 && rect.Height < 100)
                    {
                        avgHeight += rect.Height;
                        count++;
                    }
                }

                if (count == 0) return 2.0;

                avgHeight /= count;
                double targetHeight = 30.0;
                double factor = targetHeight / avgHeight;

                return Math.Max(1.0, Math.Min(3.0, factor)); // 限制在 1~3 倍之間
            }
        }

        /// <summary>影像處理（灰階 + 增強）</summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private string ProcessImage(string imagePath, string savePath, string smACID, int iIDX = 0)
        {
            if (string.IsNullOrEmpty(imagePath)) { return null; }

            string s_fileExt = ".jpg";
            string s_ServerTime = DateTime.Now.ToString("ddHHmmssfff"); //系統時間2
            string s_fileN = Path.GetFileName(string.Concat(smACID, "x", s_ServerTime, "_proc", iIDX));
            string s_fileNM = string.Concat(s_fileN, s_fileExt);
            try
            {
                using (Mat src = Cv2.ImRead(imagePath, ImreadModes.Grayscale))
                {
                    string processedPath = Path.Combine(savePath, s_fileNM);
                    // 影像前處理
                    Cv2.ImWrite(processedPath, src);
                    return processedPath;
                }
            }
            catch (Exception ex)
            {
                var ExMsg1 = string.Concat("ProcessImage失敗: imagePath:", imagePath, ",ex.Message:", ex.Message);
                LOG.Error(ExMsg1, ex); //throw;
            }
            return null;
        }

        /// <summary>
        /// 根據指定的module掃描文字
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="module">預設使用tesseract。可用選項: "tesseract"</param>
        /// <param name="textType">預設使用"number+chinese"。可用選項: "number+chinese"、"number"</param>
        /// <returns></returns>
        private string RecognizeText(string imagePath, string module = "tesseract", string textType = "number+chinese")
        {
            if (string.IsNullOrEmpty(imagePath)) { return null; }
            string rtext = "";
            try
            {
                switch (module)
                {
                    //case "google": // return RecognizeTextUsingGoogle(imagePath);
                    default:
                        switch (textType)
                        {
                            case "number":
                                rtext = RecognizeTextUsingTesseract(imagePath, "ocrb");
                                LOG.Debug(string.Concat("#RecognizeText: ", rtext));
                                return rtext;
                            default:
                                rtext = RecognizeTextUsingTesseract(imagePath);
                                LOG.Debug(string.Concat("#RecognizeText: ", rtext));
                                return rtext;
                        }
                }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.Message, ex);
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// 使用 Tesseract OCR 進行辨識
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="language">預設使用"ocrb+chi_tra"。可用選項: "ocrb+chi_tra"、"ocrb"、"chi_tra"(其他選項參照Tesseract官方文檔，新增其他選項需要一併新增.traineddata檔案)</param>
        /// <returns></returns>
        private string RecognizeTextUsingTesseract(string imagePath, string language = "ocrb+chi_tra")
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string tessDataPath = Path.Combine(basePath, "tessdata");
            if (!Directory.Exists(tessDataPath))
            {
                tessDataPath = Server.MapPath("~/tessdata");
            }
            //LOG.Debug(string.Concat("##RTT,tessDataPath:", tessDataPath));
            if (!Directory.Exists(tessDataPath))
            {
                // TODO: REPLACE ABSOLUTE PATH OF THE DIRECTORY tessdata HERE
                tessDataPath = @"REPLACE ABSOLUTE PATH HERE";
            }
            //LOG.Debug(string.Concat("##RTT,tessDataPath:", tessDataPath));
            //LOG.Debug(string.Concat("##RTT,language:", language));
            using (var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default))
            {
                LOG.Debug(string.Concat("##RTT,language:", language));
                if (language == "ocrb")
                {
                    engine.SetVariable("tessedit_char_whitelist", "0123456789");
                }
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText().Trim();
                    }
                }
            }
        }

        /// <summary>
        /// 提取數字（正則表達式）
        /// </summary>
        /// <param name="ocrText"></param>
        /// <returns>數字字串</returns>
        private string ExtractNumber(string ocrText)
        {
            if (string.IsNullOrEmpty(ocrText)) return "";
            ocrText = Regex.Replace(ocrText, @"\D", "");

            var matches = Regex.Matches(ocrText, @"\b\d{1,14}\b");

            if (matches.Count > 0)
            {
                ocrText = ocrText.Replace(" ", "");
                var filteredNumbers = matches.Cast<Match>().Where(m => m.Value.Length >= 3).Select(m => m.Value).ToList();
                if (filteredNumbers.Count > 0)
                {
                    return string.Join(" ", filteredNumbers);
                }
            }
            return "";
        }

        /// <summary>
        /// 提取中文字或數字（正則表達式）
        /// (有中文優先回傳中文字串，無中文才尋找數字字串)
        /// </summary>
        /// <param name="ocrText"></param>
        /// <returns></returns>
        private string ExtractChineseOrNumber(string ocrText)
        {
            if (string.IsNullOrEmpty(ocrText)) return "";
            // 檢查 OCR 文字中是否有中文
            var chineseMatches = Regex.Matches(ocrText, @"[\u4e00-\u9fa5]+");

            // 檢查 OCR 文字中是否有數字
            var numberMatches = Regex.Matches(ocrText, @"\b\d{1,14}\b");

            if (chineseMatches.Count > 0)
            {
                return string.Concat(chineseMatches.Cast<Match>().Select(m => m.Value));

            }

            if (numberMatches.Count > 0)
            {
                var filteredNumbers = numberMatches.Cast<Match>().Where(m => m.Value.Length >= 3).Select(m => m.Value).ToList();
                if (filteredNumbers.Count > 0)
                {
                    return string.Join(" ", filteredNumbers);
                }
            }
            return "";
        }

        /// <summary>取得民國年月日</summary>
        /// <param name="ocrText"></param>
        /// <returns></returns>
        private static string ExtractRocDate(string ocrText)
        {
            // The regex looks for one or more digits (\d+), followed by '年', then digits for month and day. // It captures these digit groups.
            if (string.IsNullOrEmpty(ocrText)) return "";
            if (ocrText.IndexOf("]") > -1) { ocrText = ocrText.Replace("]", "1"); }
            string pattern = @"(\d+)\s*年\s*(\d+)\s*月\s*(\d+)\s*日";
            Match match = Regex.Match(ocrText, pattern);
            if (match.Success)
            {
                // Group 1 is the year, Group 2 is the month, Group 3 is the day
                return $"{match.Groups[1].Value}年{match.Groups[2].Value}月{match.Groups[3].Value}日";
            }
            return "";//"Date not found";
        }

        /// <summary>
        /// 取得銀行代碼 V_BANKLIST
        /// </summary>
        /// <returns></returns>
        private string GetBankCodeByCodeOrName(string bankCodeOrName)
        {
            var bankList = new WDAIIPWEBService().GetBankList();
            bankCodeOrName = bankCodeOrName.Replace(" ", "");
            if (bankCodeOrName.All(char.IsDigit))
            {
                return (bankList.Where(bank => bank.BnakCode == bankCodeOrName).FirstOrDefault()?.BnakCode) ?? "";
            }
            else
            {
                return (bankList.Where(bank => bank.Text.Contains(bankCodeOrName)).FirstOrDefault()?.BnakCode) ?? "";
            }
        }

        /// <summary>
        /// 取得分行代碼 V_BRANCHLIST
        /// </summary>
        /// <returns></returns>
        private string GetBranchCodeByCodeOrName(string bankCode, string branchCodeOrName)
        {
            if (string.IsNullOrEmpty(bankCode) || string.IsNullOrEmpty(branchCodeOrName)) return "";
            var branchList = new WDAIIPWEBService().GetBranchList(bankCode);
            branchCodeOrName = branchCodeOrName.Replace(" ", "");
            if (branchCodeOrName.All(char.IsDigit))
            {
                return (branchList.Where(branch => branch.BranchCode == branchCodeOrName).FirstOrDefault()?.BranchCode) ?? "";
            }
            else if (branchCodeOrName.Length > 3)
            {
                branchCodeOrName = branchCodeOrName.Substring(0, 4);
            }
            return (branchList.Where(branch => branch.Text.Contains(branchCodeOrName)).FirstOrDefault()?.BranchCode) ?? "";
        }

        /// <summary>
        /// 刪除OCR暫存的檔案
        /// </summary>
        /// <param name="originalFilePath"></param>
        private void DeleteTemporaryFiles(string originalFilePath)
        {
            try
            {
                //LOG.Debug(string.Concat("#UAPI.originalFilePath:", originalFilePath));
                if (originalFilePath == null || string.IsNullOrEmpty(originalFilePath)) { return; }
                // 刪除上傳的原圖片
                if (System.IO.File.Exists(originalFilePath))
                {
                    System.IO.File.Delete(originalFilePath);
                    LOG.Debug("檔案刪除: " + originalFilePath);
                }
            }
            catch (Exception ex)
            {
                var ExMsg1 = string.Concat("檔案刪除失敗: originalFilePath:", originalFilePath, ",ex.Message:", ex.Message);
                LOG.Error(ExMsg1, ex); //throw;
            }
        }

        /// <summary>
        /// 解析完整存摺封面資訊
        /// </summary>
        /// <param name="ocrText"></param>
        /// <returns></returns>
        private List<string> ExtractBankCode(string ocrText)
        {
            if (string.IsNullOrEmpty(ocrText)) { return null; }

            var matches = Regex.Matches(ocrText, @"\b\d{1,14}\b");

            string first = "";
            string second = "";
            string third = "";

            if (matches.Count > 0)
            {
                var filteredNumbers = matches.Cast<Match>().Where(m => m.Value.Length >= 3).Select(m => m.Value).ToList();

                foreach (var match in filteredNumbers)
                {
                    string value = match;
                    int length = value.Length;

                    if (length == 3 && first == "") { first = value; }
                    else if (length == 7 && second == "") { second = value; }
                    else if (length >= 8 && third == "") { third = value; }
                }

                return new List<string> { first, second, third }.Where(x => x != null).ToList();
            }
            return null;
        }

        /// <summary>解析文字辨視生日、身分證、發證日期、發證地</summary>
        /// <param name="ocrText"></param>
        /// <returns></returns>
        private List<string> ExtractRocPassPortCode(string ocrText)
        {
            if (string.IsNullOrEmpty(ocrText)) { return null; }
            //var matches = Regex.Matches(ocrText, @"\b\d{1,14}\b");
            //身分證字號/出生年月日/發證日期/發證地
            string first = "", second = "", third = "", fourth = "";
            //身分證字號 string idNumberPattern = @"[A-Z]\d{9}";
            Match match1 = Regex.Match(ocrText, @"[A-Z]\d{9}");
            first = match1.Value; // A234567890
            //出生年月日 Extract Date
            Match match2 = Regex.Match(ocrText, @"\d{2,3} 年 \d{1,2} 月 \d{1,2} 日");
            second = match2.Value; // 57 年 6 月 5 日
            if (string.IsNullOrEmpty(second)) { second = ExtractRocDate(ocrText); }
            //發證日期 3. 擷取 94 年 7 月 1 日
            Match match3 = Regex.Match(ocrText, @"(?<=民國 )\d{2,3} 年 \d{1,2} 月 \d{1,2} 日");
            third = match3.Value; // 94 年 7 月 1 日
            //發證地 4. 擷取 ( 北 市 ﹚ 換 發
            Match match4 = Regex.Match(ocrText, @"\( 北 市 ﹚ 換 發");
            fourth = match4.Value; // ( 北 市 ﹚ 換 發
            //string datePattern = @"\s*\d{1,3}\s*年\s*\d{1,2}\s*月\s*\d{1,2}\s*日";
            //MatchCollection dateMatches = Regex.Matches(ocrText, datePattern);
            //foreach (Match match in dateMatches)
            //{
            //    Console.WriteLine($"Found Date: {match.Value}");
            //}
            return new List<string> { first, second, third, fourth }.Where(x => x != null).ToList();
        }

        /// <summary>添加文字浮水印</summary>
        /// <param name="imagePath"></param>
        /// <param name="savePath"></param>
        /// <param name="smACID"></param>
        /// <returns></returns>
        private string ADDWatermarkImage(string imagePath, string savePath, string smACID)
        {
            if (string.IsNullOrEmpty(imagePath)) { return null; }

            string s_fileExt = ".jpg";
            var EPRS10 = string.Concat("w_f", RandomStringGenerator.GetRS10());
            string s_ServerTime = DateTime.Now.ToString("ddHHmmssfff"); //系統時間2
            string s_fileN = Path.GetFileName(string.Concat(EPRS10, "x", s_ServerTime));
            string s_fileNM = string.Concat(s_fileN, s_fileExt);
            string waterimagePaht = Path.Combine(savePath, s_fileNM);

            // 讀取原始圖片 imagePath "input.jpg"
            using (Mat src = Cv2.ImRead(imagePath))
            {
                // 設定浮水印文字 //"在職訓練網使用";
                string watermarkText = "OJT.WDA.GOV.TW";
                int[] ylist = { 60, 110, 180, 230 };
                //Scalar color = new Scalar(255, 255, 255); // 白色文字
                Scalar color = new Scalar(64, 64, 64); // 深灰色
                double fontSize = 1.5; // 字體大小
                int thickness = 2; // 字體粗細
                foreach (int iY in ylist)
                {
                    //浮水印位置// 添加文字浮水印
                    var position = new OpenCvSharp.Point(45, iY);
                    Cv2.PutText(src, watermarkText, position, HersheyFonts.HersheySimplex, fontSize, color, thickness);
                }
                Cv2.ImWrite(waterimagePaht, src);
                // 儲存結果圖片// 釋放資源 src.Dispose();
            }
            //LOG.Debug(string.Concat("#UAPIF.waterimagePaht:", waterimagePaht));
            return waterimagePaht;
        }

        /// <summary>添加圖片浮水印,中文字失敗</summary>
        /// <param name="imagePath"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public string ADDWatermarkImageErr(string imagePath, string savePath, string smACID)
        {
            // 檢查路徑是否為空 return Content("錯誤：圖片路徑不能為空。");
            if (string.IsNullOrEmpty(imagePath)) { return null; }

            string s_fileExt = ".jpg";
            var EPRS10 = string.Concat("w_f", RandomStringGenerator.GetRS10());
            string s_ServerTime = DateTime.Now.ToString("ddHHmmssfff"); //系統時間2
            string s_fileN = Path.GetFileName(string.Concat(EPRS10, "x", s_ServerTime));
            string s_fileNM = string.Concat(s_fileN, s_fileExt);
            string waterimagePaht = Path.Combine(savePath, s_fileNM);

            // 讀取圖片檔並轉換為 Mat 對象
            using (Mat src = Cv2.ImRead(imagePath))
            {
                // --- 浮水印邏輯，與上一個範例相同 ---
                string watermarkText = "僅供報名使用，翻拍無效";
                Scalar watermarkColor = Scalar.Red;
                double fontScale = 2.0;
                int thickness = 3;
                HersheyFonts font = HersheyFonts.HersheyComplex;

                int baseline = -1;
                OpenCvSharp.Size textSize = Cv2.GetTextSize(watermarkText, font, fontScale, thickness, out baseline);
                int textX = src.Cols - textSize.Width - 20;
                int textY = src.Rows - 20;

                var position = new OpenCvSharp.Point(textX, textY);
                Cv2.PutText(src, watermarkText, position, font, fontScale, watermarkColor, thickness, LineTypes.AntiAlias);

                // --- 將處理後的圖片保存到新的檔案路徑 ---
                //string newPath = Path.Combine(Path.GetDirectoryName(fullPath), "watermarked_" + Path.GetFileName(fullPath));
                // 保存圖片
                Cv2.ImWrite(waterimagePaht, src);
            }
            return waterimagePaht;
        }

        /// <summary>添加圖片浮水印,中文字成功</summary>
        /// <param name="imagePath"></param>
        /// <param name="savePath"></param>
        /// <param name="smACID"></param>
        /// <returns></returns>
        public string ADDWatermarkImageErr2(string imagePath, string savePath, string smACID) {
            // 浮水印文字
            string watermarkText = "僅供內部使用";
            string s_fileExt = ".jpg";
            var EPRS10 = string.Concat("w_f", RandomStringGenerator.GetRS10());
            string s_ServerTime = DateTime.Now.ToString("ddHHmmssfff"); //系統時間2
            string s_fileN = Path.GetFileName(string.Concat(EPRS10, "x", s_ServerTime));
            string s_fileNM = string.Concat(s_fileN, s_fileExt);
            string waterimagePaht = Path.Combine(savePath, s_fileNM);
            //try,{,},catch (Exception ex),{,Response.Write("發生錯誤：" + ex.Message);,},
            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // 使用支援中文字的字型，例如微軟正黑體
                    Font font = new Font("Microsoft JhengHei", 32, FontStyle.Bold);
                    SolidBrush brush = new SolidBrush(Color.FromArgb(128, 255, 0, 0)); // 半透明紅色
                    // 浮水印位置：左下角
                    int x = 44;
                    int y = bitmap.Height - 120;
                    graphics.DrawString(watermarkText, font, brush, new PointF(x, y));
                    // 儲存為新檔案
                    bitmap.Save(waterimagePaht);//, ImageFormat.Jpeg //Response.Write("圖片已儲存成功：" + savePath);
                }
            }
            return waterimagePaht;
        }

    }
}
