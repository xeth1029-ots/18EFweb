using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// ClassConfirm005 區域產業據點/課程錄訓名單查詢 ViewModel
    /// </summary>
    public class ClassConfirm005ViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public ClassConfirm005ViewModel()
        {
            this.Form = new ClassConfirm005FormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public ClassConfirm005FormModel Form { get; set; }

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<ClassConfirm005GridModel> Grid { get; set; }

        /// <summary>
        /// 單位課程查詢條件
        /// </summary>
        public ClassConfirm005ClassFormModel ClassForm { get; set; }

        /// <summary>
        /// 學生查詢結果
        /// </summary>
        public ClassConfirm005DetailModel Detail { get; set; }

        /// <summary>
        /// 分署別
        /// </summary>
        public IList<SelectListItem> DISTIDITEM_list
        {
            get
            {
                //etraining直接寫死
                var dictionary = new Dictionary<string, string>
                {
                    {"001","北基宜花金馬分署" },
                    {"003","桃竹苗分署" },
                    {"004","中彰投分署" },
                    {"005","雲嘉南分署" },
                    {"006","高屏澎東分署" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }
    }

    /// <summary>
    /// 查詢條件
    /// </summary>
    public class ClassConfirm005FormModel : PagingResultsViewModel
    {
        //格式檢核(檢核是不是數字)
        [RegularExpression("^[0-9]*$", ErrorMessage = "課程代碼輸入格式錯誤(請輸入數字)")]
        [Display(Name = "班別代碼")]
        public string OCID_TEXT { get; set; }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public decimal? OCID
        {
            get
            {
                decimal? rtn = null;
                decimal num = 0;

                if (!string.IsNullOrEmpty(this.OCID_TEXT) && decimal.TryParse(this.OCID_TEXT, out num))
                {
                    rtn = num;
                }

                return rtn;
            }
        }

        /// <summary>
        /// 班別名稱 (課程關鍵字)
        /// </summary>
        [Display(Name = "課程名稱關鍵字")]
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 上課地點-分署別
        /// </summary>
        [Display(Name = "分署別")]
        public string DISTIDITEM { get; set; }
    }

    /// <summary>
    /// 查詢結果
    /// </summary>
    public class ClassConfirm005GridModel
    {
        /// <summary>
        /// ?
        /// </summary>
        public int? SEQNO { get; set; }

        /// <summary>
        /// 報名起日期
        /// </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>
        /// 報名起日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string SENTERDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(SENTERDATE); }
        }

        /// <summary>
        /// 報名訖日期 
        /// </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>
        /// 報名訖日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string FENTERDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwFormatLongString(FENTERDATE); }
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
            get { return MyHelperUtil.DateTimeToTwString(STDATE); }
        }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 開訓日期 (民國年 yyyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(FTDATE); }
        }

        /// <summary>
        /// 班別代碼
        /// </summary>
        public decimal? OCID { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string TRAINCLASS { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string REGBETWEEN { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public DateTime? REGSDATE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public DateTime? REGEDATE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string CYCLTYPE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public decimal? THOURS { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string TRAINBETWEEN { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public DateTime? TRAINSDATE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public DateTime? TRAINEDATE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string TPERIOD { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string TPERIODNAME { get; set; }

        /// <summary>
        /// ? (decode(a.TPropertyID,0,'職前訓練', 1,'在職訓練',2,'委託訓練'))
        /// </summary>
        public string TRAINATTR { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string TRAINCENTER { get; set; }

        /// <summary>
        /// ? nvl(f.CTName,'其他')
        /// </summary>
        public string TRAINPLACE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string TRAINEDGE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string TRAINTIME { get; set; }

        /// <summary> 訓練單位 </summary>
        public string TRAINORG { get; set; }

        /// <summary> CJOBNAME </summary>
        public string CJOBNAME { get; set; }

        /// <summary> GUID </summary>
        public string CFGUID { get; set; }

        /// <summary> 序號 </summary>
        public decimal? CFSEQNO { get; set; }

        /// <summary> (建檔日)公告日期 </summary>
        public DateTime? CONFIRDATE { get; set; }

        /// <summary> (建檔日)公告日期 (民國年 yyy/MM/dd) </summary>
        public string CONFIRDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.CONFIRDATE); }
        }

        /// <summary> 公告期間 </summary>
        public string CONFIRDATERANGE { get; set; }

        /// <summary> 文號 </summary>
        public string ODNUMBER { get; set; }

        /// <summary>公告文號2</summary>
        public string ODNUMBER2 { get; set; }

        /// <summary> 發布單位</summary>
        public string CFORGNAME { get; set; }

        /// <summary> 審核者</summary>
        public string ROVEDACCT { get; set; }

        /// <summary> 公告者</summary>
        public string ANNMENTACCT { get; set; }

        /// <summary> 公告日期 </summary>
        public DateTime? ANNMENTDATE { get; set; }
        
        /// <summary> 公告日期 (民國年 yyy/MM/dd) </summary>
        public string ANNMENTDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.ANNMENTDATE); }
        }
        
        /// <summary> 公告日期 (民國年 yyy/MM/dd) </summary>
        public string ANNMENTDATERANGE { get; set; }

    }

    /// <summary>
    /// 單位課程查詢條件
    /// </summary>
    public class ClassConfirm005ClassFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 班別代碼
        /// </summary>
        public decimal? OCID { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string CFGUID { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string CFSEQNO { get; set; }
    }

    /// <summary>
    /// 學生查詢結果
    /// </summary>
    public class ClassConfirm005DetailModel
    {
        /// <summary> 班別代碼 </summary>
        public decimal? OCID { get; set; }

        /// <summary> 訓練單位 </summary>
        public string TRAINORG { get; set; }

        ///  <summary>班別</summary>
        public string TRAINCLASS { get; set; }

        //<summary>訓練班別</summary> //public string CLASSCNAME { get; set; }

        /// <summary>
        /// 公告日期
        /// </summary>
        public DateTime? CONFIRDATE { get; set; }

        /// <summary> 公告日期 (民國年 yyy/MM/dd) </summary>
        public string CONFIRDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.CONFIRDATE); }
        }

        /// <summary> 公告期間 </summary>
        public string CONFIRDATERANGE { get; set; }

        /// <summary> 文號 </summary>
        public string ODNUMBER { get; set; }

        /// <summary>公告文號2</summary>
        public string ODNUMBER2 { get; set; }

        /// <summary> 發布單位</summary>
        public string CFORGNAME { get; set; }

        /// <summary> 公告日期2 </summary>
        public DateTime? ANNMENTDATE { get; set; }

        /// <summary> 公告日期2 (民國年 yyy/MM/dd) </summary>
        public string ANNMENTDATE_TW { get { return MyHelperUtil.DateTimeToTwString(this.ANNMENTDATE); } }
        
        /// <summary> 公告期間2 </summary>
        public string ANNMENTDATERANGE { get; set; }

        /// <summary>
        /// 學生名單資料
        /// </summary>
        public IList<ClassConfirm005StudGridModel> UserGrid { get; set; }
    }

    /// <summary>
    /// 學生清單資料
    /// </summary>
    public class ClassConfirm005StudGridModel
    {
        /// <summary> 班別代碼 </summary>
        public decimal? OCID { get; set; }

        /// <summary> ?確認名單序號 </summary>
        public string CFGUID { get; set; }

        /// <summary> 確認名單序號-序號 </summary>
        public decimal? CFSEQNO { get; set; }

        /// <summary> 甄試號碼 </summary>
        public string EXAMNO { get; set; }

        /// <summary>
        /// 學員姓名
        /// </summary>
        public string STUDNAME { get; set; }

        /// <summary>
        /// 學員姓名
        /// </summary>
        public string STUDNAME2 { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        public string IDNO { get; set; }

        /// <summary>
        /// 身分證字號（有遮罩）
        /// </summary>
        public string IDNO2 { get; set; }


        /// <summary>
        /// ?
        /// </summary>
        public string EXAMPLUS { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string EIDENTITY { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public Double? SUMOFGRAD { get; set; }

        /// <summary>
        /// 甄試結果 STUDMODE_N
        /// </summary>
        public string STUDMODE_N { get; set; }

        /// <summary> 公告日期 </summary>
        public string CONFIRDATE { get; set; }

        /// <summary> 公告期間 </summary>
        public string CONFIRDATE2 { get; set; }

        /// <summary> 文號 </summary>
        public string ODNUMBER { get; set; }

        /// <summary>公告文號2</summary>
        public string ODNUMBER2 { get; set; }

    }
}