using System;
using System.Collections.Generic;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models
{
    public class TrainingSubsidyViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public TrainingSubsidyViewModel()
        {
            this.Form = new TrainingSubsidyFormModel();
        }

        /// <summary>
        /// 查詢條件
        /// </summary>
        public TrainingSubsidyFormModel Form { get; set; }

        /// <summary>
        /// 近三年內課程報名及參訓情形資料
        /// </summary>
        public NearYearTrainDetailModel NearYearDetail { get; set; }
    }

    /// <summary>
    /// 查詢條件
    /// </summary>
    public class TrainingSubsidyFormModel
    {
        /// <summary>
        /// 現在時段是否進行查詢作業(是true/否false)
        /// </summary>
        public bool CANVIEW { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        public string IDNO { get; set; }

        /// <summary>
        /// 出生日期 (文字)
        /// </summary>
        public string BIRTHDAY_TEXT { get; set; }
    }

    #region 近三年內課程報名及參訓情形
    /// <summary>
    /// 補助金申請
    /// </summary>
    public class DefStdCostGridModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string RID { get; set; }

        /// <summary>
        /// 班別中文名稱
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 班級學員代碼
        /// </summary>
        public Int64? SOCID { get; set; }

        /// <summary>
        /// 學員經費撥款狀態
        /// 0失敗 1通過  NULL
        /// </summary>
        public string APPLIEDSTATUS { get; set; }

        /// <summary>
        /// 撥款日期
        /// </summary>
        public DateTime? ALLOTDATE { get; set; }

        /// <summary>
        /// 學員經費審核狀態-申請
        /// Y成功 N失敗 R退件修正 NULL未審核
        /// </summary>
        public string APPLIEDSTATUSM { get; set; }

        /// <summary>
        /// 補助金額
        /// </summary>
        public Int64? SUMOFMONEY { get; set; }


        /// <summary>
        /// 班級代碼
        /// </summary>
        public Int64? SC_SOCID { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public string YEARS { get; set; }

        /// <summary>
        /// 是否為在職者補助身份
        /// Y:是 N:否 NULL:未選擇
        /// 46 補助辦理保母職業訓練 
        /// 47 補助辦理照顧服務員職業訓練
        /// </summary>
        public string WORKSUPPIDENT { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 訓練計畫代碼
        /// </summary>
        public string TPLANID { get; set; }
    }

    /// <summary>
    /// 近三年課程報名/參訓情形
    /// </summary>
    public class NearYearTrainDetailModel
    {
        /// <summary>
        /// 補助總額
        /// </summary>
        public Int64? MoneyTotal { get; set; }

        /// <summary>
        /// 經費審核通過總額
        /// </summary>
        public Int64? DEFSTDCOST { get; set; }

        /// <summary>
        /// 剩餘可用額度
        /// </summary>
        public Int64? RemainSub { get; set; }

        /// <summary>
        /// 補助額度清單
        /// </summary>
        public IList<TrainingSubsidyGridModel> Grid { get; set; }


        /// <summary>補助金額資訊-前次3年7萬</summary>
        public string MoneyShow1 { get; set; }

        /// <summary>補助金額資訊-最新3年7萬</summary>
        public string MoneyShow2 { get; set; }

        /// <summary>
        /// 補助費用已達標（6萬/9萬）提示訊息文字
        /// </summary>
        public string Money69Msg { get; set; }

        /// <summary>
        /// 檢核到有重複的課程時間時所需顯示的提示文字
        /// </summary>
        public string DoubleMsg { get; set; }

        /// <summary>
        /// 存在重複的課程時間狀況之警示訊息
        /// </summary>
        public string DoubleDescMsg { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SDate2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EDate2 { get; set; }

        /// <summary>
        /// 補助金申請資料
        /// </summary>
        public IList<DefStdCostGridModel> StdCostGrid { get; set; }

        /// <summary>
        /// (本次)參訓課程清單
        /// </summary>
        public IList<NearYearTrainGridModel> NearYearGrid { get; set; }

        /// <summary>
        /// (前次)參訓課程清單
        /// </summary>
        public IList<NearYearTrainGridModel> NearYearGrid2 { get; set; }

        /// <summary>
        /// 計畫訓練內容簡介
        /// </summary>
        public IList<TrainDescGridModel> TrainDescList { get; set; }
    }

    /// <summary>
    /// 合併計算三年七萬的計畫
    /// </summary>
    public class TrainingSubsidyGridModel
    {
        /// <summary>
        /// 計畫年度 (ex:西元'2004')
        /// </summary>
        public string PLANYEAR { get; set; }

        /// <summary>
        /// 計畫年度 (民國年)
        /// </summary>
        public string PLANYEAR_TW {
            get
            {
                string rtn = string.Empty;

                if (!string.IsNullOrEmpty(this.PLANYEAR))
                {
                    rtn = (Convert.ToInt32(this.PLANYEAR) - 1911).ToString();
                }

                return rtn;
            }
        }

        /// <summary>
        /// 大計畫代碼
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary>
        /// 計畫代碼
        /// </summary>
        public Int64? PLANID { get; set; }

        /// <summary>
        /// 訓練計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 訓練機構名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 班級名稱
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 開訓日期(西元年yyyy/MM/dd)
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
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 結訓日期(西元年yyyy/MM/dd)
        /// </summary>
        public string FTDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.FTDATE); }
        }

        /// <summary>
        /// 開訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get { return (FTDATE.HasValue ? MyHelperUtil.DateTimeToTwString(FTDATE.Value) : ""); }
        }

        /// <summary>
        /// 受訓期間
        /// </summary>
        public string TDATERANGE
        {
            get
            {
                string rtn = string.Empty;

                if (!string.IsNullOrEmpty(this.STDATE_TW) || !string.IsNullOrEmpty(this.FTDATE_TW))
                {
                    rtn = this.STDATE_TW + " ~ " + this.FTDATE_TW;
                }
                return rtn;
            }
        }

        /// <summary>
        /// 審核狀態代碼(Y通過 N失敗 R退件修正 null)
        /// </summary>
        public string APPLIEDSTATUSM {get;set;}

        /// <summary>
        /// 撥款狀態中文描述
        /// </summary>
        public string APPLIEDSTATUSM_TEXT { get; set; }

        /// <summary>
        /// 撥款狀態代碼(0失敗 1通過 NULL)
        /// </summary>
        public string APPLIEDSTATUS { get; set; }

        /// <summary>
        /// 撥款狀態中文描述
        /// </summary>
        public string APPLIEDSTATUS_TEXT { get; set; }

        /// <summary>
        /// 補助金額
        /// </summary>
        public decimal? SUMOFMONEY { get; set; }

        /// <summary>
        /// 學員狀態代碼
        /// </summary>
        public string SDSTATUS { get; set; }
    }

    /// <summary>
    /// 近三年課程報名/參訓情形清單資訊
    /// </summary>
    public class NearYearTrainGridModel
    {
        /// <summary>
        /// 資料類型 1.前次 2.本次
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 檢查是否有重複的課程時間註記
        /// 1 有
        /// </summary>
        public string DOUBLE_DESC { get; set; }

        /// <summary>
        /// 檢查是否有重複的課程時間，有則顯示註記文字
        /// </summary>
        public string DOUBLE_DESC_TEXT { get; set; }

        /// <summary>
        /// e網報名代碼
        /// </summary>
        public Int64? ESERNUM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Int64? ESETID { get; set; }

        /// <summary>
        /// 報名日期
        /// </summary>
        public DateTime? RELENTERDATE { get; set; }

        /// <summary>
        /// 報名日期(西元年yyyy/MM/dd)
        /// </summary>
        public string RELENTERDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.RELENTERDATE));
            }
        }

        /// <summary>
        /// 報名備註
        /// </summary>
        public string SIGNUPMEMO { get; set; }

        /// <summary>
        /// 身分證
        /// </summary>
        public string IDNO { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 出生日期(西元年yyyy/MM/dd)
        /// </summary>
        public string BIRTHDAY_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.BIRTHDAY));
            }
        }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 訓練起日
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 訓練起日（西元年yyyy/MM/dd）
        /// </summary>
        public string STDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.STDATE));
            }
        }

        /// <summary>
        /// 訓練迄日
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 訓練迄日（西元年yyyy/MM/dd）
        /// </summary>
        public string FTDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.FTDATE));
            }
        }

        /// <summary>
        /// 訓練起迄日
        /// </summary>
        public string TROUND { get; set; }

        /// <summary>
        /// 訓練計畫年度
        /// </summary>
        public string YEARS { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 轄區名稱
        /// </summary>
        public string DISTNAME { get; set; }

        /// <summary>
        /// 訓練單位名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 班級課程名稱
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 訓練時數
        /// </summary>
        public int? THOURS { get; set; }

        /// <summary>
        /// 訓練人數
        /// </summary>
        public int? TNUM { get; set; }

        /// <summary>
        /// 收件成功序號資訊
        /// </summary>
        public string SIGNNO_TEXT { get; set; }

        /// <summary>
        /// 報名狀態
        /// 0:收件完成 1:報名成功 2:報名失敗 3:正取(Key_SelResult) 4:備取 5:未錄取
        /// </summary>
        public Int64? SIGNUPSTATUS { get; set; }

        /// <summary>
        /// 報名狀態 中文描述
        /// </summary>
        public string SIGNUPSTATUS_TEXT { get; set; }

        /// <summary>
        /// ref:FN_GET_PLAN_ONCLASS
        /// </summary>
        public string WEEKS2 { get; set; }

        /// <summary>
        /// 總費用（平均每人額度）
        /// </summary>
        public Int64? TOTALCOST { get; set; }

        /// <summary>
        /// 經費來源-政府負擔金額（平均每人額度）
        /// </summary>
        public Int64? DEFGOVCOST { get; set; }

        /// <summary>
        /// 系統日期（db）
        /// </summary>
        public DateTime? ANOW { get; set; }

        /// <summary>
        /// 系統日期(西元年yyyy/MM/dd)
        /// </summary>
        public string ANOW_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.ANOW));
            }
        }

        /// <summary>
        /// 若超過開訓日 且超過報名結束日 則為1 否為0 (1:啟動'未錄取 顯示功能。)
        /// </summary>
        public int? XDAY1 { get; set; }

        /// <summary>
        /// 6個月內的報名資料，供計算使用金額比較使用
        /// </summary>
        public int? XDAY2 { get; set; }

        /// <summary>
        /// 開訓後15天
        /// </summary>
        public int? XDAY3 { get; set; }

        /// <summary>
        /// 補助費申請狀態文字說明
        /// </summary>
        public string SUBSIDYCOST_TEXT { get; set; }

        /// <summary>
        /// 補助費合計
        /// </summary>
        public string SUMOFMONEY { get; set; }
    }

    /// <summary>
    /// 要比對的課程資料
    /// </summary>
    public class TrainDescGridModel
    {
        /// <summary>
        /// 計畫訓練內容簡介代碼
        /// </summary>
        public Int64? PTDID { get; set; }

        /// <summary>
        /// 訓練日期-起
        /// </summary>
        public DateTime? STRAINDATE { get; set; }

        /// <summary>
        /// 訓練日期-起(西元年yyyy/MM/dd)
        /// </summary>
        public string STRAINDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.STRAINDATE));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string STRAINDATE1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string STRAINDATE2 { get; set; }

        /// <summary>
        /// 時間型態
        /// </summary>
        public DateTime? STRAINDATE1_DT { get; set; }

        public DateTime? STRAINDATE2_DT { get; set; }

        /// <summary>
        /// 單元名稱(97年產學訓授課時間)
        /// </summary>
        public string PNAME { get; set; }

        /// <summary>
        /// 時數(97年產學訓時數)
        /// </summary>
        public int? PHOUR { get; set; }

        /// <summary>
        /// 訓練課程代碼
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 訓練起日
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 訓練起日-起(西元年yyyy/MM/dd)
        /// </summary>
        public string STDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.STDATE));
            }
        }

        /// <summary>
        /// 訓練迄日
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 訓練迄日-起(西元年yyyy/MM/dd)
        /// </summary>
        public string FTDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.FTDATE));
            }
        }
    }

    /// <summary>
    /// 政府負擔補助金額資訊
    /// </summary>
    public class GovCostGridModel
    {
        /// <summary>
        /// 訓練起日
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 訓練起日(西元年yyyy/MM/dd)
        /// </summary>
        public string STDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.STDATE));
            }
        }

        /// <summary>
        /// 訓練迄日
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 訓練迄日(西元年yyyy/MM/dd)
        /// </summary>
        public string FTDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.FTDATE));
            }
        }

        /// <summary>
        /// 經費來源-政府負擔金額（每人平均金額）
        /// </summary>
        public string CLSDEFGOVCOST { get; set; }

        /// <summary>
        /// 總費用（每人平均金額）
        /// </summary>
        public string CLSTTLCOST { get; set; }
    }

    /// <summary>
    /// 審核通過/參訓過的三年起迄區間資訊
    /// </summary>
    public class YearsPeriodGridModel
    {
        /// <summary>
        /// 訓練起始日期
        /// </summary>
        public DateTime? SDATE { get; set; }

        /// <summary>
        /// 訓練起始日期（西元年yyyy/MM/dd）
        /// </summary>
        public string SDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.SDATE));
            }
        }

        /// <summary>
        /// 訓練結束日（訓練起日往後推算3年）
        /// </summary>
        public DateTime? EDATE { get; set; }

        /// <summary>
        /// 訓練結束日（西元年yyyy/MM/dd）
        /// </summary>
        public string EDATE_AD
        {
            get
            {
                return MyHelperUtil.TransTwToAdYear(MyHelperUtil.TransToTwYear(this.EDATE));
            }
        }

        public string PLANYEAR { get; set; }

        public DateTime? STDATE { get; set; }
    }

    /// <summary>
    /// 已實際請領補助費
    /// </summary>
    public class SubsidyCost28GridModel
    {
        /// <summary>
        /// 班級編號
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 補助費合計
        /// </summary>
        public decimal? SUMOFMONEY { get; set; }

        public string DEFGOVCOST { get; set; }
    }
    #endregion
}