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
    /// <summary>
    /// 線上報名資料
    /// </summary>
    [Table("STUD_ENTERTRAIN2DELDATA")]
    [DisplayName("線上報名資料")]
    public class TblSTUD_ENTERTRAIN2DELDATA : IDBRow
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public TblSTUD_ENTERTRAIN2DELDATA() { }

        /// <summary>流水號 PK</summary>
        public Decimal? SEID { get; set; }

        /// <summary>線上報名職類流水號 (Stud_EnterType2.eSerNum)</summary>
        public Decimal? ESERNUM { get; set; }

        /// <summary>戶籍地址-郵遞區號</summary>
        public Decimal? ZIPCODE2 { get; set; }

        /// <summary>戶籍地址-地址</summary>
        public string HOUSEHOLDADDRESS { get; set; }

        /// <summary>主要參訓身份別 (01.一般身份者 03.負擔家計婦女 04.中高齡(45歲) 05.原住民 06.身心障礙者 07.生活扶助戶)</summary>
        public string MIDENTITYID { get; set; }

        /// <summary>障礙類別</summary>
        public string HANDTYPEID { get; set; }

        /// <summary>障礙等級</summary>
        public string HANDLEVELID { get; set; }

        /// <summary>受訓前工作單位名稱 1</summary>
        public string PRIORWORKORG1 { get; set; }

        /// <summary>職稱 1</summary>
        public string TITLE1 { get; set; }

        /// <summary>受訓前工作單位名稱 2</summary>
        public string PRIORWORKORG2 { get; set; }

        /// <summary>職稱 2</summary>
        public string TITLE2 { get; set; }

        /// <summary>任職起日 1 (yyyy/mm/dd)</summary>
        public DateTime? SOFFICEYM1 { get; set; }

        /// <summary>任職迄日 1 (yyyy/mm/dd)</summary>
        public DateTime? FOFFICEYM1 { get; set; }

        /// <summary>任職起日 2 (yyyy/mm/dd)</summary>
        public DateTime? SOFFICEYM2 { get; set; }

        /// <summary>任職迄日 2 (yyyy/mm/dd)</summary>
        public DateTime? FOFFICEYM2 { get; set; }

        /// <summary>受訓前薪資</summary>
        public Decimal? PRIORWORKPAY { get; set; }

        /// <summary>失業週數 (受訓前失業周數)</summary>
        public string REALJOBLESS { get; set; }

        /// <summary>失業週數代碼 (受訓前失業周數代碼)</summary>
        public string JOBLESSID { get; set; }

        /// <summary>交通方式</summary>
        public Decimal? TRAFFIC { get; set; }

        /// <summary>是否供求才廠商查詢</summary>
        public string SHOWDETAIL { get; set; }

        /// <summary>郵政或金融 (0:郵政 1:金融)</summary>
        public Decimal? ACCTMODE { get; set; }

        /// <summary>郵政-局號</summary>
        public string POSTNO { get; set; }

        /// <summary>金融-總代號</summary>
        public string ACCTHEADNO { get; set; }

        /// <summary>銀行名稱</summary>
        public string BANKNAME { get; set; }

        /// <summary>分行代碼</summary>
        public string ACCTEXNO { get; set; }

        /// <summary>分行名稱</summary>
        public string EXBANKNAME { get; set; }

        /// <summary>帳號</summary>
        public string ACCTNO { get; set; }

        /// <summary>第一次投保勞保日 (yyyy/mm/dd)</summary>
        public DateTime? FIRDATE { get; set; }

        /// <summary>公司名稱</summary>
        public string UNAME { get; set; }

        /// <summary>服務單位統一編號</summary>
        public string INTAXNO { get; set; }

        /// <summary>保險證號</summary>
        public string ACTNO { get; set; }

        /// <summary>投保公司名稱</summary>
        public string ACTNAME { get; set; }

        /// <summary>服務部門</summary>
        public string SERVDEPT { get; set; }

        /// <summary>職稱</summary>
        public string JOBTITLE { get; set; }

        /// <summary>公司郵遞區號</summary>
        public Decimal? ZIP { get; set; }

        /// <summary>公司地址</summary>
        public string ADDR { get; set; }

        /// <summary>公司電話</summary>
        public string TEL { get; set; }

        /// <summary>公司傳真</summary>
        public string FAX { get; set; }

        /// <summary>個人到任目前任職公司起日 (yyyy/mm/dd)</summary>
        public DateTime? SDATE { get; set; }

        /// <summary>個人到任目前職務起日 (yyyy/mm/dd)</summary>
        public DateTime? SJDATE { get; set; }

        /// <summary>最近升遷日期 (yyyy/mm/dd)</summary>
        public DateTime? SPDATE { get; set; }

        /// <summary>是否由公司推薦參訓 (1:Yes 2:No)</summary>
        public Decimal? Q1 { get; set; }

        /// <summary>參訓動機 1 (1:有2:無)</summary>
        public Decimal? Q2_1 { get; set; }

        /// <summary>參訓動機 2 (1:有2:無)</summary>
        public Decimal? Q2_2 { get; set; }

        /// <summary>參訓動機 3 (1:有2:無)</summary>
        public Decimal? Q2_3 { get; set; }

        /// <summary>參訓動機 4 (1:有2:無)</summary>
        public Decimal? Q2_4 { get; set; }

        /// <summary>Q3 訓後動向 (1.轉換工作 2.留任 3.其他)</summary>
        public Decimal? Q3 { get; set; }

        /// <summary>Q3 訓後動向其他</summary>
        public string Q3_OTHER { get; set; }

        /// <summary>Q4 服務單位行業別</summary>
        public string Q4 { get; set; }

        /// <summary>Q5 服務單位是否屬於中小企業 (1:Yes 0:No)</summary>
        public Decimal? Q5 { get; set; }

        /// <summary>個人年資</summary>
        public Decimal? Q61 { get; set; }

        /// <summary>公司年資</summary>
        public Decimal? Q62 { get; set; }

        /// <summary>職位年資</summary>
        public Decimal? Q63 { get; set; }

        /// <summary>升遷年資</summary>
        public Decimal? Q64 { get; set; }

        /// <summary>是否願意收到職訓通知</summary>
        public string ISEMAIL { get; set; }

        /// <summary>異動者</summary>
        public string MODIFYACCT { get; set; }

        /// <summary>異動時間</summary>
        public DateTime? MODIFYDATE { get; set; }

        /// <summary>投保類別 (1.勞 2.農 3.漁 4.軍)</summary>
        public string ACTTYPE { get; set; }

        /// <summary>服務單位規模 (1.30人以下 2.31~100人 3.101~500人 4.501人以上)</summary>
        public string SCALE { get; set; }

        /// <summary>戶籍地址-郵遞區號後 2 碼</summary>
        public Decimal? ZIPCODE2_2W { get; set; }
        /// <summary>ZIPCODE2_6W</summary>
        public string ZIPCODE2_6W { get; set; }

        /// <summary>投保單位電話</summary>
        public string ACTTEL { get; set; }

        /// <summary>投保單位郵遞區號前三碼</summary>
        public string ZIPCODE3 { get; set; }

        /// <summary>投保單位郵遞區號後兩碼</summary>
        public string ZIPCODE3_2W { get; set; }
        /// <summary>ZIPCODE3_6W</summary>
        public string ZIPCODE3_6W { get; set; }

        /// <summary>投保單位地址</summary>
        public string ACTADDRESS { get; set; }

        /// <summary>具被保險人身分</summary>
        public string INSURED { get; set; }

        /// <summary></summary>
        public string SERVDEPTID { get; set; }

        /// <summary></summary>
        public string JOBTITLEID { get; set; }

        /// <summary></summary>
        public Decimal? ZIPCODE3_N { get; set; }

        /// <summary></summary>
        public Decimal? ZIPCODE2_N { get; set; }

        /// <summary></summary>
        public Decimal? ZIP2W { get; set; }
        /// <summary>ZIP6W</summary>
        public string ZIP6W { get; set; }


        /// <summary></summary>
        public Decimal? ZIP_N { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.STUD_ENTERTRAIN2DELDATA;
        }
    }
}