using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WDAIIP.WEB.Commons;
using Turbo.Commons;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// [CLAMFUNCM 系統程式資料檔] table model
    /// </summary>
    [Table("CLAMFUNCM")]
    [DisplayName("系統程式資料檔")]
    public class TblCLAMFUNCM : DBRowModel, IDBRow, IDBRowOper, IClearField
    {
        /// <summary>
        /// SYS_ID 系統代號
        /// </summary>
        [DisplayName("系統代號")]
        [StringLength(4, ErrorMessage = "{0}最多{1}個字")]
        [Key]
        public string SYS_ID
        {
            get { return _SYS_ID; }
            set { if(!this.clearField.IsContainProperty("SYS_ID")) this.clearField.Add("SYS_ID"); _SYS_ID = value; }
        }
        private string _SYS_ID { get; set; }

        /// <summary>
        /// MODULES 模組代號
        /// </summary>
        [DisplayName("模組代號")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        [Key]
        public string MODULES
        {
            get { return _MODULES; }
            set { if(!this.clearField.IsContainProperty("MODULES")) this.clearField.Add("MODULES"); _MODULES = value; }
        }
        private string _MODULES { get; set; }

        /// <summary>
        /// SUBMODULES 子模組代號
        /// </summary>
        [DisplayName("子模組代號")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        [Key]
        public string SUBMODULES
        {
            get { return _SUBMODULES; }
            set { if(!this.clearField.IsContainProperty("SUBMODULES")) this.clearField.Add("SUBMODULES"); _SUBMODULES = value; }
        }
        private string _SUBMODULES { get; set; }

        /// <summary>
        /// PRGID 程式代號
        /// </summary>
        [DisplayName("程式代號")]
        [StringLength(10, ErrorMessage = "{0}最多{1}個字")]
        [Key]
        public string PRGID
        {
            get { return _PRGID; }
            set { if(!this.clearField.IsContainProperty("PRGID")) this.clearField.Add("PRGID"); _PRGID = value; }
        }
        private string _PRGID { get; set; }

        /// <summary>
        /// PRGNAME 程式名稱
        /// </summary>
        [DisplayName("程式名稱")]
        [StringLength(20, ErrorMessage = "{0}最多{1}個字")]
        public string PRGNAME
        {
            get { return _PRGNAME; }
            set { if(!this.clearField.IsContainProperty("PRGNAME")) this.clearField.Add("PRGNAME"); _PRGNAME = value; }
        }
        private string _PRGNAME { get; set; }

        /// <summary>
        /// PRGORDER 顯示順序
        /// </summary>
        [DisplayName("顯示順序")]
        [StringLength(6, ErrorMessage = "{0}最多{1}個字")]
        public string PRGORDER
        {
            get { return _PRGORDER; }
            set { if(!this.clearField.IsContainProperty("PRGORDER")) this.clearField.Add("PRGORDER"); _PRGORDER = value; }
        }
        private string _PRGORDER { get; set; }

        /// <summary>
        /// OPENAUTH 開放授權
        /// </summary>
        [DisplayName("開放授權")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        public string OPENAUTH
        {
            get { return _OPENAUTH; }
            set { if(!this.clearField.IsContainProperty("OPENAUTH")) this.clearField.Add("OPENAUTH"); _OPENAUTH = value; }
        }
        private string _OPENAUTH { get; set; }

        /// <summary>
        /// SHOWMENU 顯示功能選單
        /// </summary>
        [DisplayName("顯示功能選單")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        public string SHOWMENU
        {
            get { return _SHOWMENU; }
            set { if(!this.clearField.IsContainProperty("SHOWMENU")) this.clearField.Add("SHOWMENU"); _SHOWMENU = value; }
        }
        private string _SHOWMENU { get; set; }

        /// <summary>
        /// QUERYSTRING 傳入值
        /// </summary>
        [DisplayName("傳入值")]
        [StringLength(100, ErrorMessage = "{0}最多{1}個字")]
        public string QUERYSTRING
        {
            get { return _QUERYSTRING; }
            set { if(!this.clearField.IsContainProperty("QUERYSTRING")) this.clearField.Add("QUERYSTRING"); _QUERYSTRING = value; }
        }
        private string _QUERYSTRING { get; set; }

        /// <summary>
        /// MODUSERID 異動者帳號
        /// </summary>
        [DisplayName("異動者帳號")]
        [StringLength(10, ErrorMessage = "{0}最多{1}個字")]
        public string MODUSERID
        {
            get { return _MODUSERID; }
            set { if(!this.clearField.IsContainProperty("MODUSERID")) this.clearField.Add("MODUSERID"); _MODUSERID = value; }
        }
        private string _MODUSERID { get; set; }

        /// <summary>
        /// MODUSERNAME 異動者姓名
        /// </summary>
        [DisplayName("異動者姓名")]
        [StringLength(20, ErrorMessage = "{0}最多{1}個字")]
        public string MODUSERNAME
        {
            get { return _MODUSERNAME; }
            set { if(!this.clearField.IsContainProperty("MODUSERNAME")) this.clearField.Add("MODUSERNAME"); _MODUSERNAME = value; }
        }
        private string _MODUSERNAME { get; set; }

        /// <summary>
        /// MODIP 異動ip
        /// </summary>
        [DisplayName("異動ip")]
        [StringLength(45, ErrorMessage = "{0}最多{1}個字")]
        public string MODIP
        {
            get { return _MODIP; }
            set { if(!this.clearField.IsContainProperty("MODIP")) this.clearField.Add("MODIP"); _MODIP = value; }
        }
        private string _MODIP { get; set; }

        /// <summary>
        /// MODTIME 異動時間
        /// </summary>
        [DisplayName("異動時間")]
        [StringLength(13, ErrorMessage = "{0}最多{1}個字")]
        public string MODTIME
        {
            get { return _MODTIME; }
            set { if(!this.clearField.IsContainProperty("MODTIME")) this.clearField.Add("MODTIME"); _MODTIME = value; }
        }
        private string _MODTIME { get; set; }

        /// <summary>
        /// NETID 
        /// </summary>
        [DisplayName("放置位址")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        public string NETID
        {
            get { return _NETID; }
            set { if(!this.clearField.IsContainProperty("NETID")) this.clearField.Add("NETID"); _NETID = value; }
        }
        private string _NETID { get; set; }

        /// <summary>
        /// PRGID2 新程式代號
        /// </summary>
        [DisplayName("新程式代號")]
        [StringLength(30, ErrorMessage = "{0}最多{1}個字")]
        public string PRGID2
        {
            get { return _PRGID2; }
            set { if(!this.clearField.IsContainProperty("PRGID2")) this.clearField.Add("PRGID2"); _PRGID2 = value; }
        }
        private string _PRGID2 { get; set; }

        /// <summary>
        /// QUERYSTRING2 網頁查詢參數值。注意！本屬性只配合 PRGID2 屬性值一起使用。
        /// </summary>
        [DisplayName("網頁查詢參數值")]
        [StringLength(100, ErrorMessage = "{0}最多{1}個字")]
        public string QUERYSTRING2
        {
            get { return _QUERYSTRING2; }
            set { if(!this.clearField.IsContainProperty("QUERYSTRING2")) this.clearField.Add("QUERYSTRING2"); _QUERYSTRING2 = value; }
        }
        private string _QUERYSTRING2 { get; set; }

        /// <summary>
        /// 回傳 Table 名稱
        /// </summary>
        DBRowTableName IDBRow.GetTableName()
        {
            return WDAIIP.WEB.Commons.StaticCodeMap.TableName.CLAMFUNCM;
        }
        
    }

    /// <summary>
    /// [CLAMFUNCM 系統程式資料檔] 擴充 model
    /// </summary>
    public class TblCLAMFUNCMExt : TblCLAMFUNCM
    {
    }
}

