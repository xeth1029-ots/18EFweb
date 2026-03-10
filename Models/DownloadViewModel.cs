using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Commons;
using Turbo.Commons;


namespace WDAIIP.WEB.Models
{
    #region DownloadViewModel

    public class DownloadViewModel
    {
        public DownloadViewModel()
        {
            this.Form = new DownloadFormModel();
        }

        /// <summary>
        /// 查詢條件 FormModel
        /// </summary>
        public DownloadFormModel Form { get; set; }

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<DownloadGridModel> Grid { get; set; }

        /// <summary>
        /// 編輯明細頁 DetailModel
        /// </summary>
        public DownloadDetailModel Detail { get; set; }

        /// <summary>
        /// Download的計畫別中文名稱
        /// </summary>
        public string DLPlanName { get; set; }

        /// <summary>
        /// Download的計畫別清單 GridModel
        /// </summary>
        public IList<DownloadTypeGridModel> DownloadType_list { get; set; }
    }

    #endregion

    #region DownloadFormModel

    public class DownloadFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 下載類別
        /// </summary>
        public string KINDID { get; set; }

        /// <summary>
        /// 計畫類別
        /// </summary>
        public string PLANID { get; set; }
    }

    #endregion

    #region DownloadGridModel

    public class DownloadGridModel : TblTB_DLFILE
    {
        /// <summary>
        /// 下載類別名稱
        /// </summary>
        public string KIND_NAME { get; set; }

        /// <summary>
        /// 計畫類別名稱
        /// </summary>
        public string PLAN_NAME { get; set; }

        /// <summary>
        /// 檔案1所對應的「檔案圖示」
        /// </summary>
        public string FILE1_IMG
        {
            get
            {
                return MyHelperUtil.GetFileIcon(this.FILE1_TYPE);
            }
        }

        /// <summary>
        /// 檔案2所對應的「檔案圖示」
        /// </summary>
        public string FILE2_IMG
        {
            get
            {
                return MyHelperUtil.GetFileIcon(this.FILE2_TYPE);
            }
        }

        /// <summary>
        /// 上架日-起 (民國年)
        /// </summary>
        public string START_DATE_TW
        {
            get
            {
                return (this.START_DATE.HasValue ? HelperUtil.TransToTwYear(this.START_DATE.Value) : "");
            }
        }
    }

    #endregion

    #region DownloadTypeGridModel

    public class DownloadTypeGridModel
    {
        /// <summary>
        /// 計畫類別代碼
        /// </summary>
        public string CODE { get; set; }

        /// <summary>
        /// 計畫類別名稱
        /// </summary>
        public string TEXT { get; set; }
    }

    #endregion

    #region DownloadDetailModel

    public class DownloadDetailModel
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public DownloadDetailModel()
        {
        }

        #region 資料表欄位

        /// <summary>下載流水號</summary>
        public Decimal? DLID { get; set; }

        /// <summary>下載類別</summary>
        public string KINDID { get; set; }

        /// <summary>計畫類別</summary>
        public string PLANID { get; set; }

        /// <summary>下載標題</summary>
        public string DLTITLE { get; set; }

        /// <summary>檔名1</summary>
        public string FILE1_NAME { get; set; }

        /// <summary>檔案類型1</summary>
        public string FILE1_TYPE { get; set; }

        /// <summary>檔案大小1</summary>
        public string FILE1_SIZE { get; set; }

        /// <summary>檔名2</summary>
        public string FILE2_NAME { get; set; }

        /// <summary>檔案類型2</summary>
        public string FILE2_TYPE { get; set; }

        /// <summary>檔案大小2</summary>
        public string FILE2_SIZE { get; set; }

        /// <summary>上架日-起</summary>
        public DateTime? START_DATE { get; set; }

        /// <summary>上架日-迄</summary>
        public DateTime? END_DATE { get; set; }

        /// <summary>下載次數</summary>
        public int? DLCOUNT { get; set; }

        /// <summary>上傳日期</summary>
        public DateTime? UPLOADDATE { get; set; }

        /// <summary>啟用</summary>
        public string ISUSED { get; set; }

        /// <summary>說明</summary>
        public string MEMO { get; set; }

        /// <summary>異動者</summary>
        public string MODIFYACCT { get; set; }

        /// <summary>異動時間</summary>
        public DateTime? MODIFYDATE { get; set; }

        #endregion
    }

    #endregion
}