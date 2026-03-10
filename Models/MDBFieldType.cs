using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// MDB 資料欄位型別
    /// </summary>
    public class MDBFieldType
    {
        /// <summary>文字</summary>
        public static MDBFieldType TEXT = new MDBFieldType("TEXT");

        /// <summary>備忘</summary>
        public static MDBFieldType MEMO = new MDBFieldType("MEMO");

        /// <summary>數字（整數、實數）</summary>
        public static MDBFieldType NUMBER = new MDBFieldType("NUMBER");

        /// <summary>西元日期（不含時間）</summary>
        public static MDBFieldType DATE = new MDBFieldType("DATE");

        /// <summary>西元日期時間</summary>
        public static MDBFieldType DATETIME = new MDBFieldType("DATETIME");

        /// <summary>金額</summary>
        public static MDBFieldType CURRENCY = new MDBFieldType("CURRENCY");

        /// <summary>二進位內容</summary>
        public static MDBFieldType BINARY = new MDBFieldType("BINARY");

        /// <summary>全球唯一識別碼</summary>
        public static MDBFieldType GUID  = new MDBFieldType("GUID");

        #region 物件實作內容
        /// <summary>資料欄位型別名稱</summary>
        public string Name { get; set; }

        /// <summary>建構子</summary>
        /// <param name="name">資料欄位型別名稱</param>
        public MDBFieldType(string name)
        {
            this.Name = name.ToUpper();
        }
        #endregion
    }
}