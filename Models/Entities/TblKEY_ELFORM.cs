using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;


namespace WDAIIP.WEB.Models
{
    public class TblKEY_ELFORM : IDBRow
    {
        [Column("ELNO")]
        public long? ELNO { get; set; }

        [Column("ELID")]
        public string ELID { get; set; }

        [Column("ENAME")]
        public string ENAME { get; set; }

        [Column("EDESC")]
        public string EDESC { get; set; }

        [Column("ESORT")]
        public long? ESORT { get; set; }

        /// <summary>預設建構子</summary>
        public TblKEY_ELFORM() { }

        /// <summary>傳回資料表名稱</summary>
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.KEY_ELFORM;
        }

    }
}