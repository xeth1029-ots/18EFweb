using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;


namespace WDAIIP.WEB.Commons
{
    /// <summary>
    /// 系統代碼及表格名稱列舉
    /// </summary>
    public partial class StaticCodeMap
    {
        /// <summary>
        /// 系統表格名稱列舉
        /// </summary>
        public class TableName
        {
            /// <summary>系統帳號檔</summary>
            public static DBRowTableName SYS_USER = DBRowTableName.Instance("SYS_USER");

            /// <summary>
            /// (AUTH_ACCOUNT)
            /// </summary>
            public static DBRowTableName AUTH_ACCOUNT = DBRowTableName.Instance("AUTH_ACCOUNT");

            /// <summary>
            /// 系統帳號檔
            /// </summary>
            public static DBRowTableName AUTH_RELSHIP = DBRowTableName.Instance("AUTH_RELSHIP");

            /// <summary>
            /// (CLASS_CLASSINFO)
            /// </summary>
            public static DBRowTableName CLASS_CLASSINFO = DBRowTableName.Instance("CLASS_CLASSINFO");

            /// <summary>
            /// (CLASS_ENTERSIGNNO)
            /// </summary>
            public static DBRowTableName CLASS_ENTERSIGNNO = DBRowTableName.Instance("CLASS_ENTERSIGNNO");

            /// <summary>
            /// (CLASS_STUDENTSOFCLASS)
            /// </summary>
            public static DBRowTableName CLASS_STUDENTSOFCLASS = DBRowTableName.Instance("CLASS_STUDENTSOFCLASS");

            /// <summary>
            /// (CLASS_TEACHER)
            /// </summary>
            public static DBRowTableName CLASS_TEACHER = DBRowTableName.Instance("CLASS_TEACHER");

            /// <summary>
            /// (E_CLSTRACE)
            /// </summary>
            public static DBRowTableName E_CLSTRACE = DBRowTableName.Instance("E_CLSTRACE");

            /// <summary>
            /// (E_CONTENT)
            /// </summary>
            public static DBRowTableName E_CONTENT = DBRowTableName.Instance("E_CONTENT");

            /// <summary>
            /// (E_MEMBER)
            /// </summary>
            public static DBRowTableName E_MEMBER = DBRowTableName.Instance("E_MEMBER");

            /// <summary>
            /// (HOME_NEWS)
            /// </summary>
            public static DBRowTableName HOME_NEWS = DBRowTableName.Instance("HOME_NEWS");

            /// <summary>
            /// (HOME_NEWS3)
            /// </summary>
            public static DBRowTableName HOME_NEWS3 = DBRowTableName.Instance("HOME_NEWS3");

            /// <summary>
            /// (ID_CITY)
            /// </summary>
            public static DBRowTableName ID_CITY = DBRowTableName.Instance("ID_CITY");

            /// <summary>
            /// (ID_PLAN)
            /// </summary>
            public static DBRowTableName ID_PLAN = DBRowTableName.Instance("ID_PLAN");

            /// <summary>
            /// (ID_ZIP)
            /// </summary>
            public static DBRowTableName ID_ZIP = DBRowTableName.Instance("ID_ZIP");

            /// <summary>
            /// (KEY_CLASSCATELOG)
            /// </summary>
            public static DBRowTableName KEY_CLASSCATELOG = DBRowTableName.Instance("KEY_CLASSCATELOG");

            /// <summary>
            /// (KEY_DEGREE)
            /// </summary>
            public static DBRowTableName KEY_DEGREE = DBRowTableName.Instance("KEY_DEGREE");

            /// <summary>
            /// (KEY_GRADSTATE)
            /// </summary>
            public static DBRowTableName KEY_GRADSTATE = DBRowTableName.Instance("KEY_GRADSTATE");

            /// <summary>
            /// (KEY_HOURRAN)
            /// </summary>
            public static DBRowTableName KEY_HOURRAN = DBRowTableName.Instance("KEY_HOURRAN");

            /// <summary>
            /// (KEY_IDENTITY)
            /// </summary>
            public static DBRowTableName KEY_IDENTITY = DBRowTableName.Instance("KEY_IDENTITY");

            /// <summary>
            /// (KEY_JOBTITLE)
            /// </summary>
            public static DBRowTableName KEY_JOBTITLE = DBRowTableName.Instance("KEY_JOBTITLE");

            /// <summary>
            /// (KEY_SERVDEPT)
            /// </summary>
            public static DBRowTableName KEY_SERVDEPT = DBRowTableName.Instance("KEY_SERVDEPT");

            /// <summary>
            /// (KEY_TRADE)
            /// </summary>
            public static DBRowTableName KEY_TRADE = DBRowTableName.Instance("KEY_TRADE");

            /// <summary>
            /// (KEY_TRAINEXP)
            /// </summary>
            public static DBRowTableName KEY_TRAINEXP = DBRowTableName.Instance("KEY_TRAINEXP");

            /// <summary>
            /// (KEY_TRAINTYPE)
            /// </summary>
            public static DBRowTableName KEY_TRAINTYPE = DBRowTableName.Instance("KEY_TRAINTYPE");

            /// <summary>
            /// (MEMBER)
            /// </summary>
            public static DBRowTableName MEMBER = DBRowTableName.Instance("MEMBER");

            /// <summary>
            /// (ORG_ORGINFO)
            /// </summary>
            public static DBRowTableName ORG_ORGINFO = DBRowTableName.Instance("ORG_ORGINFO");

            /// <summary>
            /// (ORG_ORGPLANINFO)
            /// </summary>
            public static DBRowTableName ORG_ORGPLANINFO = DBRowTableName.Instance("ORG_ORGPLANINFO");

            /// <summary>
            /// (PLAN_PLANINFO)
            /// </summary>
            public static DBRowTableName PLAN_PLANINFO = DBRowTableName.Instance("PLAN_PLANINFO");

            /// <summary>
            /// (PLAN_TRAINDESC)
            /// </summary>
            public static DBRowTableName PLAN_TRAINDESC = DBRowTableName.Instance("PLAN_TRAINDESC");

            /// <summary>
            /// (PLAN_TRAINPLACE)
            /// </summary>
            public static DBRowTableName PLAN_TRAINPLACE = DBRowTableName.Instance("PLAN_TRAINPLACE");

            /// <summary>
            /// (PLAN_VERREPORT)
            /// </summary>
            public static DBRowTableName PLAN_VERREPORT = DBRowTableName.Instance("PLAN_VERREPORT");

            /// <summary>專長能力標籤-PLAN_ABILITY</summary>
            public static DBRowTableName PLAN_ABILITY = DBRowTableName.Instance("PLAN_ABILITY");

            /// <summary>
            /// (STUD_BLACKLIST)
            /// </summary>
            public static DBRowTableName STUD_BLACKLIST = DBRowTableName.Instance("STUD_BLACKLIST");

            /// <summary> (STUD_DELENTERTYPE2) </summary>
            public static DBRowTableName STUD_DELENTERTYPE2 = DBRowTableName.Instance("STUD_DELENTERTYPE2");

            /// <summary>
            /// 報名資料暫存檔(STUD_ENTERTEMP)
            /// </summary>
            public static DBRowTableName STUD_ENTERTEMP = DBRowTableName.Instance("STUD_ENTERTEMP");

            /// <summary> 報名資料暫存檔(備份用) </summary>
            public static DBRowTableName STUD_ENTERTEMPDELDATA = DBRowTableName.Instance("STUD_ENTERTEMPDELDATA");

            /// <summary>
            /// e網報名資料暫存檔(STUD_ENTERTEMP2)
            /// </summary>
            public static DBRowTableName STUD_ENTERTEMP2 = DBRowTableName.Instance("STUD_ENTERTEMP2");

            /// <summary>
            /// e網報名資料暫存檔(備份用)
            /// </summary>
            public static DBRowTableName STUD_ENTERTEMP2DELDATA = DBRowTableName.Instance("STUD_ENTERTEMP2DELDATA");

            /// <summary>
            /// (STUD_ENTERTEMP3)
            /// </summary>
            public static DBRowTableName STUD_ENTERTEMP3 = DBRowTableName.Instance("STUD_ENTERTEMP3");

            /// <summary>
            /// e網報名資料暫存檔(備份用)
            /// </summary>
            public static DBRowTableName STUD_ENTERTEMP3DELDATA = DBRowTableName.Instance("STUD_ENTERTEMP3DELDATA");

            /// <summary>
            /// 線上報名資料檔(STUD_ENTERTRAIN2)
            /// </summary>
            public static DBRowTableName STUD_ENTERTRAIN2 = DBRowTableName.Instance("STUD_ENTERTRAIN2");

            /// <summary>
            /// 線上報名資料備份檔(STUD_ENTERTRAIN2DELDATA) 
            /// </summary>
            public static DBRowTableName STUD_ENTERTRAIN2DELDATA = DBRowTableName.Instance("STUD_ENTERTRAIN2DELDATA");

            /// <summary>(STUD_ENTERTYPE)</summary>
            public static DBRowTableName STUD_ENTERTYPE = DBRowTableName.Instance("STUD_ENTERTYPE");

            /// <summary>(STUD_ENTERTYPEDELDATA)</summary>
            public static DBRowTableName STUD_ENTERTYPEDELDATA = DBRowTableName.Instance("STUD_ENTERTYPEDELDATA");

            /// <summary>(STUD_DELENTERTYPE)</summary>
            public static DBRowTableName STUD_DELENTERTYPE = DBRowTableName.Instance("STUD_DELENTERTYPE");

            /// <summary>(STUD_ENTERTYPE2)</summary>
            public static DBRowTableName STUD_ENTERTYPE2 = DBRowTableName.Instance("STUD_ENTERTYPE2");
            /// <summary>(STUD_ENTERTYPE2DELDATA)</summary>
            public static DBRowTableName STUD_ENTERTYPE2DELDATA = DBRowTableName.Instance("STUD_ENTERTYPE2DELDATA");
            /// <summary>STUD_QUESTIONFIN 參訓學員訓後動態調查表</summary>
            public static DBRowTableName STUD_QUESTIONFIN = DBRowTableName.Instance("STUD_QUESTIONFIN");
            /// <summary>STUD_QUESTIONFAC2 產投學員意見調查紀錄檔(2016) </summary>
            public static DBRowTableName STUD_QUESTIONFAC2 = DBRowTableName.Instance("STUD_QUESTIONFAC2");
            /// <summary>STUD_QUESTRAINING 受訓期間意見調查表</summary>
            public static DBRowTableName STUD_QUESTRAINING = DBRowTableName.Instance("STUD_QUESTRAINING");
            /// <summary>STUD_QUESTIONARY 期末學員滿意度調查表</summary>
            public static DBRowTableName STUD_QUESTIONARY = DBRowTableName.Instance("STUD_QUESTIONARY");
            /// <summary>(STUD_SELRESULT)</summary>
            public static DBRowTableName STUD_SELRESULT = DBRowTableName.Instance("STUD_SELRESULT");

            /// <summary>
            /// (STUD_SERVICEPLACE)
            /// </summary>
            public static DBRowTableName STUD_SERVICEPLACE = DBRowTableName.Instance("STUD_SERVICEPLACE");

            /// <summary>
            /// (STUD_STUDENTINFO)
            /// </summary>
            public static DBRowTableName STUD_STUDENTINFO = DBRowTableName.Instance("STUD_STUDENTINFO");

            /// <summary>
            /// (STUD_SUBDATA)
            /// </summary>
            public static DBRowTableName STUD_SUBDATA = DBRowTableName.Instance("STUD_SUBDATA");

            /// <summary>
            /// (STUD_SUBSIDYCOST)
            /// </summary>
            public static DBRowTableName STUD_SUBSIDYCOST = DBRowTableName.Instance("STUD_SUBSIDYCOST");

            /// <summary>
            /// (STUD_TRAINBG)
            /// </summary>
            public static DBRowTableName STUD_TRAINBG = DBRowTableName.Instance("STUD_TRAINBG");

            /// <summary>
            /// (SYS_VAR)
            /// </summary>
            public static DBRowTableName SYS_VAR = DBRowTableName.Instance("SYS_VAR");

            /// <summary>
            /// TB_CONTENT [綜合訊息內容(?!)]
            /// </summary>
            public static DBRowTableName TB_CONTENT = DBRowTableName.Instance("TB_CONTENT");

            /// <summary>
            /// (TEACH_TEACHERINFO)
            /// </summary>
            public static DBRowTableName TEACH_TEACHERINFO = DBRowTableName.Instance("TEACH_TEACHERINFO");

            /// <summary>
            /// TB_QA Q&A資料表
            /// </summary>
            public static DBRowTableName TB_QA = DBRowTableName.Instance("TB_QA");

            /// <summary>
            /// CODE1 參數代碼表
            /// </summary>
            public static DBRowTableName CODE1 = DBRowTableName.Instance("CODE1");

            /// <summary>
            /// TB_BANNER上稿資料表
            /// </summary>
            public static DBRowTableName TB_BANNER = DBRowTableName.Instance("TB_BANNER");

            /// <summary>
            /// TB_DLFILE 檔案(資料)下載資料表
            /// </summary>
            public static DBRowTableName TB_DLFILE = DBRowTableName.Instance("TB_DLFILE");

            /// <summary>
            /// 最新消息分段內容資料表
            /// </summary>
            public static DBRowTableName TB_CONTENT_SECTION = DBRowTableName.Instance("TB_CONTENT_SECTION");

            /// <summary>
            /// 附件檔
            /// </summary>
            public static DBRowTableName TB_FILE = DBRowTableName.Instance("TB_FILE");

            /// <summary>
            /// TB_KEYWORD_LOG 關鍵字Log資料表
            /// </summary>
            public static DBRowTableName TB_KEYWORD_LOG = DBRowTableName.Instance("TB_KEYWORD_LOG");

            /// <summary>
            /// TB_VIEWRECORD 網站課程瀏覽紀錄資料表
            /// </summary>
            public static DBRowTableName TB_VIEWRECORD = DBRowTableName.Instance("TB_VIEWRECORD");

            /// <summary>
            /// TB_VIEWRECORD2 瀏覽點擊紀錄資料表
            /// </summary>
            public static DBRowTableName TB_VIEWRECORD2 = DBRowTableName.Instance("TB_VIEWRECORD2");

            /// <summary>
            /// SYS_AUTONUM 系統資料表pk序號資料表
            /// </summary>
            public static DBRowTableName SYS_AUTONUM = DBRowTableName.Instance("SYS_AUTONUM");

            /// <summary>
            /// CLASS_PLAN_INFO 計畫課程資料表
            /// </summary>
            public static DBRowTableName CLASS_PLAN_INFO = DBRowTableName.Instance("CLASS_PLAN_INFO");

            /// <summary>SYS_SIGNUP_CTL 產投課程報名-收件處理 Thread Pool 控制檔</summary>
            public static DBRowTableName SYS_SIGNUP_CTL = DBRowTableName.Instance("SYS_SIGNUP_CTL");

            /// <summary>SYS_SIGNUP_STATUS 產投課程報名收件處理狀態暫存檔</summary>
            public static DBRowTableName SYS_SIGNUP_STATUS = DBRowTableName.Instance("SYS_SIGNUP_STATUS");

            /// <summary>SYS_SIGNUP_STATISTICS 產投課程報名收件處理狀態統計檔</summary>
            public static DBRowTableName SYS_SIGNUP_STATISTICS = DBRowTableName.Instance("SYS_SIGNUP_STATISTICS");

            /// <summary>TB_MEMSEARCH 速配課程資料檔</summary>
            public static DBRowTableName TB_MEMSEARCH = DBRowTableName.Instance("TB_MEMSEARCH");
            /// <summary>STDALL 舊版學生資料檔</summary>
            public static DBRowTableName STDALL = DBRowTableName.Instance("STDALL");
            /// <summary>HISTORY_STUDENTINFO93 舊版學生資料檔</summary>
            public static DBRowTableName HISTORY_STUDENTINFO93 = DBRowTableName.Instance("HISTORY_STUDENTINFO93");
            /// <summary>數位結訓證明 線上申請</summary>
            public static DBRowTableName STUD_DIGICERTAPPLY = DBRowTableName.Instance("STUD_DIGICERTAPPLY");
            /// <summary>TblSTUD_DIGICERTAPPLYDL  STUD_DIGICERTAPPLYDL  數位結訓證明 線上申請 下載時間 資料表</summary>
            public static DBRowTableName STUD_DIGICERTAPPLYDL = DBRowTableName.Instance("STUD_DIGICERTAPPLYDL");
            /// <summary>STUD_DIGICERTCLASS  數位結訓證明 線上申請班級關聯 資料表</summary>
            public static DBRowTableName STUD_DIGICERTCLASS = DBRowTableName.Instance("STUD_DIGICERTCLASS");
            /// <summary>EMAIL驗證碼</summary>
            public static DBRowTableName E_EMAILCODE = DBRowTableName.Instance("E_EMAILCODE");
            /// <summary>可線上簽署表單文件</summary>
            public static DBRowTableName KEY_ELFORM = DBRowTableName.Instance("KEY_ELFORM");
            /// <summary>學員線上簽署表單文件</summary>
            public static DBRowTableName STUD_ELFORM = DBRowTableName.Instance("STUD_ELFORM");
            public static DBRowTableName STUD_ENTERTEMP4 = DBRowTableName.Instance("STUD_ENTERTEMP4");
            /// <summary>上傳銀行存摺</summary>
            public static DBRowTableName E_IMG1 = DBRowTableName.Instance("E_IMG1");
            /// <summary>上傳身分證件驗證</summary>
            public static DBRowTableName E_IMG2 = DBRowTableName.Instance("E_IMG2");

        }
    }
}
