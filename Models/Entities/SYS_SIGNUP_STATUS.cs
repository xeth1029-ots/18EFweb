using WDAIIP.WEB.Commons;
using Turbo.DataLayer;
using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace WDAIIP.WEB.Models.Entities
{

    /// <summary>
    /// 產投課程報名收件處理狀態暫存檔
    /// </summary>
    public class TblSYS_SIGNUP_STATUS : IDBRow
	{
        public TblSYS_SIGNUP_STATUS()
        {
        }

        /// <summary>
        /// 收件處理主機IP
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 收件處理的 SessionID (等同於 SessionModel.SessionID)
        /// </summary>
        public string SessionID { get; set; }

        /// <summary>
        /// 收件處理的 Thread ID
        /// </summary>
        public int? ThreadID { get; set; }

        /// <summary>
        /// 排隊號碼 (這個不是產投報名結果序號, 只是 Queing 系統的處理順序)
        /// </summary>
        [JsonProperty]
        public int? QueueSeq { get; set; }


        /// <summary>
        /// Queuing 排隊時間
        /// </summary>
        [JsonProperty]
        public DateTime? QueueTime { get; set; }

        /// <summary>
        /// 開始處理時間
        /// </summary>
        [JsonProperty]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 處理結束時間
        /// </summary>
        [JsonProperty]
        public DateTime? FinishTime { get; set; }

        /// <summary>
        /// 報名人身分證號
        /// </summary>
        [JsonProperty]
        public string IDNO { get; set; }

        /// <summary>
        /// 報名班級代碼
        /// </summary>
        [JsonProperty]
        public long? OCID { get; set; }

        /// <summary>
        /// 前一個作業狀態
        /// </summary>
        [JsonProperty]
        public int? PreStatus { get; set; }

        /// <summary>
        /// 作業狀態: 
        /// -1.起始,
        /// 0.排隊等候處理,
        /// 1.處理作業中,
        /// 2.處理結束(報名收件成功),
        /// 3.處理結束(報名收件失敗),
        /// 9.處理結束(系統異常)
        /// </summary>
        [JsonProperty]
        public int? Status { get; set; }

        /// <summary>
        /// 報名失敗的原因訊息
        /// </summary>
        [JsonProperty]
        public string ErrMsg { get; set; }


        /// <summary>
        /// 報名成功取得的順位序號
        /// </summary>
        [JsonProperty]
        public long? SignNo { get; set; }

        /// <summary>
        /// 是否為等待逾時
        /// </summary>
        [JsonProperty]
        public bool? Timeout { get; set; }

        /// <summary>
        /// 等待處理所花費時間(毫秒)
        /// <para>StartTime - QueueTime</para>
        /// </summary>
        [JsonProperty]
        public long? WaitTimes { get; set; }

        /// <summary>
        /// 報名作業總共花費時間(毫秒)
        /// <para>FinishTime - StartTime</para>
        /// </summary>
        [JsonProperty]
        public long? ProcessTimes { get; set; }

        public DBRowTableName GetTableName()
		{
			return StaticCodeMap.TableName.SYS_SIGNUP_STATUS;
		}

	}
}
