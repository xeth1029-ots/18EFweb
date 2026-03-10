using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Models.Entities
{
    public class TblTB_FILE : IDBRow
    {
        /// <summary>
        /// 檔案ID
        /// </summary>
        public decimal? FILEID { get; set; }

        /// <summary>
        /// 檔案群組ID
        /// </summary>
        public decimal? F_GROUPID { get; set; }

        /// <summary>
        /// 功能代碼 (CODE1.ITEM=F_FUNID , 1 最新消息 /  2 資料下載)
        /// </summary>
        public string F_FUNID { get; set; }

        /// <summary>
        /// 原檔名
        /// </summary>
        public string FILE_ORINAME { get; set; }

        /// <summary>
        /// 存放系統實體檔名
        /// </summary>
        public string FILE_PHYNAME { get; set; }

        /// <summary>
        /// 檔案類型(.pdf、.odt、.ods、..odp)
        /// </summary>
        public string FILE_TYPE { get; set; }

        /// <summary>
        /// 檔案大小(Ex:20MB)
        /// </summary>
        public string FILE_SIZE { get; set; }

        /// <summary>
        /// 檔案序號 (第幾個附件，填1、2、3、4...)
        /// </summary>
        public string FILE_NO { get; set; }

        /// <summary>
        /// 下載次數
        /// </summary>
        public Int64? DLCOUNT { get; set; }

        /// <summary>
        /// 啟用狀態 (Y 啟用/N 停用)
        /// </summary>
        public string ISUSED { get; set; }

        /// <summary>
        /// 異動者
        /// </summary>
        public string MODIFYACCT { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        public DateTime? MODIFYDATE { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.TB_FILE;
        }
    }
}