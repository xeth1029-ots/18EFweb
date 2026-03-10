using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Commons;
using Turbo.Commons;


namespace WDAIIP.WEB.Models
{
    #region TrainingHistoryViewModel

    public class TrainingHistoryViewModel
    {
        public TrainingHistoryViewModel()
        {
            this.Form = new TrainingHistoryFormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public TrainingHistoryFormModel Form { get; set; }

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<TrainingHistoryGridModel> Grid { get; set; }

        /// <summary>
        /// 自辦在職課程
        /// </summary>
        public IList<TrainingHistoryGridModel> Grid06 { get; set; }

        /// <summary>
        /// 舊的參訓資料查詢結果
        /// </summary>
        public IList<TrainingHistoryGrid2Model> Grid2 { get; set; }

        /// <summary>
        /// 提示文字
        /// </summary>
        public string DefStdCostMsg { get; set; }
    }

    #endregion

    #region TrainingHistoryFormModel

    public class TrainingHistoryFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 現在時段是否進行查詢作業
        /// </summary>
        public bool CANVIEW { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        public string IDNO { get; set; }

        /// <summary>
        /// 出生日期 (文字)
        /// </summary>
        public string BIRTHDAY_STR { get; set; }
    }

    #endregion

    #region TrainingHistoryGridModel

    public class TrainingHistoryGridModel
    {
        #region 20190304前版本
        /// <summary>
        /// 流水號
        /// </summary>
        public Int64 SEQ { get; set; }

        /// <summary>
        /// 學員流水號 ID (yyyymmddhhmmss+兩碼流水號)
        /// </summary>
        public string SID { get; set; }

        /// <summary>
        /// 身份證字號
        /// </summary>
        public string IDNO { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 中文姓名
        /// </summary>
        public string NAME { get; set; }

        /// <summary>
        /// 流水ID (流水號)
        /// </summary>
        public Decimal? SOCID { get; set; }

        /// <summary>
        /// 班別中文名稱
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 期別
        /// </summary>
        public string CYCLTYPE { get; set; }

        /// <summary>
        /// 年度 (西元年後兩碼)
        /// </summary>
        public string YEARS { get; set; }

        /// <summary>
        /// 開班流水號
        /// </summary>
        public Decimal? OCID { get; set; }

        /// <summary>
        /// 班別名稱 (?!)
        /// </summary>
        public string CLASSNAME { get; set; }

        /// <summary>
        /// 計畫年度 (ex:西元'2004')
        /// </summary>
        public string PLANYEAR { get; set; }

        /// <summary>
        /// 計畫年度 (民國年)
        /// </summary>
        public string PLANYEAR_TW { get; set; }

        /// <summary>
        /// 計畫代碼
        /// </summary>
        public Decimal? PLANID { get; set; }

        /// <summary>
        /// 廠商統一編號
        /// </summary>
        public string COMIDNO { get; set; }

        /// <summary>
        /// 序號 (PlanID and ComIDNO相同時加一否則從一開始)
        /// </summary>
        public Decimal? SEQNO { get; set; }

        /// <summary>
        /// 補助金額
        /// </summary>
        public Decimal? DEFSTDCOST { get; set; }

        /// <summary>
        /// 預算別代碼 (01.公務 02.就保 03.就安(審核後預算別))
        /// </summary>
        public string BUDID { get; set; }

        /// <summary>
        /// 預算別名稱
        /// </summary>
        public string BUDNAME { get; set; }

        /// <summary> 學員狀態 (Default 1 ； 1.在訓 2.離訓 3.退訓 4.續訓 5.結訓)</summary>
        public Decimal? STUDSTATUS { get; set; }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 最近的三年補助期間
        /// </summary>
        public string TDATERANGE { get; set; }

        /// <summary>
        /// WRDATE1 (?!)
        /// </summary>
        public DateTime? WRDATE1 { get; set; }

        /// <summary>
        /// WRDATE2 (?!)
        /// </summary>
        public DateTime? WRDATE2 { get; set; }

        /// <summary>
        /// CANWRITE (?!)
        /// </summary>
        public int CANWRITE { get; set; }

        /// <summary>
        /// ATODAY (?!)
        /// </summary>
        public DateTime? ATODAY { get; set; }

        /// <summary>
        /// PLANNAME (?!)
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// TPLANID (?!)
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary>
        /// 開訓日期 (西元年yyyy/MM/dd)
        /// </summary>
        public string STDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.STDATE); }
        }

        /// <summary>
        /// 開訓日期 (民國年)
        /// </summary>
        public string STDATE_TW
        {
            get { return (STDATE.HasValue ? MyHelperUtil.DateTimeToTwString(STDATE.Value) : ""); }
        }

        /// <summary>
        /// 結訓日期 (西元年yyyy/MM/dd)
        /// </summary>
        public string FTDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FTDATE); }
        }

        /// <summary>
        /// 結訓日期 (民國年)
        /// </summary>
        public string FTDATE_TW
        {
            get { return (FTDATE.HasValue ? MyHelperUtil.DateTimeToTwString(FTDATE.Value) : ""); }
        }

        /// <summary>
        /// WRDATE1 (西元年yyyy/MM/dd)
        /// </summary>
        public string WRDATE1_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.WRDATE1); }
        }

        /// <summary>
        /// WRDATE2 (西元年yyyy/MM/dd)
        /// </summary>
        public string WRDATE2_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.WRDATE2); }
        }

        /// <summary>
        /// 系統日 (西元年yyyy/MM/dd)
        /// </summary>
        public string ATODAY_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.ATODAY); }
        }

        /// <summary>
        /// 特定字體顯示 (true 是 / false 否)
        /// </summary>
        public bool RedBoldStar { get; set; }

        /// <summary>
        /// 提示字
        /// </summary>
        public string ToolTips { get; set; }
        #endregion

    }

    #endregion

    #region TrainingHistoryGrid2Model
    public class TrainingHistoryGrid2Model
    {
        /// <summary>
        /// 轄區分署(轄區中心)
        /// </summary>
        public string DISTNAME { get; set; }

        /// <summary>
        /// 年度
        /// </summary>
        public string YEARS { get; set; }

        /// <summary>
        /// 年度(民國年)
        /// </summary>
        public string YEARS_TW
        {
            get
            {
                string rtn = string.Empty;

                if (!string.IsNullOrEmpty(this.YEARS))
                {
                    rtn = (Convert.ToInt32(this.YEARS) - 1911).ToString();
                }

                return rtn;
            }
        }

        /// <summary>
        /// 計畫代碼
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 轄區單位
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 班別
        /// </summary>
        public string CLASSNAME { get; set; }

        /// <summary>
        /// 受訓時數
        /// </summary>
        public string THOURS { get; set; }

        /// <summary>
        /// 開訓日
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 開訓日(民國年yyy/MM/dd)
        /// </summary>
        public string STDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.STDATE); }
        }

        /// <summary>
        /// 結訓日
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 結訓日(民國年yyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.FTDATE); }
        }

        public string TRAINHOURS { get; set; }

        /// <summary>
        /// 受訓期間
        /// </summary>
        public string TROUND
        {
            get
            {
                string rtn = string.Empty;

                if (this.STDATE.HasValue || this.FTDATE.HasValue)
                {
                    rtn = this.STDATE_TW + "<br>|<br>" + this.FTDATE_TW;
                }

                return rtn;
            }
        }

        /// <summary>
        /// 上課時間
        /// </summary>
        public string WEEKS { get; set; }

        /// <summary>
        /// 訓練狀態(1在訓 2離訓 3退訓 4續訓 5結訓 9不符參訓資格)
        /// </summary>
        public int STUDSTATUS { get; set; }

        /*get{string rtn = string.Empty;switch (this.STUDSTATUS){case "1":rtn = "在訓";break;case "2":rtn = "離訓";break;case "3":rtn = "退訓";break;case "4":rtn = "續訓";break;case "5":rtn = "結訓";break;case "9":rtn = "不符參訓資格";break;}return rtn;}*/

        /// <summary>
        /// 訓練狀態中文描述
        /// </summary>
        public string STUDSTATUS_TEXT { get; set; }

        /// <summary>
        /// 離退訓原因代碼
        /// </summary>
        public string RTREASONID { get; set; }

        /// <summary>
        /// 離退訓原因中文描述
        /// </summary>
        public string RTREASONID_TEXT { get; set; }

        /// <summary>
        /// 離退訓原因(其他)
        /// </summary>
        public string RTREASOOTHER { get; set; }
    }
    #endregion

    #region HistoryStudentInfo93GridModel 
    /// <summary>
    /// 93年學員參訓資料（from history_studentinfo93）
    /// </summary>
    public class HistoryStudentInfo93GridModel : TblHISTORY_STUDENTINFO93
    {
        public string TMID_TEXT { get; set; }
    }
    #endregion

    #region  HistoryClassStudsOfClassGridModel
    /// <summary>
    /// 學員參訓資料(from class_studentsofclass)
    /// </summary>
    public class HistoryClassStudsOfClassGridModel
    {
        /// <summary>
        /// 身分證號
        /// </summary>
        public string IDNO { get; set; }

        /// <summary>
        /// 轄區分署名稱
        /// </summary>
        public string DISTNAME { get; set; }

        /// <summary>
        /// 訓練機構名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 班別名稱
        /// </summary>
        public string CLASSNAME { get; set; }

        /// <summary>
        /// 受訓時數
        /// </summary>
        public Int32? THOURS { get; set; }

        /// <summary>
        /// 開訓日
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 訓練時數
        /// </summary>
        public Int64? TRAINHOURS { get; set; }

        /// <summary>
        /// 離訓日期
        /// </summary>
        public DateTime? REJECTTDATE1 { get; set; }

        /// <summary>
        /// 退訓日期
        /// </summary>
        public DateTime? REJECTTDATE2 { get; set; }

        /// <summary>
        /// 訓練狀態
        /// </summary>
        public int STUDSTATUS { get; set; }

        /// <summary>
        /// 訓練計畫
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 年度
        /// </summary>
        public string YEARS { get; set; }

        /// <summary>
        /// 上課時間
        /// </summary>
        public string WEEKS { get; set; }

        /// <summary>
        /// 離退訓原因代碼
        /// </summary>
        public string RTREASONID { get; set; }

        /// <summary>
        /// 離退訓原因中文描述
        /// </summary>
        public string RTREASONID_TEXT { get; set; }

        /// <summary>
        /// 離退訓原因(其他)
        /// </summary>
        public string RTREASOOTHER { get; set; }
    }
    #endregion

    #region WsGetTrainingGridModel
    /// <summary>
    /// 職前webservice 回傳欄位資訊
    /// </summary>
    public class WsGetTrainingGridModel
    {
        /// <summary>
        /// 身分證字號
        /// </summary>
        public string IDNO { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string NAME { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 轄區分署
        /// </summary>
        public string DISTNAME { get; set; }

        /// <summary>
        /// 公司統編
        /// </summary>
        public string COMIDNO { get; set; }
        /// <summary>
        /// 轄區分署代碼
        /// </summary>
        public string DISTID { get; set; }

        /// <summary>
        /// 年度
        /// </summary>
        public Int32? YEARS { get; set; }

        /// <summary>
        /// 計畫代碼
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary>
        /// 訓練計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 訓練機構
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 訓練職類名稱
        /// </summary>
        public string TRAINNAME { get; set; }

        /// <summary>
        /// 通俗職類名稱
        /// </summary>
        public string CJOB_NAME { get; set; }

        /// <summary>
        /// 班級名稱
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 受訓時數
        /// </summary>
        public Int32? THOURS { get; set; }

        /// <summary>
        /// 受訓期間
        /// </summary>
        public string TROUND { get; set; }

        /// <summary>
        /// 上課時間
        /// </summary>
        public string WEEKS { get; set; }

        /// <summary>
        /// 訓練狀態
        /// </summary>
        public string TFLAG { get; set; }

        /// <summary>
        /// 是否為在職者補助身份 (Y是 N否 NULL未選擇)
        /// </summary>
        public string WORKSUPPIDENT { get; set; }

        /// <summary>
        /// 補助金額
        /// </summary>
        public decimal? SUMOFMONEY { get; set; }

        /// <summary>
        /// 支付金額
        /// </summary>
        public Int64? PAYMONEY { get; set; }

        /// <summary>
        /// 審核備註
        /// </summary>
        public string APPLIEDNOTE { get; set; }

        /// <summary>
        /// 補助比例代碼(1:80%  2:100%  9:0%)
        /// </summary>
        public string SUPPLYID { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string MEMO1 { get; set; }

        /// <summary>
        /// 預算別代碼(01公務 02就保 03就安)
        /// </summary>
        public string BUDID { get; set; }

        /// <summary>
        /// 預算別中文描述
        /// </summary>
        public string BUDIDNAME { get; set; }

        /// <summary>
        /// 撥款日期
        /// </summary>
        public string ALLOTDATE { get; set; }

        /// <summary>
        /// 學員經費審核狀態-申請(Y:成功 N:失敗 R:退件修正 null:未審核 )
        /// </summary>
        public string APPLIEDSTATUSM { get; set; }

        /// <summary>
        /// 學員經費審核狀態中文描述
        /// </summary>
        public string APPLIEDSTATUSMNAME { get; set; }

        /// <summary>
        /// 學員經費撥款狀態代碼(0失敗 1通過 null)
        /// </summary>
        public string APPLIEDSTATUS { get; set; }

        /// <summary>
        /// 學員經費撥款狀態中文描述
        /// </summary>
        public string APPLIEDSTATUSNAME { get; set; }

        /// <summary>
        /// 學員狀態(1在訓 2離訓 3退訓 4續訓 5結訓)
        /// </summary>
        public int STUDSTATUS { get; set; }

        /// <summary>
        /// 學員狀態中文描述
        /// </summary>
        public string STUDSTATUSNAME { get; set; }

        /// <summary>
        /// 開訓日
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日
        /// </summary>
        public DateTime? FTDATE { get; set; }
    }
    #endregion
}