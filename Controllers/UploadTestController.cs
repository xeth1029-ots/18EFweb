using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DiffMatchPatch;
using log4net;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// 多選檔案上傳範例
    /// </summary>
    public class UploadTestController : Controller
    {
        protected ILog LOG = LogManager.GetLogger(typeof(UploadTestController));

        public ActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// (非同步)檔案上傳接收 Action Method,
        /// 在挑選多個檔案時, 這個 method 會被呼叫多次(每一個檔案呼叫1次)
        /// 上傳處理成功後, 回應單一字串 "ok" 或 "success" 即可,
        /// 若處理發生錯誤, 可以丟出 Exception 或回傳明確的 HttpStatusCodeResult()
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Upload(string ID, string CAT)
        {
            // Post 進來的非檔案參數, 可用 model 去 binding 接收
            // 或用一般的參數串列去接收
            // 可用來辨識此次上傳檔案的關聯資訊(如: 檔案所屬資料的ID)

            // 1. 存儲上傳的檔案

            // 上傳檔案儲存的路徑
            string saveLocation = WDAIIP.WEB.Models.ConfigModel.UploadTempPath;
            string savePath = HttpContext.Server.MapPath(saveLocation);

            try
            {
                // 配合 _FileUpload 多檔案上傳的機制時,
                // 上傳檔案無法直接 Binding 到 Model 中,
                // 要自行逐一處理
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    if (!string.IsNullOrEmpty(file.FileName)
                        && file.ContentLength > 0)
                    {
                        // 上傳檔案儲存的名稱(依系統業務邏輯去命名)
                        string saveFileName = ID + "_" + file.FileName;
                        string saveFullPath = savePath + "\\" + saveFileName;

                        LOG.Debug("Upload : " + file.FileName + " => " + saveFullPath );
                        file.SaveAs(saveFullPath);
                    }
                }

                // 2. 呼叫 DAO 儲存檔案資訊到 DB, 依業務功能自行定義 DAO
                // TODO


                return Content("success");
            }
            catch (Exception ex)
            {
                LOG.Error("Upload Failed: " + ex.Message, ex);
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        public ActionResult UpTest1()
        {
            //UploadTest/UpTest1 http://localhost:2866/UploadTest/UpTest1
            return Content(Diff_TEST1());
        }

        string Diff_TEST1() {

            string oldText = "這是一個測試。這是要刪除的部分。這是未變更的部分。";
            string newText = "這是一個測試。這是新增的內容。這是未變更的部分。";

            diff_match_patch dmp = new diff_match_patch();
            List<Diff> diffs = dmp.diff_main(oldText, newText);
            dmp.diff_cleanupSemantic(diffs);

            StringBuilder formattedText = new StringBuilder();

            foreach (Diff diff in diffs)
            {
                if (diff.operation == Operation.DELETE)
                {
                    formattedText.Append($"<span style='color: red; text-decoration: line-through;'>{diff.text}</span>");
                }
                else if (diff.operation == Operation.INSERT)
                {
                    formattedText.Append($"<span style='color: blue; font-weight: bold;'>{diff.text}</span>");
                }
                else
                {
                    formattedText.Append(diff.text);
                }
            }
            //Console.WriteLine("HTML 格式輸出:");
            //Console.WriteLine(formattedText.ToString());
            return formattedText.ToString();

        }

        void Test_loaddata() {
            
        }
    }
}