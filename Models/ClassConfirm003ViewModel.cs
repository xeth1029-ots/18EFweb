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
    public class ClassConfirm003ViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public ClassConfirm003ViewModel()
        {
            this.Form = new ClassConfirm003FormModel();
        }

        /// <summary>
        /// 分署名稱
        /// </summary>
        public string DISTID_TEXT { get; set; }

        /// <summary>
        /// 分署清單-查詢結果
        /// </summary>
        public IList<ClassConfirm003DistNameGridModel> DistNameGrid_list { get; set; }

        /// <summary>
        /// 課程清單-查詢條件
        /// </summary>
        public ClassConfirm003FormModel Form { get; set; }

        /// <summary>
        /// 課程清單-查詢結果
        /// </summary>
        public IList<ClassConfirm003GridModel> Grid_list { get; set; }

        /// <summary>
        /// 單位課程-查詢結果
        /// </summary>
        public IList<ClassConfirm003UnitGridModel> UnitGrid_list { get; set; }

        /// <summary>
        /// 學生清單-查詢條件
        /// </summary>
        public ClassConfirm003StudFormModel StudForm { get; set; }

        /// <summary>
        /// 學生清單-查詢結果
        /// </summary>
        public IList<ClassConfirm003StudGridModel> StudGrid_list { get; set; }
    }

    /// <summary>
    /// 分署清單-查詢結果-充電起飛計畫
    /// </summary>
    public class ClassConfirm003DistNameGridModel
    {
        /// <summary>
        /// 分署代碼
        /// </summary>
        public string DISTID { get; set; }

        /// <summary>
        /// 分署名稱
        /// </summary>
        public string DISTNAME { get; set; }
    }

    /// <summary>
    /// 課程清單-查詢條件-充電起飛計畫
    /// </summary>
    public class ClassConfirm003FormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 是否為第一次進入此功能 (Y 是, N 否)
        /// </summary>
        public string IsFirst { get; set; }

        /// <summary>
        /// 分署代碼
        /// </summary>
        public string DISTID { get; set; }

        /// <summary>
        /// 輸入訓練單位名稱
        /// </summary>
        [Display(Name = "訓練單位名稱")]
        public string ORGNAME { get; set; }

        /// <summary>
        /// 輸入訓練班別名稱
        /// </summary>
        [Display(Name = "班別名稱檢索")]
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 分署名稱
        /// </summary>
        [Display(Name = "分署名稱")]
        public string DISTID_TEXT { set; get; }


        //檢核輸入項
        //public bool CheckArgument()
        //{
        //    bool result = true;
        //    if (this.DISTID == null) { result = false; }
        //    if (string.IsNullOrEmpty(this.ORGNAME) && string.IsNullOrEmpty(this.CLASSCNAME)) { result = false; }
        //    return result;
        //}

    }

    /// <summary>
    /// 課程清單-查詢結果-充電起飛計畫
    /// </summary>
    public class ClassConfirm003GridModel : TblCLASS_CLASSINFO
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
    }

    /// <summary>
    /// 單位課程-查詢結果-充電起飛計畫
    /// </summary>
    public class ClassConfirm003UnitGridModel : TblCLASS_CLASSINFO
    {
        /// <summary>
        /// 文號
        /// </summary>
        public string ODNUMBER { get; set; }

        /// <summary>
        /// 發布單位
        /// </summary>
        public string CFORGNAME { get; set; }

        /// <summary>
        /// 公告期間
        /// </summary>
        public string CONFIRDATERANGE { get; set; }

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
        /// 公告確認者依系統帳號
        /// </summary>
        public string CONFIRACCT { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string CONFIRNAME { get; set; }

        /// <summary>
        /// 公告GUID
        /// </summary>
        public string CFGUID { get; set; }

        /// <summary>
        /// 公告序號
        /// </summary>
        public decimal? CFSEQNO { get; set; }
    }

    /// <summary>
    /// 學生清單-查詢條件-充電起飛計畫
    /// </summary>
    public class ClassConfirm003StudFormModel
    {
        /// <summary>
        /// 分署別
        /// </summary>
        public string DISTID { get; set; }

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
    /// 學生清單-查詢結果-充電起飛計畫
    /// </summary>
    public class ClassConfirm003StudGridModel
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
}