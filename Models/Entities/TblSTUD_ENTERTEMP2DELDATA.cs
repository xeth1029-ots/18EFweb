using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// STUD_ENTERTEMP2DELDATA e網報名資料暫存檔
    /// </summary>
    public class TblSTUD_ENTERTEMP2DELDATA : IDBRow
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public TblSTUD_ENTERTEMP2DELDATA() { }

        /// <summary>流水ID (產學訓從10000000號開始編)</summary>
        public Int64? ESETID { get; set; }

        /// <summary>Stud_EnterTemp.SETID (報名資料暫存檔，可能同學員身份證字號)</summary>
        public Int64? SETID { get; set; }

        /// <summary>身分證字號</summary>
        public string IDNO { get; set; }

        /// <summary>姓名</summary>
        public string NAME { get; set; }

        /// <summary>性別 (男：M 女：F)</summary>
        public string SEX { get; set; }

        /// <summary>出生日期 (yyyy/mm/dd)</summary>
        public DateTime? BIRTHDAY { get; set; }

        /// <summary>身份別 (1：本國 2：外籍)</summary>
        public Decimal? PASSPORTNO { get; set; }

        /// <summary>婚姻狀況 (1.已 2.未(預設))</summary>
        public Decimal? MARITALSTATUS { get; set; }

        /// <summary>學歷代碼 (01.國中(含)以下 02.高中/職 03.專科 04.大學(大學) 05.碩士 06.博士)</summary>
        public string DEGREEID { get; set; }

        /// <summary>畢業狀況代碼 (Key_GradState)</summary>
        public string GRADID { get; set; }

        /// <summary>學校名稱</summary>
        public string SCHOOL { get; set; }

        /// <summary>科系名稱</summary>
        public string DEPARTMENT { get; set; }

        /// <summary>兵役代碼</summary>
        public string MILITARYID { get; set; }

        /// <summary>郵遞區號</summary>
        public Decimal? ZIPCODE { get; set; }

        /// <summary>通訊地址</summary>
        public string ADDRESS { get; set; }

        /// <summary>聯絡電話(日)</summary>
        public string PHONE1 { get; set; }

        /// <summary>聯絡電話(夜)</summary>
        public string PHONE2 { get; set; }

        /// <summary>行動電話</summary>
        public string CELLPHONE { get; set; }

        /// <summary>Email</summary>
        public string EMAIL { get; set; }

        /// <summary>同意否 (Y/N)</summary>
        public string ISAGREE { get; set; }

        /// <summary>異動者</summary>
        public string MODIFYACCT { get; set; }

        /// <summary>異動時間</summary>
        public DateTime? MODIFYDATE { get; set; }

        /// <summary>郵遞區號</summary>
        public Decimal? ZIPCODE2W { get; set; }

        /// <summary>郵遞區號</summary>
        public string ZIPCODE6W { get; set; }

        /// <summary></summary>
        public Decimal? LAINFLAG { get; set; }

        /// <summary></summary>
        public Decimal? ZIPCODE_N { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.STUD_ENTERTEMP2DELDATA;
        }
    }
}