using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    public class TblSTDALL : IDBRow
    {
        public Int64? STDID { get; set; }
        public string YEARS { get; set; }
        public string DISTID{ get; set; }
        public string DISTNAME { get; set; }
        public string COSUNIT { get; set; }
        public string TRINUNIT { get; set; }
        public string TPLANID { get; set; }
        public string PLANNAME { get; set; }
        public string CLASSNAME { get; set; }
        public DateTime? SDATE { get; set; }
        public DateTime? EDATE { get; set; }
        public string NAME { get; set; }
        public string SID { get; set; }
        public DateTime? BIRTH { get; set; }
        public string SEX { get; set; }
        public string IDENT { get; set; }
        public string ADDR { get; set; }
        public string TEL { get; set; }
        public DateTime? TRANDATE { get; set; }
        public string MODIFYACCT { get; set; }
        public DateTime? MODIFYDATE { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.STDALL;
        }
    }
}