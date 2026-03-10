using WDAIIP.WEB.Commons;
using Turbo.DataLayer;
using System;
using System.ComponentModel.DataAnnotations;

namespace WDAIIP.WEB.Models.Entities
{

	/// <summary>
	/// 系統別代碼檔
	/// </summary>
	public class SYSCODE : IDBRow
	{

        /// <summary>
        /// 系統別代碼
        /// </summary>
        public string SYS_ID { get; set; }

        /// <summary>
        /// 系統別名稱
        /// </summary>
        public string SYS_NAME { get; set; }

		public DBRowTableName GetTableName()
		{
			return StaticCodeMap.TableName.SYSCODE;
		}

	}
}
