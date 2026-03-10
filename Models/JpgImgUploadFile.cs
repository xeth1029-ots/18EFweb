using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.Commons;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 圖片檔案上傳類, 可用來上傳 .jpg
    /// </summary>
    public class JpgImgUploadFile : UploadFile
    {
        private static IList<AcceptFileType> acceptFileTypes = new List<AcceptFileType>();

        /// <summary>
        /// static contrustor 
        /// 用來定義 acceptFileTypes
        /// </summary>
        static JpgImgUploadFile()
        {
            acceptFileTypes.Add(AcceptFileType.JPG);
        }

        /// <summary>
        /// 預設 JpgImgUploadFile 建構子
        /// </summary>
        public JpgImgUploadFile()
        {

        }

        /// <summary>
        /// 指定上傳檔案儲存路徑, 建構 ImgUploadFile
        /// </summary>
        /// <param name="locationPath">相對於 ContextRoot 的路徑</param>
        public JpgImgUploadFile(string locationPath) : base(locationPath)
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