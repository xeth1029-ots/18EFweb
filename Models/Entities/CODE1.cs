using WDAIIP.WEB.Commons;
using Turbo.DataLayer;
using System;
using System.ComponentModel.DataAnnotations;

namespace WDAIIP.WEB.Models.Entities
{

	/// <summary>
	/// 
	/// </summary>
	public class TblCODE1 : IDBRow
	{
		/// <summary>
		/// 
		/// </summary>
		public string MODIP { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string MODTIME { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string MODUSERNAME { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string MODUSERID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string AUX1 { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string FREQ { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string HIST { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string DESCR { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string CODE { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ITEM { get; set; }

		public DBRowTableName GetTableName()
		{
			return StaticCodeMap.TableName.CODE1;
		}

	}
}
