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
    /// [CLAMDBURM 帳號資料檔] table model
    /// </summary>
    [Table("CLAMDBURM")]
    [DisplayName("帳號資料檔")]
    public class TblCLAMDBURM : DBRowModel, IDBRow, IDBRowOper, IClearField
    {
        /// <summary>
        /// USERNO 帳號
        /// </summary>
        [DisplayName("帳號")]
        [StringLength(10, ErrorMessage = "{0}最多{1}個字")]
        [Key]
        public string USERNO
        {
            get { return _USERNO; }
            set { if(!this.clearField.IsContainProperty("USERNO")) this.clearField.Add("USERNO"); _USERNO = value; }
        }
        private string _USERNO { get; set; }

        /// <summary>
        /// USERNAME 姓名
        /// </summary>
        [DisplayName("姓名")]
        [StringLength(20, ErrorMessage = "{0}最多{1}個字")]
        public string USERNAME
        {
            get { return _USERNAME; }
            set { if(!this.clearField.IsContainProperty("USERNAME")) this.clearField.Add("USERNAME"); _USERNAME = value; }
        }
        private string _USERNAME { get; set; }

        /// <summary>
        /// PWD 密碼
        /// </summary>
        [DisplayName("密碼")]
        [StringLength(50, ErrorMessage = "{0}最多{1}個字")]
        public string PWD
        {
            get { return _PWD; }
            set { if(!this.clearField.IsContainProperty("PWD")) this.clearField.Add("PWD"); _PWD = value; }
        }
        private string _PWD { get; set; }

        /// <summary>
        /// BIRTHDAY 出生日期
        /// </summary>
        [DisplayName("出生日期")]
        [StringLength(7, ErrorMessage = "{0}最多{1}個字")]
        public string BIRTHDAY
        {
            get { return _BIRTHDAY; }
            set { if(!this.clearField.IsContainProperty("BIRTHDAY")) this.clearField.Add("BIRTHDAY"); _BIRTHDAY = value; }
        }
        private string _BIRTHDAY { get; set; }

        /// <summary>
        /// AUTHNUM 驗證碼
        /// </summary>
        [DisplayName("驗證碼")]
        [StringLength(4, ErrorMessage = "{0}最多{1}個字")]
        public string AUTHNUM
        {
            get { return _AUTHNUM; }
            set { if(!this.clearField.IsContainProperty("AUTHNUM")) this.clearField.Add("AUTHNUM"); _AUTHNUM = value; }
        }
        private string _AUTHNUM { get; set; }

        /// <summary>
        /// UNITID 單位
        /// </summary>
        [DisplayName("單位")]
        [StringLength(4, ErrorMessage = "{0}最多{1}個字")]
        public string UNITID
        {
            get { return _UNITID; }
            set { if(!this.clearField.IsContainProperty("UNITID")) this.clearField.Add("UNITID"); _UNITID = value; }
        }
        private string _UNITID { get; set; }

        /// <summary>
        /// SUBUNIT 科處室
        /// </summary>
        [DisplayName("科處室")]
        [StringLength(2, ErrorMessage = "{0}最多{1}個字")]
        public string SUBUNIT
        {
            get { return _SUBUNIT; }
            set { if(!this.clearField.IsContainProperty("SUBUNIT")) this.clearField.Add("SUBUNIT"); _SUBUNIT = value; }
        }
        private string _SUBUNIT { get; set; }

        /// <summary>
        /// SUBUNITN 科處室名稱
        /// </summary>
        [DisplayName("科處室名稱")]
        [StringLength(20, ErrorMessage = "{0}最多{1}個字")]
        public string SUBUNITN
        {
            get { return _SUBUNITN; }
            set { if(!this.clearField.IsContainProperty("SUBUNITN")) this.clearField.Add("SUBUNITN"); _SUBUNITN = value; }
        }
        private string _SUBUNITN { get; set; }

        /// <summary>
        /// TITLENAME 職稱
        /// </summary>
        [DisplayName("職稱")]
        [StringLength(20, ErrorMessage = "{0}最多{1}個字")]
        public string TITLENAME
        {
            get { return _TITLENAME; }
            set { if(!this.clearField.IsContainProperty("TITLENAME")) this.clearField.Add("TITLENAME"); _TITLENAME = value; }
        }
        private string _TITLENAME { get; set; }

        /// <summary>
        /// EMAIL E-Mail
        /// </summary>
        [DisplayName("E-Mail")]
        [StringLength(50, ErrorMessage = "{0}最多{1}個字")]
        public string EMAIL
        {
            get { return _EMAIL; }
            set { if(!this.clearField.IsContainProperty("EMAIL")) this.clearField.Add("EMAIL"); _EMAIL = value; }
        }
        private string _EMAIL { get; set; }

        /// <summary>
        /// TELNO 聯絡電話
        /// </summary>
        [DisplayName("聯絡電話")]
        [StringLength(30, ErrorMessage = "{0}最多{1}個字")]
        public string TELNO
        {
            get { return _TELNO; }
            set { if(!this.clearField.IsContainProperty("TELNO")) this.clearField.Add("TELNO"); _TELNO = value; }
        }
        private string _TELNO { get; set; }

        /// <summary>
        /// FAXNO 傳真電話
        /// </summary>
        [DisplayName("傳真電話")]
        [StringLength(30, ErrorMessage = "{0}最多{1}個字")]
        public string FAXNO
        {
            get { return _FAXNO; }
            set { if(!this.clearField.IsContainProperty("FAXNO")) this.clearField.Add("FAXNO"); _FAXNO = value; }
        }
        private string _FAXNO { get; set; }

        /// <summary>
        /// CZIPCODE 通訊住址-郵遞區號
        /// </summary>
        [DisplayName("通訊住址-郵遞區號")]
        [StringLength(5, ErrorMessage = "{0}最多{1}個字")]
        public string CZIPCODE
        {
            get { return _CZIPCODE; }
            set { if(!this.clearField.IsContainProperty("CZIPCODE")) this.clearField.Add("CZIPCODE"); _CZIPCODE = value; }
        }
        private string _CZIPCODE { get; set; }

        /// <summary>
        /// CADDRESS 通訊住址
        /// </summary>
        [DisplayName("通訊住址")]
        [StringLength(30, ErrorMessage = "{0}最多{1}個字")]
        public string CADDRESS
        {
            get { return _CADDRESS; }
            set { if(!this.clearField.IsContainProperty("CADDRESS")) this.clearField.Add("CADDRESS"); _CADDRESS = value; }
        }
        private string _CADDRESS { get; set; }

        /// <summary>
        /// AUTHSTATUS 帳號使用狀態
        /// </summary>
        [DisplayName("帳號使用狀態")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        public string AUTHSTATUS
        {
            get { return _AUTHSTATUS; }
            set { if(!this.clearField.IsContainProperty("AUTHSTATUS")) this.clearField.Add("AUTHSTATUS"); _AUTHSTATUS = value; }
        }
        private string _AUTHSTATUS { get; set; }

        /// <summary>
        /// AUTHDESC 駁回/註銷原因
        /// </summary>
        [DisplayName("駁回/註銷原因")]
        [StringLength(50, ErrorMessage = "{0}最多{1}個字")]
        public string AUTHDESC
        {
            get { return _AUTHDESC; }
            set { if(!this.clearField.IsContainProperty("AUTHDESC")) this.clearField.Add("AUTHDESC"); _AUTHDESC = value; }
        }
        private string _AUTHDESC { get; set; }

        /// <summary>
        /// AUTHDATES 帳號有效權限起日
        /// </summary>
        [DisplayName("帳號有效權限起日")]
        [StringLength(7, ErrorMessage = "{0}最多{1}個字")]
        public string AUTHDATES
        {
            get { return _AUTHDATES; }
            set { if(!this.clearField.IsContainProperty("AUTHDATES")) this.clearField.Add("AUTHDATES"); _AUTHDATES = value; }
        }
        private string _AUTHDATES { get; set; }

        /// <summary>
        /// AUTHDATEE 帳號有效權限迄日
        /// </summary>
        [DisplayName("帳號有效權限迄日")]
        [StringLength(7, ErrorMessage = "{0}最多{1}個字")]
        public string AUTHDATEE
        {
            get { return _AUTHDATEE; }
            set { if(!this.clearField.IsContainProperty("AUTHDATEE")) this.clearField.Add("AUTHDATEE"); _AUTHDATEE = value; }
        }
        private string _AUTHDATEE { get; set; }

        /// <summary>
        /// ERRCT 密碼錯誤次數
        /// </summary>
        [DisplayName("密碼錯誤次數")]
        [NumberValidtion(1, 0)]
        public Int64? ERRCT
        {
            get { return _ERRCT; }
            set { if(!this.clearField.IsContainProperty("ERRCT")) this.clearField.Add("ERRCT"); _ERRCT = value; }
        }
        private Int64? _ERRCT { get; set; }

        /// <summary>
        /// APPDATE 帳號申請日期
        /// </summary>
        [DisplayName("帳號申請日期")]
        [StringLength(7, ErrorMessage = "{0}最多{1}個字")]
        public string APPDATE
        {
            get { return _APPDATE; }
            set { if(!this.clearField.IsContainProperty("APPDATE")) this.clearField.Add("APPDATE"); _APPDATE = value; }
        }
        private string _APPDATE { get; set; }

        /// <summary>
        /// PKINO 自然人憑證序號
        /// </summary>
        [DisplayName("自然人憑證序號")]
        [StringLength(40, ErrorMessage = "{0}最多{1}個字")]
        public string PKINO
        {
            get { return _PKINO; }
            set { if(!this.clearField.IsContainProperty("PKINO")) this.clearField.Add("PKINO"); _PKINO = value; }
        }
        private string _PKINO { get; set; }

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
        /// PHONE 手機
        /// </summary>
        [DisplayName("手機")]
        [StringLength(30, ErrorMessage = "{0}最多{1}個字")]
        public string PHONE
        {
            get { return _PHONE; }
            set { if(!this.clearField.IsContainProperty("PHONE")) this.clearField.Add("PHONE"); _PHONE = value; }
        }
        private string _PHONE { get; set; }

        /// <summary>
        /// CHKDATE 憑證上傳日期
        /// </summary>
        [DisplayName("憑證上傳日期")]
        [StringLength(7, ErrorMessage = "{0}最多{1}個字")]
        public string CHKDATE
        {
            get { return _CHKDATE; }
            set { if(!this.clearField.IsContainProperty("CHKDATE")) this.clearField.Add("CHKDATE"); _CHKDATE = value; }
        }
        private string _CHKDATE { get; set; }

        /// <summary>
        /// UPPKI 憑證上傳Flag
        /// </summary>
        [DisplayName("憑證上傳Flag")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        public string UPPKI
        {
            get { return _UPPKI; }
            set { if(!this.clearField.IsContainProperty("UPPKI")) this.clearField.Add("UPPKI"); _UPPKI = value; }
        }
        private string _UPPKI { get; set; }

        /// <summary>
        /// CLAUPCHK 
        /// </summary>
        [DisplayName("")]
        [StringLength(30, ErrorMessage = "{0}最多{1}個字")]
        public string CLAUPCHK
        {
            get { return _CLAUPCHK; }
            set { if(!this.clearField.IsContainProperty("CLAUPCHK")) this.clearField.Add("CLAUPCHK"); _CLAUPCHK = value; }
        }
        private string _CLAUPCHK { get; set; }

        /// <summary>
        /// CNAME 主管姓名
        /// </summary>
        [DisplayName("主管姓名")]
        [StringLength(20, ErrorMessage = "{0}最多{1}個字")]
        public string CNAME
        {
            get { return _CNAME; }
            set { if(!this.clearField.IsContainProperty("CNAME")) this.clearField.Add("CNAME"); _CNAME = value; }
        }
        private string _CNAME { get; set; }

        /// <summary>
        /// UPERRCT 
        /// </summary>
        [DisplayName("")]
        [StringLength(5, ErrorMessage = "{0}最多{1}個字")]
        public string UPERRCT
        {
            get { return _UPERRCT; }
            set { if(!this.clearField.IsContainProperty("UPERRCT")) this.clearField.Add("UPERRCT"); _UPERRCT = value; }
        }
        private string _UPERRCT { get; set; }

        /// <summary>
        /// UNPKI 登入附加驗證方式 0:免驗證, 1:自然人憑證, 2:自發憑證檔驗證
        /// </summary>
        [DisplayName("登入附加驗證方式 0:免驗證, 1:自然人憑證, 2:自發憑證檔驗證")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        public string UNPKI
        {
            get { return _UNPKI; }
            set { if(!this.clearField.IsContainProperty("UNPKI")) this.clearField.Add("UNPKI"); _UNPKI = value; }
        }
        private string _UNPKI { get; set; }

        /// <summary>
        /// SELF_CN 自發憑證檔序號
        /// </summary>
        [DisplayName("自發憑證檔序號")]
        [StringLength(40, ErrorMessage = "{0}最多{1}個字")]
        public string SELF_CN
        {
            get { return _SELF_CN; }
            set { if(!this.clearField.IsContainProperty("SELF_CN")) this.clearField.Add("SELF_CN"); _SELF_CN = value; }
        }
        private string _SELF_CN { get; set; }

        /// <summary>
        /// CERT_APPLY 技檢中心憑證申請 1:提出申請, 2:退回, 3:已通過
        /// </summary>
        [DisplayName("技檢中心憑證申請 1:提出申請, 2:退回, 3:已通過")]
        [StringLength(1, ErrorMessage = "{0}最多{1}個字")]
        public string CERT_APPLY
        {
            get { return _CERT_APPLY; }
            set { if(!this.clearField.IsContainProperty("CERT_APPLY")) this.clearField.Add("CERT_APPLY"); _CERT_APPLY = value; }
        }
        private string _CERT_APPLY { get; set; }

        /// <summary>
        /// CERT_APPLY_REJECT 技檢中心憑證申請退回原因
        /// </summary>
        [DisplayName("技檢中心憑證申請退回原因")]
        [StringLength(200, ErrorMessage = "{0}最多{1}個字")]
        public string CERT_APPLY_REJECT
        {
            get { return _CERT_APPLY_REJECT; }
            set { if(!this.clearField.IsContainProperty("CERT_APPLY_REJECT")) this.clearField.Add("CERT_APPLY_REJECT"); _CERT_APPLY_REJECT = value; }
        }
        private string _CERT_APPLY_REJECT { get; set; }

        /// <summary>
        /// 回傳 Table 名稱
        /// </summary>
        DBRowTableName IDBRow.GetTableName()
        {
            return WDAIIP.WEB.Commons.StaticCodeMap.TableName.CLAMDBURM;
        }
        
    }

    /// <summary>
    /// [CLAMDBURM 帳號資料檔] 擴充 model
    /// </summary>
    public class TblCLAMDBURMExt : TblCLAMDBURM
    {
        /// <summary>
        /// BIRTHDAY 出生日期: 西元日期轉換
        /// </summary>
        public string BIRTHDAY_AD 
        {
            get { return HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(this.BIRTHDAY)); }
            set { this.BIRTHDAY = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value)); }
        }

        /// <summary>
        /// AUTHDATES 帳號有效權限起日: 西元日期轉換
        /// </summary>
        public string AUTHDATES_AD 
        {
            get { return HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(this.AUTHDATES)); }
            set { this.AUTHDATES = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value)); }
        }

        /// <summary>
        /// AUTHDATEE 帳號有效權限迄日: 西元日期轉換
        /// </summary>
        public string AUTHDATEE_AD 
        {
            get { return HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(this.AUTHDATEE)); }
            set { this.AUTHDATEE = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value)); }
        }

        /// <summary>
        /// APPDATE 帳號申請日期: 西元日期轉換
        /// </summary>
        public string APPDATE_AD 
        {
            get { return HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(this.APPDATE)); }
            set { this.APPDATE = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value)); }
        }

        /// <summary>
        /// CHKDATE 憑證上傳日期: 西元日期轉換
        /// </summary>
        public string CHKDATE_AD 
        {
            get { return HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(this.CHKDATE)); }
            set { this.CHKDATE = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value)); }
        }

    }
}

