using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Controllers
{
    public class ElFormSignatureController : BaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //private static string strSID = ConfigModel.SSOSystemID;

        // GET: ElFormSignature
        /// <summary>
        /// 使用者QR Code二維條碼，輸入此網址http://XXX.XXX.XXX.XXX/ElFormSignature/Index?epe=XXXXX
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            StudOnlineViewModel model = new StudOnlineViewModel();
            ActionResult rtn;

            //從網址取得身分證的加密字串
            string strELNO_encode = Request.QueryString["epe"];
            string strSOCID_encode = Request.QueryString["eps"];
            string strOCID_encode = Request.QueryString["epo"];
            string strIDNO_encode = Request.QueryString["epi"];
            if (string.IsNullOrEmpty(strELNO_encode)) { LOG.Error(new System.ArgumentNullException("epe")); return base.SetPageNotFound(); } //404錯誤畫面
            if (string.IsNullOrEmpty(strSOCID_encode)) { LOG.Error(new System.ArgumentNullException("eps")); return base.SetPageNotFound(); } //404錯誤畫面
            if (string.IsNullOrEmpty(strOCID_encode)) { LOG.Error(new System.ArgumentNullException("epo")); return base.SetPageNotFound(); } //404錯誤畫面
            if (string.IsNullOrEmpty(strIDNO_encode)) { LOG.Error(new System.ArgumentNullException("epi")); return base.SetPageNotFound(); } //404錯誤畫面

            //把加密字串解密成身分證
            string strELNO = DecodeString(strELNO_encode);
            string strSOCID = DecodeString(strSOCID_encode);
            string strOCID = DecodeString(strOCID_encode);
            string strIDNO = DecodeString(strIDNO_encode);

            LOG.Debug(string.Concat("#Index, epe:", strELNO_encode, " ,eps:", strSOCID_encode, " ,epo:", strOCID_encode, " ,epi:", strIDNO_encode));
            LOG.Debug(string.Concat("#Index, elno:", strELNO, " ,socid:", strSOCID, " ,ocid:", strOCID, " ,idno:", strIDNO));
            if (string.IsNullOrEmpty(strELNO)) { LOG.Error(new System.ArgumentNullException("strELNO")); return base.SetPageNotFound(); } //404錯誤畫面
            if (string.IsNullOrEmpty(strSOCID)) { LOG.Error(new System.ArgumentNullException("strSOCID")); return base.SetPageNotFound(); } //404錯誤畫面
            if (string.IsNullOrEmpty(strOCID)) { LOG.Error(new System.ArgumentNullException("strOCID")); return base.SetPageNotFound(); } //404錯誤畫面
            if (string.IsNullOrEmpty(strIDNO)) { LOG.Error(new System.ArgumentNullException("strIDNO")); return base.SetPageNotFound(); } //404錯誤畫面

            long iELNO = long.Parse(strELNO);
            long iSOCID = long.Parse(strSOCID);
            long iOCID = long.Parse(strOCID);

            var Form1 = new StudOnlineFormModel();
            model.Form = Form1;
            model.Form.ELNO = strELNO;
            model.Form.SOCID = strSOCID;
            model.Form.OCID = strOCID;
            model.Form.IDNO = strIDNO;

            //檢查這身分證是否已申請過
            var dao = new WDAIIPWEBDAO();
            var where = new TblSTUD_ELFORM { ELNO = iELNO, SOCID = iSOCID, OCID = iOCID, IDNO = strIDNO }; //等同SQL語法：SELECT * FROM e_young_plan WHERE ypa_id = '身份證字號'
            var item = dao.GetRow(where);
            //if (item == null) //如果沒有抓到資料，表示此登入者沒有在【計畫申請】畫面上，按過【計畫申請】按鈕。不可以做線上簽名
            //{
            //    //sm.LastResultMessage = "網址輸入的GET參數idno，它沒有按過計畫申請。"; //測試
            //    return base.SetPageNotFound(); //404錯誤畫面
            //}
            if (item != null) //檢查這 是否已做過【線上簽名】
            {
                string UploadRootPath = ConfigModel.UploadElSignPath;
                string sElFormSignPath = ConfigModel.ElFormSignPath;
                var webPath = item.FILEPATH1 ?? string.Concat(UploadRootPath, "/", sElFormSignPath);
                string savePath = Server.MapPath(webPath);
                string curFile = string.Concat(savePath, "/", item.P1_LINK);
                bool fgS2 = System.IO.File.Exists(curFile);
                if (!fgS2)
                {
                    dao.Delete(where);
                }
                else
                {
                    SessionModel sm = SessionModel.Get();
                    sm.LastResultMessage = "嗨！你已經簽過了，請關閉【線上簽名】。";
                    return View("End", model);
                }
            }
            rtn = View("Index", model);
            return rtn;
        }

        /// <summary>
        /// 使用者完成線上簽名後，要導向到完成畫面。
        /// </summary>
        /// <returns></returns>
        public ActionResult End()
        {
            //SessionModel sm = SessionModel.Get();
            //PlanApplyViewModel model = new PlanApplyViewModel();
            ActionResult rtn = View("End");
            return rtn;
        }

        /// <summary>
        /// 使用者按下【送出】按鈕
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult CheckSignature(string epe, string eps, string epo, string epi, string imageData)
        //public PartialViewResult CheckSignature(string ypaid, string imageData) 
        //public void CheckSignature(string ypaid, string imageData)
        {
            //StudOnlineViewModel.CheckArgument(this.HttpContext);
            var model = new StudOnlineViewModel();
            //SessionModel sm = SessionModel.Get();

            try
            {

                //取得身分證
                var strELNO = epe;
                var strOCID = epo;
                var iELNO = long.Parse(strELNO);
                var iSOCID = long.Parse(eps);
                var iOCID = long.Parse(epo);
                var strIDNO = epi;

                var dao = new WDAIIPWEBDAO();
                var w1 = new TblCLASS_STUDENTSOFCLASS { SOCID = iSOCID, OCID = iOCID };
                var item1 = dao.GetRow(w1);
                if (item1 == null) //如果沒有抓到資料，表示此登入者沒有在【計畫申請】畫面上，按過【計畫申請】按鈕。不可以做線上簽名
                {
                    var msg1 = "查無會員上課資訊!";
                    LOG.Error("#CheckSignature " + msg1, new System.ArgumentNullException(msg1));
                    return base.SetPageNotFound(); //404錯誤畫面
                }
                var w2 = new TblSTUD_STUDENTINFO { SID = item1.SID, IDNO = strIDNO };
                var item2 = dao.GetRow(w2);
                if (item2 == null) //如果沒有抓到資料，表示此登入者沒有在【計畫申請】畫面上，按過【計畫申請】按鈕。不可以做線上簽名
                {
                    var msg1 = "查無會員上課資訊!!";
                    LOG.Error("#CheckSignature " + msg1, new System.ArgumentNullException(msg1));
                    return base.SetPageNotFound(); //404錯誤畫面
                }
                var w3 = new TblCLASS_CLASSINFO { OCID = iOCID };
                var item3 = dao.GetRow(w3);
                if (item3 == null) //如果沒有抓到資料，表示此登入者沒有在【計畫申請】畫面上，按過【計畫申請】按鈕。不可以做線上簽名
                {
                    var msg1 = "查無會員課程資訊!";
                    LOG.Error("#CheckSignature " + msg1, new System.ArgumentNullException(msg1));
                    return base.SetPageNotFound(); //404錯誤畫面
                }

                //取得畫圖的資訊字串
                var strImageData = imageData;

                //上傳儲存簽名圖片檔的路徑。例如eYIP專案上的/Upload/PlanApplySignature/
                string UploadRootPath = ConfigModel.UploadElSignPath;
                string sElFormSignPath = ConfigModel.ElFormSignPath;
                string webPath = string.Concat(UploadRootPath, "/", sElFormSignPath, "/", item3.PLANID, "/", item3.OCID, "/");
                string savePath = HttpContext.Server.MapPath(webPath);

                //如果路徑不存在，就建立 
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                //取得存檔目錄與檔名。例如 /Upload/PlanApplySignature/A12345678_201912201021.png
                string fileName = string.Concat(strIDNO, "x", strELNO, "x", DateTime.Now.ToString("yyyyMMddHHmm"), ".png");
                var fileSavePath = Path.Combine(savePath, fileName);

                //開始儲存成簽名圖片檔
                byte[] binaryData = Convert.FromBase64String(strImageData);
                FileStream file = new FileStream(fileSavePath, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(file);
                bw.Write(binaryData);
                bw.Close();

                //進行update語法，將線上簽名相關資料，更新到資料庫。
                updateStudElForm(iELNO, iSOCID, iOCID, strIDNO, fileName, webPath); //參數= (A123456789, A12345678_201912201021.png)
            }
            catch (Exception ex)
            {
                SessionModel sm = SessionModel.Get();
                sm.LastResultMessage = "錯誤=" + ex.Message;
                LOG.Error("ElFormSignatureController CheckSignature ex:" + ex.Message, ex);
                //rtn = base.SetPageNotFound(); //404錯誤畫面
            }

            //按下【送出】按鈕後，要切換到End網頁。
            AjaxResultStruct result = new AjaxResultStruct();
            result.data = "1";
            return Content(result.Serialize(), "application/json");
        }

        //進行update命令動作，將線上簽名的相關資料，更新到資料庫e_young_plan資料表→ypa_sway欄、ypa_link欄、ypa_scdate欄。
        public void updateStudElForm(long iELNO, long iSOCID, long iOCID, string IDNO, string SignatureFileName, string filepath1)
        {
            //SessionModel sm = SessionModel.Get();
            var dao = new WDAIIPWEBDAO();
            var w1 = new TblCLASS_STUDENTSOFCLASS { SOCID = iSOCID, OCID = iOCID };
            var item = dao.GetRow(w1);
            if (item == null) //如果沒有抓到資料，表示此登入者沒有在【計畫申請】畫面上，按過【計畫申請】按鈕。不可以做線上簽名
            {
                //sm.LastResultMessage = "網址輸入的GET參數idno，它沒有按過計畫申請。"; //測試//return base.SetPageNotFound(); //404錯誤畫面
                var exA = new System.ArgumentNullException("查無學生上課資訊!");
                LOG.Error("#updateStudElForm 查無學生上課資訊!", exA);
                throw exA;
            }
            var w2 = new TblSTUD_STUDENTINFO { SID = item.SID, IDNO = IDNO };
            var item2 = dao.GetRow(w2);
            if (item2 == null) //如果沒有抓到資料，表示此登入者沒有在【計畫申請】畫面上，按過【計畫申請】按鈕。不可以做線上簽名
            {
                //sm.LastResultMessage = "網址輸入的GET參數idno，它沒有按過計畫申請。"; //測試//return base.SetPageNotFound(); //404錯誤畫面
                var exA = new System.ArgumentNullException("查無學生上課資訊!!");
                LOG.Error("#updateStudElForm 查無學生上課資訊!!", exA);
                throw exA;
            }

            //取得目前(DB)系統時間
            var keyDao = new MyKeyMapDAO();
            var nowTime = keyDao.GetSysDateNow();

            try
            {
                long iCSELNO = dao.GetNewId("STUD_ELFORM_CSELNO_SEQ,STUD_ELFORM,CSELNO").Value;
                //dao. //dao.BeginTransaction(); //把要update的線上簽名資料，寫入資料集合裡。
                var tmp = new TblSTUD_ELFORM
                {
                    CSELNO = iCSELNO,
                    ELNO = iELNO,
                    SOCID = iSOCID,
                    OCID = iOCID,
                    IDNO = IDNO,
                    P1_LINK = SignatureFileName,
                    FILEPATH1 = filepath1,
                    CREATEACCT = IDNO, //sm.ACID,
                    CREATEDATE = nowTime,
                    SIGNDACCT = IDNO, //sm.ACID,
                    SIGNDATE = nowTime,
                    MODIFYACCT = IDNO, //sm.ACID,
                    MODIFYDATE = nowTime
                };
                //開始進行insert語法，來新增資料
                dao.Insert(tmp);
                //dao.CommitTransaction();
            }
            catch (Exception ex)
            {
                //dao.RollBackTransaction();
                LOG.Error("ElFormSignatureController updatePlanApplySignature ex:" + ex.Message, ex);
                throw ex;
            }
        }


        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null) { return null; };
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        private Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }

        #region 【計畫申請】的線上簽名之Url GET字串解密
        /// <summary>字串解密</summary>
        /// <param name="toDecrypt"></param>
        /// <returns></returns>
        public static string DecodeString(string toDecrypt)
        {
            if (string.IsNullOrEmpty(toDecrypt)) { return null; };
            byte[] encodedDataAsBytes = Convert.FromBase64String(toDecrypt.Replace(" ", "+"));
            return System.Text.Encoding.UTF8.GetString(encodedDataAsBytes);
        }
        #endregion
    }
}