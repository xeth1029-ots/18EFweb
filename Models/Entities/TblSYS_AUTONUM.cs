using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// sys_autonum 系統資料表Pk序號記錄表
    /// </summary>
    public class TblSYS_AUTONUM : IDBRow
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public TblSYS_AUTONUM() { }

        /// <summary>
        /// 資料表序號名稱
        /// </summary>
        public string TABLENAME { get; set; }

        /// <summary>
        /// 資料表pk field current value
        /// </summary>
        public Int64? CURVAL { get; set; }

        /// <summary>
        /// 資料異動時間
        /// </summary>
        public DateTime? MTIME { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.SYS_AUTONUM;
        }
    }
}