using WDAIIP.WEB.Commons;
using Turbo.DataLayer;
using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace WDAIIP.WEB.Models.Entities
{

    /// <summary>
    /// 產投課程報名收件處理狀態統計檔
    /// </summary>
    public class TblSYS_SIGNUP_STATISTICS : IDBRow
	{
        public TblSYS_SIGNUP_STATISTICS()
        {
        }

        /// <summary>
        /// 主機
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 60分鐘統計級距 (1~59)
        /// </summary>
        public int? Idx { get; set; }

        /// <summary>
        /// 統計日期(YYYYMMDD)
        /// <para>跨天歸零重新計算</para>
        /// </summary>
        public string StatDate { get; set; }

        /// <summary>
        /// 是否為當前統計級距
        /// </summary>
        public bool? IsCurrent { get; set; }

        /// <summary>
        /// 平均等待時間
        /// </summary>
        public long? AvgWaitTime { get; set; }

        /// <summary>
        /// 平均處理時間
        /// </summary>
        public long? AvgProcessTime { get; set; }

        /// <summary>
        /// 等待人數
        /// </summary>
        public int? Wait { get; set; }

        /// <summary>
        /// 處理中人數
        /// </summary>
        public int? Process { get; set; }

        /// <summary>
        /// 等待逾時人數
        /// </summary>
        public int? Timeouts { get; set; }

        /// <summary>
        /// 報名系異常人數
        /// </summary>
        public int? Error { get; set; }

        /// <summary>
        /// 報名失敗人數
        /// </summary>
        public int? Fail { get; set; }

        /// <summary>
        /// 報名收件成功人數
        /// </summary>
        public int? Success { get; set; }

        public DBRowTableName GetTableName()
		{
			return StaticCodeMap.TableName.SYS_SIGNUP_STATISTICS;
		}

	}
}
