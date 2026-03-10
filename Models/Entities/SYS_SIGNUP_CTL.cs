using WDAIIP.WEB.Commons;
using Turbo.DataLayer;
using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace WDAIIP.WEB.Models.Entities
{

    /// <summary>
    /// 產投課程報名-收件處理 Thread Pool 控制檔
    /// </summary>
    public class TblSYS_SIGNUP_CTL : IDBRow
	{
        public TblSYS_SIGNUP_CTL()
        {
        }

        /// <summary>
        /// 收件處理主機IP
        /// </summary>
        public string HOST { get; set; }

        /// <summary>
        /// 排隊號碼計數器日期 (每日重新給號)
        /// </summary>
        public int? QDAY { get; set; }

        /// <summary>
        /// 排隊號碼計數器 (由1開始)
        /// </summary>
        public int? SEQ { get; set; }

        /// <summary>
        /// 當前輪到的號碼 (由1開始)
        /// </summary>
        public int? CURSEQ { get; set; }

        /// <summary>
        /// 報名作業 [運行中] 處理程序數量計數器 (由0開始)
        /// </summary>
        public int? PCOUNT { get; set; }

        /// <summary>
        /// 報名作業 [運行中+等待中] 處理程序總數量計數器 (由0開始)
        /// </summary>
        public int? NCOUNT { get; set; }

        public DBRowTableName GetTableName()
		{
			return StaticCodeMap.TableName.SYS_SIGNUP_CTL;
		}

	}
}
