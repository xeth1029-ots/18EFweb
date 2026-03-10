using WDAIIP.WEB.Commons;
using Turbo.DataLayer;
using System;
using System.ComponentModel.DataAnnotations;

namespace WDAIIP.WEB.Models.Entities
{

	/// <summary>
	/// 群組權限資料檔
	/// </summary>
	public class TblCLAMGMAPM : IDBRow
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
		/// 列印權限
		/// </summary>
		public string PRG_P { get; set; }

		/// <summary>
		/// 查詢權限
		/// </summary>
		public string PRG_Q { get; set; }

		/// <summary>
		/// 刪除權限
		/// </summary>
		public string PRG_D { get; set; }

		/// <summary>
		/// 修改權限
		/// </summary>
		public string PRG_U { get; set; }

		/// <summary>
		/// 新增權限
		/// </summary>
		public string PRG_I { get; set; }

		/// <summary>
		/// 程式代號
		/// </summary>
		public string PRGID { get; set; }

		/// <summary>
		/// 子模組
		/// </summary>
		public string SUBMODULES { get; set; }

		/// <summary>
		/// 模組
		/// </summary>
		public string MODULES { get; set; }

		/// <summary>
		/// 系統
		/// </summary>
		public string SYS_ID { get; set; }

		/// <summary>
		/// 群組代碼
		/// </summary>
		public string GRPID { get; set; }

		/// <summary>
		/// 內/外網
		/// </summary>
		public string NETID { get; set; }

		public DBRowTableName GetTableName()
		{
			return StaticCodeMap.TableName.CLAMGMAPM;
		}

	}
}
