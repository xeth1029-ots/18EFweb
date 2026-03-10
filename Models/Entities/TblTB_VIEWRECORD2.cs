using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// TB_VIEWRECORD2 瀏覽點擊紀錄資料表
    /// </summary>
    [Table("TB_VIEWRECORD2")]
    [DisplayName("TblTB_VIEWRECORD2")]
    public class TblTB_VIEWRECORD2 : IDBRow
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public Int64? SEQNO { get; set; }

        /// <summary>
        /// 使用者IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 瀏覽點擊時間
        /// </summary>
        public DateTime? CLICKTIME { get; set; }

        /// <summary>
        /// 瀏覽點擊種類 1:新冠肺炎點擊
        /// </summary>
        public Int64? CLICKTYPE { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.TB_VIEWRECORD2;
        }
    }
}