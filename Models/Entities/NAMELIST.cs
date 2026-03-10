using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WDAIIP.WEB.Commons;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// [NAMELIST 名單主檔] table model
    /// </summary>
    [DisplayName("名單主檔")]
    public class NAMELIST : DBRowModel, IDBRow, IDBRowOper, IClearField
    {
        /// <summary>
        /// NAMELIST_ID 名單主檔ID【ref:SEQ_NAMELIST】
        /// </summary>
        [DisplayName("名單主檔ID")]
        [NumberValidtion(18, 0)]
        [SequenceField("SEQ_NAMELIST")]
        public Int64? NAMELIST_ID
        {
            get { return _NAMELIST_ID; }
            set { if (!this.clearField.IsContainProperty("NAMELIST_ID")) this.clearField.Add("NAMELIST_ID"); _NAMELIST_ID = value; }
        }
        private Int64? _NAMELIST_ID { get; set; }

        /// <summary>
        /// PROF_ID 報到場次類型內部ID
        /// </summary>
        [DisplayName("報到場次")]
        [Required]
        public Int64? PROF_ID
        {
            get { return _PROF_ID; }
            set { if (!this.clearField.IsContainProperty("PROF_ID")) this.clearField.Add("PROF_ID"); _PROF_ID = value; }
        }
        private Int64? _PROF_ID { get; set; }

        

        /// <summary>
        /// NAME 姓名
        /// </summary>
        [DisplayName("姓名")]
        [StringLength(20, ErrorMessage = "{0}最多{1}個字")]
        [Required]
        public string NAME
        {
            get { return _NAME; }
            set { if (!this.clearField.IsContainProperty("NAME")) this.clearField.Add("NAME"); _NAME = value; }
        }
        private string _NAME { get; set; }

        /// <summary>
        /// 名單ID (身分證字號或其他辨識用ID)
        /// </summary>
        [DisplayName("名單ID")]
        [StringLength(10, ErrorMessage = "{0}最多{1}個字")]
        [Required]
        public string IDNO
        {
            get { return _IDNO; }
            set { if (!this.clearField.IsContainProperty("IDNO")) this.clearField.Add("IDNO"); _IDNO = value; }
        }
        private string _IDNO { get; set; }

        /// <summary>
        /// SEX 性別
        /// </summary>
        [DisplayName("性別")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        public string SEX
        {
            get { return _SEX; }
            set { if (!this.clearField.IsContainProperty("SEX")) this.clearField.Add("SEX"); _SEX = value; }
        }
        private string _SEX { get; set; }

        /// <summary>
        /// BIRTHDAY 生日
        /// </summary>
        [DisplayName("生日")]
        [StringLength(8, ErrorMessage = "{0}最多{1}個字")]
        public string BIRTHDAY
        {
            get { return _BIRTHDAY; }
            set { if (!this.clearField.IsContainProperty("BIRTHDAY")) this.clearField.Add("BIRTHDAY"); _BIRTHDAY = value; }
        }
        private string _BIRTHDAY { get; set; }


        /// <summary>
        /// ZIPCODE 通訊郵遞區號
        /// </summary>
        [DisplayName("通訊郵遞區號")]
        [StringLength(5, ErrorMessage = "{0}最多{1}個字")]
        public string ZIPCODE
        {
            get { return _ZIPCODE; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE")) this.clearField.Add("ZIPCODE"); _ZIPCODE = value; }
        }
        private string _ZIPCODE { get; set; }

        /// <summary>
        /// ADDRESS 通訊地址
        /// </summary>
        [DisplayName("通訊地址")]
        [StringLength(50, ErrorMessage = "{0}最多{1}個字")]
        public string ADDRESS
        {
            get { return _ADDRESS; }
            set { if (!this.clearField.IsContainProperty("ADDRESS")) this.clearField.Add("ADDRESS"); _ADDRESS = value; }
        }
        private string _ADDRESS { get; set; }

        /// <summary>
        /// HTEL 電話(宅)【PHONE】
        /// </summary>
        [DisplayName("電話(宅)")]
        [StringLength(30, ErrorMessage = "{0}最多{1}個字")]
        public string HTEL
        {
            get { return _HTEL; }
            set { if (!this.clearField.IsContainProperty("HTEL")) this.clearField.Add("HTEL"); _HTEL = value; }
        }
        private string _HTEL { get; set; }

        /// <summary>
        /// OTEL 電話(公)【PHONE】
        /// </summary>
        [DisplayName("電話(公)")]
        [StringLength(30, ErrorMessage = "{0}最多{1}個字")]
        public string OTEL
        {
            get { return _OTEL; }
            set { if (!this.clearField.IsContainProperty("OTEL")) this.clearField.Add("OTEL"); _OTEL = value; }
        }
        private string _OTEL { get; set; }

        /// <summary>
        /// MOTEL 手機【MOBILE】
        /// </summary>
        [DisplayName("手機")]
        [StringLength(50, ErrorMessage = "{0}最多{1}個字")]
        public string MOTEL
        {
            get { return _MOTEL; }
            set { if (!this.clearField.IsContainProperty("MOTEL")) this.clearField.Add("MOTEL"); _MOTEL = value; }
        }
        private string _MOTEL { get; set; }

        /// <summary>
        /// FAX 傳真【PHONE】
        /// </summary>
        [DisplayName("傳真")]
        [StringLength(30, ErrorMessage = "{0}最多{1}個字")]
        public string FAX
        {
            get { return _FAX; }
            set { if (!this.clearField.IsContainProperty("FAX")) this.clearField.Add("FAX"); _FAX = value; }
        }
        private string _FAX { get; set; }

        /// <summary>
        /// EMAIL 電子信箱
        /// </summary>
        [DisplayName("電子信箱")]
        [StringLength(50, ErrorMessage = "{0}最多{1}個字")]
        public string EMAIL
        {
            get { return _EMAIL; }
            set { if (!this.clearField.IsContainProperty("EMAIL")) this.clearField.Add("EMAIL"); _EMAIL = value; }
        }
        private string _EMAIL { get; set; }

        /// <summary>
        /// TITLE 單位職稱
        /// </summary>
        [DisplayName("單位職稱")]
        [StringLength(100, ErrorMessage = "{0}最多{1}個字")]
        public string TITLE
        {
            get { return _TITLE; }
            set { if (!this.clearField.IsContainProperty("TITLE")) this.clearField.Add("TITLE"); _TITLE = value; }
        }
        private string _TITLE { get; set; }

        /// <summary>
        /// TCOMMENT 備註
        /// </summary>
        [DisplayName("備註")]
        [StringLength(100, ErrorMessage = "{0}最多{1}個字")]
        public string TCOMMENT
        {
            get { return _TCOMMENT; }
            set { if (!this.clearField.IsContainProperty("TCOMMENT")) this.clearField.Add("TCOMMENT"); _TCOMMENT = value; }
        }
        private string _TCOMMENT { get; set; }
        
        /// <summary>
        /// NFC_ID NFC報到綁定序號
        /// </summary>
        [DisplayName("NFC卡號")]
        [StringLength(40, ErrorMessage = "{0}最多{1}個字")]
        public string NFC_ID
        {
            get { return _NFC_ID; }
            set { if (!this.clearField.IsContainProperty("NFC_ID")) this.clearField.Add("NFC_ID"); _NFC_ID = value; }
        }
        private string _NFC_ID { get; set; }
        

        /// <summary>
        /// 回傳 Table 名稱
        /// </summary>
        DBRowTableName IDBRow.GetTableName()
        {
            return StaticCodeMap.TableName.NAMELIST;
        }
        
    }

    /// <summary>
    /// [NAMELIST 名單主檔] 擴充 model
    /// </summary>
    public class NAMELISTExt : NAMELIST
    {

        /// <summary>
        /// HTEL 電話(宅): 電話號碼 擴充資料欄位
        /// </summary>
        public PhoneNumberModel HTEL_PHONE 
        {
            get { return PhoneNumberModel.Parse(this.HTEL); }
            set { this.HTEL = value.ToString(); }
        }

        /// <summary>
        /// OTEL 電話(公): 電話號碼 擴充資料欄位
        /// </summary>
        public PhoneNumberModel OTEL_PHONE 
        {
            get { return PhoneNumberModel.Parse(this.OTEL); }
            set { this.OTEL = value.ToString(); }
        }

        /// <summary>
        /// MOTEL 手機: 手機號碼 擴充資料欄位
        /// </summary>
        public MobileNumberModel MOTEL_MOBILE 
        {
            get { return MobileNumberModel.Parse(this.MOTEL); }
            set { this.MOTEL = value.ToString(); }
        }

        /// <summary>
        /// FAX 傳真: 電話號碼 擴充資料欄位
        /// </summary>
        public PhoneNumberModel FAX_PHONE 
        {
            get { return PhoneNumberModel.Parse(this.FAX); }
            set { this.FAX = value.ToString(); }
        }

    }
}

