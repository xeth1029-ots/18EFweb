using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.Commons;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 檔案上傳類, 可用來上傳 .xls
    /// </summary>
    public class XlsUploadFile : UploadFile
    {
        private static IList<AcceptFileType> acceptFileTypes = new List<AcceptFileType>();

        /// <summary>
        /// static contrustor 
        /// 用來定義 acceptFileTypes
        /// </summary>
        static XlsUploadFile()
        {
            acceptFileTypes.Add(AcceptFileType.XLSX);
            acceptFileTypes.Add(AcceptFileType.XLS);
        }

        /// <summary>
        /// 預設 XlsUploadFile 建構子
        /// </summary>
        public XlsUploadFile()
        {

        }

        /// <summary>
        /// 指定上傳檔案儲存路徑, 建構 XlsUploadFile
        /// </summary>
        /// <param name="locationPath">相對於 ContextRoot 的路徑</param>
        public XlsUploadFile(string locationPath) : base(locationPath)
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