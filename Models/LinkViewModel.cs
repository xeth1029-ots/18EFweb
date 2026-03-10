using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Models.Entities;


namespace WDAIIP.WEB.Models
{
    #region LinkViewModel

    public class LinkViewModel
    {
        public LinkViewModel()
        {
            this.Form = new LinkFormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public LinkFormModel Form { get; set; }

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<LinkGridModel> Grid { get; set; }
    }

    #endregion

    #region LinkFormModel

    public class LinkFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// [FUNID]
        /// </summary>
        public string FUNID { get; set; }

        /// <summary>
        /// [SUB_FUNID]
        /// </summary>
        public string SUB_FUNID { get; set; }

        /// <summary>
        /// 序號
        /// </summary>
        public int? SEQNO { get; set; }
    }

    #endregion

    #region LinkGridModel

    public class LinkGridModel : TblTB_CONTENT
    {
    }

    #endregion
}