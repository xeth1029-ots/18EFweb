using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    [Serializable]
    public class TblSTUD_ELFORM : IDBRow
    {
        public long? CSELNO { get; set; }
        public long? ELNO { get; set; }
        public long? SOCID { get; set; }
        public long? OCID { get; set; }
        public string IDNO { get; set; }
        public string P1_LINK { get; set; }
        public string FILEPATH1 { get; set; }
        public string CREATEACCT { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public string SIGNDACCT { get; set; }
        public DateTime? SIGNDATE { get; set; }
        public string SENDACCT { get; set; }
        public DateTime? SENDDATE { get; set; }
        public string MODIFYACCT { get; set; }
        public DateTime? MODIFYDATE { get; set; }

        /// <summary>預設建構子</summary>
        public TblSTUD_ELFORM() { }

        /// <summary>傳回資料表名稱</summary>
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.STUD_ELFORM;
        }
    }

}