using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Commons.Filter;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// 共用的檔案轉存 Controller
    /// </summary>
    [LoginRequired]
    public class ExportController : WDAIIP.WEB.Controllers.BaseController
    {

        /// <summary>
        /// 將前端 Browser 中透過 PrintGrid() 或 PrintDetail() 產生的列印頁面內容, 打包成 Word Header 後返回檔案內容
        /// </summary>
        /// <param name="expContent">要轉存的檔案內容(在前端頁面中 PrintGrid 或 PrintDetail 產生的內容)</param>
        /// <param name="expName">要匯出的檔案名稱</param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult Word(string expContent, string expName)
        {
            return Export(expContent, expName, "doc");
        }

        /// <summary>
        /// 將前端 Browser 中透過 PrintGrid() 或 PrintDetail() 產生的列印頁面內容, 打包成 Excel Header 後返回檔案內容
        /// </summary>
        /// <param name="expContent">要轉存的檔案內容(在前端頁面中 PrintGrid 或 PrintDetail 產生的內容)</param>
        /// <param name="expName">要匯出的檔案名稱</param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false)]
        public ActionResult Excel(string expContent, string expName)
        {
            return Export(expContent, expName, "xls");
        }

        private ActionResult Export(string expContent, string expName, string fileType)
        {
            string contentType = "application/msword";

            if (string.IsNullOrEmpty(expName))
            {
                expName = "Exp" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }
            if (string.IsNullOrEmpty(fileType))
            {
                fileType = "doc";
            }

            if ("xls".Equals(fileType))
            {
                contentType = "application/vnd.ms-excel";
            }

            string UrlBase = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);
            string BootstrapCSS = UrlBase + UrlHelper.GenerateContentUrl("~/Content/bootstrap3-3-6.min.css", HttpContext);
            string BaseCSS = UrlBase + UrlHelper.GenerateContentUrl("~/Assets/css/base.css", HttpContext);
            string PrintCSS = UrlBase + UrlHelper.GenerateContentUrl("~/Assets/css/printDOC.css", HttpContext);

            Response.AddHeader("Content-Disposition", string.Format("filename={0}.{1}", expName, fileType));
            Response.ContentType = contentType;

            Response.Write(string.Format("<!DOCTYPE html><html><head>\n<title>轉存: {0}.{1}</title>\n", expName, fileType));

            Response.Write(string.Format("<link href=\"{0}\" rel=\"stylesheet\"/>\n", BootstrapCSS));
            Response.Write(string.Format("<link href=\"{0}\" rel=\"stylesheet\"/>\n", BaseCSS));
            Response.Write(string.Format("<link href=\"{0}\" rel=\"stylesheet\"/>\n", PrintCSS));

            Response.Write("</head>\n<body>\n");
            Response.Write(expContent);
            Response.Write("</body>\n</html>\n");
            Response.Flush();
            return null;
        }
	}
}