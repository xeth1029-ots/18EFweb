using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    #region NewsViewModel
    public class NewsViewModel
    {
        public NewsViewModel()
        {
            this.Form = new NewsFormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public NewsFormModel Form { get; set; }

        /// <summary>
        /// 查詢結果清單 GridModel
        /// </summary>
        public IList<NewsGridModel> Grid { get; set; }

        /// <summary>
        /// 明細頁 DetailModel
        /// </summary>
        public NewsDetailModel Detail { get; set; }
    }
    #endregion

    #region NewsFormModel
    public class NewsFormModel : PagingResultsViewModel
    {

    }
    #endregion

    #region NewsGridModel
    public class NewsGridModel : TblTB_CONTENT
    {
        /// <summary>
        /// 排序序號
        /// </summary>
        public Int64? SEQ { get; set; }

        /// <summary>
        /// (西元年yyyy/MM/dd)
        /// </summary>
        public string C_SDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.C_SDATE); }
        }
    }
    #endregion

    #region NewsDetailModel
    public class NewsDetailModel : TblTB_CONTENT
    {
        /// <summary>
        /// (西元年yyyy/MM/dd)
        /// </summary>
        public string C_SDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.C_SDATE); }
        }
    }
    #endregion

}