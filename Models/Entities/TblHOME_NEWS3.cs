using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// HOME_NEWS3 資料表實體
    /// </summary>
    public class TblHOME_NEWS3 : IDBRow
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal? HN3ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SUBJECT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? STOPSDATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? STOPEDATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? POSTDATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CREATEACCT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CREATEDATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MODIFYACCT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? MODIFYDATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ISDELETE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DELETEACCT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DELETEDATE { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.HOME_NEWS3;
        }
    }
}