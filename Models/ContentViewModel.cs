using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Models
{
    #region ContentViewModel
    public class ContentViewModel
    {
        public ContentViewModel()
        {
            this.Form = new ContentFormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public ContentFormModel Form { get; set; }

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<ContentGridModel> Grid { get; set; }

        /// <summary>
        /// 明細頁 DetailModel
        /// </summary>
        public ContentDetailModel Detail { get; set; }

    }
    #endregion

    #region ContentFormModel
    public class ContentFormModel : PagingResultsViewModel
    {

    }
    #endregion

    #region ContentGridModel
    public class ContentGridModel : TblTB_CONTENT
    {
        /// <summary>
        /// 排序序號
        /// </summary>
        public Int64? SEQ { get; set; }

        /// <summary>
        /// 上架日 (西元年 yyyy/MM/dd)
        /// </summary>
        public string C_SDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.C_SDATE); }
        }

        /// <summary>
        /// 上架日 (民國年 yyy/MM/dd)
        /// </summary>
        public string C_SDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.C_SDATE); }
        }
    }
    #endregion

    #region ContentDetailModel
    public class ContentDetailModel : TblTB_CONTENT
    {
        /// <summary>
        /// (西元年yyyy/MM/dd)
        /// </summary>
        public string C_SDATE_AD
        {
            get { return MyHelperUtil.DateTimeToString(this.C_SDATE); }
        }

        /// <summary>
        /// (民國年 yyy/MM/dd)
        /// </summary>
        public string C_SDATE_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.C_SDATE); }
        }

        /// <summary>
        /// 分段內容資訊
        /// </summary>
        public IList<TblTB_CONTENT_SECTION> SECTIONGrid { get; set; }

        /// <summary>
        /// 附件下載資訊
        /// </summary>
        public IList<FileGridModel> FILEGrid { get; set; }
    }
    #endregion

    #region FileGridModel
    /// <summary>
    /// 附件清單 GridModel
    /// </summary>
    public class FileGridModel : TblTB_FILE
    {
        /// <summary>
        /// 檔案標題
        /// </summary>
        public string FILE_TITLE
        {
            get
            {
                string rtn = string.Empty;
                if (!string.IsNullOrEmpty(this.FILE_ORINAME))
                {
                    rtn = this.FILE_ORINAME.Substring(0, this.FILE_ORINAME.LastIndexOf("."));
                }
                return rtn;
            }
        }

        /// <summary>
        /// 附檔圖示
        /// </summary>
        public string FILE_IMG
        {
            get
            {
                return MyHelperUtil.GetFileIcon(this.FILE_TYPE);
            }
        }
    }
    #endregion
}