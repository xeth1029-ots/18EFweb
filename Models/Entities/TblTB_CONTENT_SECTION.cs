using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary> TB_CONTENT_SECTION 最新消息分段內容資料表 </summary>
    public class TblTB_CONTENT_SECTION : IDBRow
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public decimal? SEQNO { get; set; }

        /// <summary>
        /// 主內容ID (ref:TB_CONTENT.SEQNO)
        /// </summary>
        public decimal? CONTENTID { get; set; }

        /// <summary>
        /// 分段編號 (1、2、3、4，目前共分4區塊)
        /// </summary>
        public string SEC_NO { get; set; }

        /// <summary>
        /// 分段內容
        /// </summary>
        public string SEC_CONTENT { get; set; }

        /// <summary>
        /// 分段圖檔路徑名稱
        /// </summary>
        public string SEC_PICTURE { get; set; }

        /// <summary>
        /// 分段圖檔提示文字
        /// </summary>
        public string SEC_PICTURE_ALT { get; set; }

        /// <summary>
        /// 圖檔對齊方式 (L：靠左、R：靠右、I：文繞圖)
        /// </summary>
        public string ALIGN_TYPE { get; set; }

        /// <summary>
        /// 異動者
        /// </summary>
        public string MODIFYACCT { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        public DateTime? MODIFYDATE { get; set; }

        public DBRowTableName GetTableName()
        {
            throw new NotImplementedException();
        }
    }
}