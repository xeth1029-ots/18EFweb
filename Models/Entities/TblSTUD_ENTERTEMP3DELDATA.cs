using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary> e網報名基本資料備份檔 </summary>
    [Table("STUD_ENTERTEMP3DELDATA")]
    [DisplayName("產投外網學員資料維護備份檔")]
    public class TblSTUD_ENTERTEMP3DELDATA : DBRowModel, IDBRow, IDBRowOper, IClearField
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public TblSTUD_ENTERTEMP3DELDATA() { }

        ///<summary>
        ///
        ///</summary>
        public Int64? ESETID3
        {
            get { return _ESETID3; }
            set { if (!this.clearField.IsContainProperty("ESETID3")) this.clearField.Add("ESETID3"); _ESETID3 = value; }
        }
        private Int64? _ESETID3 { get; set; }

        /// <summary>
        /// 身份證字號
        /// </summary>
        public string IDNO
        {
            get { return _IDNO; }
            set { if (!this.clearField.IsContainProperty("IDNO")) this.clearField.Add("IDNO"); _IDNO = value; }
        }
        private string _IDNO { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string NAME
        {
            get { return _NAME; }
            set { if (!this.clearField.IsContainProperty("NAME")) this.clearField.Add("NAME"); _NAME = value; }
        }
        private string _NAME { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public string SEX
        {
            get { return _SEX; }
            set { if (!this.clearField.IsContainProperty("SEX")) this.clearField.Add("SEX"); _SEX = value; }
        }
        private string _SEX { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? BIRTHDAY
        {
            get { return _BIRTHDAY; }
            set { if (!this.clearField.IsContainProperty("BIRTHDAY")) this.clearField.Add("BIRTHDAY"); _BIRTHDAY = value; }
        }
        private DateTime? _BIRTHDAY { get; set; }

        /// <summary>
        /// 身份別
        /// </summary>
        public Int64? PASSPORTNO
        {
            get { return _PASSPORTNO; }
            set { if (!this.clearField.IsContainProperty("PASSPORTNO")) this.clearField.Add("PASSPORTNO"); _PASSPORTNO = value; }
        }
        private Int64? _PASSPORTNO { get; set; }

        /// <summary>
        /// 婚姻狀況
        /// </summary>
        public Int64? MARITALSTATUS
        {
            get { return _MARITALSTATUS; }
            set { if (!this.clearField.IsContainProperty("MARITALSTATUS")) this.clearField.Add("MARITALSTATUS"); _MARITALSTATUS = value; }
        }
        private Int64? _MARITALSTATUS { get; set; }

        /// <summary>
        /// 最高學歷
        /// </summary>
        public string DEGREEID
        {
            get { return _DEGREEID; }
            set { if (!this.clearField.IsContainProperty("DEGREEID")) this.clearField.Add("DEGREEID"); _DEGREEID = value; }
        }
        private string _DEGREEID { get; set; }

        /// <summary>
        /// 畢業狀況
        /// </summary>
        public string GRADID
        {
            get { return _GRADID; }
            set { if (!this.clearField.IsContainProperty("GRADID")) this.clearField.Add("GRADID"); _GRADID = value; }
        }
        private string _GRADID { get; set; }

        private string _SCHOOL { get; set; }
        /// <summary> 學校名稱 </summary>
        public string SCHOOL
        {
            get { return _SCHOOL; }
            set { if (!this.clearField.IsContainProperty("SCHOOL")) this.clearField.Add("SCHOOL"); _SCHOOL = value; }
        }

        private string _DEPARTMENT { get; set; }
        /// <summary> 科系名稱 </summary>
        public string DEPARTMENT
        {
            get { return _DEPARTMENT; }
            set { if (!this.clearField.IsContainProperty("DEPARTMENT")) this.clearField.Add("DEPARTMENT"); _DEPARTMENT = value; }
        }

        private string _MILITARYID { get; set; }
        /// <summary> 兵役代碼 </summary>
        public string MILITARYID
        {
            get { return _MILITARYID; }
            set { if (!this.clearField.IsContainProperty("MILITARYID")) this.clearField.Add("MILITARYID"); _MILITARYID = value; }
        }

        private Int64? _ZIPCODE1 { get; set; }
        /// <summary> 郵遞區號 </summary>
        public Int64? ZIPCODE1
        {
            get { return _ZIPCODE1; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE1")) this.clearField.Add("ZIPCODE1"); _ZIPCODE1 = value; }
        }

        private Int64? _ZIPCODE1_2W { get; set; }
        /// <summary> 郵遞區號_後兩碼 </summary>
        public Int64? ZIPCODE1_2W
        {
            get { return _ZIPCODE1_2W; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE1_2W")) this.clearField.Add("ZIPCODE1_2W"); _ZIPCODE1_2W = value; }
        }

        private string _ZIPCODE1_6W { get; set; }
        /// <summary> 郵遞區號_6碼 </summary>
        public string ZIPCODE1_6W
        {
            get { return _ZIPCODE1_6W; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE1_6W")) this.clearField.Add("ZIPCODE1_6W"); _ZIPCODE1_6W = value; }
        }

        private string _ADDRESS { get; set; }
        /// <summary> 地址 </summary>
        public string ADDRESS
        {
            get { return _ADDRESS; }
            set { if (!this.clearField.IsContainProperty("ADDRESS")) this.clearField.Add("ADDRESS"); _ADDRESS = value; }
        }

        private Int64? _ZIPCODE2 { get; set; }
        /// <summary> 郵遞區號 </summary>
        public Int64? ZIPCODE2
        {
            get { return _ZIPCODE2; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE2")) this.clearField.Add("ZIPCODE2"); _ZIPCODE2 = value; }
        }

        private Int64? _ZIPCODE2_2W { get; set; }
        /// <summary> 郵遞區號_後兩碼 </summary>
        public Int64? ZIPCODE2_2W
        {
            get { return _ZIPCODE2_2W; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE2_2W")) this.clearField.Add("ZIPCODE2_2W"); _ZIPCODE2_2W = value; }
        }

        private string _ZIPCODE2_6W { get; set; }
        /// <summary> 郵遞區號_6碼 </summary>
        public string ZIPCODE2_6W
        {
            get { return _ZIPCODE2_6W; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE2_6W")) this.clearField.Add("ZIPCODE2_6W"); _ZIPCODE2_6W = value; }
        }

        /// <summary> 地址 </summary>
        public string HOUSEHOLDADDRESS
        {
            get { return _HOUSEHOLDADDRESS; }
            set { if (!this.clearField.IsContainProperty("HOUSEHOLDADDRESS")) this.clearField.Add("HOUSEHOLDADDRESS"); _HOUSEHOLDADDRESS = value; }
        }
        private string _HOUSEHOLDADDRESS { get; set; }

        /// <summary> 聯絡電話(日) </summary>
        public string PHONE1
        {
            get { return _PHONE1; }
            set { if (!this.clearField.IsContainProperty("PHONE1")) this.clearField.Add("PHONE1"); _PHONE1 = value; }
        }
        private string _PHONE1 { get; set; }

        /// <summary> 聯絡電話(夜) </summary>
        public string PHONE2
        {
            get { return _PHONE2; }
            set { if (!this.clearField.IsContainProperty("PHONE2")) this.clearField.Add("PHONE2"); _PHONE2 = value; }
        }
        private string _PHONE2 { get; set; }

        /// <summary> 行動電話 </summary>
        public string CELLPHONE
        {
            get { return _CELLPHONE; }
            set { if (!this.clearField.IsContainProperty("CELLPHONE")) this.clearField.Add("CELLPHONE"); _CELLPHONE = value; }
        }
        private string _CELLPHONE { get; set; }

        /// <summary> 電子郵件 </summary>
        public string EMAIL
        {
            get { return _EMAIL; }
            set { if (!this.clearField.IsContainProperty("EMAIL")) this.clearField.Add("EMAIL"); _EMAIL = value; }
        }
        private string _EMAIL { get; set; }

        /// <summary> 主要參訓身份別 </summary>
        public string MIDENTITYID
        {
            get { return _MIDENTITYID; }
            set { if (!this.clearField.IsContainProperty("MIDENTITYID")) this.clearField.Add("MIDENTITYID"); _MIDENTITYID = value; }
        }
        private string _MIDENTITYID { get; set; }

        /// <summary>
        /// 受訓前薪資
        /// </summary>
        public Int64? PRIORWORKPAY
        {
            get { return _PRIORWORKPAY; }
            set { if (!this.clearField.IsContainProperty("PRIORWORKPAY")) this.clearField.Add("PRIORWORKPAY"); _PRIORWORKPAY = value; }
        }
        private Int64? _PRIORWORKPAY { get; set; }

        /// <summary>
        /// 郵政/銀行帳號
        /// </summary>
        public Int64? ACCTMODE
        {
            get { return _ACCTMODE; }
            set { if (!this.clearField.IsContainProperty("ACCTMODE")) this.clearField.Add("ACCTMODE"); _ACCTMODE = value; }
        }
        private Int64? _ACCTMODE { get; set; }

        /// <summary>
        /// 局號
        /// </summary>
        public string POSTNO
        {
            get { return _POSTNO; }
            set { if (!this.clearField.IsContainProperty("POSTNO")) this.clearField.Add("POSTNO"); _POSTNO = value; }
        }
        private string _POSTNO { get; set; }

        /// <summary>
        /// 金融-總代號
        /// </summary>
        public string ACCTHEADNO
        {
            get { return _ACCTHEADNO; }
            set { if (!this.clearField.IsContainProperty("ACCTHEADNO")) this.clearField.Add("ACCTHEADNO"); _ACCTHEADNO = value; }
        }
        private string _ACCTHEADNO { get; set; }

        /// <summary>
        /// 銀行名稱
        /// </summary>
        public string BANKNAME
        {
            get { return _BANKNAME; }
            set { if (!this.clearField.IsContainProperty("BANKNAME")) this.clearField.Add("BANKNAME"); _BANKNAME = value; }
        }
        private string _BANKNAME { get; set; }

        /// <summary>
        /// 分行代碼
        /// </summary>
        public string ACCTEXNO
        {
            get { return _ACCTEXNO; }
            set { if (!this.clearField.IsContainProperty("ACCTEXNO")) this.clearField.Add("ACCTEXNO"); _ACCTEXNO = value; }
        }
        private string _ACCTEXNO { get; set; }

        /// <summary>
        /// 分行名稱
        /// </summary>
        public string EXBANKNAME
        {
            get { return _EXBANKNAME; }
            set { if (!this.clearField.IsContainProperty("EXBANKNAME")) this.clearField.Add("EXBANKNAME"); _EXBANKNAME = value; }
        }
        private string _EXBANKNAME { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        public string ACCTNO
        {
            get { return _ACCTNO; }
            set { if (!this.clearField.IsContainProperty("ACCTNO")) this.clearField.Add("ACCTNO"); _ACCTNO = value; }
        }
        private string _ACCTNO { get; set; }

        /// <summary>
        /// 企業名稱
        /// </summary>
        public string UNAME
        {
            get { return _UNAME; }
            set { if (!this.clearField.IsContainProperty("UNAME")) this.clearField.Add("UNAME"); _UNAME = value; }
        }
        private string _UNAME { get; set; }

        /// <summary>
        /// 服務單位統一編號
        /// </summary>
        public string INTAXNO
        {
            get { return _INTAXNO; }
            set { if (!this.clearField.IsContainProperty("INTAXNO")) this.clearField.Add("INTAXNO"); _INTAXNO = value; }
        }
        private string _INTAXNO { get; set; }

        /// <summary>
        /// 投保公司名稱
        /// </summary>
        public string ACTNAME
        {
            get { return _ACTNAME; }
            set { if (!this.clearField.IsContainProperty("ACTNAME")) this.clearField.Add("ACTNAME"); _ACTNAME = value; }
        }
        private string _ACTNAME { get; set; }

        /// <summary>
        /// 投保類別
        /// </summary>
        public string ACTTYPE
        {
            get { return _ACTTYPE; }
            set { if (!this.clearField.IsContainProperty("ACTTYPE")) this.clearField.Add("ACTTYPE"); _ACTTYPE = value; }
        }
        private string _ACTTYPE { get; set; }

        /// <summary>
        /// 保險證號
        /// </summary>
        public string ACTNO
        {
            get { return _ACTNO; }
            set { if (!this.clearField.IsContainProperty("ACTNO")) this.clearField.Add("ACTNO"); _ACTNO = value; }
        }
        private string _ACTNO { get; set; }

        /// <summary>
        /// 投保單位電話
        /// </summary>
        public string ACTTEL
        {
            get { return _ACTTEL; }
            set { if (!this.clearField.IsContainProperty("ACTTEL")) this.clearField.Add("ACTTEL"); _ACTTEL = value; }
        }
        private string _ACTTEL { get; set; }

        /// <summary>
        /// 投保單位郵遞區號前三碼
        /// </summary>
        public Int64? ZIPCODE3
        {
            get { return _ZIPCODE3; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE3")) this.clearField.Add("ZIPCODE3"); _ZIPCODE3 = value; }
        }
        private Int64? _ZIPCODE3 { get; set; }

        /// <summary>
        /// 投保單位郵遞區號後兩碼
        /// </summary>
        public Int64? ZIPCODE3_2W
        {
            get { return _ZIPCODE3_2W; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE3_2W")) this.clearField.Add("ZIPCODE3_2W"); _ZIPCODE3_2W = value; }
        }
        private Int64? _ZIPCODE3_2W { get; set; }

        private string _ZIPCODE3_6W { get; set; }
        /// <summary> 郵遞區號_6碼 </summary>
        public string ZIPCODE3_6W
        {
            get { return _ZIPCODE3_6W; }
            set { if (!this.clearField.IsContainProperty("ZIPCODE3_6W")) this.clearField.Add("ZIPCODE3_6W"); _ZIPCODE3_6W = value; }
        }

        /// <summary>
        /// 投保單位地址
        /// </summary>
        public string ACTADDRESS
        {
            get { return _ACTADDRESS; }
            set { if (!this.clearField.IsContainProperty("ACTADDRESS")) this.clearField.Add("ACTADDRESS"); _ACTADDRESS = value; }
        }
        private string _ACTADDRESS { get; set; }

        /// <summary>
        /// 服務部門
        /// </summary>
        public string SERVDEPT
        {
            get { return _SERVDEPT; }
            set { if (!this.clearField.IsContainProperty("SERVDEPT")) this.clearField.Add("SERVDEPT"); _SERVDEPT = value; }
        }
        private string _SERVDEPT { get; set; }

        /// <summary>
        /// 職務
        /// </summary>
        public string JOBTITLE
        {
            get { return _JOBTITLE; }
            set { if (!this.clearField.IsContainProperty("JOBTITLE")) this.clearField.Add("JOBTITLE"); _JOBTITLE = value; }
        }
        private string _JOBTITLE { get; set; }

        /// <summary>
        /// 是否由公司推薦參訓
        /// </summary>
        public Int64? Q1
        {
            get { return _Q1; }
            set { if (!this.clearField.IsContainProperty("Q1")) this.clearField.Add("Q1"); _Q1 = value; }
        }
        private Int64? _Q1 { get; set; }

        /// <summary>
        /// 參訓動機1
        /// </summary>
        public Int64? Q2_1
        {
            get { return _Q2_1; }
            set { if (!this.clearField.IsContainProperty("Q2_1")) this.clearField.Add("Q2_1"); _Q2_1 = value; }
        }
        private Int64? _Q2_1 { get; set; }

        /// <summary>
        /// 參訓動機2
        /// </summary>
        public Int64? Q2_2
        {
            get { return _Q2_2; }
            set { if (!this.clearField.IsContainProperty("Q2_2")) this.clearField.Add("Q2_2"); _Q2_2 = value; }
        }
        private Int64? _Q2_2 { get; set; }

        /// <summary>
        /// 參訓動機3
        /// </summary>
        public Int64? Q2_3
        {
            get { return _Q2_3; }
            set { if (!this.clearField.IsContainProperty("Q2_3")) this.clearField.Add("Q2_3"); _Q2_3 = value; }
        }
        private Int64? _Q2_3 { get; set; }


        /// <summary>
        /// 參訓動機4
        /// </summary>
        public Int64? Q2_4
        {
            get { return _Q2_4; }
            set { if (!this.clearField.IsContainProperty("Q2_4")) this.clearField.Add("Q2_4"); _Q2_4 = value; }
        }
        private Int64? _Q2_4 { get; set; }

        /// <summary>
        /// 訓後動向
        /// </summary>
        public Int64? Q3
        {
            get { return _Q3; }
            set { if (!this.clearField.IsContainProperty("Q3")) this.clearField.Add("Q3"); _Q3 = value; }
        }
        private Int64? _Q3 { get; set; }

        /// <summary>
        /// 訓後動向_OTH
        /// </summary>
        public string Q3_OTHER
        {
            get { return _Q3_OTHER; }
            set { if (!this.clearField.IsContainProperty("Q3_OTHER")) this.clearField.Add("Q3_OTHER"); _Q3_OTHER = value; }
        }
        private string _Q3_OTHER { get; set; }

        /// <summary>
        /// 服務單位行業別
        /// </summary>
        public string Q4
        {
            get { return _Q4; }
            set { if (!this.clearField.IsContainProperty("Q4")) this.clearField.Add("Q4"); _Q4 = value; }
        }
        private string _Q4 { get; set; }

        /// <summary>
        /// 服務單位是否 屬於中小企業
        /// </summary>
        public Int64? Q5
        {
            get { return _Q5; }
            set { if (!this.clearField.IsContainProperty("Q5")) this.clearField.Add("Q5"); _Q5 = value; }
        }
        private Int64? _Q5 { get; set; }

        /// <summary>
        /// 個人工作年資
        /// </summary>
        public double? Q61
        {
            get { return _Q61; }
            set { if (!this.clearField.IsContainProperty("Q61")) this.clearField.Add("Q61"); _Q61 = value; }
        }
        private double? _Q61 { get; set; }

        /// <summary>
        /// 在這家公司 的年資
        /// </summary>
        public double? Q62
        {
            get { return _Q62; }
            set { if (!this.clearField.IsContainProperty("Q62")) this.clearField.Add("Q62"); _Q62 = value; }
        }
        private double? _Q62 { get; set; }

        /// <summary>
        /// 在這職位的年資
        /// </summary>
        public double? Q63
        {
            get { return _Q63; }
            set { if (!this.clearField.IsContainProperty("Q63")) this.clearField.Add("Q63"); _Q63 = value; }
        }
        private double? _Q63 { get; set; }

        /// <summary>
        /// 最近升遷離 本職幾年
        /// </summary>
        public double? Q64
        {
            get { return _Q64; }
            set { if (!this.clearField.IsContainProperty("Q64")) this.clearField.Add("Q64"); _Q64 = value; }
        }
        private double? _Q64 { get; set; }

        /// <summary>
        /// 是否希望 定期收到產業人才投資方案最新課程資訊
        /// </summary>
        public string ISEMAIL
        {
            get { return _ISEMAIL; }
            set { if (!this.clearField.IsContainProperty("ISEMAIL")) this.clearField.Add("ISEMAIL"); _ISEMAIL = value; }
        }
        private string _ISEMAIL { get; set; }

        /// <summary>
        /// 同意否
        /// </summary>
        public string ISAGREE
        {
            get { return _ISAGREE; }
            set { if (!this.clearField.IsContainProperty("ISAGREE")) this.clearField.Add("ISAGREE"); _ISAGREE = value; }
        }
        private string _ISAGREE { get; set; }

        /// <summary>
        /// 異動者
        /// </summary>
        public string MODIFYACCT
        {
            get { return _MODIFYACCT; }
            set { if (!this.clearField.IsContainProperty("MODIFYACCT")) this.clearField.Add("MODIFYACCT"); _MODIFYACCT = value; }
        }
        private string _MODIFYACCT { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        public DateTime? MODIFYDATE
        {
            get { return _MODIFYDATE; }
            set { if (!this.clearField.IsContainProperty("MODIFYDATE")) this.clearField.Add("MODIFYDATE"); _MODIFYDATE = value; }
        }
        private DateTime? _MODIFYDATE { get; set; }

        /// <summary>服務單位代碼</summary>
        public string SERVDEPTID
        {
            get { return _SERVDEPTID; }
            set { if (!this.clearField.IsContainProperty("SERVDEPTID")) this.clearField.Add("SERVDEPTID"); _SERVDEPTID = value; }
        }
        private string _SERVDEPTID { get; set; }

        /// <summary>職務代碼</summary>
        public string JOBTITLEID
        {
            get { return _JOBTITLEID; }
            set { if (!this.clearField.IsContainProperty("JOBTITLEID")) this.clearField.Add("JOBTITLEID"); _JOBTITLEID = value; }
        }
        private string _JOBTITLEID { get; set; }

        /// <summary>ZIPCODE1_N</summary>
        public Int64? ZIPCODE1_N { get; set; }

        /// <summary>ZIPCODE3_N</summary>
        public Int64? ZIPCODE3_N { get; set; }

        /// <summary>ZIPCODE2_N</summary>
        public Int64? ZIPCODE2_N { get; set; }

        /// <summary>
        /// 是否為報名產投 (Y/NULL 是, N 否)
        /// </summary>
        public string ISPLAN28
        {
            get { return _ISPLAN28; }
            set { if (!this.clearField.IsContainProperty("ISPLAN28")) this.clearField.Add("ISPLAN28"); _ISPLAN28 = value; }
        }
        private string _ISPLAN28 { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.STUD_ENTERTEMP3DELDATA;
        }
    }
}