using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WDAIIP.WEB.Commons;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// [CHECKIN 報到記錄檔] table model
    /// </summary>
    [DisplayName("報到檔")]
    public class CHECKIN : DBRowModel, IDBRow, IDBRowOper, IClearField
    {

        /// <summary>
        /// CHECKIN_ITEM_ID 報到項目檔內部ID 【ref: TES_CHECKIN_ITEM.CHECKIN_ITEM_ID】
        /// </summary>
        [DisplayName("報到項目檔內部ID")]
        [NumberValidtion(18, 0)]
        public Int64? CHECKIN_ITEM_ID
        {
            get { return _CHECKIN_ITEM_ID; }
            set { if (!this.clearField.IsContainProperty("CHECKIN_ITEM_ID")) this.clearField.Add("CHECKIN_ITEM_ID"); _CHECKIN_ITEM_ID = value; }
        }
        private Int64? _CHECKIN_ITEM_ID { get; set; }

        /// <summary>
        /// NAMELIST_ID 名單主檔ID 【ref: NAMELIST.NAMELIST_ID】
        /// </summary>
        [DisplayName("名單主檔ID")]
        [NumberValidtion(18, 0)]
        public Int64? NAMELIST_ID
        {
            get { return _NAMELIST_ID; }
            set { if (!this.clearField.IsContainProperty("NAMELIST_ID")) this.clearField.Add("NAMELIST_ID"); _NAMELIST_ID = value; }
        }
        private Int64? _NAMELIST_ID { get; set; }

        /// <summary>
        /// DATETIME 報到時間 yyyyMMddHH24MISS
        /// </summary>
        [DisplayName("報到時間")]
        [StringLength(14, ErrorMessage = "{0}最多{1}個字")]
        public string DATETIME
        {
            get { return _DATETIME; }
            set { if (!this.clearField.IsContainProperty("DATETIME")) this.clearField.Add("DATETIME"); _DATETIME = value; }
        }
        private string _DATETIME { get; set; }

        /// <summary>
        /// CHECKIN_TYPE 報到類型【ENUM:A-RFID報到,B-人工報到,C-取消報到】
        /// </summary>
        [DisplayName("報到類型")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        public string CHECKIN_TYPE
        {
            get { return _CHECKIN_TYPE; }
            set { if (!this.clearField.IsContainProperty("CHECKIN_TYPE")) this.clearField.Add("CHECKIN_TYPE"); _CHECKIN_TYPE = value; }
        }
        private string _CHECKIN_TYPE { get; set; }

        /// <summary>
        /// 回傳 Table 名稱
        /// </summary>
        DBRowTableName IDBRow.GetTableName()
        {
            return StaticCodeMap.TableName.CHECKIN;
        }
        
    }

    /// <summary>
    /// [CHECKIN 報到檔] 擴充 model
    /// </summary>
    public class CHECKINExt : CHECKIN
    {

        /// <summary>
        /// CHECKIN_TYPE 報到類型: 參數值擴充欄位
        /// </summary>
        public enum CHECKIN_TYPE_ENUM 
        {
            /// <summary>
            /// RFID報到
            /// </summary>
            A = 0,

            /// <summary>
            /// 人工報到
            /// </summary>
            B = 1,

            /// <summary>
            /// 取消報到
            /// </summary>
            C = 2
        }

    }
}

