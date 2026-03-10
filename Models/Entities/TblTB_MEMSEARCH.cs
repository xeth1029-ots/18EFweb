using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    public class TblTB_MEMSEARCH : IDBRow
    {
        /// <summary>會員序號 (Ref:E_member.mem_sn) </summary>
        public decimal? MEM_SN { get; set; }

        /// <summary> 策略性產業別名稱字串 </summary>
        //public string TMIDNAME { get; set; }

        /// <summary>訓練業別ID字串</summary>
        public string TMID { get; set; }

        /// <summary> 縣市別名稱字串 </summary>
        //public string CTNAME { get; set; }

        /// <summary> 縣市別ID字串</summary>
        public string CTID { get; set; }

        /// <summary> 分署別名稱字串 </summary>
        //public string DISTNAME { get; set; }

        /// <summary> 分署別ID字串 </summary>
        public string DISTID { get; set; }

        /// <summary> 通俗職類名稱字串</summary>
        //public string CJOBNAME { get; set; }

        /// <summary> 通俗職類ID字串 </summary>
        public string CJOBNO { get; set; }

        /// <summary> 是否要發送通知郵件(for自辦在職) </summary>
        public string SENDMAIL06 { get; set; }

        /// <summary>是否要發送通知郵件(for產投)</summary>
        public string SENDMAIL28 { get; set; }

        /// <summary>是否要發送通知郵件(for區域產業據點)</summary>
        public string SENDMAIL70 { get; set; }

        /// <summary>建檔時間</summary>
        public DateTime? CREATEDATE { get; set; }

        /// <summary> 異動時間 </summary>
        public DateTime? MODIFYDATE { get; set; }

        /// <summary> 通俗職類ID字串 2019</summary>
        public string CJOBNO_19 { get; set; }
        
        /// <summary> 能寄信Y </summary>
        public string CANSEND { get; set; }

        /// <summary> 異動時間2019 </summary>
        public DateTime? MODIFYDATE19 { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.TB_MEMSEARCH;
        }
    }
}