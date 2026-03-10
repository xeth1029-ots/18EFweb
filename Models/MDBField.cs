using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// MDB 資料欄位
    /// </summary>
    public class MDBField
    {
        /// <summary>MDB 資料欄位名稱</summary>
        string _Name = "";

        /// <summary>MDB 資料欄位名稱（必須是大寫英文）</summary>
        public string Name {
            get { return _Name; }
            set { _Name = string.IsNullOrEmpty(value) ? "" : value.ToUpper(); }
        }

        /// <summary>MDB 資料欄位順序</summary>
        int _Seq { get; set; }

        /// <summary>MDB 資料欄位順序</summary>
        public int Seq
        {
            get { return _Seq; }
            set { _Seq = value; }
        }

        /// <summary>MDB 資料欄位型別</summary>
        public MDBFieldType Type { get; set; }

        /// <summary>預設的建構子</summary>
        public MDBField() {
            this.Type = MDBFieldType.TEXT;
        }

        /// <summary>建構子</summary>
        /// <param name="name">MDB 資料欄位名稱</param>
        public MDBField(string name) {
            this.Name = name;
            this.Type = MDBFieldType.TEXT;
        }

        /// <summary>建構子</summary>
        /// <param name="name">MDB 資料欄位名稱</param>
        /// <param name="type">MDB 資料欄位型別</param>
        public MDBField(string name, MDBFieldType type) {
            this.Name = name;
            this.Type = type;
        }

        /// <summary>建構子</summary>
        /// <param name="name">MDB 資料欄位名稱</param>
        /// <param name="type">MDB 資料欄位型別</param>
        /// <param name="seq">MDB 資料欄位順序</param>
        public MDBField(string name, MDBFieldType type, int seq)
        {
            this.Name = name;
            this.Type = type;
            this.Seq = seq;
        }
    }
}