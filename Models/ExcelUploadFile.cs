using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.Commons;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// Excel 檔案上傳類, 可用來上傳 .xls 及 .xlsx =>只有.xlsx檔可運作
    /// </summary>
    public class ExcelUploadFile : UploadFile
    {
        private static IList<AcceptFileType> acceptFileTypes = new List<AcceptFileType>();
        /// <summary>
        /// static contrustor 
        /// 用來定義 acceptFileTypes
        /// </summary>
        static ExcelUploadFile()
        {
            //acceptFileTypes.Add(AcceptFileType.XLS);
            acceptFileTypes.Add(AcceptFileType.XLSX);
        }

        /// <summary>
        /// 預設 ExcelUploadFile 建構子
        /// </summary>
        public ExcelUploadFile()
        {

        }

        /// <summary>
        /// 指定上傳檔案儲存路徑, 建構 ExcelUploadFile
        /// </summary>
        /// <param name="locationPath">相對於 ContextRoot 的路徑</param>
        public ExcelUploadFile(string locationPath) : base(locationPath)
        {

        } 

        /// <summary>
        /// 取得可接受的上傳檔案類型
        /// </summary>
        /// <returns></returns>
        public override IList<AcceptFileType> GetAcceptFileTypes()
        {
            return acceptFileTypes;
        }
    }
}