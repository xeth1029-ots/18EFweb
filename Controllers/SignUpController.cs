using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Commons.Filter;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Services;
using Newtonsoft.Json;

namespace WDAIIP.WEB.Controllers
{
    public class SignUpController : LoginBaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string cst_SEnterDate2016_28 = "2016/01/26 12:00:00";

        // GET: SignUp 報名
        public ActionResult Index(string PlanType, string ProvideLocation, Int64? OCID)
        {
            SessionModel sm = SessionModel.Get();
            Int64 ridNum = -1;
            bool fg_Get_ridNum = Int64.TryParse(sm.RID, out ridNum);
            if (!fg_Get_ridNum) { throw new ArgumentNullException("ridNum"); }
            if (!"Y".Equals(sm.IsOnlineSignUp) && string.IsNullOrEmpty(PlanType))
            {
                throw new ArgumentNullException("PlanType");
            }
            if (!"Y".Equals(sm.IsOnlineSignUp) && string.IsNullOrEmpty(ProvideLocation))
            {
                throw new ArgumentNullException("ProvideLocation");
            }
            if (OCID == null)
            {
                //sm.LastErrorMessage = "無課程報名資料，請依正常程序進行報名!";
                //sm.RedirectUrlAfterBlock = Url.Action("Index", "ClassSearch");
                //LOG.Error("無課程報名資料，請依正常程序進行報名!");
                throw new ArgumentNullException("OCID");
            }

            SignUpViewModel model = new SignUpViewModel();
            WDAIIPWEBService serv = new WDAIIPWEBService();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            ActionResult rtn = null;

            ClassClassInfoExtModel classInfo = dao.GetOCIDDateByOCID(OCID.Value);
            if ("Y".Equals(sm.IsOnlineSignUp)) { model.Form.ProvideLocation = "N"; }

            //加 - 密
            Turbo.Crypto.AesTk aesTk = new Turbo.Crypto.AesTk();
            string dtime_f1 = "yyyy-MM-dd HH:mm:ss";
            // 取得當前的日期和時間// 將日期和時間加 1 小時// 將日期和時間格式化為文字 // 輸出格式化的日期和時間
            int iOCID = 0;//classInfo.OCID.HasValue ? (int)classInfo.OCID.Value : 0;
            string SENTERDATE_f1 = null;
            int iCLSID = 0;
            string s_fdEFTIME = null;
            string s_log1 = null;
            string s_DB3D0C = null;
            if (classInfo != null)
            {
                iOCID = classInfo.OCID.HasValue ? (int)classInfo.OCID.Value : 0;
                SENTERDATE_f1 = classInfo.SENTERDATE.HasValue ? classInfo.SENTERDATE.Value.ToString(dtime_f1) : "";
                iCLSID = classInfo.CLSID.HasValue ? (int)classInfo.CLSID.Value : 0;
                s_fdEFTIME = DateTime.Now.AddHours(1).ToString(dtime_f1);
                s_log1 = string.Concat("OCID=", iOCID, "&SENTERDATE=", SENTERDATE_f1, "&CLSID=", iCLSID, "&EFTIME=", s_fdEFTIME);
                //LOG.Debug(string.Concat("##Index.classInfo::", s_log1));
            }
            s_DB3D0C = string.IsNullOrEmpty(s_log1) ? "" : aesTk.Encrypt(s_log1);

            //訓練計劃類型("1":產投、"2":在職、"5":區域產業據點)
            model.Form.PlanType = PlanType;
            model.Form.ProvideLocation = ProvideLocation;
            model.Form.OCID = iOCID; //OCID;
            model.Form.DB3D0C = s_DB3D0C;
            model.Form.NAME = sm.UserName;
            model.Form.BIRTHDAY = sm.Birthday;
            model.Form.IDNO = sm.ACID;

            rtn = View("Index", model);

            if (classInfo == null || string.IsNullOrEmpty(s_DB3D0C) || !classInfo.OCID.HasValue || !classInfo.SENTERDATE.HasValue || !classInfo.CLSID.HasValue)
            {
                sm.LastErrorMessage = "找不到指定的班級資料!";

                if ("Y".Equals(sm.IsOnlineSignUp))
                {
                    sm.RedirectUrlAfterBlock = Url.Action("Online1", "SignUp");
                }
                else
                {
                    sm.RedirectUrlAfterBlock = Url.Action("Detail", "ClassSearch", new { ocid = OCID, PlanType = PlanType, ProvideLocation = ProvideLocation });
                }
            }
            else if (serv.StopEnterTempMsg())
            {
                sm.RedirectUrlAfterBlock = Url.Action("Detail", "ClassSearch", new { ocid = OCID, PlanType = PlanType, ProvideLocation = ProvideLocation });
            }
            else
            {
                // 取得會員資料
                TblMEMBER memInfo = dao.GetMemberByRIDIDNO(ridNum, sm.ACID);

                if (memInfo != null)
                {
                    TrainingHistoryFormModel form = new TrainingHistoryFormModel
                    {
                        IDNO = sm.ACID,
                        BIRTHDAY_STR = sm.Birthday
                    };
                    IList<TrainingHistoryGridModel> hisDefStdCostList = dao.QueryTrainingHistory(form);

                    //產投課程才需檢核是否填寫參訓學員訓後動態調查表
                    if ("1".Equals(PlanType))
                    {
                        if (hisDefStdCostList != null && hisDefStdCostList.Count > 0)
                        {
                            IList<TrainingHistoryGridModel> list = hisDefStdCostList.Where(x => x.CANWRITE == 1).ToList();
                            if (list != null && list.Count > 0)
                            {
                                var item = list[0];

                                if (DateTime.Compare(item.ATODAY.Value, item.WRDATE1.Value) >= 0
                                    && DateTime.Compare(item.ATODAY.Value, item.WRDATE2.Value) <= 0)
                                {
                                    sm.LastResultMessage = "您尚有結訓課程未填寫參訓學員訓後動態調查表，請儘快填寫完畢。";
                                }
                            }
                        }
                    }

                    //訓練計劃類型("1":產投、"2":在職、"5":區域產業據點)
                    switch (PlanType)
                    {
                        case "1":
                            //壓力測試模式, 直接導向 Detail
                            if (ConfigModel.StressTestMode)
                            {
                                rtn = Detail(model);
                            }
                            else
                            {
                                rtn = View("Index", model);
                            }
                            break;
                        case "2":
                            rtn = Detail(model);
                            break;
                        case "5":
                            rtn = Detail(model);
                            break;
                    }
                }
            }

            return rtn;
        }

        /// <summary> 線上報名-課程代碼查詢 </summary>
        /// <param name="ocid"></param>
        /// <returns></returns>
        //[HttpGet]
        public ActionResult Online1()
        {
            SessionModel sm = SessionModel.Get();
            SignUpViewModel model = new SignUpViewModel();
            WDAIIPWEBService serv = new WDAIIPWEBService();

            sm.IsOnlineSignUp = "Y"; //2019-01-07 add 是否從線上報名功能輸入課程代碼進行報名
            model.Form.PlanType = "2"; //2019-01-07 add 計畫類別選項:預設選在職課程報名
            serv.StopEnterTempMsg();

            return View("Online1", model);
        }

        /// <summary> 個人報名基本資料明細頁（產投/在職） </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Detail(SignUpViewModel model)
        {
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBService serv = new WDAIIPWEBService();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            model.Detail = new SignUpDetailModel();
            DateTime aNow = DateTime.Now;

            string strMsg = string.Empty;

            string s_PlanType = (model != null && model.Form != null) ? model.Form.PlanType : string.Empty;

            // "1":28:產投,"2":06:在職,"5":70:區域產業據點
            string s_tplanID = (s_PlanType == "1" ? "28" : s_PlanType == "2" ? "06" : s_PlanType == "5" ? "70" : string.Empty);

            string s_IDNO = (model != null && model.Form != null) ? model.Form.IDNO : string.Empty;

            //檢核報名查詢參數資訊
            strMsg = CheckPage(model.Form);

            if (!string.IsNullOrEmpty(strMsg))
            {
                if (ConfigModel.StressTestMode)
                {
                    // 壓力測試模式, 若有檢核失敗, 直接丟出 http 500
                    rtn = SignUpFail(strMsg);
                }
                else
                {
                    // 一般模式
                    sm.LastResultMessage = strMsg;
                    sm.RedirectUrlAfterBlock = "";
                    rtn = View("CheckEdit", model);
                }
            }
            else
            {
                // 檢測是否停止報名(僅顯示提示訊息用)
                serv.StopEnterTempMsg();

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
                    s_DB3D0C = string.IsNullOrEmpty(model.Form.DB3D0C) ? null : aesTk.Decrypt(model.Form.DB3D0C);
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
                    LOG.Error($"#Detail: {ex.Message}", ex);
                    s_DB3D0C = "";
                }
                //LOG.Debug(string.Concat("##s_DB3D0C:", s_DB3D0C));
                //LOG.Debug(string.Concat("vOCID:", vOCID));
                //LOG.Debug(string.Concat("vSENTERDATE:", vSENTERDATE));
                //LOG.Debug(string.Concat("vCLSID:", vCLSID));

                //報名資料再確認（查班級資料）
                ClassClassInfoExtModel classInfo = dao.GetOCIDDateByPlan(model.Form.OCID.Value, s_tplanID);

                if (classInfo == null || string.IsNullOrEmpty(vOCID) || string.IsNullOrEmpty(vSENTERDATE) || string.IsNullOrEmpty(vCLSID)
                    || !classInfo.OCID.HasValue || !classInfo.SENTERDATE.HasValue || !classInfo.CLSID.HasValue)
                {
                    aNow = (new MyKeyMapDAO()).GetSysDateNow();
                    model.Detail = new SignUpDetailModel();
                    strMsg = string.Concat("該課程代號有誤，請重新查詢!!<br>", "目前系統時間為(", MyHelperUtil.DateTimeToTwFormatLongString(aNow), ")");

                    if (!string.IsNullOrEmpty(strMsg))
                    {
                        sm.LastResultMessage = strMsg;
                        // 檢核有問題時做導頁
                        sm.RedirectUrlAfterBlock = Url.Action("Detail", "ClassSearch"
                            , new
                            {
                                ocid = model.Form.OCID
                                ,
                                plantype = model.Form.PlanType
                                ,
                                ProvideLocation = model.Form.ProvideLocation
                            });
                    }
                }
                else if (vOCID != classInfo.OCID.Value.ToString() || vSENTERDATE != classInfo.SENTERDATE.Value.ToString(dtime_f1)
                    || vCLSID != classInfo.CLSID.Value.ToString() || i_res1 == -1 || i_res2 != -1)
                {
                    aNow = (new MyKeyMapDAO()).GetSysDateNow();
                    model.Detail = new SignUpDetailModel();
                    strMsg = string.Concat("該課程代號有誤，請重新查詢!!!<br>", "目前系統時間為(", MyHelperUtil.DateTimeToTwFormatLongString(aNow), ")");

                    if (!string.IsNullOrEmpty(strMsg))
                    {
                        sm.LastResultMessage = strMsg;
                        // 檢核有問題時做導頁
                        sm.RedirectUrlAfterBlock = Url.Action("Detail", "ClassSearch"
                            , new
                            {
                                ocid = model.Form.OCID
                                ,
                                plantype = model.Form.PlanType
                                ,
                                ProvideLocation = model.Form.ProvideLocation
                            });
                    }
                }
                else
                {

                    //檢核報名日期
                    strMsg = this.CheckEnterDate(model.Form.OCID.Value, s_tplanID);

                    if (!string.IsNullOrEmpty(strMsg))
                    {
                        //不在可報名的時間內時要提示訊息並導頁
                        model.Detail = new SignUpDetailModel();
                    }
                    else
                    {
                        // 取得當前的日期和時間// 將日期和時間加 1 小時// 將日期和時間格式化為文字 // 輸出格式化的日期和時間
                        //string dtime_f1 = "yyyy-MM-dd HH:mm:ss";
                        int iOCID = classInfo.OCID.HasValue ? (int)classInfo.OCID.Value : 0;
                        string SENTERDATE_f1 = classInfo.SENTERDATE.HasValue ? classInfo.SENTERDATE.Value.ToString(dtime_f1) : "";
                        int iCLSID = classInfo.CLSID.HasValue ? (int)classInfo.CLSID.Value : 0;
                        string s_fdEFTIME = DateTime.Now.AddHours(1).ToString(dtime_f1);
                        string s_log1 = string.Concat("OCID=", iOCID, "&SENTERDATE=", SENTERDATE_f1, "&CLSID=", iCLSID, "&EFTIME=", s_fdEFTIME);
                        //LOG.Debug(string.Concat("##Detail.classInfo::", s_log1));
                        s_DB3D0C = aesTk.Encrypt(s_log1);

                        //顯示班別名稱
                        model.Detail.CLASSCNAME = classInfo.CLASSCNAME + (classInfo.CLASSCNAME.IndexOf("班") >= 0 ? "" : "班");
                        model.Detail.OCID = (Int64)classInfo.OCID;
                        model.Detail.DB3D0C = s_DB3D0C;

                        #region 黑名單判斷
                        IList<Hashtable> listblock = dao.GetBlackList2(s_IDNO, s_tplanID);
                        strMsg = MyCommonUtil.Show_BlockMsg1(listblock);
                        #endregion

                        if (string.IsNullOrEmpty(strMsg))
                        {
                            //載入報名資料
                            serv.LoadEnterData(model);

                            //如為產投課程才能顯示產投報名資料填寫區
                            model.Detail.ISPLAN28 = "28".Equals(classInfo.TPLANID) ? "Y" : "N";
                            model.Detail.STDATE = classInfo.STDATE;
                        }
                    }

                    if (!string.IsNullOrEmpty(strMsg))
                    {
                        sm.LastResultMessage = strMsg;
                        // 檢核有問題時做導頁
                        sm.RedirectUrlAfterBlock = Url.Action("Detail", "ClassSearch"
                            , new
                            {
                                ocid = model.Form.OCID
                                ,
                                plantype = model.Form.PlanType
                                ,
                                ProvideLocation = model.Form.ProvideLocation
                            });
                    }
                }

                model.Detail.IsSignUp = true;
                MyCommonUtil.HtmlDecode(model.Detail); //2019-01-18 fix 中文編碼轉置問題（&#XXXXX;）

                if (ConfigModel.StressTestMode)
                {
                    // 壓力測試模式, 下列必填欄位帶入預設值, 以避免要人工操作介入
                    FillRequireDataUnderStressTest(model);

                    if (!string.IsNullOrEmpty(strMsg))
                    {
                        // 壓力測試模式, 若有檢核失敗, 直接丟出 http 500
                        rtn = SignUpFail(strMsg);
                    }
                    else
                    {
                        // 壓力測試模式, 直接導向 SignUp
                        rtn = SignUp(model);
                    }
                }
                else
                {
                    // 一般模式
                    rtn = View("Detail", model);
                }
            }

            return rtn;
        }

        /// <summary>
        ///  壓力測試模式, 必填欄位帶入預設值, 以避免要人工操作介入
        /// </summary>
        /// <param name="model"></param>
        private void FillRequireDataUnderStressTest(SignUpViewModel model)
        {
            model.Detail.MIDENTITYID = IsNullOrEmpty(model.Detail.MIDENTITYID, "01");
            model.Detail.ZIPCODE2 = string.IsNullOrWhiteSpace(model.Detail.ZIPCODE2) ? "999" : model.Detail.ZIPCODE2;
            //model.Detail.ZIPCODE2_2W = string.IsNullOrWhiteSpace(Convert.ToString(model.Detail.ZIPCODE2_2W)) ? 99 : model.Detail.ZIPCODE2_2W;
            model.Detail.HOUSEHOLDADDRESS = IsNullOrEmpty(model.Detail.HOUSEHOLDADDRESS, "99999");
            model.Detail.PRIORWORKPAY = IsNullOrEmpty(model.Detail.PRIORWORKPAY, "");
            model.Detail.UNAME = IsNullOrEmpty(model.Detail.UNAME, "NA");
            model.Detail.INTAXNO = IsNullOrEmpty(model.Detail.INTAXNO, "99999999");
            model.Detail.ACTNAME = IsNullOrEmpty(model.Detail.ACTNAME, "NA");
            model.Detail.ACTTYPE = IsNullOrEmpty(model.Detail.ACTTYPE, "1");
            model.Detail.SERVDEPTID = IsNullOrEmpty(model.Detail.SERVDEPTID, "99");  // 服務部門
            model.Detail.ACTNO = IsNullOrEmpty(model.Detail.ACTNO, "0123456789");
            model.Detail.Q1 = model.Detail.Q1.HasValue ? model.Detail.Q1 : 0;
            model.Detail.Q2_4 = IsNullOrEmpty(model.Detail.Q2_4, "1");
            model.Detail.Q3 = model.Detail.Q3.HasValue ? model.Detail.Q3 : 0;
            model.Detail.Q4 = IsNullOrEmpty(model.Detail.Q4, "99");
            model.Detail.Q5 = model.Detail.Q5.HasValue ? model.Detail.Q5 : 1;
            model.Detail.Q61 = IsNullOrEmpty(model.Detail.Q61, "1");
            model.Detail.Q62 = IsNullOrEmpty(model.Detail.Q62, "1");
            model.Detail.Q63 = IsNullOrEmpty(model.Detail.Q63, "1");
            model.Detail.Q64 = IsNullOrEmpty(model.Detail.Q64, "1");
            model.Detail.JOBTITLEID = IsNullOrEmpty(model.Detail.JOBTITLEID, "99");  // 職務
            model.Detail.ISEMAIL = IsNullOrEmpty(model.Detail.ISEMAIL, "N");
            model.Detail.ISCHECK = IsNullOrEmpty(model.Detail.ISCHECK, "Y");  // 本人 確認 於開訓當日為具就業保險、勞工保險或農民保險被保險人身分之在職勞工
            model.Detail.ISCHECK2 = IsNullOrEmpty(model.Detail.ISCHECK2, "Y");  // 本人 確認 上述為個人最新及正確資料    
            model.Detail.ISAGREE = "Y";
        }

        private string IsNullOrEmpty(string str, string defaultVal)
        {
            return string.IsNullOrEmpty(str) ? defaultVal : str;
        }

        /// <summary>
        /// 壓力測試模式, 用來丟出 http 500 及 報名失敗錯誤訊息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private ActionResult SignUpFail(string msg)
        {
            Response.StatusCode = 500;
            Response.StatusDescription = "SignUp Fail";
            return Content($"[SignUp Fail]: \n{msg}");
        }

        /// <summary> 送出報名(產投報名) </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SignUp(SignUpViewModel model)
        {
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            ClassSignUpService serv = new ClassSignUpService();
            WDAIIPWEBService webServ = new WDAIIPWEBService();

            const string csErrMsg1 = "資料庫連結中斷，請再試一次，造成您不便之處，還請見諒。";
            string strMsg = string.Empty;

            //檢核是否在停止報名段內
            if (webServ.StopEnterTempMsg())
            {
                if (ConfigModel.StressTestMode)
                {
                    // 壓力測試模式, 直接回應 Http 500 及 訊息
                    return SignUpFail(sm.LastErrorMessage);
                }
                else
                {
                    return View("Detail", model);
                }
            }

            //檢核報名資料內容
            model.Valid(ModelState);

            rtn = View("Detail", model);

            ClassClassInfoExtModel classInfo = null;

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                bool blClassExists = false;  // 要報名的課程是否存在
                bool blOKFlag2 = true; //資料庫連結正常 True/ 異常 False
                string straIDNO = model.Detail.IDNO;

                try
                {
                    // 檢查報名暫存檔是否已存在, 若是則引用流水號
                    TblSTUD_ENTERTEMP enterTemp = dao.GeteSETIDByStudEnterTemp(straIDNO);
                    // 取得stud_entertemp.setid
                    model.Detail.SETID = (enterTemp != null && enterTemp.SETID.HasValue) ? enterTemp.SETID : null;
                    // 載入班級初始資料(class_classinfo)
                    classInfo = dao.GetClassClassInfo(model.Form.OCID.Value);

                    model.ClassInfo = classInfo;
                    // 再次確定班級資料
                    blClassExists = (model.ClassInfo != null) ? true : false;
                }
                catch (Exception ex)
                {
                    LOG.Error($"#SignUp: {ex.Message}", ex);
                    strMsg = ex.Message;//ex.ToString();
                    blOKFlag2 = false;
                }

                //加解-密
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
                    s_DB3D0C = string.IsNullOrEmpty(model.Detail.DB3D0C) ? null : aesTk.Decrypt(model.Detail.DB3D0C);
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
                    LOG.Error($"#SignUp: {ex.Message}", ex);
                    s_DB3D0C = "";
                }

                if (classInfo == null || string.IsNullOrEmpty(vOCID) || string.IsNullOrEmpty(vSENTERDATE) || string.IsNullOrEmpty(vCLSID)
                    || !classInfo.OCID.HasValue || !classInfo.SENTERDATE.HasValue || !classInfo.CLSID.HasValue)
                {
                    //查無班級資料
                    blClassExists = false;
                    strMsg = $"找不到您要報名的課程資料!，課程代碼:{model.Form.OCID.Value}";
                    LOG.Warn($"#SignUp: {strMsg}");
                    sm.LastErrorMessage = strMsg;
                    rtn = View("Detail", model);
                    return rtn;
                }
                else if (vOCID != classInfo.OCID.Value.ToString() || vSENTERDATE != classInfo.SENTERDATE.Value.ToString(dtime_f1)
                    || vCLSID != classInfo.CLSID.Value.ToString() || i_res1 == -1 || i_res2 != -1)
                {
                    //查無班級資料
                    blClassExists = false;
                    strMsg = $"找不到您要報名的課程資料!，課程代碼:{vOCID}";
                    LOG.Warn($"#SignUp: {strMsg}");
                    sm.LastErrorMessage = strMsg;
                    rtn = View("Detail", model);
                    return rtn;
                }
                if (!blOKFlag2)
                {
                    //查詢時發生錯誤
                    strMsg = $"{csErrMsg1}(若持續出現此問題，請聯絡系統管理者)!!!<br><br>{strMsg}";
                    LOG.Warn($"#SignUp: {strMsg}");
                    sm.LastErrorMessage = strMsg;
                    rtn = View("Detail", model);
                    return rtn;
                }
                else if (!blClassExists)
                {
                    //查無班級資料
                    strMsg = $"找不到您要報名的課程資料，課程代碼:{model.Form.OCID.Value}";
                    LOG.Warn($"#SignUp: {strMsg}");
                    sm.LastErrorMessage = strMsg;
                    rtn = View("Detail", model);
                    return rtn;
                }

                if (!string.IsNullOrEmpty(strMsg))
                {
                    if (ConfigModel.StressTestMode)
                    {
                        // 壓力測試模式, 直接回應 Http 500 及 訊息
                        rtn = SignUpFail(strMsg);
                    }
                    else
                    {
                        // 一般模式
                        sm.LastErrorMessage = strMsg;
                        rtn = View("Detail", model);
                    }
                }
                else
                {
                    sm.SignUpOCID = $"{model.Form.OCID.Value}";

                    // 報名處理作業
                    // 1.儲存e網報名暫存資料（stud_entertemp2）
                    // 2.儲存班級報名資料（stud_entertype2, stud_entertrain2）
                    // 3.報名產投課程時會額外產生一組報名結果序號
                    serv.ProcessEnter(model);

                    // 注意:ProcessEnter() 會啟動報名作業排隊程序, 然後立即就返回. 
                    // 不會立即有報名結果, 一律導向 SignUpResult,再透過 AJAX 持續檢查處理結果

                    if (ConfigModel.StressTestMode)
                    {
                        // 壓力測試模式, 直接回應訊息, 表示送出報名已正確接受及處理中
                        rtn = Content("SignUp Send OK");
                    }
                    else
                    {
                        // 一般模式, 回應 SignUpStatus 報名狀態頁
                        rtn = RedirectToAction("SignUpStatus");
                    }

                }
            }
            else
            {
                // 檢核失敗
                if (ConfigModel.StressTestMode)
                {
                    // 壓力測試模式, 直接返回 Http 500 及 訊息
                    var err = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage);
                    strMsg = "";
                    foreach (var e in err)
                    {
                        strMsg += $"{e}，\n";
                    }
                    rtn = SignUpFail(strMsg);
                }

            }
            return rtn;
        }

        /// <summary>檢查記錄在 SessionModel 中的報名處理作業狀態</summary>
        /// <param name="json">若此值為'Y', 設計供AJAX呼叫用, 以 JSON 格式回傳 <see cref="ClassSignUpStatus"/></param>
        /// <returns></returns>
        public ActionResult SignUpStatus(string json)
        {
            SessionModel sm = SessionModel.Get();

            // 測試報名逾時 - TestSignUpTimeout
            ViewBag.TestSignUpTimeout = (ConfigModel.IsTestSignUpTimeout ? "Y" : "");

            if ("Y".Equals(json))
            {
                // 以 JSON 格式回傳 ClassSignUpStatus
                string host = Server.MachineName;

                ClassSignUpStatus signUpStatus = ClassSignUpStatus.GetSignUpInfo(host, sm.SessionID); //檢查記錄在 SessionModel 中的報名處理作業狀態

                //序號有誤，且錯誤訊息為空
                bool fgQueueNG = (signUpStatus.QueueSeq.HasValue && signUpStatus.QueueSeq.Value <= 0 && string.IsNullOrEmpty(signUpStatus.ErrMsg));

                //序號有誤，且錯誤訊息為空
                if (fgQueueNG) { signUpStatus.ErrMsg = "找不到報名作業狀態資訊"; }

                string jsonContent = signUpStatus.Serialize();
                LOG.Info($"SignUpStatus: {jsonContent}");
                return new JsonContentResult(jsonContent);
            }

            // 返回作業等待頁面
            ViewBag.OCID = sm.SignUpOCID;
            return View("SignUpStatus");
        }


        /// <summary>06:在職課程報名作業</summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SignUp06(SignUpViewModel model)
        {
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBService webServ = new WDAIIPWEBService();
            ClassSignUpService serv = new ClassSignUpService();
            string strMsg = string.Empty;

            //檢核是否在停止報名段內
            if (webServ.StopEnterTempMsg())
            {
                return View("Detail", model);
            }

            //檢核報名資料內容
            model.Valid(ModelState);

            rtn = View("Detail", model);

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                serv.ProcessEnter2(model, sm);

                if (string.IsNullOrEmpty(model.SignUpErrMsg))
                {
                    sm.LastResultMessage = string.Format("({0})送出報名資料成功！", Convert.ToString(model.Form.OCID));
                    //sm.RedirectUrlAfterBlock = Url.Action("Detail","ClassSearch",new { plantype = model.Form.PlanType, ocid=model.Form.OCID, providelocation = model.Form.ProvideLocation });
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "EnterClass");
                }
                else
                {
                    sm.LastErrorMessage = model.SignUpErrMsg;
                    sm.RedirectUrlAfterBlock = "";
                }
            }

            return rtn;
        }

        /// <summary>70:區域產業據點-課程報名作業</summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SignUp70(SignUpViewModel model)
        {
            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBService webServ = new WDAIIPWEBService();
            ClassSignUpService serv = new ClassSignUpService();
            string strMsg = string.Empty;

            //檢核是否在停止報名段內
            if (webServ.StopEnterTempMsg())
            {
                return View("Detail", model);
            }

            //檢核報名資料內容
            model.Valid(ModelState);

            rtn = View("Detail", model);

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                serv.ProcessEnter2(model, sm);

                if (string.IsNullOrEmpty(model.SignUpErrMsg))
                {
                    sm.LastResultMessage = string.Format("({0})送出報名資料成功！", Convert.ToString(model.Form.OCID));
                    //sm.RedirectUrlAfterBlock = Url.Action("Detail","ClassSearch",new { plantype = model.Form.PlanType, ocid=model.Form.OCID, providelocation = model.Form.ProvideLocation });
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "EnterClass");
                }
                else
                {
                    sm.LastErrorMessage = model.SignUpErrMsg;
                    sm.RedirectUrlAfterBlock = "";
                }
            }

            return rtn;
        }

        /// <summary> 產投課程報名結果序號頁面 ref online4.aspx </summary>
        /// <param name="ocid">班級代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SignUpResult(decimal? ocid)
        {
            SessionModel sm = SessionModel.Get();
            ClassSignUpService serv = new ClassSignUpService();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            if (!ocid.HasValue)
            {
                // 由 SessionModel 取得產投課程報名班級代碼
                string s_OCID = sm.SignUpOCID;
                long i_OCID = -1;
                if (!Int64.TryParse(s_OCID, out i_OCID)) { throw new ArgumentException("找不到產投課程報名班級代碼 !"); }
                ocid = i_OCID;
            }

            ClassClassInfoExtModel classInfo = null;
            TblSTUD_ENTERTYPE2 type2Info = null;
            IList<Hashtable> enterType2CntList = null;

            string idno = sm.ACID;
            bool blEnter = false;
            decimal? iSignNo = 0;
            string className = string.Empty;
            string Msg = string.Empty;
            string enterRetMsg = "<span style=\"color:red;font-weight:bold;\">線上報名收件失敗</span>";
            string signNoMsg = string.Empty;

            ActionResult rtn = View("SignUpResult");

            //測試線上報名狀況
            blEnter = dao.CheckEnterType2(ocid.Value, idno);
            if (blEnter)
            {
                //報名成功 取得 線上報名序號 SignNo
                type2Info = dao.GetEnterType2SignNo(ocid.Value, idno);
                if (type2Info != null)
                {
                    iSignNo = type2Info.SIGNNO;
                }
            }
            else
            {
                sm.LastErrorMessage = $"找不到您的報名收件資料!\nIDNO={idno}, OCID={ocid}";
                return rtn;
            }

            //查詢顯示報名班級資訊(組合班級資訊ocid orgname classname)
            classInfo = dao.GetClassByOCID(ocid.Value);
            if (classInfo != null)
            {
                className = string.Format("{0} {1}-{2}"
                    , ocid
                    , Convert.ToString(classInfo.ORGNAME)
                    , Convert.ToString(classInfo.CLASSCNAME));
            }

            if (blEnter && iSignNo.HasValue && iSignNo > 0)
            {
                enterRetMsg = "<span style=\"color:#090;font-weight:bold;\">線上報名收件成功</span>";

                //Call TIMS.Del_ENTERTYPE2err(objconn, IDNO.Value, OCID1.Value)
                // 刪除異常資料： stud_entertype2 ( signno is null), stud_entertrain2
                //serv.DeleteEnterErr(ocid.Value, idno, iSignNo.Value);
            }

            //Msg = "<li><span style=\"font-weight:bold\">{0}</span>，{1}，請避開每日 {2} ~ {3} 時段至「報名及參訓課程查詢」>>「近3年內課程報名及參訓情形」，查詢近3年內補助費使用情形(含預估部分)。</li>";
            Msg = "";
            //2019-01-29 fix 問題單400：調整第一項說明文字
            Msg += "<li><span style=\"font-weight:bold\">{0}</span>，{1}，請避開每日 {2} ~ {3} 時段至「會員專區」》「補助額度使用情形」，查詢近3年內補助費用使用情形（含預估部分）。</li>";
            Msg += "<li>線上報名收件成功後，尚未完成報名作業，請主動與訓練單位聯繫後續繳費及繳交相關資料事宜，才算完成報名作業喔!</li>";
            Msg += "<li>產業人才投資方案補助對象需為開訓當日具勞保、就保、農保投保身分的在職勞工，如報名完成或完成繳費後，訓練單位會於開訓當日確認學員是否具補助資格，如符合補助資格，始完成產業人才投資方案的錄訓作業。</li>";
            Msg += "<li>以上訊息如您已清楚了解，請勾選下方欄位查看您的報名序號。</li>";

            ViewBag.msg1 = string.Format(Msg, className, enterRetMsg, ConfigModel.TimeNoUseS, ConfigModel.TimeNoUseE);

            signNoMsg = (iSignNo > 0) ? $"您的序號為：{iSignNo}" : "<span style=\"color:red\">線上報名收件失敗!</span>";
            ViewBag.signNoMsg = signNoMsg;

            string signNoMsg2 = "";
            signNoMsg2 += " <span color=\"red\">如已繳納訓練費用，但因個人因素，於開訓日前辦理退訓者，應依下列規定辦理退費：";
            signNoMsg2 += " <br>(一)訓練單位至多得收取本署核定訓練費用5%，餘者退還學員。";
            signNoMsg2 += " <br>(二)學分班退費標準依教育部規定辦理。";
            signNoMsg2 += " <br>已開訓但未逾訓練總時數1/3者，訓練單位應退還本署核定訓練費用50%，但已逾訓練總時數1/3者，不予退費。";
            signNoMsg2 += " <br>匯款退費者，學員須自行負擔匯款手續費用或退款金額中扣除。</span>";
            ViewBag.signNoMsg2 = signNoMsg2;

            //查詢該學員 報名且未開訓之班級已達3班者
            enterType2CntList = dao.GetEnterType2Cnt(idno);

            if (enterType2CntList != null && enterType2CntList.Count >= 3)
            {
                sm.LastResultMessage = "您已報名還未開訓之班級已達 3 班，為珍惜訓練資源，若您尚未開訓的班級中有確定無法參訓者";
                sm.LastResultMessage += "，煩請至【線上報名查詢】或洽訓練單位取消您的報名，讓更多人有參訓機會，謝謝！";
            }

            return rtn;
        }

        /// <summary> 資料載入時檢核欄位 </summary>
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
            //else,{,if (!MyHelperUtil.IsIDNO(form.IDNO)),{,if (!string.IsNullOrEmpty(strMsg)) strMsg += "<br />";,strMsg += "身分證號碼錯誤(如果有此身分證號碼，請聯絡系統管理者)!";,},}
            return strMsg;
        }

        /// <summary>
        /// 檢核課程報名日期
        /// </summary>
        /// <param name="ocid"></param>
        /// <param name="tplanid"></param>
        /// <returns></returns>
        private string CheckEnterDate(Int64 ocid, string tplanid)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            string msg = string.Empty;

            ClassClassInfoExtModel classInfo = dao.GetOCIDDateByPlan(ocid, tplanid);

            if (classInfo == null)
            {
                msg = "報名班級資料有誤，請重新查詢！";
            }
            else
            {
                //取得系統時間
                DateTime aNow = (new MyKeyMapDAO()).GetSysDateNow();

                int chkTime1 = DateTime.Compare(aNow, Convert.ToDateTime(classInfo.SENTERDATE.Value)); //過報名時間大於0
                int chkTime2 = DateTime.Compare(Convert.ToDateTime(classInfo.FENTERDATE.Value), aNow); //未到結束時間(大於0)

                if (chkTime1 >= 0 && chkTime2 >= 0)
                {
                    //為配合2016年度課程公告作業，擬將2016年上半年度核可並轉班完成之課程統一於2016年1月23日0:01起方才能於產投報名網站上查詢到公告課程。
                    if (DateTime.Compare(aNow, Convert.ToDateTime(cst_SEnterDate2016_28)) <= 0)
                    {
                        if ("2016".Equals(classInfo.YEARS))
                        {
                            chkTime1 = -1;
                            chkTime2 = -1;
                            classInfo.SENTERDATE = Convert.ToDateTime(cst_SEnterDate2016_28);
                        }
                    }
                }

                if (!(chkTime1 > 0 && chkTime2 >= 0))
                {
                    msg = (chkTime1 < 0) ? $"此班級將於({classInfo.SENTERDATE_TW}) 開始報名!!!" : $"{classInfo.FENTERDATE_TW} 此班報名時間已過!!!";
                }
            }
            return msg;
        }

    }
}
