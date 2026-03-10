using WDAIIP.WEB.Commons;
using Turbo.DataLayer;
using System;
using System.ComponentModel.DataAnnotations;

namespace WDAIIP.WEB.Models.Entities
{

	/// <summary>
	/// 地區/縣巿別資料檔
	/// </summary>
	public class TblCLAMCITY : IDBRow
	{
		/// <summary>
		/// 異動時間
		/// </summary>
		public string MODTIME { get; set; }

		/// <summary>
		/// 異動ip
		/// </summary>
		public string MODIP { get; set; }

		/// <summary>
		/// 異動者姓名
		/// </summary>
		public string MODUSERNAME { get; set; }

		/// <summary>
		/// 異動者帳號
		/// </summary>
		public string MODUSERID { get; set; }

		/// <summary>
		/// 地區別
		/// </summary>
		public string AREANO { get; set; }

		/// <summary>
		/// 縣巿區鄉鎮
		/// </summary>
		public string ZIPNAME { get; set; }

		/// <summary>
		/// 郵遞區號(前三碼)
		/// </summary>
		public string ZIPCODE { get; set; }

		/// <summary>
		/// 縣巿別名稱
		/// </summary>
		public string CITYNAME { get; set; }

		/// <summary>
		/// 縣/巿別代碼
		/// </summary>
		public string CITYCODE { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string LABOR_DEPT { get; set; }

		public DBRowTableName GetTableName()
		{
			return StaticCodeMap.TableName.CLAMCITY;
		}

	}
}
