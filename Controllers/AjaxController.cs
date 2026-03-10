using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration; // Assuming App.config usage
using System.Management;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Services;


//using WDAIIP.WEB.Commons.Filter;,using System.Threading.Tasks;,using Newtonsoft.Json;,using System.Linq;,using System.IO;,using System.Drawing;
/// <summary>WDAIIP.WEB.Controllers.BaseController </summary>
namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// 這個類集中放置一些 Ajax 動作會用的的下拉代碼清單控制 action  
    /// </summary>
    public class AjaxController : BaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 取得系統時間
        /// <summary>取得系統時間(民國年時間 yyyy)</summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSysDate()
        {
            var result = new AjaxResultStruct
            {
                data = MyHelperUtil.DateTimeToTwFormatLongString((new MyKeyMapDAO()).GetSysDateNow())
            };
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>TEST-取得西元年格式系統時間（db time</summary>
        /// <returns></returns>
        public ActionResult GetADSysDate()
        {
            var result = new AjaxResultStruct
            {
                data = MyHelperUtil.DateTimeToADFormatLongString((new MyKeyMapDAO()).GetSysDateNow())
            };
            return Content(result.Serialize(), "application/json");
        }
        #endregion

        #region 會員報名資料維護
        /// <summary>
        /// 查詢是否需顯示停止報名訊息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult StopEnterTempMsg()
        {
            var result = new AjaxResultStruct();
            var serv = new WDAIIPWEBService();
            var sm = SessionModel.Get();

            if (serv.StopEnterTempMsg())
            {
                result.status = true; //在停止報名期間內回傳true，不允許繼續往下跑報名流程
                result.data = sm.LastResultMessage;
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 檢核是否在黑名單中
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckBlackList()
        {
            SessionModel sm = SessionModel.Get();
            var result = new AjaxResultStruct();
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            string msg = string.Empty;
            string retMsg = string.Empty;

            if (sm == null || string.IsNullOrEmpty(sm.ACID))
            {
                retMsg = "登入資訊有誤，請重新登入，再試一次！";
                //顯示訊息並停止報名維護作業
                result.status = false;
                result.data = retMsg;
                return Content(result.Serialize(), "application/json");
            }

            #region 黑名單判斷
            IList<Hashtable> listblock = (new WDAIIPWEBDAO()).GetBlackList(Convert.ToString(sm.ACID));
            //IList<Hashtable> listblock = (new WDAIIPWEBDAO()).GetBlackList2(s_IDNO, s_tplanID);
            retMsg = MyCommonUtil.Show_BlockMsg1(listblock);
            #endregion

            if (!string.IsNullOrEmpty(retMsg))
            {
                //有黑名單，顯示訊息並停止報名維護作業
                result.status = false;
                result.data = retMsg;
            }
            else
            {
                //無黑名單，回傳系統時間
                result.status = true;
                retMsg = MyHelperUtil.DateTimeToTwFormatLongString(DateTime.Now);
                result.data = retMsg;
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary> 線上報名-輸入課程代碼-送出報名(檢核) </summary>
        /// <param name="ocid"></param>
        /// <param name="plantype"></param>
        /// <returns></returns>
        public ActionResult CheckSendEnterClass1(string ocid, string plantype)
        {
            string msg = string.Empty;
            AjaxResultStruct result = new AjaxResultStruct() { status = false };
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            ClassClassInfoExtModel classInfo = null;

            long i_ocid = -1;
            if (!Int64.TryParse(ocid, out i_ocid))
            {
                i_ocid = -1;
                result.data = "課程代號有誤,請重新選擇正確的課程代號!!!";
                LOG.WarnFormat("[ALERT] {0}!!!", result.data);
                return Content(result.Serialize(), "application/json");
            }
            if (i_ocid == -1)
            {
                result.data = "課程代號有誤,請重新選擇正確的課程代號!!!";
                LOG.WarnFormat("[ALERT] {0}!!!", result.data);
                return Content(result.Serialize(), "application/json");
            }
            if (string.IsNullOrEmpty(plantype))
            {
                LOG.Warn("[ALERT] AjaxController CheckSendEnterClass1 : plantype 不可為空值");
                return base.SetPageNotFound(); //throw new ArgumentException("plantype");
            }

            switch (plantype)
            {
                case "1": //產投
                case "2": //在職進修
                case "5": //區域據點
                    break;
                default:
                    LOG.Warn("[ALERT] AjaxController CheckSendEnterClass1 : plantype 格式錯誤");
                    return base.SetPageNotFound();
                    //break;
            }

            if (string.IsNullOrEmpty(msg))
            {
                DateTime sysDate = keyDao.GetSysDateNow();
                string tplanid = string.Empty;
                switch (plantype)
                {
                    case "1": //產投
                        tplanid = "28";
                        break;
                    case "2": //在職進修
                        tplanid = "06";
                        break;
                    case "5": //區域據點
                        tplanid = "70";
                        break;
                    default:
                        LOG.Warn("[ALERT] AjaxController CheckSendEnterClass1 : plantype 格式錯誤");
                        return base.SetPageNotFound();
                }
                if (string.IsNullOrEmpty(tplanid)) { return base.SetPageNotFound(); }

                classInfo = (new WDAIIPWEBDAO()).GetOCIDDateByPlan(i_ocid, tplanid);

                if (classInfo == null)
                {
                    msg = $"請輸入正確的課程名稱代號!!!<br><br>目前系統時間為({MyHelperUtil.DateTimeToTwFormatLongString(sysDate)})";
                }
                else
                {
                    long chkTime1 = DateTime.Compare(sysDate, classInfo.SENTERDATE.Value);
                    long chkTime2 = DateTime.Compare(classInfo.FENTERDATE.Value, sysDate);

                    if (chkTime1 >= 0 && chkTime2 >= 0)
                    {
                        //報名產投課程時判斷是否已額滿
                        if ("28".Equals(classInfo.TPLANID))
                        {
                            int iType = this.GetEnterType2Func(classInfo);

                            if (iType == 2)
                            {
                                msg = $"本班級已報名額滿，現在報名將列為備取，請再與訓練單位聯繫確認!!!<br>按【確定】將進入報名程序，按【取消】則取消報名程序<br><br>目前系統時間為({MyHelperUtil.DateTimeToTwFormatLongString(sysDate)})";
                            }
                        }

                        result.status = true;
                    }
                    else if (chkTime1 < 0)
                    {
                        //此班級將於(該班可報名時間)開始報名
                        msg = $"此班級將於({MyHelperUtil.DateTimeToTwFormatLongString(classInfo.SENTERDATE.Value)})開始報名!!! <br><br>目前系統時間為({MyHelperUtil.DateTimeToTwFormatLongString(sysDate)})";
                    }
                    else
                    {
                        //此班報名時間已過
                        msg = $"{MyHelperUtil.DateTimeToTwFormatLongString(classInfo.FENTERDATE.Value)} 此班報名時間已過!!!<br><br>目前系統時間為({MyHelperUtil.DateTimeToTwFormatLongString(sysDate)})";
                    }
                }
            }

            result.data = msg;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary> 進入方式選擇 ref:sUtl_EtrType2Func </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int GetEnterType2Func(ClassClassInfoExtModel model)
        {
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            int ret = 1; //1:可繼續run報名作業 / 2:額滿訊息提示(confirm message)後再進入後續的報名作業
            int iEnterCnt = (new WDAIIPWEBDAO()).GetEnterCount((Int64)model.OCID.Value); //目前報名人數
            //bool blFlag : 1:招生中(false)'2:接受以備取報名(true)'3:報名額滿(true)
            bool blFlag = "1".Equals(model.ADMISSIONS) ? false : true;
            //報名人數額滿
            if (blFlag && iEnterCnt >= model.TNUM) { ret = 2; }
            return ret;
        }

        /// <summary>
        /// 送出報名檢核
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckSendEnterOnline2(string ocid, string plantype, string birth, string DB3D0C)
        {
            AjaxResultStruct result = new AjaxResultStruct();
            string msg = string.Empty;
            result.status = false;

            long i_ocid = -1;
            bool fg_ocid_ok = long.TryParse(ocid, out i_ocid);
            if (!fg_ocid_ok)
            {
                msg = "請輸入正確的課程名稱代號!!!";
                return base.SetPageNotFound(); //throw new ArgumentException("plantype");
            }

            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            ClassSignUpService serv = new ClassSignUpService();
            ClassClassInfoExtModel classInfo = null;

            if (string.IsNullOrEmpty(plantype))
            {
                LOG.Warn("[ALERT] AjaxController CheckSendEnterOnline2 : plantype 不可為空值");
                return base.SetPageNotFound(); //throw new ArgumentException("plantype");
            }

            switch (plantype)
            {
                case "1": //產投
                case "2": //在職進修
                case "5": //區域據點
                    break;
                default:
                    LOG.Warn("[ALERT] AjaxController CheckSendEnterOnline2 : plantype 格式錯誤");
                    return base.SetPageNotFound();
                    //break;
            }

            if (string.IsNullOrEmpty(birth))
            {
                LOG.Warn("[ALERT] AjaxController CheckSendEnterOnline2 : birth 不可為空值");
                return base.SetPageNotFound(); //throw new Exception("birth 格式錯誤");
            }
            DateTime dBirth;
            if (!DateTime.TryParse(birth, out dBirth))
            {
                LOG.Warn("[ALERT] AjaxController CheckSendEnterOnline2 : birth 格式錯誤");
                return base.SetPageNotFound(); //throw new Exception("birth 格式錯誤");
            }

            if (string.IsNullOrEmpty(msg))
            {
                DateTime sysDate = keyDao.GetSysDateNow();
                string tplanid = string.Empty;
                switch (plantype)
                {
                    case "1": //產投
                        tplanid = "28";
                        break;
                    case "2": //在職進修
                        tplanid = "06";
                        break;
                    case "5": //區域據點
                        tplanid = "70";
                        break;
                    default:
                        LOG.Warn("[ALERT] AjaxController CheckSendEnterOnline2 : plantype 格式錯誤");
                        return base.SetPageNotFound();
                }

                //加解 - 密(解)
                Turbo.Crypto.AesTk aesTk = new Turbo.Crypto.AesTk();
                string dtime_f1 = "yyyy-MM-dd HH:mm:ss";
                string s_DB3D0C = "";
                string vOCID = ""; //MyCommonUtil.GetMyValue1(s_DB3D0C, "OCID");
                string vSENTERDATE = ""; //MyCommonUtil.GetMyValue1(s_DB3D0C, "SENTERDATE");
                string vCLSID = ""; //MyCommonUtil.GetMyValue1(s_DB3D0C, "CLSID");
                string vEFTIME = "";
                DateTime dSENTERDATE = DateTime.Now.AddHours(-1);
                DateTime dEFTIME = DateTime.Now.AddHours(-1);
                // 比較兩個時間
                int i_res1 = 0;// DateTime.Compare(DateTime.Now, dSENTERDATE);
                int i_res2 = 0;//DateTime.Compare(DateTime.Now, dEFTIME);
                try
                {
                    //s_log1 = string.Concat("OCID=", iOCID, "&SENTERDATE=", SENTERDATE_f1, "&CLSID=", iCLSID, "&EFTIME=", s_fdEFTIME);
                    s_DB3D0C = string.IsNullOrEmpty(DB3D0C) ? null : aesTk.Decrypt(DB3D0C);
                    vOCID = string.IsNullOrEmpty(s_DB3D0C) ? null : MyCommonUtil.GetMyValue1(s_DB3D0C, "OCID");
                    vSENTERDATE = string.IsNullOrEmpty(s_DB3D0C) ? null : MyCommonUtil.GetMyValue1(s_DB3D0C, "SENTERDATE");
                    vCLSID = string.IsNullOrEmpty(s_DB3D0C) ? null : MyCommonUtil.GetMyValue1(s_DB3D0C, "CLSID");
                    vEFTIME = string.IsNullOrEmpty(s_DB3D0C) ? null : MyCommonUtil.GetMyValue1(s_DB3D0C, "EFTIME");
                    if (!DateTime.TryParse(vSENTERDATE, out dSENTERDATE)) { throw new ArgumentNullException("DB3D0C.SENTERDATE"); };
                    if (!DateTime.TryParse(vEFTIME, out dEFTIME)) { throw new ArgumentNullException("DB3D0C.EFTIME"); };
                    i_res1 = DateTime.Compare(DateTime.Now, dSENTERDATE);
                    i_res2 = DateTime.Compare(DateTime.Now, dEFTIME);
                }
                catch (Exception ex)
                {
                    LOG.Error("#CheckSendEnterOnline2: " + ex.Message, ex);
                    s_DB3D0C = "";
                }

                //查詢課程資料
                classInfo = (new WDAIIPWEBDAO()).GetOCIDDateByPlan(i_ocid, tplanid);

                if (classInfo == null || string.IsNullOrEmpty(vOCID) || string.IsNullOrEmpty(vSENTERDATE) || string.IsNullOrEmpty(vCLSID)
                    || !classInfo.OCID.HasValue || !classInfo.SENTERDATE.HasValue || !classInfo.CLSID.HasValue)
                {
                    msg = "報名班級資料有誤，請重新查詢！<br><br>";
                    //msg += "目前系統時間為(" + MyHelperUtil.DateTimeToTwFormatLongString(sysDate) + ")";
                }
                else if (vOCID != classInfo.OCID.Value.ToString() || vSENTERDATE != classInfo.SENTERDATE.Value.ToString(dtime_f1)
                    || vCLSID != classInfo.CLSID.Value.ToString() || i_res1 == -1 || i_res2 != -1)
                {
                    //LOG.Debug( String.Concat("#vSENTERDATE: " + vSENTERDATE));
                    //LOG.Debug(String.Concat("#vSENTERDATE2: " + classInfo.SENTERDATE.Value.ToString(dtime_f1)));
                    //LOG.Debug(String.Concat("#vCLSID: " + vCLSID));
                    //LOG.Debug(String.Concat("#vCLSID2: " + classInfo.CLSID.Value.ToString()));
                    //LOG.Debug(String.Concat("#i_res1: " + i_res1));
                    //LOG.Debug(String.Concat("#i_res2: " + i_res2));
                    msg = "報名班級資料有誤，請重新查詢！!<br><br>";
                    //msg += "目前系統時間為(" + MyHelperUtil.DateTimeToTwFormatLongString(sysDate) + ")";
                }
                else
                {
                    long chkTime1 = DateTime.Compare(sysDate, classInfo.SENTERDATE.Value);
                    long chkTime2 = DateTime.Compare(classInfo.FENTERDATE.Value, sysDate);

                    //todo 為配合2016年度課程公告作業，擬將2016年上半年度核可並轉班完成之課程統一於2016年1月23日0:01起方才能於產投報名網站上查詢到公告課程。 
                    if (chkTime1 >= 0 && chkTime2 >= 0)
                    {
                        //在報名時間內
                        result.status = true;
                    }
                    else if (chkTime1 < 0)
                    {
                        //此班級將於(該班可報名時間)開始報名
                        msg = "此班級將於(" + MyHelperUtil.DateTimeToTwFormatLongString(classInfo.SENTERDATE.Value) + ")開始報名!!! <br><br>";
                    }
                    else
                    {
                        //此班報名時間已過
                        msg = MyHelperUtil.DateTimeToTwFormatLongString(classInfo.FENTERDATE.Value) + " 此班報名時間已過!!!<br><br>";
                    }

                    if (string.IsNullOrEmpty(msg))
                    {
                        if ("28".Equals(classInfo.TPLANID))
                        {
                            //檢測此學員是否 可參訓 產業人才投資方案(大於15歲者)
                            if (!serv.CheckYearsOld15(dBirth, classInfo.STDATE))
                            {
                                msg = "學員資格 年齡不滿15歲 不符合可參訓條件！<br>";
                                result.status = false;
                            }
                        }

                    }
                }

                if (!string.IsNullOrEmpty(msg))
                {
                    msg += "目前系統時間為(" + MyHelperUtil.DateTimeToTwFormatLongString(sysDate) + ")";
                }
                //todo 黑名單判斷 2009/07/27 by waiming
            }

            result.data = msg;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 檢查報名狀況
        /// </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckData1X(string ocid, string plantype)
        {
            var result = new AjaxResultStruct();
            ClassSignUpService serv = new ClassSignUpService();
            //WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            SessionModel sm = SessionModel.Get();
            string msg = string.Empty;

            Int64 num = 0;
            if (string.IsNullOrEmpty(ocid))
            {
                LOG.Warn("[ALERT] AjaxController CheckData1X post (404): ocid 班級號碼有誤，請重新查詢");
                return base.SetPageNotFound(); //throw new ArgumentNullException("ocid");
            }
            if (!Int64.TryParse(ocid, out num))
            {
                LOG.Warn("[ALERT] AjaxController CheckData1X post (404): ocid 格式錯誤");
                return base.SetPageNotFound(); //throw new Exception("ocid 格式錯誤");
            }
            if (string.IsNullOrEmpty(plantype))
            {
                LOG.Warn("[ALERT] AjaxController CheckData1X post (404): plantype 不可為空值");
                return base.SetPageNotFound(); //throw new ArgumentException("plantype");
            }

            string tplanid = string.Empty;
            switch (plantype)
            {
                case "1": //產投
                    tplanid = "28";
                    break;
                case "2": //在職進修
                    tplanid = "06";
                    break;
                case "5": //區域據點
                    tplanid = "70";
                    break;
                default:
                    LOG.Warn("[ALERT] AjaxController CheckData1X : plantype 格式錯誤");
                    return base.SetPageNotFound();
                    //break;
            }

            ClassClassInfoExtModel classInfo = (new WDAIIPWEBDAO()).GetOCIDDateByPlan(num, tplanid);
            DateTime aNow = (new MyKeyMapDAO()).GetSysDateNow();
            if (classInfo == null)
            {
                msg = "報名班級資料有誤，請重新查詢！";
            }
            else
            {
                int chkTime1 = DateTime.Compare(aNow, Convert.ToDateTime(classInfo.SENTERDATE.Value));
                int chkTime2 = DateTime.Compare(aNow, Convert.ToDateTime(classInfo.FENTERDATE.Value));

                if (chkTime1 < 0)
                {
                    msg = "本班次尚未開放報名!!!";
                }
                else if (chkTime2 > 0)
                {
                    msg = classInfo.FENTERDATE_TW + " 此班報名時間已過!!!";
                }
                else if (chkTime1 >= 0 && chkTime2 <= 0)
                {
                    // 在開放報名期間, do nothing here
                    // string idno = sm.ACID;
                    // 產投重覆報名檢核, 移到 ClassSignUpService
                }
            }

            if (string.IsNullOrEmpty(msg))
            {
                result.status = true;
            }
            else
            {
                msg += string.Format("<br>目前系統時間為( {0} )", MyHelperUtil.DateTimeToTwFormatLongString(aNow));
                result.data = msg;
            }

            return Content(result.Serialize(), "application/json");
        }
        #endregion

        #region 會員報名記錄查詢
        /// <summary>
        /// 檢核是否可以允許取消報名 var data = { esetid: esetid, esernum: esernum, ocid: ocid };
        /// </summary>
        /// <param name="esernum"></param>
        /// <returns></returns>
        public ActionResult ChkCancelEnterType2(Int64 esetid, Int64 esernum, Int64 ocid)
        {
            var result = new AjaxResultStruct();
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBService serv = new WDAIIPWEBService();

            result.status = true;
            if (!serv.CheckCancelEnterClass(esetid, esernum, ocid))
            {
                result.data = sm.LastResultMessage;
            }

            return Content(result.Serialize(), "application/json");
        }
        #endregion

        #region 產投招訓簡章資訊
        public ActionResult GetWGReportMsg(string planid, string ocid, string rid)
        {
            Int64 iPlanid = 0;
            Int64 iOCID = 0;

            if (string.IsNullOrEmpty(planid))
            {
                LOG.Warn("[ALERT] AjaxController GetWGReportMsg post (404): planid 不可為空值");
                return base.SetPageNotFound(); //throw new ArgumentNullException("ocid");
            }
            if (!Int64.TryParse(planid, out iPlanid))
            {
                LOG.Warn("[ALERT] AjaxController GetWGReportMsg post (404): planid 格式錯誤");
                return base.SetPageNotFound(); //throw new Exception("planid 格式錯誤");
            }
            if (string.IsNullOrEmpty(ocid))
            {
                LOG.Warn("[ALERT] AjaxController GetWGReportMsg post (404): ocid 不可為空值");
                return base.SetPageNotFound(); //throw new ArgumentNullException("ocid");
            }
            if (!Int64.TryParse(ocid, out iOCID))
            {
                LOG.Warn("[ALERT] AjaxController GetWGReportMsg post (404): ocid 格式錯誤");
                return base.SetPageNotFound(); //throw new Exception("ocid 格式錯誤");
            }

            var result = new AjaxResultStruct();

            WGReportModel model = (new WDAIIPWEBDAO()).GetWGReport(iPlanid, iOCID, rid);

            result.data = model;

            return Content(result.Serialize(), "application/json");
        }
        #endregion

        #region 取得縣市代碼所對應的鄉鎮市區域郵遞區號
        /// <summary>
        /// Ajax 取得縣市代碼所對應的鄉鎮市區域郵遞區號清單
        /// </summary>
        /// <param name="CTID">縣市代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetZipCode(string CTID)
        {
            int iCTID = -1;
            if (!string.IsNullOrEmpty(CTID) && !int.TryParse(CTID, out iCTID)) { LOG.Warn("ActionResult GetZipCode: 傳入值有誤!CTID"); return base.SetPageNotFound(); }

            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCountyZipList(CTID);
            list.Insert(0, new KeyMapModel { CODE = "", TEXT = "請選擇" });
            return MyCommonUtil.BuildOptionHtmlAjaxResult(list, null, null);
        }
        #endregion

        /// <summary>GetZipName 取得 縣市轄區中文 查詢郵遞區號</summary>
        /// <param name="zipcode"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetZipName(string zipcode)
        {
            var result = new AjaxResultStruct();

            if (string.IsNullOrEmpty(zipcode)) { return Content(result.Serialize(), "application/json"); }

            string s_zipname = (new WDAIIPWEBDAO()).GetZipCodeName(zipcode);

            if (string.IsNullOrEmpty(s_zipname)) { s_zipname = "(郵遞區號有誤!)"; }

            result.data = s_zipname;

            return Content(result.Serialize(), "application/json");
        }

        #region 取得縮小單位範圍
        /// <summary> Ajax 取得縮小單位範圍清單 </summary>
        /// <param name="CTID">縣市代碼</param>
        /// <param name="SCHOOLNAME">單位名稱關鍵字</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSchoolList(string CTID, string ZIPCODE, string SCHOOLNAME)
        {
            Int64? iCTID = null;
            iCTID = MyCommonUtil.get_Int64_null(CTID);
            Int64? iZIPCODE = null;
            iZIPCODE = MyCommonUtil.get_Int64_null(ZIPCODE);
            //if ($.trim(data.arg.CTID) == "" && $.trim(data.arg.ZIPCODE) == "" && $.trim(data.arg.SCHOOLNAME) == "") data.box.html(blankHtml);
            if (string.IsNullOrEmpty(CTID) && string.IsNullOrEmpty(ZIPCODE) && string.IsNullOrEmpty(SCHOOLNAME)) { return base.SetPageNotFound(); }
            //string s_msg1 = "";
            //s_msg1 = string.Format("##GetSchoolList-ActionResult CTID={0},ZIPCODE={1},SCHOOLNAME={2};", CTID, ZIPCODE, SCHOOLNAME);
            //LOG.Debug(s_msg1);
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetSchoolList(iCTID, iZIPCODE, SCHOOLNAME);
            list.Insert(0, new KeyMapModel { CODE = "", TEXT = "請選擇" });
            return MyCommonUtil.BuildOptionHtmlAjaxResult(list, null, null);
        }
        #endregion

        #region GetFooter 回傳網站表尾畫面資訊
        /// <summary> 查詢取得網站表尾相關資訊 </summary>
        /// <returns></returns>
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 60)]
        public ActionResult GetFooter()
        {
            //[Authorize] //需要登入，才能使用此功能
            // 搭配檢視畫面（View）的Razor表單裡面，「@Html.AntiForgeryToken()」這句話以避免CSRF攻擊！！
            //[ValidateAntiForgeryToken]   // 避免CSRF攻擊
            MyKeyMapDAO dao = new MyKeyMapDAO();

            /*隱私權及安全政策*/ //ViewBag.Privacy = dao.GetFuncContent("004");

            /*政府網站資訊開放宣告*/ //ViewBag.GovAnnounce = dao.GetFuncContent("005");

            /*頁底單位資訊*/
            string strUnit = HttpUtility.HtmlDecode(dao.GetFuncContent("006"));

            /*頁底版權資訊維護資訊*/
            string strCopyright = HttpUtility.HtmlDecode(dao.GetFuncContent("007"));

            /*取得主機ip-分流識別目前連線哪一台主機 debug用 （1 完整ip, 2 最後三碼）*/
            string strLocalAddr = MyCommonUtil.GetLocalAddr("2");

            ViewBag.Unit = strUnit;
            ViewBag.Copyright = strCopyright;
            ViewBag.LocalAddr = strLocalAddr;

            if (!Request.IsAjaxRequest())
            {
                LOG.Warn("#Ajax/GetFooter,!Request.IsAjaxRequest!!");
                return Content("AuditAttack-Fail");
            }

            return PartialView("_FooterDetail");
        }
        #endregion

        #region GetZipList 查詢取得特定縣市的區域選項資訊
        /// <summary>
        /// Ajax 傳回縣市代碼對應的縣市中文名稱(單筆)
        /// </summary>
        /// <param name="CITYCODE">縣市代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetZipList(string CITYCODE)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCountyZipList(CITYCODE);

            return PartialView("_SelectOptions", list);
        }
        #endregion

        #region GetTMIDList 查詢取得特定職類課程的業別
        /// <summary>
        /// Ajax 取得 代碼所對應的 清單
        /// </summary>
        /// <param name="CTID">縣市代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetTMIDList(string JOBTMID)
        {
            IList<KeyMapModel> list = null;
            Int64 i_JOBTMID = -1;
            if (!Int64.TryParse(JOBTMID, out i_JOBTMID)) { return MyCommonUtil.BuildOptionHtmlAjaxResult(list, null, null); }

            MyKeyMapDAO dao = new MyKeyMapDAO();
            list = dao.GetTMIDItemLv2List(i_JOBTMID);
            list.Insert(0, new KeyMapModel { CODE = "", TEXT = "請選擇" });
            return MyCommonUtil.BuildOptionHtmlAjaxResult(list, null, null);
        }
        #endregion

        /// <summary> Ajax 取得 代碼所對應的 清單 </summary>
        /// <param name="PARENT1"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetTMIDJOBList(string PARENT1)
        {
            IList<KeyMapModel> list = null;
            Int64 i_PARENT1 = -1;
            if (!Int64.TryParse(PARENT1, out i_PARENT1)) { return MyCommonUtil.BuildOptionHtmlAjaxResult(list, null, null); }

            MyKeyMapDAO dao = new MyKeyMapDAO();
            list = dao.GetTMIDJOBList(i_PARENT1);
            list.Insert(0, new KeyMapModel { CODE = "", TEXT = "請選擇" });
            return MyCommonUtil.BuildOptionHtmlAjaxResult(list, null, null);
        }

        #region 會員專區-下載訓練證明 

        /// <summary>下載訓練證明</summary>
        /// <param name="rte"></param>
        /// <param name="tmd"></param>
        /// <param name="socid"></param>
        /// <param name="ocid"></param>
        /// <param name="clsid"></param>
        /// <param name="studstatus"></param>
        /// <returns></returns>
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 6)]
        public ActionResult GetOjtReportMsg(string rte, string tmd, string socid, string ocid, string clsid, string studstatus)
        {
            AjaxResultStruct result = new AjaxResultStruct();
            // rte: rte, tmd: tmd, socid: socid, ocid: ocid, studstatus: studstatus
            //LOG.Debug(string.Format("#GetOjtReportMsg, rte:{0},  tmd:{1}, socid:{2}, ocid:{3}, clsid:{4}, studstatus:{5}", rte,  tmd, socid, ocid, clsid, studstatus));
            TrainCertReportModel model = (new WDAIIPWEBDAO()).GetOjtReport(rte, tmd, socid, ocid, clsid, studstatus);
            result.status = (model != null);
            result.data = model;
            //LOG.Debug(string.Concat("#GetOjtReportMsg, result.status :", result.status));
            //LOG.Debug(string.Concat("#GetOjtReportMsg, result.data :", result.data));
            return Content(result.Serialize(), "application/json");
        }
        #endregion

        /// <summary>【就業通／關鍵字庫／技能專長】資料檢視API_1120920</summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public ActionResult GetSolrApi(string q)
        {
            string FunctionName = "AjaxController: *GetSolrApi";
            //Logging http://localhost:2866/Ajax/GetSolrApi
            //LOG.Debug($"{FunctionName} ,q={q}");
            if (string.IsNullOrEmpty(q))
            {
                LOG.Warn($"[ALERT] {FunctionName}: q 不可為空值");
                return base.SetPageNotFound(); //throw new ArgumentNullException("ocid");
            }
            LOG.Debug($"{FunctionName} ,q={q}");

            // WebRequest物件如何忽略憑證問題 // Ignoring validation for demonstration purposes
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            // TLS 1.2-基礎連接已關閉: 傳送時發生未預期的錯誤 
            // Set TLS protocol (if needed) // client.DefaultRequestHeaders.Add("Connection", "close"); // May be necessary for TLS 1.2 // client.DefaultRequestHeaders.Expect = null; // May be necessary for TLS 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;//3072

            using (var client = new HttpClient())
            {
                // Get WebApi URL from configuration
                string default_WebApi_URL1 = "https://job.taiwanjobs.gov.tw/SolrApi.aspx";
                string webApiUrl = ConfigurationManager.AppSettings["SolrApi_URL1"];
                // Replace with your default URL
                if (string.IsNullOrEmpty(webApiUrl)) { webApiUrl = default_WebApi_URL1; }

                string urlWithParams = $"{webApiUrl}?q={q}";
                LOG.Info($"{FunctionName} ,urlWithParams: {webApiUrl}, {urlWithParams}");

                // Send GET request
                HttpResponseMessage response = null;
                try
                {
                    // Send GET request
                    response = client.GetAsync(urlWithParams).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        string strResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        return Content(strResponse, "application/json");
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error($"#{FunctionName}: " + ex.Message, ex);
                    //throw;
                }
                if (response != null) { LOG.Error($"{FunctionName},HTTP request failed with status code: {response.StatusCode}"); }
                return base.SetPageNotFound();
            }
        }

        /// <summary>【就業通】職訓專長能力標籤斷字分詞處API_1130301</summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public ActionResult GetStrSegment(string str)
        {
            string FunctionName = "AjaxController: *GetStrSegment";
            //Logging http://localhost:2866/Ajax/GetSolrApi
            //LOG.Debug($"{FunctionName} ,q={q}");
            if (string.IsNullOrEmpty(str))
            {
                LOG.Warn($"[ALERT] {FunctionName}: str 不可為空值");
                return base.SetPageNotFound(); //throw new ArgumentNullException("ocid");
            }
            LOG.Debug($"{FunctionName} ,str={str}");

            // WebRequest物件如何忽略憑證問題 // Ignoring validation for demonstration purposes
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            // TLS 1.2-基礎連接已關閉: 傳送時發生未預期的錯誤 
            // Set TLS protocol (if needed) // client.DefaultRequestHeaders.Add("Connection", "close"); // May be necessary for TLS 1.2 // client.DefaultRequestHeaders.Expect = null; // May be necessary for TLS 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;//3072

            using (var client = new HttpClient())
            {
                // Get WebApi URL from configuration
                string default_WebApi_URL2 = "https://job.taiwanjobs.gov.tw/StrSegment.ashx";
                string webApiUrl = ConfigurationManager.AppSettings["StrSegment_URL2"];
                // Replace with your default URL
                if (string.IsNullOrEmpty(webApiUrl)) { webApiUrl = default_WebApi_URL2; }

                string urlWithParams = $"{webApiUrl}?str={str}";
                LOG.Info($"{FunctionName} ,urlWithParams: {webApiUrl}, {urlWithParams}");

                // Send GET request
                HttpResponseMessage response = null;
                try
                {
                    // Send GET request
                    response = client.GetAsync(urlWithParams).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        string strResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        return Content(strResponse, "application/json");
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error($"#{FunctionName}: " + ex.Message, ex);
                    //throw;
                }
                if (response != null) { LOG.Error($"{FunctionName},HTTP request failed with status code: {response.StatusCode}"); }
                return base.SetPageNotFound();
            }
        }

        #region NO USE
        //public async Task<ActionResult<StrSegmentResponse>> GetStrSegment(string str)
        //{
        //    if (string.IsNullOrEmpty(str))
        //    {
        //        LOG.Warn($"[ALERT] AjaxController GetStrSegment: str cannot be null");
        //        return BadRequest("str parameter is required"); // More specific error message
        //    }

        //    LOG.Debug($"GetStrSegment, str={str}");

        //    Configure HttpClient with TLS 1.2(if needed)
        //        HttpClient client = new HttpClient();
        //    client.DefaultRequestHeaders.Connection = "close"; // May be necessary for TLS 1.2
        //    client.DefaultRequestHeaders.Expect = null; // May be necessary for TLS 1.2

        //    string webApiUrl = ConfigurationManager.AppSettings["StrSegment_URL2"];
        //    if (string.IsNullOrEmpty(webApiUrl))
        //    {
        //        webApiUrl = "https://job.taiwanjobs.gov.tw/StrSegment.ashx";
        //    }

        //    string urlWithParams = $"{webApiUrl}?str={str}";
        //    LOG.Info($"GetStrSegment, urlWithParams: {webApiUrl}, {urlWithParams}");

        //    try
        //    {
        //        HttpResponseMessage response = await client.GetAsync(urlWithParams);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            string strResponse = await response.Content.ReadAsStringAsync();
        //            Assuming JSON response, deserialize using a suitable JSON parser
        //            StrSegmentResponse result = JsonConvert.DeserializeObject<StrSegmentResponse>(strResponse);
        //            return Ok(result);
        //        }
        //        else
        //        {
        //            LOG.Error($"HTTP request failed with status code: {response.StatusCode}");
        //            return StatusCode(response.StatusCode, "API request failed"); // More informative message
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LOG.Error($"Error during GetStrSegment: {ex.Message}");
        //        return StatusCode(StatusCodes.InternalServerError, "Internal server error"); // Generic error for unexpected issues
        //    }
        //}
        #endregion

        /// <summary>
        /// 寄送EMAIL
        /// </summary>
        /// <param name="oECdata"></param>
        void DigiSendEmail(TblE_EMAILCODE oECdata)
        {
            //var dao = new WDAIIPWEBDAO();,DateTime aNow = (new MyKeyMapDAO()).GetSysDateNow();,E_EMAILCODE
            //測試環境不寄信
            string s_Env = ConfigModel.WebEnvironment ?? "";
            if (s_Env == "test" || !string.IsNullOrEmpty(s_Env)) { oECdata.EMAIL = (new ExceptionHandler(new ErrorPageController())).cst_EmailtoMe; }
            string s_TestEmail = ConfigModel.WebTestEmail ?? "";
            if (!string.IsNullOrEmpty(s_TestEmail) && s_TestEmail.IndexOf(';') > -1) { s_TestEmail = s_TestEmail.Replace(';', ','); }
            bool fg2 = (!string.IsNullOrEmpty(s_TestEmail) && s_TestEmail.IndexOf(",") > -1);//多筆
            bool fg3 = (!string.IsNullOrEmpty(s_TestEmail));//單筆

            //2.寄email
            const string cst_Subject = "「在職訓練網」email驗證通知，請完成操作";
            const string strFromName = "系統自動發信";
            string strFromEmail = "ojt@msa.wda.gov.tw";
            var strToName = oECdata.EMVCODE; //sm.UserName;
            var strToEmail = oECdata.EMAIL;
            var strHtmlBody = oECdata.SNSTEXT;
            //WebRequest物件如何忽略憑證問題
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ExceptionHandler.ValidateServerCertificate);
            //TLS 1.2-基礎連接已關閉: 傳送時發生未預期的錯誤 
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;//3072
            string strResult = "";
            string s_LOGError = "";
            string s_Err1 = "";
            //bool flag_mail_error = false;
            //發信失敗? //https://wltims.wda.gov.tw/GetJobMail3/Service1.asmx //https://ojfile119.ejob.gov.tw/GetJobMail3/Service1.asmx
            SendMailws3.Service1 m = new SendMailws3.Service1();

            string ee1 = "";
            try
            {
                var testSubject = string.Concat(cst_Subject, "(TEST)");
                if (fg2 && !string.IsNullOrEmpty(s_TestEmail))
                {
                    foreach (var vEmail in s_TestEmail.Split(','))
                    {
                        if (!string.IsNullOrEmpty(vEmail))
                        {
                            ee1 = vEmail;
                            strResult = m.SendMailT(strFromName, strFromEmail, strToName, vEmail, testSubject, strHtmlBody, "");
                        }
                    }
                }
                else if (fg3 && !string.IsNullOrEmpty(s_TestEmail))
                {
                    ee1 = s_TestEmail;
                    strResult = m.SendMailT(strFromName, strFromEmail, strToName, s_TestEmail, testSubject, strHtmlBody, "");
                }
                else
                {
                    ee1 = strToEmail;
                    strResult = m.SendMailT(strFromName, strFromEmail, strToName, strToEmail, cst_Subject, strHtmlBody, "");
                }
            }
            catch (Exception ex)
            {
                s_Err1 = ex.Message;
                //flag_mail_error = true;
                s_LOGError = string.Format("DigiSendEmail({0}):\n sMailBody:\n{1}\n ex.Message:\n{2}\n", ee1, strHtmlBody, ex.Message);
                LOG.Error(s_LOGError, ex);
                bLOG.WarnFormat("發信失敗，請確認您的Email是否正確![{0}]{1} ", ee1, s_Err1);
                throw ex;
            }
        }

        /// <summary>
        /// 驗證_EMAIL
        /// </summary>
        /// <param name="ee1"></param>
        /// <param name="dc1"></param>
        /// <param name="idn1"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DigiCheckEMAIL(string ee1, string dc1, string idn1)
        {
            // var data = { "ee1": ee1, "dc1": dc1, "idn1": idn1};
            var dao = new WDAIIPWEBDAO();
            var bdao = new BaseDAO();
            //var IP = HttpContext.Request.UserHostAddress;
            var result = new AjaxResultStruct();
            result.status = false;
            result.message = "";
            try
            {
                if (string.IsNullOrEmpty(ee1))
                {
                    result.message = "請輸入EMAIL !";
                    return Content(result.Serialize(), "application/json");
                }
                else if (string.IsNullOrEmpty(idn1))
                {
                    result.message = "請輸入身分證字號 !";
                    return Content(result.Serialize(), "application/json");
                }
                else if (string.IsNullOrEmpty(dc1) || dc1.Length < 10)
                {
                    result.message = "案號資料有誤，請重新申請動作 !";
                    return Content(result.Serialize(), "application/json");
                }

                SessionModel sm = SessionModel.Get();
                if (sm == null || !sm.IsLogin || string.IsNullOrEmpty(sm.ACID))
                {
                    result.message = "登入資訊有誤，請重新登入動作 !";
                    return Content(result.Serialize(), "application/json");
                }
                else if (sm.ACID != idn1)
                {
                    result.message = "登入資訊輸入身分證字號有誤 !";
                    return Content(result.Serialize(), "application/json");
                }
                long iMemSN = 0;
                if (!long.TryParse(sm.MemSN.ToString(), out iMemSN))
                {
                    string s_LOGErr = "MemSN轉換失敗!"; LOG.Error(s_LOGErr);
                    //throw new ArgumentException("MemSN轉換失敗!"); //var aEx = new ArgumentException(s_LOGErr); LOG.Error(s_LOGErr, aEx);
                    result.message = s_LOGErr; //"登入資訊輸入身分證字號有誤 !";
                    return Content(result.Serialize(), "application/json");
                }

                var minutes = 5;
                // 取得資料(案號資料)
                var oECode1 = dao.GetRow(new TblSTUD_DIGICERTAPPLY { DCASENO = dc1 });
                if (oECode1 != null)
                {
                    result.message = "案號資料重複，請重新登入申請 !";
                    return Content(result.Serialize(), "application/json");
                }
                var param1 = new Hashtable { ["MEM_SN"] = iMemSN };
                var oEMlist1 = dao.QueryEmailCodeShort1(param1);
                if (oEMlist1 != null && oEMlist1.Count > 0)
                {
                    result.message = "申請資料時間過短，請稍後再申請!(5分鐘限制)"; //限制5分鐘
                    return Content(result.Serialize(), "application/json");
                }

                var param2 = new Hashtable { ["IDNO"] = sm.ACID };
                var oECodelist2 = dao.QueryDigiCertToday1(param2);
                if (oECodelist2 != null && oECodelist2.Count > 2)
                {
                    result.message = "當天申請資料超過上限次數，請隔天再申請 !(1天最多3次)"; //限制24小時內最多3次
                    return Content(result.Serialize(), "application/json");
                }
                //new TimeSpan(date1.Ticks - date2.Ticks).TotalMinutes
                //var x = oECodelist1.Where(m => DateTime.datediff()m.E_UDATE == true).Select(m => m.OCID).ToList();
                var modip = HttpContext.Request.UserHostAddress;

                //產生驗證碼
                //var MinValueTicks = DateTime.MinValue.Ticks;
                //var MaxValueTicks = DateTime.MaxValue.Ticks;
                var seed64 = DateTime.Now.Ticks;
                var rnd = new Random();
                if (seed64 <= int.MaxValue && seed64 >= int.MinValue)
                {
                    int seed = Convert.ToInt32(seed64);
                    rnd = new Random(seed);
                }
                int randomNumber = rnd.Next(1000000, 10000000); // 尾數排除在外
                var emvcode = Convert.ToString(randomNumber);

                DateTime aNow = (new MyKeyMapDAO()).GetSysDateNow();
                //var ojtweba1 = "<strong><a href=\"https://ojt.wda.gov.tw/\" target=\"_blank\" title=\"[另開新視窗]前往在職訓練網\"><span class=\"sr-only\">(另開新視窗)</span>「在職訓練網」</a></strong>";
                var ojtweba1 = "https://ojt.wda.gov.tw";
                var SNStext = "";
                //SNStext += "「在職訓練網」email驗證通知，請完成操作";
                SNStext += "在職訓練學員 您好：<br/><br/>";
                SNStext += string.Concat("Email驗證密碼: 【", emvcode, "】<br/><br/>");
                SNStext += string.Concat("請於", minutes, "分鐘內，至「在職訓練網」( ", ojtweba1, " ) 輸入電子郵件驗證碼，若驗證密碼已逾期，請在原頁面重新操作，謝謝您。<br/><br/>");
                SNStext += string.Concat("＊此為系統自動發送信件，請勿直接回信。", aNow.ToString("yyyy-MM-dd HH:mm:ss"), "<br/>");

                long iEMAIL_SN = dao.GetNewId("E_EMAILCODE_EMAIL_SN_SEQ,E_EMAILCODE,EMAIL_SN").Value; //dao.GetAutoNum()
                var oECdata = new TblE_EMAILCODE()
                {
                    EMAIL_SN = iEMAIL_SN,
                    MEM_SN = iMemSN,
                    DCASENO = dc1,
                    EMAIL = ee1,
                    EMVCODE = emvcode,
                    CHECKS = "0",
                    SNSTEXT = SNStext,
                    E_OPUSER = sm.RID,
                    E_CDATE = aNow
                };

                bool flag_mail_error = false;
                try
                {
                    //2.寄email
                    DigiSendEmail(oECdata);
                    //strResult = m.SendMailT(strFromName, strFromEmail, strToName, strToEmail, cst_Subject, strHtmlBody, "");
                }
                catch (Exception ex)
                {
                    flag_mail_error = true;
                    LOG.Error(ex.Message, ex); //throw;
                }
                if (flag_mail_error)
                {
                    //bLOG.WarnFormat("發信失敗，請確認您的Email是否正確![{0}]{1} ", ee1, s_Err1);
                    result.message = "發信失敗，請確認您的Email是否正確!";
                    return Content(result.Serialize(), "application/json");
                }

                dao.Insert(oECdata);
                //1.寫資料庫
                result.status = true;
            }
            catch (Exception ex)
            {
                this.LogError("CheckCPHNOE Failed : " + ex.Message, ex);
                //result.status = false;
                result.message = ex.Message;
                return Content(result.Serialize(), "application/json");
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>下載 數位結訓證明下載-封面</summary>
        /// <param name="dcano"></param>
        /// <param name="dcaseno"></param>
        /// <param name="emvcode"></param>
        /// <returns></returns>
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 6)]
        public ActionResult GetOJTSD14031C(string dcano, string dcaseno, string emvcode)
        {
            AjaxResultStruct result = new AjaxResultStruct();
            var dao = new WDAIIPWEBDAO();
            DigiCertRptModel model = dao.QueryOjtSD14031C(dcano, dcaseno, emvcode);
            result.status = (model != null);
            result.data = model;
            SAVE_STUD_DIGICERTAPPLYDL(dcano);
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>下載 數位結訓證明下載-內容</summary>
        /// <param name="dcano"></param>
        /// <param name="dcaseno"></param>
        /// <param name="emvcode"></param>
        /// <returns></returns>
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 6)]
        public ActionResult GetOJTSD14031D(string dcano, string dcaseno, string emvcode)
        {
            AjaxResultStruct result = new AjaxResultStruct();
            var dao = new WDAIIPWEBDAO();
            DigiCertRptModel model = dao.QueryOjtSD14031D(dcano, dcaseno, emvcode);
            result.status = (model != null);
            result.data = model;
            SAVE_STUD_DIGICERTAPPLYDL(dcano);
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>儲存下載時間-STUD_DIGICERTAPPLYDL</summary>
        /// <param name="dcano"></param>
        private void SAVE_STUD_DIGICERTAPPLYDL(string dcano)
        {
            SessionModel sm = SessionModel.Get();
            //取得目前(DB)系統時間
            var keyDao = new MyKeyMapDAO();
            var nowTime = keyDao.GetSysDateNow();
            var dao = new WDAIIPWEBDAO();
            var i_dlano = dao.GetNewId("STUD_DIGICERTAPPLYDL_DLANO_SEQ,STUD_DIGICERTAPPLYDL,DLANO").Value;
            var i_dcano = long.Parse(dcano);
            var oDLANO1 = new TblSTUD_DIGICERTAPPLYDL() { DLANO = i_dlano, DCANO = i_dcano, MODIFYACCT = sm.ACID, MODIFYDATE = nowTime };
            dao.Insert(oDLANO1);
        }

        /// <summary>
        /// 會員專區--學員線上表單
        /// </summary>
        /// <param name="rte"></param>
        /// <param name="socid"></param>
        /// <param name="ocid"></param>
        /// <param name="sid"></param>
        /// <param name="elno"></param>
        /// <returns></returns>
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 6)]
        public ActionResult GetSignReport1(string rte, string socid, string ocid, string sid, string elno)
        {
            AjaxResultStruct result = new AjaxResultStruct();
            var dao = new WDAIIPWEBDAO();
            var model = dao.QuerySignReport1(socid, ocid, sid, elno);
            if ((model != null))
            {
                model.RTE = rte;
                model.ELNO = elno;
            }
            //有取得資料，且有報表名稱
            result.status = (model != null && !string.IsNullOrEmpty(model.PRINTFILENAME1));
            result.data = model;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 取得銀行下拉選單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 6)]
        public JsonResult GetBankList()
        {
            LOG.Debug("#Ajax/GetBankList");
            WDAIIPWEBService serv = new WDAIIPWEBService();
            var result = serv.GetBankList();
            return Json(result);
        }

        /// <summary>
        /// 取得銀行下拉選單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetBranchList(string code)
        {
            LOG.Debug("#Ajax/GetBranchList");
            WDAIIPWEBService serv = new WDAIIPWEBService();
            var result = serv.GetBranchList(code);
            return Json(result);
        }

        #region Easter Egg
        /// <summary>Test-查詢檔案-最後異動日期</summary>
        /// <returns></returns>
        public ActionResult FileLastModifiedt4()
        {
            var result = new AjaxResultStruct();
            bool fg_ft4 = false;
            string rst = "";
            /*https://ojt.wda.gov.tw/Ajax/FileLastModifiedt4*/
            // -- \c$\web\WEB SRC\sou\WDAIIP.WEB\bin\WDAIIP.WEB.dll 35.svn_up_ojt_WEB_1_rc //"C:\\web\\WEB\\bin\\WDAIIP.WEB.dll";
            string filePath = Server.MapPath("~/bin/WDAIIP.WEB.dll");
            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    DateTime lastModified = fileInfo.LastWriteTime;
                    rst += $"檔案：{filePath},最後異動日期：{lastModified}";
                    fg_ft4 = true;
                }
                else
                {
                    rst += $"檔案不存在：{filePath}!";
                }
            }
            catch (Exception ex)
            {
                rst += $"發生錯誤：{ex.Message}!!!";
            }
            result.status = fg_ft4;
            result.message = rst;
            return Content(result.Serialize(), "application/json");
        }
        /// <summary>
        /// Test-下載config檔案
        /// </summary>
        /// <returns></returns>
        public ActionResult GetShowWebConfigTxt1()
        {
            // Ajax/GetShowWebConfigTxt1
            var result = new AjaxResultStruct();
            bool fg_rst = false;
            string rst_msg = "";
            string filePath = Server.MapPath("~/Web.config");
            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    var Vdff = DateTime.Now.ToString("fff");
                    // 設定檔案的 Content-Type // 對於文字檔，通常是 "text/plain" string contentType = "text/plain"; // 設定下載檔案的名稱
                    string fileName = string.Concat("WCfT", Vdff, ".txt");
                    // 返回 FileResult，讓瀏覽器下載檔案
                    return File(filePath, "text/plain", fileName);
                }
                else
                {
                    rst_msg += $"檔案不存在：{filePath}!";
                }
            }
            catch (Exception ex)
            {
                rst_msg += $"發生錯誤：{ex.Message}!!!";
            }
            result.status = fg_rst;
            result.message = rst_msg;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// Test-.NET Framework 版本
        /// </summary>
        /// <returns></returns>
        public ActionResult QueryDotnetVer()
        {
            var result = new AjaxResultStruct();
            string rst_msg = ""; //RuntimeVer.QueryDotnetVer();
            bool fg_status = false;
            try
            {
                rst_msg = RuntimeVer.GetCurrentDotNetVersionDetails();
                fg_status = true;
            }
            catch (Exception ex)
            {
                rst_msg = string.Concat(rst_msg, "::", ex.Message);
            }
            result.status = fg_status;
            result.message = rst_msg;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// Test-獲取開機時間
        /// </summary>
        /// <param name="args"></param>
        public ActionResult GetSystemBootTime()
        {
            var result = new AjaxResultStruct();
            // 獲取自系統啟動以來經過的毫秒數
            long tickCount = Environment.TickCount;//.TickCount64;

            // 將毫秒轉換為 TimeSpan
            TimeSpan uptime = TimeSpan.FromMilliseconds(tickCount);

            // 計算開機時間
            DateTime bootTime = DateTime.Now - uptime;

            string rst_msg = $"系統已運行時間: {uptime.Days} 天 {uptime.Hours} 小時 {uptime.Minutes} 分鐘 {uptime.Seconds} 秒, 系統開機時間: {bootTime}";
            // 您也可以只獲取開機時間的日期部分 //Console.WriteLine($"系統開機日期: {bootTime.ToShortDateString()}");
            result.message = rst_msg;
            return Content(result.Serialize(), "application/json");
        }
        /// <summary>
        /// Test-TestRandom56
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTestRandom56()
        {
            var result = new AjaxResultStruct();
            string rst_msg = $"(new Random()).Next(-5, 6): {(new Random()).Next(-5, 6)}";
            result.message = rst_msg;
            return Content(result.Serialize(), "application/json");
        }

        //using System;,using System.Management;,using System.Text;
        /// <summary>
        /// 查詢本機系統的記憶體狀態（包括總量、已使用與空閒空間）
        /// </summary>
        /// <returns>記憶體狀態字串</returns>
        public ActionResult GetMemoryStatus()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            var sb1 = new StringBuilder();

            foreach (ManagementObject queryObj in searcher.Get())
            {
                // 取得總實體記憶體 (KB 轉為 GB)
                double totalMemory = Math.Round(Convert.ToDouble(queryObj["TotalVisibleMemorySize"]) / 1024 / 1024, 2);

                // 取得目前可用實體記憶體 (KB 轉為 GB)
                double freeMemory = Math.Round(Convert.ToDouble(queryObj["FreePhysicalMemory"]) / 1024 / 1024, 2);

                // 計算已使用記憶體
                double usedMemory = Math.Round(totalMemory - freeMemory, 2);

                sb1.AppendLine($"總記憶體: {totalMemory} GB");
                sb1.AppendLine($"已使用: {usedMemory} GB");
                sb1.AppendLine($"空閒中: {freeMemory} GB");
            }

            //return sb1.ToString();string rst_msg = sb1.ToString();
            var result = new AjaxResultStruct();
            result.message = sb1.ToString();
            return Content(result.Serialize(), "application/json");
        }
        #endregion
    }
}
