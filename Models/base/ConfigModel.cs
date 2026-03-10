using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Configuration;


namespace WDAIIP.WEB.Models
{
    // 這個檔案集中定義系統組態設定變數(舊系統 DefineVar 有用到的值也納入這裡)
    public static class ConfigModel
    {
        /// <summary>
        /// 系統預設最高管理帳號
        /// </summary>
        public const string Admin = "superadmin";

        /// <summary>
        /// 所屬主功能表：最新消息
        /// </summary>
        public const string MainMenu01 = "01";

        /// <summary>
        /// 所屬主功能表：課程查詢報名
        /// </summary>
        public const string MainMenu03 = "03";

        /// <summary>
        /// 所屬主功能表：課程錄訓名單
        /// </summary>
        public const string MainMenu04 = "04";

        /// <summary>
        /// 所屬主功能表：相關連結
        /// </summary>
        public const string MainMenu05 = "05";

        /// <summary>
        /// 所屬主功能表：資料下載
        /// </summary>
        public const string MainMenu06 = "06";

        /// <summary>
        /// 所屬主功能表：會員專區
        /// </summary>
        public const string MainMenu07 = "07";

        /// <summary>
        /// 所屬主功能表：Q&A
        /// </summary>
        public const string MainMenu08 = "08";

        /// <summary>
        /// 所屬主功能表：數位結訓證明
        /// </summary>
        public const string MainMenu09 = "09";

        /// <summary>
        /// 預設分頁筆數(常數定義), 如果 web.config 中有設定 DefaultPageSize 則為以 web.config 中設定的為主
        /// </summary>
        private const int _DefaultPageSize = 10;

        private static string _StressTestIDFile = null;

        /// <summary>
        /// 壓力測試用的模擬登入身份證號清單檔案名稱 
        /// (有設定時會自動啟用 StressTestMode)
        /// <para>須為文字檔案, 每一行1筆 IDNO,  同時該IDNO須存在於 MEMBER 及 E_MEMBER 表格中</para> 
        /// <para>檔案必須放置於 App_Data 目錄下</para>
        /// </summary>
        public static string StressTestIDFile
        {
            get
            {
                if (_StressTestIDFile == null)
                {
                    _StressTestIDFile = ConfigurationManager.AppSettings["StressTestIDFile"];
                }
                return _StressTestIDFile;
            }
        }

        /// <summary>
        /// 是否啟用壓力測試模式(是否有設定 StressTestIDFile), 
        /// 若啟用則 LoginRequired 會隨機由 StressTestIDFile 清單中載入模擬使用者登入 
        /// </summary>
        public static bool StressTestMode
        {
            get
            {
                bool testModel = !string.IsNullOrWhiteSpace(StressTestIDFile);
                return testModel;
            }
        }

        private static int _defaultProcessEnterWorkerMaxThreads = 50;

        /// <summary>
        /// 最大報名背景處理個數 (Thread Pool 容量)
        /// <para>可由 AppSetting 'ProcessEnterWorkerMaxThreads' 設定, 預設為 50</para>
        /// </summary>
        public static int ProcessEnterWorkerMaxThreads
        {
            get
            {
                int iMaxThreads;

                string maxThreads = ConfigurationManager.AppSettings["ProcessEnterWorkerMaxThreads"];

                int.TryParse(maxThreads, out iMaxThreads);

                return (iMaxThreads > 0) ? iMaxThreads : _defaultProcessEnterWorkerMaxThreads;
            }
        }

        /// <summary>
        /// 報名處理排隊等待最長時間(秒)
        /// </summary>
        private static int _defaultProcessEnterWorkerWaitTimeout = 180;

        /// <summary>
        /// 報名處理排隊等待最長時間(秒), 
        /// 若大於這個值還無法進行報名處理, 則應結束等待返回
        /// <para>可由 AppSetting 'ProcessEnterWorkerWaitTimeout' 設定, 預設為 180 秒</para>
        /// </summary>
        public static int ProcessEnterWorkerWaitTimeout
        {
            get
            {
                int iWaitTimeout;

                string waitTimeout = ConfigurationManager.AppSettings["ProcessEnterWorkerWaitTimeout"];

                int.TryParse(waitTimeout, out iWaitTimeout);

                return (iWaitTimeout > 0) ? iWaitTimeout : _defaultProcessEnterWorkerWaitTimeout;
            }
        }

        #region (目前未使用)

        /// <summary>
        /// 啟用壓力測試模式(AppSettings StressTestMode=Y)時,
        /// 系統會引用模擬的使用者資訊
        /// </summary>
        //public static LoginUserInfo StressTestUserInfo
        //{
        //    get
        //    {
        //        LoginUserInfo user = new LoginUserInfo();
        //        string strUserNo = ConfigurationManager.AppSettings["StressTestUserNo"];
        //        string strUserName = ConfigurationManager.AppSettings["StressTestUserName"];
        //        //string strUserExamKind = ConfigurationManager.AppSettings["StressTestUserExamKind"];
        //        string strUserRole = ConfigurationManager.AppSettings["StressTestUserRole"];

        //        if (string.IsNullOrEmpty(strUserNo))
        //        {
        //            strUserNo = "nobody";
        //        }
        //        if (string.IsNullOrEmpty(strUserName))
        //        {
        //            strUserName = strUserNo;
        //        }

        //        user.UserNo = strUserNo;
        //        user.User = new eYVTRmngUser();
        //        user.User.USR_ID = strUserNo;
        //        user.User.USR_NAME = strUserName;

        //        user.AppUser.UsrID = strUserNo;
        //        user.AppUser.UsrName = strUserName;
        //        user.AppUser.UsrGrpID = Convert.ToInt64(strUserRole);

        //        return user;
        //    }
        //}

        #endregion

        /// <summary>
        /// 預設分頁筆數
        /// </summary>
        public static int DefaultPageSize
        {
            get
            {
                int iPageSize;

                string pageSize = ConfigurationManager.AppSettings["DefaultPageSize"];

                return (int.TryParse(pageSize, out iPageSize)) ? iPageSize : _DefaultPageSize;
            }
        }

        /// <summary> 暫存路徑 </summary>
        public static string TempPath
        {
            get
            {
                string path = ConfigurationManager.AppSettings["TempPath"];

                if (string.IsNullOrEmpty(path))
                {
                    path = (new UrlHelper()).Content("~/App_Data/Temp");
                }

                return path;
            }
        }

        /// <summary> 上傳檔案暫存路徑 </summary>
        public static string UploadTempPath
        {
            get
            {
                string path = ConfigurationManager.AppSettings["UploadPath"];

                if (string.IsNullOrEmpty(path))
                {
                    path = (new UrlHelper()).Content("~/Upload");
                }

                return path;
            }
        }

        /// <summary> 上傳檔案容量上限(Bytes) </summary>
        public static string UploadFileLimit
        {
            get
            {
                string limit = ConfigurationManager.AppSettings["UploadFileLimit"];

                if (string.IsNullOrWhiteSpace(limit))
                {
                    limit = "10485760";
                }

                return limit;
            }
        }

        /// <summary>
        /// 下載檔案暫存路徑
        /// </summary>
        public static string DownloadPath
        {
            get
            {
                string path = ConfigurationManager.AppSettings["DownloadPath"];

                if (string.IsNullOrEmpty(path))
                {
                    path = (new UrlHelper()).Content("/downfile2");
                }

                return path;
            }
        }

        /// <summary>
        /// 會員登入-單一簽入系統代碼
        /// </summary>
        public static string SSOSystemID
        {
            get
            {
                string rtn = ConfigurationManager.AppSettings["SSOSystemID"];

                if (string.IsNullOrEmpty(rtn))
                {
                    rtn = (new UrlHelper()).Content("004");
                }

                return rtn;
            }
        }

        /// <summary>
        /// 會員登入-單一簽入金鑰(私鑰)
        /// </summary>
        public static string RSAPrivateKeyFile
        {
            get
            {
                string rtn = ConfigurationManager.AppSettings["RSAPrivateKeyFile"];

                if (string.IsNullOrEmpty(rtn))
                {
                    rtn = (new UrlHelper()).Content("~/App_Data/0004RSA_privateKey.xml");
                }

                return rtn;
            }
        }

        /// <summary>
        /// 會員登入-單一簽入金鑰(公鑰)
        /// </summary>
        public static string RSAPublicKeyFile
        {
            get
            {
                string rtn = ConfigurationManager.AppSettings["RSAPublicKeyFile"];

                if (string.IsNullOrEmpty(rtn))
                {
                    rtn = (new UrlHelper()).Content("~/App_Data/0004RSA_publicKey.xml");
                }

                return rtn;
            }
        }

        /// <summary>
        /// 就業通登入頁URL
        /// </summary>
        public static string TwJobsLogin_URL
        {
            get
            {
                string rtn = ConfigurationManager.AppSettings["TwJobsLogin_URL"];

                if (string.IsNullOrEmpty(rtn))
                {
                    rtn = (new UrlHelper()).Content("http://127.0.0.1/member/Logout.aspx");
                }

                return rtn;
            }
        }

        /// <summary>
        /// 就業通登出頁URL
        /// </summary>
        public static string TwJobsLogout_URL
        {
            get
            {
                string rtn = ConfigurationManager.AppSettings["TwJobsLogout_URL"];

                if (string.IsNullOrEmpty(rtn))
                {
                    rtn = (new UrlHelper()).Content("http://127.0.0.1/member/Logout.aspx");
                }

                return rtn;
            }
        }

        //MemberTestLogin
        public static string MemberTestLogin
        {
            get
            {
                return ConfigurationManager.AppSettings["MemberTestLogin"];
            }
        }

        /// <summary> 測試登入用flag </summary>
        public static string LoginTest
        {
            get
            {
                string rtn = ConfigurationManager.AppSettings["LoginTest"];
                //if (string.IsNullOrEmpty(rtn)) { rtn = ""; }
                return rtn ?? "";
            }
        }

        /// <summary>測試環境</summary>
        public static bool TurboTestLocal
        {
            get
            {
                string rtn = ConfigurationManager.AppSettings["TurboTestLocal"];
                //if (string.IsNullOrEmpty(rtn)) { rtn = ""; }
                return (!string.IsNullOrEmpty(rtn) && rtn.Equals("Y")) ? true : false;
            }
        }

        /// <summary>
        /// 功能不提供查詢時段-起始時間（HH:mm）
        /// </summary>
        private static string _TimeNoUseS { get; set; }

        /// <summary>
        /// 功能不提供查詢時段-起始時間（HH:mm）TimeNoUseS
        /// </summary>
        public static string TimeNoUseS
        {
            get
            {
                if (string.IsNullOrEmpty(_TimeNoUseS))
                {
                    _TimeNoUseS = ConfigurationManager.AppSettings["TimeNoUseS"];
                }
                return _TimeNoUseS;
            }
        }

        /// <summary>
        /// 功能不提供查詢時段-結束時間（HH:mm）
        /// </summary>
        private static string _TimeNoUseE { get; set; }

        /// <summary>
        /// 功能不提供查詢時段-結束時間（HH:mm）TimeNoUseE
        /// </summary>
        public static string TimeNoUseE
        {
            get
            {
                if (string.IsNullOrEmpty(_TimeNoUseE))
                {
                    _TimeNoUseE = ConfigurationManager.AppSettings["TimeNoUseE"];
                }
                return _TimeNoUseE;
            }
        }

        /// <summary> 報名時檢核重疊時段的機制 </summary>
        private static string _DoubleStopEnter2 { get; set; }

        /// <summary> 報名時檢核重疊時段的機制 (Y 啟用, N 停用) </summary>
        public static string DoubleStopEnter2
        {
            get
            {
                string rtn = ConfigurationManager.AppSettings["DoubleStopEnter2"];

                if (!string.IsNullOrEmpty(rtn)) { return rtn; }

                return "N";
            }
        }

        /// <summary>產投 iReport server url </summary>
        private static string _RptQuery_URL { get; set; }

        /// <summary> 產投 iReport server url </summary>
        public static string RptQuery_URL
        {
            get
            {
                if (!string.IsNullOrEmpty(_RptQuery_URL)) { return _RptQuery_URL; }

                string s_defrpturl1 = "http://vm-tims:8080/ReportServer3/report?";
                //rtn = (new UrlHelper()).Content(s_defrpturl1);
                _RptQuery_URL = ConfigurationManager.AppSettings["RptQuery_URL"] ?? s_defrpturl1;

                return _RptQuery_URL;
            }
        }

        /// <summary> 停機公告-起始時間（yyyy/MM/dd HH:mm:ss）</summary>
        public static string StopTimeS { get { return ConfigurationManager.AppSettings["StopTimeS"] ?? ""; } }

        /// <summary> 停機公告-結束時間（yyyy/MM/dd HH:mm:ss）</summary>
        public static string StopTimeE { get { return ConfigurationManager.AppSettings["StopTimeE"] ?? ""; } }

        /// <summary> 停機公告-導頁網址 </summary>
        public static string StopUrl { get { return ConfigurationManager.AppSettings["StopUrl"] ?? ""; } }

        /// <summary> 課程查詢報名-地圖找課程 </summary>
        public static string As_ClassMapSch1 { get { return ConfigurationManager.AppSettings["ClassMapSch1"] ?? ""; } }

        /// <summary>下載訓練證明 TrainCert</summary>
        public static string As_TrainCert1 { get { return ConfigurationManager.AppSettings["TrainCert1"] ?? ""; } }

        /// <summary>網站環境參數</summary>
        public static string WebEnvironment { get { return ConfigurationManager.AppSettings["WebEnvironment"] ?? ""; } }

        /// <summary>網站測試環境寄信參數</summary>
        public static string WebTestEmail { get { return ConfigurationManager.AppSettings["WebTestEmail"] ?? ""; } }

        /// <summary>數位結訓證明 DigiCert1</summary>
        public static string As_DigiCert1 { get { return ConfigurationManager.AppSettings["DigiCert1"] ?? ""; } }

        /// <summary>「學員線上表單」功能</summary>
        public static string As_StudOnline1 { get { return ConfigurationManager.AppSettings["StudOnline1"] ?? ""; } }

        /// <summary>學員線上表單 - 測試機列印報表-key="YTEST" value="Y1</summary>
        public static string As_YTEST { get { return ConfigurationManager.AppSettings["YTEST"] ?? ""; } }

        /// <summary>加密字串</summary>
        /// <param name="toEncodelong"></param>
        /// <returns></returns>
        public static string Encodelong(long? toEncodelong)
        {
            var toEncode = toEncodelong.HasValue ? toEncodelong.Value.ToString() : "";
            byte[] toEncodeAsBytes = System.Text.Encoding.UTF8.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }
        /// <summary>加密字串</summary>
        /// <param name="toEncodeStr"></param>
        /// <returns></returns>
        public static string EncodeString(string toEncodeStr)
        {
            var toEncode = !string.IsNullOrEmpty(toEncodeStr) ? toEncodeStr : "";
            byte[] toEncodeAsBytes = System.Text.Encoding.UTF8.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }

        /// <summary>解密</summary>
        /// <param name="toDecrypt"></param>
        /// <returns></returns>
        public static string DecodeString(string toDecrypt)
        {
            if (string.IsNullOrEmpty(toDecrypt)) { return null; };
            byte[] encodedDataAsBytes = Convert.FromBase64String(toDecrypt.Replace(" ", "+"));
            return System.Text.Encoding.UTF8.GetString(encodedDataAsBytes);
        }

        /// <summary>下載 / LibreOffice 安定版</summary>
        public static string libreofficestillUrl { get { return "https://zh-tw.libreoffice.org/download/libreoffice-still/"; } }

        /// <summary>產投網址</summary>
        public static string tplanid28_url1 { get { return "https://course.taiwanjobs.gov.tw/download?pid=5b3c203b-8270-464b-b8c9-9ab30e68f121"; } }
        /// <summary>充飛網址</summary>
        public static string tplanid54_url1 { get { return "https://course.taiwanjobs.gov.tw/download?pid=bdc127bd-8367-4374-a323-24b3d82746bc"; } }
        /// <summary>分署自辦網址</summary>
        public static string tplanid06_url1 { get { return "https://course.taiwanjobs.gov.tw/download?pid=ac67a1b8-e1fc-4ef5-a0d7-f84fc3ed2c82"; } }
        /// <summary>關鍵就業力課程網址</summary>
        public static string core_url1 { get { return "https://core.wda.gov.tw/Home/Temp_IntroCourse"; } }
        /// <summary>充電再出發訓練計畫</summary>
        public static string coursefly_url1 { get { return "https://course.taiwanjobs.gov.tw/download?pid=b0dbdf1f-28a1-4a4c-bf75-b9f46ca44ef5"; } }
        /// <summary>其他政府單位課程 Other government unit courses</summary>
        public static string Othgovuntclass_url1 { get { return "https://its.taiwanjobs.gov.tw/Course/OtherGovSearch"; } }


        /// <summary>簽名檔存放路徑 ElSignPath / ojtup</summary>
        public static string UploadElSignPath
        {
            get
            {
                string path = ConfigurationManager.AppSettings["ElSignPath"];
                // (new UrlHelper()).Content("~/upojt");
                if (string.IsNullOrEmpty(path)) { path = "~/upojt"; }

                return path;
            }
        }
        /// <summary>儲存簽名子資料夾位置</summary>
        public static string ElFormSignPath { get { return "ElFormSign"; } }
        /// <summary>存放路徑 upojt/web</summary>
        public static string UploadOJTWEBPath
        {
            get
            {
                string path = ConfigurationManager.AppSettings["UploadOJTWEBPath"];
                // (new UrlHelper()).Content("~/upojt/web");
                if (string.IsNullOrEmpty(path)) { path = "~/upojt/web"; }

                return path;
            }
        }
        /// <summary>上傳存摺</summary>
        public static string ExIMG1xPath { get { return "ExIMG1"; } }

        /// <summary>上傳身分驗證</summary>
        public static string ExIMG2xPath { get { return "ExIMG2"; } }

        /// <summary>114年確定性需求8：網站建置AI影像辯識功能</summary>
        public static bool IsUseExUploadIMG
        {
            get
            {
                string path = ConfigurationManager.AppSettings["UseExUploadIMG"] ?? "";
                return path == "Y";
            }
        }
        /// <summary>測試報名逾時</summary>
        public static bool IsTestSignUpTimeout
        {
            get
            {
                string path = ConfigurationManager.AppSettings["TestSignUpTimeout"] ?? "";
                return path == "Y";
            }
        }
        /// <summary>檢測如果是測試機為true 正式機為false '報表環境參數 TIMS:為正式機 TEST:測試機 ReportQueryPath: TEST/TIMS</summary>
        public static bool CHK_IS_TEST_ENVC
        {
            get
            {
                string s1 = ConfigurationManager.AppSettings["ReportQueryPath"] ?? "";
                return s1 == "TEST";
            }

        }

        /// <summary>3年可用補助額（10萬）</summary>
        public static long IntSubCostMoneyTotalMax1
        {
            get
            {
                const int cst_sumMoneyMax10 = 100000;
                const int cst_sumMoneyMax7 = 70000;
                bool fg_test = ConfigModel.CHK_IS_TEST_ENVC; //(fg_test || fg_use1)
                bool fg_use1 = (DateTime.Now.Year >= 2025);
                return ((fg_test || fg_use1) ? cst_sumMoneyMax10 : cst_sumMoneyMax7);
            }
        }

        /// <summary>3年可用補助額（10萬）文字</summary>
        public static string Txt3YSubCost
        {
            get
            {
                const string cst_txt3Y10W = "3年10萬";
                const string cst_txt3Y7W = "3年7萬";
                bool fg_test = ConfigModel.CHK_IS_TEST_ENVC; //(fg_test || fg_use1)
                bool fg_use1 = (DateTime.Now.Year >= 2025);
                return ((fg_test || fg_use1) ? cst_txt3Y10W : cst_txt3Y7W);
            }
        }

        /// <summary>使用時間倒數提示功能</summary>
        public static bool FG_USE_CLASS_TIMER { get { return CHK_IS_TEST_ENVC; } }

    }
}