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
    /// TB_VIEWRECORD 網站課程瀏覽記錄
    /// </summary>
    [Table("TB_VIEWRECORD")]
    [DisplayName("TblTB_VIEWRECORD")]
    public class TblTB_VIEWRECORD : IDBRow
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public Int64? SEQNO { get; set; }

        /// <summary>
        /// 計畫代碼(ref:KEY_PLAN.TPLANID)
        /// </summary>
        public string TPLANID { get; set; }

        /// <summary>
        /// 班別代碼
        /// </summary>
        public Int64? OCID { get; set; }

        /// <summary>
        /// 使用者IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 瀏覽時間
        /// </summary>
        public DateTime? VIEWTIME { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.TB_VIEWRECORD;
        }
    }
}