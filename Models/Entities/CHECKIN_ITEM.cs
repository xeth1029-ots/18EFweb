using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WDAIIP.WEB.Commons;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// [CHECKIN_ITEM 報到項目檔] table model
    /// </summary>
    [DisplayName("報到項目檔")]
    public class CHECKIN_ITEM : DBRowModel, IDBRow, IDBRowOper, IClearField
    {
        /// <summary>
        /// CHECKIN_ITEM_ID 內部ID
        /// </summary>
        [DisplayName("內部ID")]
        [SequenceField("SEQ_CHECKIN_ITEM")]
        public Int64? CHECKIN_ITEM_ID
        {
            get { return _CHECKIN_ITEM_ID; }
            set { if (!this.clearField.IsContainProperty("CHECKIN_ITEM_ID")) this.clearField.Add("CHECKIN_ITEM_ID"); _CHECKIN_ITEM_ID = value; }
        }
        private Int64? _CHECKIN_ITEM_ID { get; set; }

        /// <summary>
        /// PROF_ID 報到場次_ID 【ref:NFCPROF】
        /// </summary>
        [DisplayName("報到場次_ID")]
        [NumberValidtion(18, 0)]
        public Int64? PROF_ID
        {
            get { return _PROF_ID; }
            set { if (!this.clearField.IsContainProperty("PROF_ID")) this.clearField.Add("PROF_ID"); _PROF_ID = value; }
        }
        private Int64? _PROF_ID { get; set; }

        /// <summary>
        /// ITEM_NAME 項目名稱
        /// </summary>
        [DisplayName("項目名稱")]
        [StringLength(40, ErrorMessage = "{0}最多{1}個字")]
        public string ITEM_NAME
        {
            get { return _ITEM_NAME; }
            set { if (!this.clearField.IsContainProperty("ITEM_NAME")) this.clearField.Add("ITEM_NAME"); _ITEM_NAME = value; }
        }
        private string _ITEM_NAME { get; set; }

        /// <summary>
        /// SEQ 排序序號
        /// </summary>
        [DisplayName("排序序號")]
        [NumberValidtion(18, 0)]
        public Int64? SEQ
        {
            get { return _SEQ; }
            set { if (!this.clearField.IsContainProperty("SEQ")) this.clearField.Add("SEQ"); _SEQ = value; }
        }
        private Int64? _SEQ { get; set; }

        /// <summary>
        /// START_TIME 報到起始時間 HHMM
        /// </summary>
        [DisplayName("報到起始時間")]
        [StringLength(4, ErrorMessage = "{0} 格式不符")]
        public string START_TIME
        {
            get { return _START_TIME; }
            set { if (!this.clearField.IsContainProperty("START_TIME")) this.clearField.Add("START_TIME"); _START_TIME = value; }
        }
        private string _START_TIME { get; set; }

        /// <summary>
        /// END_TIME 報到終止時間 HHMM
        /// </summary>
        [DisplayName("報到終止時間")]
        [StringLength(4, ErrorMessage = "{0} 格式不符")]
        public string END_TIME
        {
            get { return _END_TIME; }
            set { if (!this.clearField.IsContainProperty("END_TIME")) this.clearField.Add("END_TIME"); _END_TIME = value; }
        }
        private string _END_TIME { get; set; }

        /// <summary>
        /// 報到日期yyyyMMdd
        /// </summary>
        [DisplayName("報到日期")]
        [StringLength(8, ErrorMessage = "{0} 格式不符")]
        [Required]
        public string ITEM_DATE
        {
            get { return _ITEM_DATE; }
            set { if (!this.clearField.IsContainProperty("ITEM_DATE")) this.clearField.Add("ITEM_DATE"); _ITEM_DATE = value; }
        }
        private string _ITEM_DATE { get; set; }


        /// <summary>
        /// 所屬系統報到項目代碼
        /// </summary>
        [DisplayName("所屬系統報到項目代碼")]
        [StringLength(20, ErrorMessage = "{0}最多{1}個字")]
        [Required]
        public string SYS_ITEM_ID
        {
            get { return _SYS_ITEM_ID; }
            set { if (!this.clearField.IsContainProperty("SYS_ITEM_ID")) this.clearField.Add("SYS_ITEM_ID"); _SYS_ITEM_ID = value; }
        }
        private string _SYS_ITEM_ID { get; set; }

        /// <summary>
        /// 回傳 Table 名稱
        /// </summary>
        DBRowTableName IDBRow.GetTableName()
        {
            return StaticCodeMap.TableName.CHECKIN_ITEM;
        }
        
    }

    /// <summary>
    /// [CHECKIN_ITEM 報到項目檔] 擴充 model
    /// </summary>
    public class CHECKIN_ITEMExt : CHECKIN_ITEM
    {
    }
}

