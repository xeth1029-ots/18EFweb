using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    public class TblTB_QA : IDBRow
    {
        /// <summary>
        /// QA流水號
        /// </summary>
        public decimal? QAID { get; set; }

        /// <summary>
        /// 問題類型流水號 (ref:code1.item='QATYPE')
        /// </summary>
        public string TYPEID { get; set; }

        /// <summary>
        /// 問題內容
        /// </summary>
        public string QUESTION { get; set; }

        /// <summary>
        /// 回答內容
        /// </summary>
        public string ANSWER { get; set; }

        /// <summary>
        /// 上架起日
        /// </summary>
        public DateTime? START_DATE { get; set; }

        /// <summary>
        /// 上架迄日
        /// </summary>
        public DateTime? END_DATE { get; set; }

        /// <summary>
        /// 啟用狀態 (Y 啟用/N 停用)
        /// </summary>
        public DateTime? ISUSED { get; set; }

        /// <summary>
        /// 關鍵字
        /// </summary>
        public string KEYWORD { get; set; }

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
            return StaticCodeMap.TableName.TB_QA;
        }
    }
}