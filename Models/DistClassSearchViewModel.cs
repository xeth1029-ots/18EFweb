using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models
{
    #region DistClassSearchViewModel
    public class DistClassSearchViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public DistClassSearchViewModel()
        {
            this.Form = new DistClassSearchFormModel();
        }

        /// <summary>
        /// 計畫別姓名
        /// </summary>
        public string DISTID_TEXT { get; set; }
        
        /// <summary>
        /// 計畫別名稱
        /// </summary>
        public string PLANTYPE_TEXT { get; set; }

        /// <summary>
        /// 查詢條件
        /// </summary>
        public DistClassSearchFormModel Form { get; set; }

        /// <summary> 查詢結果清單(for 產投) </summary>
        public IList<DistClassSearchGrid1Model> Grid1 { get; set; }

        /// <summary> 查詢結果清單(for 在職進修) </summary>
        public IList<DistClassSearchGrid2Model> Grid2 { get; set; }

        /// <summary>
        /// 區域產業據點
        /// </summary>
        public IList<DistClassSearchGrid5Model> Grid5 { get; set; }

        /// <summary>
        /// 分署清單選項
        /// </summary>
        public IList<SelectListItem> DIST_list {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"001", "勞動力發展署北基宜花金馬分署"},
                    {"003","勞動力發展署桃竹苗分署" },
                    {"004","勞動力發展署中彰投分署" },
                    {"005","勞動力發展署雲嘉南分署" },
                    {"006","勞動力發展署高屏澎東分署" }
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 計畫別清單選項
        /// </summary>
        public IList<SelectListItem> PLAN_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "2", "分署自辦在職訓練"}, { "1", "產業人才投資方案" }, { "5", "區域產業據點" }
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }
    }
    #endregion

    #region DistClassSearchFormModel
    public class DistClassSearchFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 是否為第一次進入此功能 (Y 是, N 否)
        /// </summary>
        public string IsFirst { get; set; }

        /// <summary>
        /// 執行模式 (1分署別, 2計畫別)
        /// </summary>
        public string ActiveMode { get; set; }

        /// <summary>
        /// 轄區代碼 (ref:id_district.distid)
        /// </summary>
        public string DISTID { get; set; }

        /// <summary>
        /// 計畫類別 (1:產投 2：在職)
        /// </summary>
        public string PLANTYPE { get; set; }

        /// <summary>
        /// 是否提供上課位置距離 (Y:是  N:否)
        /// </summary>
        public string PROVIDELOCATION { get; set; }
    }
    #endregion

    #region DistClassSearchGrid1Model
    /// <summary> 產投課程列表 </summary>
    public class DistClassSearchGrid1Model
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

    #region DistClassSearchGrid2Model
    public class DistClassSearchGrid2Model
    {
        /// <summary>
        /// 選取Checkbox
        /// </summary>
        public bool SELECTIS { get; set; }

        /// <summary>
        /// 報名起日期
        /// </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>
        /// 報名日期(起日) (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string SENTERDATE_TW_TIME
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.SENTERDATE); }
        }

        /// <summary>
        /// 報名迄日期
        /// </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>
        /// 報名日期(迄日) (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string FENTERDATE_TW_TIME
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.FENTERDATE); }
        }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 開訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string STDATE_TW
        {
            get
            {
                return MyHelperUtil.DateTimeToTwString(this.STDATE);
            }
        }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 結訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get
            {
                return MyHelperUtil.DateTimeToTwString(this.FTDATE);
            }
        }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 課程名稱+期別
        /// </summary>
        public string TRAINCLASS { get; set; }

        /// <summary>
        /// 期別
        /// </summary>
        public string CYCLTYPE { get; set; }

        /// <summary>
        /// 訓練時數
        /// </summary>
        public Int64? THOURS { get; set; }

        /// <summary>
        /// 訓練時段代碼
        /// </summary>
        public string TPERIOD { get; set; }

        /// <summary>
        /// 訓練時段名稱
        /// </summary>
        public string TPERIODNAME { get; set; }

        /// <summary>
        /// 訓練計畫名稱 
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 轄區中心名稱
        /// </summary>
        public string TRAINCENTER { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        public string TRAINPLACE { get; set; }

        /// <summary>
        /// 受訓資格學歷名稱
        /// </summary>
        public string TRAINEDGE { get; set; }

        /// <summary>
        /// 訓練時段名稱
        /// </summary>
        public string TRAINTIME { get; set; }

        /// <summary>
        /// 廠商(機構)名稱
        /// </summary>
        public string TRAINORG { get; set; }

        /// <summary>
        /// 通俗職類名稱
        /// </summary>
        public string CJOBNAME { get; set; }

        /// <summary>
        /// 上課時間(自辦在職)
        /// </summary>
        public string NOTE3 { get; set; }

        /// <summary>
        /// 上課距離
        /// </summary>
        public Decimal? Distance { get; set; }
    }
    #endregion

    public class DistClassSearchGrid5Model
    {
        /// <summary>
        /// 選取Checkbox
        /// </summary>
        public bool SELECTIS { get; set; }

        /// <summary>
        /// 報名起日期
        /// </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>
        /// 報名日期(起日) (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string SENTERDATE_TW_TIME
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.SENTERDATE); }
        }

        /// <summary>
        /// 報名迄日期
        /// </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>
        /// 報名日期(迄日) (民國年時間 yyy/MM/dd HH:mm:ss)
        /// </summary>
        public string FENTERDATE_TW_TIME
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(this.FENTERDATE); }
        }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 開訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string STDATE_TW
        {
            get
            {
                return MyHelperUtil.DateTimeToTwString(this.STDATE);
            }
        }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 結訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get
            {
                return MyHelperUtil.DateTimeToTwString(this.FTDATE);
            }
        }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 課程名稱+期別
        /// </summary>
        public string TRAINCLASS { get; set; }

        /// <summary>
        /// 期別
        /// </summary>
        public string CYCLTYPE { get; set; }

        /// <summary>
        /// 訓練時數
        /// </summary>
        public Int64? THOURS { get; set; }

        /// <summary>
        /// 訓練時段代碼
        /// </summary>
        public string TPERIOD { get; set; }

        /// <summary>
        /// 訓練時段名稱
        /// </summary>
        public string TPERIODNAME { get; set; }

        /// <summary>
        /// 訓練計畫名稱 
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 轄區中心名稱
        /// </summary>
        public string TRAINCENTER { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        public string TRAINPLACE { get; set; }

        /// <summary>
        /// 受訓資格學歷名稱
        /// </summary>
        public string TRAINEDGE { get; set; }

        /// <summary>
        /// 訓練時段名稱
        /// </summary>
        public string TRAINTIME { get; set; }

        /// <summary>
        /// 廠商(機構)名稱
        /// </summary>
        public string TRAINORG { get; set; }

        /// <summary>
        /// 通俗職類名稱
        /// </summary>
        public string CJOBNAME { get; set; }

        /// <summary>
        /// 上課時間(自辦在職)
        /// </summary>
        public string NOTE3 { get; set; }

        /// <summary>
        /// 上課距離
        /// </summary>
        public Decimal? Distance { get; set; }
    }
}