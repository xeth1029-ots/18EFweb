using WDAIIP.WEB.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using System.Collections;

namespace WDAIIP.WEB.Models
{
    #region QAViewModel
    /// <summary>
    /// QA Q&A/常見問題 ViewModel
    /// </summary>
    public class QAViewModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public QAViewModel()
        {
            this.Form = new QAFormModel();
        }

        /// <summary>
        /// 常見問題類別代碼中文名稱
        /// </summary>
        public string TYPEID_TEXT { get; set; }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public QAFormModel Form { get; set; }

        /// <summary>
        /// QA的類別清單 GridModel
        /// </summary>
        public IList<QATYPEGridModel> QATYPE_list { get; set; }

        /// <summary>
        /// 查詢結果列 GridModel
        /// </summary>
        public IList<QAGridModel> Grid { get; set; }
    }
    #endregion

    #region QAFormModel
    /// <summary>
    /// 查詢條件
    /// </summary>
    public class QAFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 是否為第一次進入此功能 (Y 是, N 否)
        /// </summary>
        public string IsFirst { get; set; }

        /// <summary>
        /// 常見類別代碼
        /// </summary>
        public string TYPEID { get; set; }

        /// <summary>
        /// 關鍵字(hidden)
        /// </summary>
        public string KEYWORD { get; set; }

        /// <summary>
        /// 關鍵字(輸入欄位)
        /// </summary>
        public string KEYWORD_TEXT { get; set; }

        /// <summary>
        /// 檢查參數正確性 true:OK/false:NG
        /// </summary>
        /// <returns></returns>
        public bool CheckArgument()
        {
            bool result = true;
            if (this.TYPEID == null
                && this.KEYWORD == null
                && this.KEYWORD_TEXT == null) { return false; }

            if (this.TYPEID != null)
            {
                ArrayList planTypeAry = new ArrayList() { "1", "2", "5", "4", "99" };
                if (!planTypeAry.Contains(this.TYPEID)) { return false; }
            }

            return result;
        }

    }
    #endregion

    #region QATYPEGridModel
    /// <summary>
    /// QA的類別清單
    /// </summary>
    public class QATYPEGridModel
    {
        /// <summary>
        /// QA類別代碼
        /// </summary>
        public string CODE { get; set; }

        /// <summary>
        /// QA類別代碼中文描述
        /// </summary>
        public string DESCR { get; set; }
    }
    #endregion

    #region QAGridModel
    /// <summary>
    /// 查詢結果資料列結構 GridModel 
    /// </summary>
    public class QAGridModel : TblTB_QA
    {
        /// <summary>
        /// QA類別代碼中文描述
        /// </summary>
        public string TYPEID_TEXT { get; set; }
    }
    #endregion
}