using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Turbo.Commons;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace WDAIIP.WEB.Models
{
    #region ClassTraceViewModel
    [Serializable]
    public class ClassTraceViewModel
    {
        public ClassTraceViewModel()
        {
            this.Form = new ClassTraceFormModel();
        }

        /// <summary>
        /// 查詢條件
        /// </summary>
        public ClassTraceFormModel Form { get; set; }

        /// <summary>
        /// 查詢結果
        /// </summary>
        public IList<ClassTraceGridModel> Grid { get; set; }

        /// <summary>
        /// 檢核參數
        /// </summary>
        /// <param name="httpContext"></param>
        public static void CheckArgument(HttpContextBase httpContext)
        {
            string cst_str_s1 = string.Empty;
            NameValueCollection parms = httpContext.Request.Params;

            foreach (var key in parms.AllKeys)
            {
                cst_str_s1 = "TRCSN";
                if (key.Contains(cst_str_s1))
                {
                    string value = parms[key];
                    //if (value.Contains(',')) { value = value.Replace(',', '0'); }
                    Regex patterns = new Regex("^[0-9]+$");
                    if (!patterns.IsMatch(value))
                    {
                        throw new ArgumentException("ClassTraceViewModel." + cst_str_s1);
                    }
                }

                cst_str_s1 = "OCID";
                if (key.Contains(cst_str_s1))
                {
                    string value = parms[key]; //string value = "," + parms[key];
                    //if (value.Contains(',')) { value = value.Replace(',', '0'); }
                    Regex patterns = new Regex("^[0-9]+$");
                    if (!patterns.IsMatch(value))
                    {
                        throw new ArgumentException("ClassTraceViewModel." + cst_str_s1);
                    }
                }

                cst_str_s1 = "SELECTIS";
                if (key.Contains(cst_str_s1))
                {
                    //string value = "," + parms[key].ToUpper();
                    //Regex patterns = new Regex("^(,TRUE,FALSE|,FALSE,FALSE|,FALSE)+$");
                    string value = parms[key].ToUpper();
                    Regex patterns = new Regex("^(TRUE,FALSE|FALSE)+$");
                    if (!patterns.IsMatch(value))
                    {
                        throw new ArgumentException("ClassTraceViewModel." + cst_str_s1);
                    }
                }
            }
        }

    }
    #endregion



    #region ClassTraceFormModel
    [Serializable]
    public class ClassTraceFormModel : PagingResultsViewModel
    {
        public decimal? TRC_MSN { get; set; }
    }
    #endregion

    #region ClassTraceGridModel
    [Serializable]
    public class ClassTraceGridModel
    {
        /// <summary>
        /// 選取Checkbox
        /// </summary>
        public bool SELECTIS { get; set; }

        /// <summary>
        /// 開班流水號
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 班級名稱+期別
        /// </summary>
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 報名起日期
        /// </summary>
        public DateTime? SENTERDATE { get; set; }

        /// <summary>
        /// 報名訖日期
        /// </summary>
        public DateTime? FENTERDATE { get; set; }

        /// <summary>
        /// 期別
        /// </summary>
        public string CYCLTYPE { get; set; }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 結訓日期
        /// </summary>
        public DateTime? FTDATE { get; set; }

        /// <summary>
        /// 計畫代碼
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary>
        /// 計畫類別("1":產投、"2":自辦在職)
        /// </summary>
        public string PLANTYPE { get; set; }

        /// <summary>
        /// 訓練時段代碼
        /// </summary>
        public string TPERIOD { get; set; }

        /// <summary>
        /// 訓練性質
        /// </summary>
        public Int64? TPROPERTYID { get; set; }

        /// <summary>
        /// 職訓e網不顯示
        /// </summary>
        public string EVTANOSHOW { get; set; }

        /// <summary>
        /// 是否轉入成功
        /// </summary>
        public string ISSUCCESS { get; set; }

        /// <summary>
        /// 不開班
        /// </summary>
        public string NOTOPEN { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLANNAME { get; set; }

        /// <summary>
        /// 轄區中心名稱
        /// </summary>
        public string DISTNAME { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        public string CTNAME { get; set; }

        /// <summary>
        /// 訓練地點
        /// </summary>
        public string TADDRESS { get; set; }

        /// <summary>
        /// 訓練地點-鄉鎮市區名稱
        /// </summary>
        public string ZIPNAME { get; set; }

        /// <summary>
        /// 訓練地點-郵遞區號
        /// </summary>
        public string TADDRESSZIP { get; set; }

        /// <summary>
        /// 訓練機構名稱
        /// </summary>
        public string ORGNAME { get; set; }

        /// <summary>
        /// 課程追蹤檔序號
        /// </summary>
        public Int64? TRCSN { get; set; }

        /// <summary>
        /// 是否分享
        /// </summary>
        public string ISSHARE { get; set; }

    }
    #endregion
}