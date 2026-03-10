using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    public class TblHISTORY_STUDENTINFO93 : IDBRow
    {
        public Int64? SERIAL { get; set; }
        public string DISTID { get; set; }
        public string DISTNAME { get; set; }
        public string COSUNIT { get; set; }
        public string TRINUNIT { get; set; }
        public string ORGKIND { get; set; }
        public string TPLANID { get; set; }
        public string PLANNAME { get; set; }
        public string TMID { get; set; }
        public string CLASSNAME { get; set; }
        public DateTime? SDATE { get; set; }
        public DateTime? EDATE { get; set; }
        public string NAME { get; set; }
        public string IDNO { get; set; }
        public DateTime? BIRTH { get; set; }
        public string SEX { get; set; }
        public string IDENT { get; set; }
        public string DEGREEID { get; set; }
        public string ZIPCODE { get; set; }
        public string ADDR { get; set; }
        public string TEL { get; set; }
        public string TPROPERTYID { get; set; }
        public string JOBLESSID { get; set; }
        public Int16 ISREAD { get; set; }
        public Int16 ONETHREE { get; set; }
        public Int16 FOURSIX { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.HISTORY_STUDENTINFO93;
        }
    }
}