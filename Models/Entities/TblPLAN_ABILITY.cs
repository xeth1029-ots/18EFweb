using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>專長能力標籤-PLAN_ABILITY</summary>
    public class TblPLAN_ABILITY : IDBRow
    {
        public decimal? PABID { get; set; }

        /// <summary>計畫代碼</summary>
        public Decimal? PLANID { get; set; }

        /// <summary>廠商統一編號</summary>
        public string COMIDNO { get; set; }

        /// <summary>序號 (PlanID and ComIDNO相同時加一否則從一開始)</summary>
        public Decimal? SEQNO { get; set; }
        public decimal? SEQ_ID { get; set; }
        public string ABILITY { get; set; }
        public string ABILITY_DESC { get; set; }
        public string MODIFYACCT { get; set; }
        public DateTime MODIFYDATE { get; set; }

        /// <summary>傳回資料表名稱</summary>
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.PLAN_ABILITY;
        }
    }
}