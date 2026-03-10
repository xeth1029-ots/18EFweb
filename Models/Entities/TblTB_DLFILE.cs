using System;
using System.ComponentModel.DataAnnotations;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    /// <summary>
    /// TblTB_DLFILE 資料表實體
    /// </summary>
    public class TblTB_DLFILE : IDBRow
    {
        /// <summary>下載流水號</summary>
        public Decimal? DLID { get; set; }

        /// <summary>下載類別</summary>
        public string KINDID { get; set; }

        /// <summary>計畫類別</summary>
        public string PLANID { get; set; }

        /// <summary>下載標題</summary>
        public string DLTITLE { get; set; }

        /// <summary>檔名1</summary>
        public string FILE1_NAME { get; set; }

        /// <summary>檔案類型1</summary>
        public string FILE1_TYPE { get; set; }

        /// <summary>檔案大小1</summary>
        public string FILE1_SIZE { get; set; }

        /// <summary>檔名2</summary>
        public string FILE2_NAME { get; set; }

        /// <summary>檔案類型2</summary>
        public string FILE2_TYPE { get; set; }

        /// <summary>檔案大小2</summary>
        public string FILE2_SIZE { get; set; }

        /// <summary>上架日-起</summary>
        public DateTime? START_DATE { get; set; }

        /// <summary>上架日-迄</summary>
        public DateTime? END_DATE { get; set; }

        /// <summary>下載次數</summary>
        public int? DLCOUNT { get; set; }

        /// <summary>上傳日期</summary>
        public DateTime? UPLOADDATE { get; set; }

        /// <summary>啟用</summary>
        public string ISUSED { get; set; }

        /// <summary>說明</summary>
        public string MEMO { get; set; }

        /// <summary>異動者</summary>
        public string MODIFYACCT { get; set; }

        /// <summary>異動時間</summary>
        public DateTime? MODIFYDATE { get; set; }

        /// <summary>傳回資料表名稱</summary>
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.TB_DLFILE;
        }
    }
}