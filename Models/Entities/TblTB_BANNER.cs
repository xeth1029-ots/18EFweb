using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    public class TblTB_BANNER : IDBRow
    {
        /// <summary>
        /// Banner流水號
        /// </summary>
        public decimal? BANNERID { get; set; }

        /// <summary>
        /// 分類代碼 (B01 首頁上方大 ,B02 首頁下方小)
        /// </summary>
        public string TYPEID { get; set; }

        /// <summary>
        /// Banner標題
        /// </summary>
        public string B_TITLE { get; set; }

        /// <summary>
        /// Banner內容說明
        /// </summary>
        public string B_CONTENT { get; set; }

        /// <summary>
        /// 連結網址
        /// </summary>
        public string B_URL { get; set; }

        /// <summary>
        /// 圖片替代文字
        /// </summary>
        public string B_ALT { get; set; }

        /// <summary>
        /// 圖檔路徑名稱
        /// </summary>
        public string FILE_NAME { get; set; }

        /// <summary>
        /// 原檔名
        /// </summary>
        public string ORG_FILE_NAME { get; set; }

        /// <summary>
        /// 上架起日
        /// </summary>
        public DateTime? START_DATE { get; set; }

        /// <summary>
        /// 上架迄日
        /// </summary>
        public DateTime? END_DATE { get; set; }

        /// <summary>
        /// 啟用狀態 (Y 啟用 / N 停用)
        /// </summary>
        public string ISUSED { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public string CREATEACCT { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime? CREATEDATE { get; set; }

        /// <summary>
        /// 修改者
        /// </summary>
        public string MODIFYACCT { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        public DateTime? MODIFYDATE { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public Int64? SEQ { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.TB_BANNER;
        }
    }
}