using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// MDB 資料 Model
    /// </summary>
    public class MDBModel
    {
        /// <summary>MDB 資料表名稱。</summary>
        public string TableName { get; set; }

        /// <summary>MDB 資料欄位集合</summary>
        private IList<MDBField> _Fields { get; set; }

        /// <summary>MDB 資料欄位集合</summary>
        public IList<MDBField> Fields
        {
            get
            {
                return _Fields.OrderBy(o => o.Seq).ToList();
            }
            set
            {
                _Fields = value;
            }
        }

        /// <summary>MDB 資料集合</summary>
        public IList<Hashtable> Rows { get; set; }

        /// <summary>預設的建構子</summary>
        public MDBModel() { }

        /// <summary>建構子</summary>
        /// <param name="tableName">MDB 資料表名稱</param>
        public MDBModel(string tableName)
        {
            this.TableName = tableName;
        }

        /// <summary>建構子</summary>
        /// <param name="tableName">MDB 資料表名稱</param>
        /// <param name="fields">MDB 資料欄位集合</param>
        public MDBModel(string tableName, IList<MDBField> fields)
        {
            this.TableName = tableName;
            this.Fields = fields;
        }

        /// <summary>建構子</summary>
        /// <param name="tableName">MDB 資料表名稱</param>
        /// <param name="fields">MDB 資料欄位集合</param>
        /// <param name="rows">MDB 資料集合</param>
        public MDBModel(string tableName, IList<MDBField> fields, IList<Hashtable> rows)
        {
            this.TableName = tableName;
            this.Fields = fields;
            this.Rows = rows;
        }
    }
}