using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Services;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using WDAIIP.WEB.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WDAIIP.WEB.Models
{
    public class SessionModel
    {
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private HttpSessionStateBase _session;

        private HttpSessionStateBase session
        {
            get
            {
                if (_session == null)
                {
                    throw new NullReferenceException("session object is null");
                }
                return _session;
            }
        }

        private SessionModel(HttpContext httpContext)
        {
            this._session = new HttpSessionStateWrapper(httpContext.Session);
            if (this._session == null)
            {
                throw new NullReferenceException("HttpContext.Current.Session");
            }

            _session.Timeout = 30; //(int)ApplicationModel.GetClamSYS().SESSIONOUT.Value;
            logger.Debug("SessionModel(), SessionID=" + _session.SessionID);

        }

        private SessionModel(): this(HttpContext.Current)
        {
            
        }

        /// <summary>
        /// 取得/建立 SessionModel 
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static SessionModel Get()
        {
            return new SessionModel();
        }

        /// <summary>
        /// 傳入 HttpContext 取得/建立 SessionModel
        /// <para>應用於獨立的 Thread 環境中</para>
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static SessionModel Get(HttpContext httpContext)
        {
            return new SessionModel(httpContext);
        }

        private static readonly string VALIDATE_CODE = "SYS.LOGIN.VALIDATECODE";
        private static readonly string VALIDATE_DGCODE = "SYS.DIGICERT.VALIDATEDGCODE";
        private static readonly string LOGINWAY_CODE = "SYS.LOGIN.LOGINWAYCODE";
        private static readonly string LOGINUUIDCODE_CODE = "SYS.LOGIN.LOGINUUIDCODECODE";
        //private static readonly string USER_INFO = "SYS.LOGIN.USER";
        //private static readonly string CUR_ROLE = "SYS.LOGIN.ROLE";
        //private static readonly string CUR_ROLE_FUNCTION = "SYS.LOGIN.ROLE.FUNCTION";
        //private static readonly string LAST_ACTION_FUNC = "SYS.MENU.LAST_ACTION_FUNC";
        private static readonly string LAST_ACTION_PATH = "SYS.MENU.LAST_ACTION_PATH";
        //private static readonly string LAST_ACTION_NAME = "SYS.MENU.LAST_ACTION_NAME";
        private static readonly string BREADCRUMB_PATH = "SYS.MENU.BREADCRUMB_PATH";

        private static readonly string LAST_ERROR_MESSAGE = "USER.LAST_ERROR_MESSAGE";
        private static readonly string LAST_RESULT_MESSAGE = "USER.LAST_RESULT_MESSAGE";
        private static readonly string REDIRECT_BY_HTTPMETHOD = "USER.REDIRECT_BY_HTTPMETHOD";
        private static readonly string REDIRECT_AFTER_BLOCK = "USER.REDIRECT_AFTER_BLOCK";

        private static readonly string ACTIVE_FUNCTION = "SYS.ACTIVE.FUNCTION";
        //private static readonly string ACTIVE_COURSETAB = "SYS.ACTIVE.COURSETAB";
        private static readonly string IS_HINTED = "SYS.LOGIN.ISHINTED";
        //private static readonly string COURSETRACE_CNT = "SYS.ACCOUNT.COURSETRACECNT";
        private static readonly string MEM_SN = "SYS.ACCOUNT.MEMSN";
        private static readonly string MEM_USER_ID = "SYS.ACCOUNT.MEM_USER_ID";        
        private static readonly string USER_ID = "SYS.ACCOUNT.USERID";
        private static readonly string USER_NAME = "SYS.ACCOUNT.USERNAME";
        private static readonly string IDNO = "SYS.ACCOUNT.IDNO";
        private static readonly string BIRTHDAY = "SYS.ACCOUNT.BIRTHDAY";
        private static readonly string MEM_EDU = "SYS.ACCOUNT.MEMEDU";
        private static readonly string MEM_SEX = "SYS.ACCOUNT.MEMSEX";
        private static readonly string USER_RID = "SYS.ACCOUNT.RID";
        private static readonly string USER_SID = "SYS.ACCOUNT.SID";
        private static readonly string USER_DCANO = "SYS.ACCOUNT.DCANO";
        private static readonly string USER_DCASENO = "SYS.ACCOUNT.DCASENO";
        

        /// <summary>
        /// 取得 SessionID
        /// </summary>
        public string SessionID
        {
            get { return this._session.SessionID; }
        }

        /// <summary>
        /// 判斷是否已登入
        /// </summary>
        public bool IsLogin
        {
            get
            {
                return !string.IsNullOrEmpty(this.RID) 
                    && !string.IsNullOrEmpty(this.SID) 
                    && !string.IsNullOrEmpty(this.ACID) 
                    && !string.IsNullOrEmpty(this.Birthday);
            }
        }

        public override string ToString()
        {
            if(this.IsLogin)
            {
                return string.Format("{0}:{1}:{2}:{3}:{4}",
                this.ACID, this.UserName, this.Birthday, this.SID, this.RID);
            }
            else
            {
                return "NOT_LOGIN";
            }
        }

        /// <summary>
        /// 目前所在主功能項（功能表focus停駐點）
        /// </summary>
        public string ACTIVEFUNCTION
        {
            get { return (string)this.session[ACTIVE_FUNCTION]; }
            set { this.session[ACTIVE_FUNCTION] = value; }
        }

        #region 使用者登入資訊
        /// <summary> 單一簽入 登入流水號 </summary>
        public string RID
        {
            get { return (string)this.session[USER_RID]; }
            set { this.session[USER_RID] = value; }
        }

        /// <summary> 單一簽入 專案序號 </summary>
        public string SID
        {
            get { return (string)this.session[USER_SID]; }
            set { this.session[USER_SID] = value; }
        }
        /// <summary> 線上申請 專案序號 </summary>
        public string DCANO
        {
            get { return (string)this.session[USER_DCANO]; }
            set { this.session[USER_DCANO] = value; }
        }
        public string DCASENO {
            get { return (string)this.session[USER_DCASENO]; }
            set { this.session[USER_DCASENO] = value; }
        }

        //public string MEMBER_USER_ID { get; set; }
        public string ISFOREIGN { get; set; }
        /// <summary> IDNO </summary>
        public string ACID
        {
            get { return (string)this.session[IDNO]; }
            set { this.session[IDNO] = value; }
        }
        public string NAME { get; set; }
        public string SEX { get; set; }
        public string EMAIL { get; set; }
        public string ZIPCODE { get; set; }
        public string ADDR_CITY_1 { get; set; }
        public string ADDR_CITY_2 { get; set; }
        public string ADDR { get; set; }
        public string TEL1 { get; set; }
        public string TEL2 { get; set; }
        public string MOBILE { get; set; }
        public string FAX { get; set; }
        public DateTime CREATEDATE { get; set; }
        public DateTime MODIFYDATE { get; set; }

        /// <summary>
        /// 會員代碼(e_member.mem_sn)
        /// </summary>
        public Decimal? MemSN
        {
            get { return (Decimal?)this.session[MEM_SN]; }
            set { this.session[MEM_SN] = value; }
        }

        public string MEMBER_USER_ID
        {
            get { return (string)this.session[MEM_USER_ID]; }
            set { this.session[MEM_USER_ID] = value; }
        }

        /// <summary>
        /// 登入者使用者帳號 ACCOUNT 
        /// 身分證號
        /// </summary>
        public string UserID
        {
            get { return (string)this.session[USER_ID]; }
            set { this.session[USER_ID] = value; }
        }

        /// <summary>
        /// 登入使用者姓名 
        /// </summary>
        public string UserName
        {
            get { return (string)this.session[USER_NAME]; }
            set { this.session[USER_NAME] = value; }
        }

        /// <summary>
        /// 出生日期 YYYY/MM/DD
        /// </summary>
        public string Birthday
        {
            get { return (string)this.session[BIRTHDAY]; }
            set { this.session[BIRTHDAY] = value; }
        }

        /// <summary>
        /// 性別 1男 2女
        /// </summary>
        public string Sex
        {
            get { return (string)this.session[MEM_SEX]; }
            set { this.session[MEM_SEX] = value; }
        }

        /// <summary>
        /// 學歷
        /// </summary>
        public string MemEDU
        {
            get { return (string)this.session[MEM_EDU]; }
            set { this.session[MEM_EDU] = value; }
        }

        /// <summary>
        /// 登入時檢查是否已出現提示訊息
        /// </summary>
        public bool IsHinted
        {
            get
            {
                bool rtn = false;
                if (this.session[IS_HINTED] != null)
                {
                    rtn = (bool)this.session[IS_HINTED];
                }
                return rtn;
            }

            set { this.session[IS_HINTED] = value; }
        }

        #endregion

        /// <summary>使用者登入驗證碼</summary>
        public string LoginValidateCode
        {
            get { return (string)this.session[VALIDATE_CODE]; }
            set { this.session[VALIDATE_CODE] = value; }
        }

        /// <summary>使用者線上申請驗證碼</summary>
        public string DigiValidateCode
        {
            get { return (string)this.session[VALIDATE_DGCODE]; }
            set { this.session[VALIDATE_DGCODE] = value; }
        }

        public string LoginWay
        {
            get { return (string)this.session[LOGINWAY_CODE]; }
            set { this.session[LOGINWAY_CODE] = value; }
        }

        public string LoginUuidCode
        {
            get { return (string)this.session[LOGINUUIDCODE_CODE]; }
            set { this.session[LOGINUUIDCODE_CODE] = value; }
        }

        // 登入者使用者帳號資訊
        //public LoginUserInfo UserInfo
        //{
        //    get
        //    {
        //        LoginUserInfo userInfo = null;
        //        string jsonUserInfo = (string)this.session[USER_INFO];
        //        if(!string.IsNullOrWhiteSpace(jsonUserInfo) )
        //        {
        //            userInfo = JsonConvert.DeserializeObject<LoginUserInfo>(jsonUserInfo);
        //        }
        //        return userInfo;
        //    }
        //    set
        //    {
        //        this.session[USER_INFO] = JsonConvert.SerializeObject(value);
        //    }
        //}


        /// <summary>
        /// 作用中角色對應的權限功能清單
        /// </summary>
        //public IList<eYVTRmngRoleFunc> RoleFuncs
        // {
        //     get
        //     {
        //         IList<eYVTRmngRoleFunc> roleFuncs = new List<eYVTRmngRoleFunc>();
        //         string jsonRoleFunc = (string)this.session[CUR_ROLE_FUNCTION];
        //         if (!string.IsNullOrWhiteSpace(jsonRoleFunc))
        //         {
        //             roleFuncs = JsonConvert.DeserializeObject<IList<eYVTRmngRoleFunc>>(jsonRoleFunc);
        //         }
        //         return roleFuncs;
        //     }
        //     set
        //     {
        //         this.session[CUR_ROLE_FUNCTION] = JsonConvert.SerializeObject(value);
        //     }
        // }


        /// <summary>
        /// 使用者當前執行的 功能項目,
        /// 當使用者有登入時且執行系統中有定義的功能時, 才會有值, 否則為 null
        /// TblCLAMFUNCM --> TblE_FUN
        /// </summary>
        //public TblE_FUN LastActionFunc
        //{
        //    get
        //    {
        //        TblE_FUN func = null;
        //        string jsonFunc = (string)this.session[LAST_ACTION_FUNC];
        //        if (!string.IsNullOrWhiteSpace(jsonFunc))
        //        {
        //            func = JsonConvert.DeserializeObject<TblE_FUN>(jsonFunc);
        //        }
        //        return func;
        //    }
        //    set
        //    {
        //        this.session[LAST_ACTION_FUNC] = JsonConvert.SerializeObject(value);
        //    }
        //}

        /// <summary>
        /// 使用者當前執行的 程式完整 ACTION PATH
        /// </summary>
        public string LastActionPath
        {
            get { return (string)this.session[LAST_ACTION_PATH]; }
            set { this.session[LAST_ACTION_PATH] = value; }
        }

        /// <summary>
        /// 使用者當前執行的 程式 CONTROLLER PATH
        /// </summary>
        public string LastActionController
        {
            get {
                string act = LastActionPath;
                if (string.IsNullOrEmpty(act))
                {
                    return string.Empty;
                }
                else
                {
                    string[] tokes = act.Split('/');
                    if (tokes.Length > 2)
                    {
                        return tokes[0] + "/" + tokes[1];       // act:  area/controller/action
                    }
                    else
                    {
                        return tokes[0];    // act:  controller/action
                    }
                }
            }
        }

        /// <summary>
        /// 最後被記錄的應用功能錯誤提示訊息, 設定這個值, 在下一個頁面中會觸發 blockAlert() 顯示這個訊息,
        /// 每次這個訊息被讀取後會自動清除, 確保這個訊息只會在一個頁面中被觸發.
        /// </summary>
        public string LastErrorMessage
        {
            get 
            { 
                string message = (string)this.session[LAST_ERROR_MESSAGE];
                this.session[LAST_ERROR_MESSAGE] = string.Empty;
                return (string.IsNullOrEmpty(message) ? string.Empty : message.Replace("\n", "<br/>"));
            }
            set { this.session[LAST_ERROR_MESSAGE] = value; }
        }

        /// <summary>
        /// 最後被記錄的應用功能操作結果提示訊息, 設定這個值, 在下一個頁面中會觸發 blockResult() 顯示這個訊息,
        /// 每次這個訊息被讀取後會自動清除, 確保這個訊息只會在一個頁面中被觸發.
        /// </summary>
        public string LastResultMessage
        {
            get
            {
                string message = (string)this.session[LAST_RESULT_MESSAGE];
                this.session[LAST_RESULT_MESSAGE] = string.Empty;
                return (string.IsNullOrEmpty(message) ? string.Empty : message.Replace("\n","<br/>") );
            }
            set { this.session[LAST_RESULT_MESSAGE] = value; }
        }

        //20190104 增加 RedirectByHttpMethod 屬性

        /// <summary>
        /// （配合 RedirectUrlAfterBlock 屬性值使用）指示在瀏覽器端重新導向至指定網址時，
        /// 是使用 HTTP GET 還是 HTTP POST 方式來執行網址重新導向。(POST: 使用 HTTP POST 方式，GET: 使用 HTTP GET 方式)
        /// 預設值 "POST"。
        /// </summary>
        public string RedirectByHttpMethod
        {
            get
            {
                string v = Convert.ToString(this.session[REDIRECT_BY_HTTPMETHOD]);
                return string.IsNullOrEmpty(v) ? "POST" : v;
            }
            set
            {
                this.session[REDIRECT_BY_HTTPMETHOD] = string.IsNullOrEmpty(value) ? "POST" : value.ToUpper();
            }
        }

        /// <summary>
        /// 配合 LastResultMessage 運作, 若這個屬性不為空, 則在前端 blockResult() 訊息確認後, 
        /// 會以 POST 方式重導至這個 URL, POST 參數可以用 ?parm1=value1&amp;parm2=value2 的方式傳入
        /// </summary>
        public string RedirectUrlAfterBlock
        {
            get { 
                string url = (string)this.session[REDIRECT_AFTER_BLOCK];
                this.session[REDIRECT_AFTER_BLOCK] = string.Empty;
                return url;
            }
            set { this.session[REDIRECT_AFTER_BLOCK] = value; }
        }

        /// <summary>
        /// 在頁面上顯示的「程式功能模組路徑及功能名稱」字串（即麵包屑導覽路徑）
        /// </summary>
        public string Breadcrumb {
            get {
                string v = (string) this.session[BREADCRUMB_PATH];
                if (v != string.Empty) this.session[BREADCRUMB_PATH] = string.Empty;
                return v;
            }
            set {
                this.session[BREADCRUMB_PATH] = value;
            }
        }

        
        private static readonly string INDEX_CONTROLLER = "SESSION.INDEX_CONTROLLER";

        /// <summary>
        /// 導頁路徑
        /// </summary>
        public string IndexController
        {
            get { return (string)this.session[INDEX_CONTROLLER]; }
            set { this.session[INDEX_CONTROLLER] = value; }
        }


        private static readonly string NEWS_INDEX_CONTROLLER = "SESSION.NEWS_INDEX_CONTROLLER";

        /// <summary>
        /// 最新消息導頁路徑
        /// </summary>
        public string NewsIndexController
        {
            get { return (string)this.session[NEWS_INDEX_CONTROLLER]; }
            set { this.session[NEWS_INDEX_CONTROLLER] = value; }
        }


        private static readonly string SIGNUP_OCID = "SESSION.SIGNUP_OCID";
        /// <summary>
        /// 當前報名處理中的產投課程代碼
        /// </summary>
        public string SignUpOCID
        {
            get { return (string)this.session[SIGNUP_OCID]; }
            set { this.session[SIGNUP_OCID] = value; }
        }


        private static readonly string ONLINE_SIGNUP = "SESSION.ONLINE_SIGNUP";
        /// <summary>
        /// 是否由線上報名功能進行報名 (Y是 , N否)
        /// </summary>
        public string IsOnlineSignUp
        {
            get { return (string)this.session[ONLINE_SIGNUP]; }
            set { this.session[ONLINE_SIGNUP] = value; }
        }

        #region 單一簽入導頁用
        //導頁資訊
        private static readonly string REDIRECT_INFO = "SESSION.REDIRECTINFO"; 
        public TWJobsMemberDataModel RedirectInfo
        {
            get
            {
                TWJobsMemberDataModel redirectInfo = null;
                string jsonRedirectInfo = (string)this.session[REDIRECT_INFO];
                if (!string.IsNullOrWhiteSpace(jsonRedirectInfo))
                {
                    redirectInfo = JsonConvert.DeserializeObject<TWJobsMemberDataModel>(jsonRedirectInfo);
                }
                return redirectInfo;
            }
            set
            {
                this.session[REDIRECT_INFO] = JsonConvert.SerializeObject(value);
            }
        }

        private static readonly string PAGE_URL = "SESSION.PAGEURL"; //登入導頁用-URL
        
        /// <summary>
        /// 登入導頁路徑
        /// </summary>
        public string PageURL
        {
            get { return (string)this.session[PAGE_URL]; }
            set { this.session[PAGE_URL] = value; }
        }


        private static readonly string PRESIGNUP_OCID = "SESSION.PRESIGNUP_OCID"; //登入導頁用-課程代碼
        /// <summary>
        /// 當前報名檢視中的課程代碼
        /// </summary>
        public string PreSignUpOCID
        {
            get { return (string)this.session[PRESIGNUP_OCID]; }
            set { this.session[PRESIGNUP_OCID] = value; }
        }

        #endregion
    }

}