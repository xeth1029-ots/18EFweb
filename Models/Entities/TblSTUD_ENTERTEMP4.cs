using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    public class TblSTUD_ENTERTEMP4 : IDBRow
    {
        public long? ESETID4 { get; set; }
        public string IDNO { get; set; }
        public string NAME { get; set; }
        public string CONTACTPHONE { get; set; }
        public int? ZIPCODE1 { get; set; }
        public string ZIPCODE1_2W { get; set; }
        public string ZIPCODE1_6W { get; set; }
        public long? ZIPCODE1_N { get; set; }
        public string ADDRESS { get; set; }
        public string ISAGREE { get; set; }
        public string MODIFYACCT { get; set; }
        public DateTime? MODIFYDATE { get; set; }

        /// <summary>預設建構子</summary>
        public TblSTUD_ENTERTEMP4() { }

        /// <summary>傳回資料表名稱</summary>
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.STUD_ENTERTEMP4;
        }
    }
}
