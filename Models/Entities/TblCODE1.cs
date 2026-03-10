using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    public class TblCODE1 : IDBRow
    {
        /// <summary>
        /// 項目集代碼
        /// </summary>
        public string ITEM { get; set; }

        /// <summary>
        /// 項目代碼
        /// </summary>
        public string CODE { get; set; }

        /// <summary>
        /// 項目名稱
        /// </summary>
        public string DESCR { get; set; }

        /// <summary>
        /// 啟用狀態
        /// </summary>
        public string HIST { get; set; }

        /// <summary>
        /// 顯示順序
        /// </summary>
        public Int64? FREQ { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AUX1 { get; set; }

        /// <summary>
        /// 異動者帳號
        /// </summary>
        public string MODUSERID { get; set; }
        
        /// <summary>
        /// 異動者姓名
        /// </summary>
        public string MODUSERNAME { get; set; }

        /// <summary>
        /// 異動IP
        /// </summary>
        public string MODIP { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        public string MODTIME { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.CODE1;
        }
    }
}