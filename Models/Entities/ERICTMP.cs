using WDAIIP.WEB.Commons;
using Turbo.DataLayer;
using System;
using System.ComponentModel.DataAnnotations;

namespace WDAIIP.WEB.Models.Entities
{

	/// <summary>
	/// 暫存表格(REST API 範例使用)
	/// </summary>
	public class TblERICTMP : IDBRow
	{
		/// <summary>
		/// 
		/// </summary>
		public string IDNO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string NOTE { get; set; }

		public DBRowTableName GetTableName()
		{
			return StaticCodeMap.TableName.ERICTMP;
		}

	}
}
