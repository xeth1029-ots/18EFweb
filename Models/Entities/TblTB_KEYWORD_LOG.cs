using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    public class TblTB_KEYWORD_LOG : IDBRow
    {
        /// <summary>
        /// KEYWORD LOG ID (流水號)
        /// </summary>
        public decimal? KWLID { get; set; }

        /// <summary>
        /// 對應CODE1.ITEM=KWLTYPE,  1:Q&A, 2:課程查詢, 3:歷史課程查詢
        /// </summary>
        public string KWLTYPE { get; set; }

        /// <summary>
        /// 子類別ID (Q&A功能：對應[CODE1].ITEM='QATYPE'	課程查詢功能：對應[CODE1].ITEM='CSPLAN'	NULL：不分類別)
        /// </summary>
        public string SUB_TYPEID { get; set; }

        /// <summary>
        /// 關鍵字搜尋
        /// </summary>
        public string KEYWORD { get; set; }

        /// <summary>
        /// 使用者IP
        /// </summary>
        public string USERIP { get; set; }

        /// <summary>
        /// 查詢時間
        /// </summary>
        public DateTime? SEARCHDATE { get; set; }


        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.TB_KEYWORD_LOG;
        }
    }
}