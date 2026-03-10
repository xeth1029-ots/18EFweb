using WDAIIP.WEB.Commons;
using Turbo.DataLayer;
using System;
using System.ComponentModel.DataAnnotations;

namespace WDAIIP.WEB.Models.Entities
{

	/// <summary>
	/// 群組資料檔
	/// </summary>
	public class TblCLAMGRP : IDBRow
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
        /// 群組名稱
        /// </summary>
        [Display(Name = "群組名稱")]
        [Required]
        public string GRPNAME { get; set; }

        /// <summary>
        /// 群組代號
        /// </summary>
        [Display(Name = "群組代號")]
        [Required]
        public string GRPID { get; set; }

		public DBRowTableName GetTableName()
		{
			return StaticCodeMap.TableName.CLAMGRP;
		}

	}
}
