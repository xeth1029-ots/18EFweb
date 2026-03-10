using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.Commons;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// MDB 檔案上傳類, 可用來.mdb =>只有.mdb檔可運作
    /// </summary>
    public class MDBUploadFile : UploadFile
    {
        private static IList<AcceptFileType> acceptFileTypes = new List<AcceptFileType>();
        /// <summary>
        /// static contrustor 
        /// 用來定義 acceptFileTypes
        /// </summary>
        static MDBUploadFile()
        {
            acceptFileTypes.Add(AcceptFileType.MDB);
        }

        /// <summary>
        /// 預設 MDBUploadFile 建構子
        /// </summary>
        public MDBUploadFile()
        {

        }

        /// <summary>
        /// 指定上傳檔案儲存路徑, 建構 MDBUploadFile
        /// </summary>
        /// <param name="locationPath">相對於 ContextRoot 的路徑</param>
        public MDBUploadFile(string locationPath) : base(locationPath)
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