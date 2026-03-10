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
    /// [CLAMUROLE 使用者檢定角色權限設定] table model
    /// </summary>
    [Table("CLAMUROLE")]
    [DisplayName("使用者檢定角色權限設定")]
    public class TblCLAMUROLE : DBRowModel, IDBRow, IDBRowOper, IClearField
    {
        /// <summary>
        /// USERNO 使用者帳號
        /// </summary>
        [DisplayName("使用者帳號")]
        [StringLength(10, ErrorMessage = "{0}最多{1}個字")]
        [Key]
        public string USERNO
        {
            get { return _USERNO; }
            set { if(!this.clearField.IsContainProperty("USERNO")) this.clearField.Add("USERNO"); _USERNO = value; }
        }
        private string _USERNO { get; set; }

        /// <summary>
        /// GRPID 群組代碼
        /// </summary>
        [DisplayName("群組代碼")]
        [StringLength(4, ErrorMessage = "{0}最多{1}個字")]
        [Key]
        public string GRPID
        {
            get { return _GRPID; }
            set { if(!this.clearField.IsContainProperty("GRPID")) this.clearField.Add("GRPID"); _GRPID = value; }
        }
        private string _GRPID { get; set; }

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
        /// 回傳 Table 名稱
        /// </summary>
        DBRowTableName IDBRow.GetTableName()
        {
            return WDAIIP.WEB.Commons.StaticCodeMap.TableName.CLAMUROLE;
        }
        
    }

    /// <summary>
    /// [CLAMUROLE 使用者檢定角色權限設定] 擴充 model
    /// </summary>
    public class TblCLAMUROLEExt : TblCLAMUROLE
    {
    }
}

