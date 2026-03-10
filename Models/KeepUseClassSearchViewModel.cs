using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models
{
    #region KeepUseClassSearchViewModel
    public class KeepUseClassSearchViewModel
    {
        /// <summary>建構子</summary>
        public KeepUseClassSearchViewModel()
        {
            this.Form = new KeepUseClassSearchFormModel();
        }

        /// <summary> 查詢條件 </summary>
        public KeepUseClassSearchFormModel Form { get; set; }

        /// <summary> 查詢結果清單(for 產投) </summary>
        public IList<KeepUseClassSearchGrid1Model> Grid1 { get; set; }

    }
    #endregion

    #region KeepUseClassSearchFormModel
    public class KeepUseClassSearchFormModel : PagingResultsViewModel
    {

        /// <summary>是否為第一次進入此功能 (Y 是, N 否) </summary>
        public string IsFirst { get; set; }

        /// <summary> 查詢類別 (1:適用留用外國中階技術工作人力 ) </summary>
        public string KeepUseType { get; set; }

        //提供課程查詢功能的計畫類別（1 產投 , 2 在職 , 5 區域據點）
        /// <summary> 查詢計畫類別("1":產業人才投資方案、"2":在職進修訓練、"5":區域產業據點) </summary>
        public string PlanType { get; set; }

        /// <summary> 查詢計畫(產投：28)</summary>
        public string TPLANID { get; set; }

        /// <summary> 轄區代碼 (ref:id_district.distid) </summary>
        public string DISTID { get; set; }

        /// <summary> 檢核狀況異常：false 正常：true </summary>
        /// <returns></returns>
        public bool CheckArgument()
        {
            return true; 
        }
    }
    #endregion

    #region KeepUseClassSearchGrid1Model
    /// <summary> 產投課程列表 </summary>
    public class KeepUseClassSearchGrid1Model
    {
        /// <summary> 選取Checkbox </summary>
        public bool SELECTIS { get; set; }

        /// <summary> 開班流水號 </summary>
        public Int64? OCID { get; set; }

        /// <summary> 班別中文名稱 </summary>
        public string CLASSCNAME { get; set; }

        /// <summary> 班別中文名稱+期別 </summary>
        public string CLASSCNAME2 { get; set; }

        /// <summary> 訓練人數 </summary>
        public Int64? TNUM { get; set; }

        /// <summary> 訓練時數 </summary>
        public Int64? THOURS { get; set; }

        /// <summary> 上課地址學科場地名稱 </summary>
        public string CTNAME1 { get; set; }

        /// <summary> 上課地址術科場地名稱 </summary>
        public string CTNAME2 { get; set; }

        /// <summary> 是否結訓 </summary>
        public string ISCLOSED { get; set; }

        /// <summary> 開訓日期 </summary>
        public DateTime? STDATE { get; set; }

        /// <summary> 開訓日期 (民國年 yyy/MM/dd) </summary>
        public string STDATE_TW
        {
            get
            {
                return MyHelperUtil.DateTimeToTwString(this.STDATE);
            }
        }

        /// <summary> 結訓日期 </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary> 結訓日期 (民國年 yyy/MM/dd) </summary>
        public string FTDATE_TW
        {
            get
            {
                return MyHelperUtil.DateTimeToTwString(this.FTDATE);
            }
        }

        /// <summary> 報名起日期時間 </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary> 報名日期(起日) (民國年時間 yyy/MM/dd HH:mm:ss) </summary>
        public string SENTERDATE_TW_TIME
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.SENTERDATE); }
        }

        /// <summary> 報名訖日期時間 </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary> 報名日期(迄日) (民國年時間 yyy/MM/dd HH:mm:ss) </summary>
        public string FENTERDATE_TW_TIME
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.FENTERDATE); }
        }

        /// <summary> 經費來源-政府負擔 </summary>
        public string DEFGOVCOST { get; set; }

        /// <summary> 經費來源-學員負擔 </summary>
        public string DEFSTDCOST { get; set; }

        /// <summary> 報名繳費方式中文 </summary>
        public string ENTERSUPPLYSTYLE { get; set; }

        /// <summary> 招生狀態 </summary>
        public Int64? ADMISSIONS { get; set; }

        /// <summary> 廠商(機構)名稱 </summary>
        public string ORGNAME { get; set; }

        /// <summary> 原班別中文名稱 </summary>
        public string OLDCLASSCNAME { get; set; }

        /// <summary> 上課距離 </summary>
        public Decimal? Distance { get; set; }
    }
    #endregion

}