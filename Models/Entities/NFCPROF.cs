using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WDAIIP.WEB.Commons;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// [NFCPROF 報到場次類型] table model
    /// </summary>
    [Table("NFCPROF")]
    [DisplayName("報到場次類型")]
    public class NFCPROF : DBRowModel, IDBRow, IDBRowOper, IClearField
    {
        /// <summary>
        /// PROF_ID 報到場次內部ID
        /// </summary>
        [DisplayName("報到場次內部ID")]
        [SequenceField("SEQ_NFCPROF")]
        public Int64? PROF_ID
        {
            get { return _PROF_ID; }
            set { if (!this.clearField.IsContainProperty("PROF_ID")) this.clearField.Add("PROF_ID"); _PROF_ID = value; }
        }
        private Int64? _PROF_ID { get; set; }

        /// <summary>
        /// PNAME 場次名稱
        /// </summary>
        [DisplayName("場次名稱")]
        public string PNAME
        {
            get { return _PNAME; }
            set { if (!this.clearField.IsContainProperty("PNAME")) this.clearField.Add("PNAME"); _PNAME = value; }
        }
        private string _PNAME { get; set; }


        /// <summary>
        /// DATES 場次起始日YYYYMMDD
        /// </summary>
        [DisplayName("場次起始日")]
        [StringLength(8, ErrorMessage = "{0} 格式不正確")]
        public string DATES
        {
            get { return _DATES; }
            set { if (!this.clearField.IsContainProperty("DATES")) this.clearField.Add("DATES"); _DATES = value; }
        }
        private string _DATES { get; set; }

        /// <summary>
        /// DATEE 場次結束日YYYYMMDD
        /// </summary>
        [DisplayName("場次結束日")]
        [StringLength(8, ErrorMessage = "{0} 格式不正確")]
        public string DATEE
        {
            get { return _DATEE; }
            set { if (!this.clearField.IsContainProperty("DATEE")) this.clearField.Add("DATEE"); _DATEE = value; }
        }
        private string _DATEE { get; set; }


        /// <summary>
        /// MEMO 備註
        /// </summary>
        [DisplayName("備註")]
        public string MEMO
        {
            get { return _MEMO; }
            set { if (!this.clearField.IsContainProperty("MEMO")) this.clearField.Add("MEMO"); _MEMO = value; }
        }
        private string _MEMO { get; set; }


        /// <summary>
        /// STATUS 狀態: 1.有效, 0;停用
        /// </summary>
        [DisplayName("狀態")]
        public string STATUS
        {
            get { return _STATUS; }
            set { if (!this.clearField.IsContainProperty("STATUS")) this.clearField.Add("STATUS"); _STATUS = value; }
        }
        private string _STATUS { get; set; }


        /// <summary>
        /// 系統別代碼
        /// </summary>
        [DisplayName("系統別代碼")]
        public string SYS_ID
        {
            get { return _SYS_ID; }
            set { if (!this.clearField.IsContainProperty("SYS_ID")) this.clearField.Add("SYS_ID"); _SYS_ID = value; }
        }
        private string _SYS_ID { get; set; }

        /// <summary>
        /// 所屬系統場次代碼
        /// </summary>
        [DisplayName("所屬系統場次代碼")]
        [Required]
        [StringLength(20, ErrorMessage = "{0} 最多只能輸入20個字元")]
        public string SYS_PROF_ID
        {
            get { return _SYS_PROF_ID; }
            set { if (!this.clearField.IsContainProperty("SYS_PROF_ID")) this.clearField.Add("SYS_PROF_ID"); _SYS_PROF_ID = value; }
        }
        private string _SYS_PROF_ID { get; set; }

        /// <summary>
        /// 報到時IDNO是否隱碼: Y.是, N.否
        /// </summary>
        [DisplayName("報到時IDNO隱碼")]
        public string IDNO_MASK
        {
            get;
            set;
        }

        /// <summary>
        /// 回傳 Table 名稱
        /// </summary>
        DBRowTableName IDBRow.GetTableName()
        {
            return StaticCodeMap.TableName.NFCPROF;
        }
        
    }

    /// <summary>
    /// [NFCPROF 報到場次類型] 擴充 model
    /// </summary>
    public class NFCPROFExt : NFCPROF
    {

    }
}

