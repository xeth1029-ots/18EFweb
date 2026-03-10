using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    #region ClassConfirm001ViewModel
    /// <summary>
    /// Classconfirm001 產業人才投資方案/課程錄訓名單查詢 ViewModel
    /// </summary>
    public class ClassConfirm001ViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public ClassConfirm001ViewModel()
        {
            this.Form = new ClassConfirm001FormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public ClassConfirm001FormModel Form { get; set; }

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<ClassConfirm001GridModel> Grid { get; set; }

        /// <summary> 
        /// 單位課程查詢條件 FormModel
        /// </summary>
        public ClassConfirm0012FormModel Form2 { get; set; }

        /// <summary>
        /// 錄訓名單明細資料 DetailModel
        /// </summary>
        public ClassConfirm001DetailModel Detail { get; set; }
    }
    #endregion

    #region ClassConfirm001FormModel
    /// <summary>
    /// 查詢條件
    /// </summary>
    public class ClassConfirm001FormModel : PagingResultsViewModel
    {
        //格式檢核(檢核是不是數字)
        [RegularExpression("^[0-9]*$", ErrorMessage = "課程代碼輸入格式錯誤(請輸入數字)")]
        [Display(Name = "課程代碼")]
        public string OCID_TEXT { get; set; }

        /// <summary>
        /// 課程代碼
        /// </summary>
        public decimal? OCID {
            get
            {
                decimal? rtn = null;
                decimal num = 0;

                if (!string.IsNullOrEmpty(this.OCID_TEXT) && decimal.TryParse(this.OCID_TEXT, out num)) {
                    rtn = num;
                }

                return rtn;
            }
        }

        /// <summary>
        /// 班別名稱
        /// </summary>
        [Display(Name = "班別名稱檢索")]
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 訓練單位名稱
        /// </summary>
        [Display(Name = "訓練單位名稱")]
        public string ORGNAME { get; set; }
    }
    #endregion

    #region ClassConfirm001GridModel
    /// <summary>
    /// 查詢結果資料列結構
    /// </summary>
    public class ClassConfirm001GridModel : TblCLASS_CLASSINFO
    {
        /// <summary>
        /// 訓練單位名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 開訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string STDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.STDATE); }
        }

        /// <summary>
        /// 結訓日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string FTDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.FTDATE); }
        }

        /// <summary>
        /// 公告日期
        /// </summary>
        public DateTime? CONFIRDATE { get; set; }

        /// <summary>
        /// 公告日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string CONFIRDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.CONFIRDATE); }
        }

        /// <summary>
        /// 公告期間
        /// </summary>
        public string CONFIRDATERANGE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string CONFIRACCT { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string CONFIRNAME { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string CFGUID { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public decimal? CFSEQNO { get; set; }

        /// <summary>
        /// 文號
        /// </summary>
        public string ODNUMBER { get; set; }

        /// <summary>
        /// 發布單位
        /// </summary>
        public string CFORGNAME { get; set; }

    }
    #endregion

    #region ClassConfirm0012FormModel
    /// <summary>
    /// 單位課程查詢條件
    /// </summary>
    public class ClassConfirm0012FormModel : PagingResultsViewModel
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
    #endregion

    #region ClassConfirm001DetailModel
    /// <summary>
    /// 錄訓名單明細資料
    /// </summary>
    public class ClassConfirm001DetailModel
    {
        /// <summary>
        /// 班別代碼
        /// </summary>
        public decimal? OCID { get; set; }

        /// <summary>
        /// 訓練單位名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 訓練班別
        /// </summary>
        public string CLASSCNAME { get; set; }
        
        /// <summary>
        /// 公告日期
        /// </summary>
        public DateTime? CONFIRDATE { get; set; }

        /// <summary>
        /// 公告日期 (民國年 yyy/MM/dd)
        /// </summary>
        public string CONFIRDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.CONFIRDATE); }
        }

        /// <summary>
        /// 公告期間
        /// </summary>
        public string CONFIRDATERANGE { get; set; }

        /// <summary>
        /// 文號
        /// </summary>
        public string ODNUMBER { get; set; }

        /// <summary>
        /// 發布單位
        /// </summary>
        public string CFORGNAME { get; set; }

        /// <summary>
        /// 錄訓名單明細資料
        /// </summary>
        public IList<ClassConfirm0012GridModel> UserGrid { get; set; }
    }
    #endregion

    #region ClassConfirm0012GridModel
    /// <summary>
    /// 學生名單資料
    /// </summary>
    public class ClassConfirm0012GridModel
    {
        /// <summary>
        /// ?
        /// </summary>
        public string TOPTITLE2 { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string TOPTITLE3 { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string ROWNUM2 { get; set; }

        /// <summary>
        /// 訓練班別
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 班別代碼
        /// </summary>
        public decimal? OCID { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string STUDNAME2 { get; set; }

        /// <summary>
        /// 身分證字號(有遮罩)
        /// </summary>
        public string IDNO2 { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string STUDENTID { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string STUDID2 { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public decimal? SETID { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public DateTime? ENTERDATE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public decimal? SERNUM { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public decimal? SOCID { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string STUDMODE { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string STUDMODE_N { get; set; }

        /// <summary>
        /// ? (這裡是因為SQL裡面有轉成字串所以宣告成字串)
        /// </summary>
        public string CONFIRDATE { get; set; }

        /// <summary>
        /// ? (這裡是因為SQL裡面有轉成字串所以宣告成字串)
        /// </summary>
        public string CONFIRDATE2 { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string ODNUMBER { get; set; }
    }
    #endregion
}