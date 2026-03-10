using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.ReportTK.BarCode;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Services;

namespace WDAIIP.WEB.Controllers
{
    public class StudOnlineSignController : LoginBaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: StudOnlineSign
        [HttpGet]
        public ActionResult Index()
        {
            var model = new StudOnlineViewModel();
            model.Form = new StudOnlineFormModel();

            //var serv = new WDAIIPWEBService();
            var dao = new WDAIIPWEBDAO();

            SessionModel sm = SessionModel.Get();
            string UserIdno = (!string.IsNullOrEmpty(sm.UserID) ? sm.UserID.Trim().ToUpper() : "");
            string UserBirthday = (!string.IsNullOrEmpty(sm.Birthday) ? sm.Birthday : "");

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu07;

            model.Form.IDNO = UserIdno;
            model.Form.BIRTHDAY_STR = UserBirthday;

            //取得目前(DB)系統時間
            var keyDao = new MyKeyMapDAO();
            var nowTime = keyDao.GetSysDateNow();

            //檢核是否在不提供查詢時段
            model.Form.CANVIEW = this.ChkTimeNoUse(nowTime);

            if (model.Form.CANVIEW)
            {
                //檢測是否停止報名
                //dao.StopEnterTempMsg();

                //依輸入條件進行查詢
                model.Grid = dao.QueryStudOnlineSign1(model.Form);

            }
            else
            {
                string timeNoUseS = ConfigModel.TimeNoUseS;
                string timeNoUseE = ConfigModel.TimeNoUseE;
                sm.LastResultMessage = string.Format("為使民眾報名時更為順暢，本功能於每日 {0} 到 {1} 不開放查詢，請避開本時段再做查詢，謝謝。", timeNoUseS, timeNoUseE);
            }

            return View("Index", model);

        }

        //DetailF
        [HttpPost]
        public ActionResult DetailF(string SOCID, string OCID, string SID)
        {
            //把加密字串解密
            //var vSOCID = ConfigModel.DecodeString(SOCID);
            //var vOCID = ConfigModel.DecodeString(OCID);
            //var vSID = ConfigModel.DecodeString(SID);

            var model = new StudOnlineViewModel();
            model.Form = new StudOnlineFormModel();

            //var serv = new WDAIIPWEBService();
            var dao = new WDAIIPWEBDAO();

            SessionModel sm = SessionModel.Get();
            string UserIdno = (!string.IsNullOrEmpty(sm.UserID) ? sm.UserID.Trim().ToUpper() : "");
            string UserBirthday = (!string.IsNullOrEmpty(sm.Birthday) ? sm.Birthday : "");

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu07;

            model.Form.IDNO = UserIdno;
            model.Form.BIRTHDAY_STR = UserBirthday;
            model.Form.SOCID = SOCID; //SOCID;
            model.Form.OCID = OCID; //OCID;
            model.Form.SID = SID; //SID;

            //取得目前(DB)系統時間
            var keyDao = new MyKeyMapDAO();
            var nowTime = keyDao.GetSysDateNow();

            //檢核是否在不提供查詢時段
            model.Form.CANVIEW = this.ChkTimeNoUse(nowTime);

            if (model.Form.CANVIEW)
            {
                //檢測是否停止報名
                //dao.StopEnterTempMsg();

                //依輸入條件進行查詢
                model.Grid2 = dao.QueryStudOnlineSign2(model.Form);

            }
            else
            {
                string timeNoUseS = ConfigModel.TimeNoUseS;
                string timeNoUseE = ConfigModel.TimeNoUseE;
                sm.LastResultMessage = string.Format("為使民眾報名時更為順暢，本功能於每日 {0} 到 {1} 不開放查詢，請避開本時段再做查詢，謝謝。", timeNoUseS, timeNoUseE);
            }

            return View("Index2", model);

        }

        /// <summary>使用者按下【線上簽名】按鈕後，跳出QR Code二維條碼畫面</summary>
        /// <param name="s_UUID"></param>
        /// <returns></returns>
        public ActionResult submitGenerateQRCode(string qELNO, string qSOCID, string qOCID, string qSID)
        {
            ////LOG.Info(string.Format("Get /ClassSearch/Detail , YPAID={0}", YPAID));
            SessionModel sm = SessionModel.Get();
            var dao = new WDAIIPWEBDAO();
            var model = new StudOnlineViewModel();
            ActionResult rtn;

            var w1 = new TblSTUD_STUDENTINFO { SID = qSID };
            var o1 = dao.GetRow(w1);
            if (o1 == null) { throw new Exception("SID 格式錯誤"); }
            var strIDNO = o1.IDNO;

            //把字號做成加密字串
            string ELNO_encodestring = EncodeString(qELNO);
            string SOCID_encodestring = EncodeString(qSOCID);
            string OCID_encodestring = EncodeString(qOCID);
            string IDNO_encodestring = EncodeString(strIDNO);

            //組合【電子簽名】用的網址。例如 http://XXX.XXX.XXX.XXX/ElFormSignature/Index?epe=X&eps=X&epo=X&epi=XXXXX
            var UrlCx = string.Concat("~/ElFormSignature/Index?epe=", ELNO_encodestring, "&eps=", SOCID_encodestring, "&epo=", OCID_encodestring, "&epi=", IDNO_encodestring);

            string ElFormSignature_URL = string.Concat(Request.Url.Scheme, "://", Request.Url.Authority, Url.Content(UrlCx));
            //bLOG.Debug(string.Concat("#submitGenerateQRCode: ElFormSignature_URL: \n", ElFormSignature_URL));

            //輸入文字，得到QR Code二維條碼的byte資料
            byte[] data = BarCodeUtils.GenerateQRCode(ElFormSignature_URL);
            System.Drawing.Image oImage = null;
            System.Drawing.Bitmap oBitmap = null;

            try
            {
                MemoryStream oMemoryStream = new MemoryStream(data);
                //設定資料流位置
                oMemoryStream.Position = 0;
                oImage = System.Drawing.Image.FromStream(oMemoryStream);
                //建立副本
                oBitmap = new System.Drawing.Bitmap(oImage);

                //做成要輸出到view之<img>用的src字串
                ViewBag.QRCodeImage = "data:image/png;base64," + Convert.ToBase64String(oMemoryStream.ToArray());
            }
            catch (Exception ex)
            {
                bLOG.Error(string.Concat("submitGenerateQRCode():", ex.Message), ex);
                throw ex;
            }

            var openhref2 = string.Concat("<a target=\"_blank\" href=\"", ElFormSignature_URL, "\">OPEN-QRCODE</a><br>");
            var LRMsg1 = string.Concat("請注意~此為「個人」線上簽名<br><br><br>", "請使用手機拍照輸入QR Code二維條碼，進行電子簽名<br>(完成後請重新整理網頁)<br>", "<img src=", ViewBag.QRCodeImage, " /><br>", openhref2);

            //彈出訊息，顯示QR條碼(正式)
            sm.LastResultMessage = LRMsg1;

            //rtn = RedirectToAction("Index", "StudOnlineSign"); //重新導向
            rtn = DetailF(qSOCID, qOCID, qSID);

            return rtn;
        }

        /// <summary>
        /// 使用者按下【線上簽名】按鈕後，如果已完成線上簽名過，就顯示簽名圖片
        /// </summary>
        /// <param name="s_UUID"></param>
        /// <returns></returns>
        public ActionResult submitShowSignatureImage(string hELNO, string hSOCID, string hOCID, string hSID)
        {
            //bLOG.Info(string.Format("submitShowSignatureImage , hELNO={0}", hELNO));
            SessionModel sm = SessionModel.Get();
            var model = new StudOnlineViewModel();
            ActionResult rtn;

            //查詢線上簽名資料
            var dao = new WDAIIPWEBDAO();
            var w1 = new TblSTUD_STUDENTINFO { SID = hSID };
            var o1 = dao.GetRow(w1);
            if (o1 == null) { throw new Exception("SID 格式錯誤"); }

            var strIDNO = o1.IDNO;
            var strOCID = hOCID;

            long iELNO = long.Parse(hELNO);
            long iSOCID = long.Parse(hSOCID);
            long iOCID = long.Parse(hOCID);
            var where = new TblSTUD_ELFORM { ELNO = iELNO, SOCID = iSOCID, OCID = iOCID, IDNO = strIDNO };
            var item = dao.GetRow(where);

            if (item == null || string.IsNullOrEmpty(item.P1_LINK))
            {
                sm.LastResultMessage = "嗨！你還沒有線上簽名，所以查無資料！";

                rtn = DetailF(hSOCID, hOCID, hSID);

                return rtn;
            }

            //如果有線上簽名資料。//簽名檔位置
            string strP1_LINK = (item != null) ? item.P1_LINK : "";

            //string saveLocation = ConfigModel.UploadTempPath;
            string sElFormSignPath = ConfigModel.ElFormSignPath;

            //string uploadElFormSignPath = string.Concat(HttpContext.Server.MapPath(saveLocation), "/", sElFormSignPath, "/", strOCID, "/");
            string uploadSignPath = string.Concat(ConfigModel.UploadTempPath, "/", sElFormSignPath, "/", strOCID, "/");

            //組合顯示簽名圖片的路徑
            string showImage_Path = string.Concat(Url.Content(uploadSignPath), strP1_LINK);

            //彈出訊息，顯示簽名圖片
            sm.LastResultMessage = string.Concat("<img src=", showImage_Path, " width=500 />");

            //rtn = RedirectToAction("Index", "StudOnlineSign"); //重新導向至計畫申請的Index
            rtn = DetailF(hSOCID, hOCID, hSID);

            return rtn;
        }

        /// <summary> 資料維護頁 </summary>
        /// <returns></returns>
        public ActionResult Detail(string dhELNO, string dhSOCID, string dhOCID, string dhSID)
        {
            //LOG.Debug("GET EnterType/Detail");

            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            string UserIdno = (!string.IsNullOrEmpty(sm.UserID) ? sm.UserID.Trim().ToUpper() : "");
            string UserBirthday = (!string.IsNullOrEmpty(sm.Birthday) ? sm.Birthday : "");

            var dao = new WDAIIPWEBDAO();
            var w1 = new TblSTUD_STUDENTINFO { SID = dhSID };
            var o1 = dao.GetRow(w1);
            if (o1 == null) { throw new Exception("SID/dhSID 格式錯誤"); }
            if (o1.IDNO != sm.ACID) { throw new Exception("IDNO/ACID 查詢資料錯誤"); }

            var model = new StudOnlineViewModel();
            model.Detail = new StudOnlineDetailModel() { IsNew = false };
            model.Form = new StudOnlineFormModel
            {
                IDNO = UserIdno,
                BIRTHDAY_STR = UserBirthday,
                SOCID = dhSOCID,
                OCID = dhOCID,
                SID = dhSID,
                ELNO = dhELNO
            };

            //var serv = new WDAIIPWEBService();
            var w2 = new TblSTUD_ENTERTEMP4 { IDNO = o1.IDNO };
            var o2 = dao.GetRow(w2);
            if (o2 == null)
            {
                var serv = new WDAIIPWEBService();

                serv.LoadEnterData(model);

                if (model != null && model.Detail != null)
                {
                    model.Detail.IsNew = true;
                    MyCommonUtil.HtmlDecode(model.Detail); //2019-01-18 fix 中文編碼轉置問題（&#XXXXX;）
                    rtn = View("Detail", model);
                    return rtn;
                }
            }

            model.Detail.IsNew = false;
            model.Detail.ESETID4 = o2.ESETID4;

            model.Detail.IDNO = o2.IDNO;
            model.Detail.NAME = HttpUtility.HtmlDecode(o2.NAME);
            model.Detail.CONTACTPHONE = o2.CONTACTPHONE;
            model.Detail.ZIPCODE1 = o2.ZIPCODE1;
            model.Detail.ZIPCODE1_6W = o2.ZIPCODE1_6W;
            model.Detail.ZIPCODE1_2W = MyCommonUtil.GET_ZIPCODE2W(o2.ZIPCODE1_6W, null);
            model.Detail.ADDRESS = o2.ADDRESS;
            model.Detail.ISAGREE = o2.ISAGREE;
            model.Detail.MODIFYACCT = o2.MODIFYACCT;
            model.Detail.MODIFYDATE = o2.MODIFYDATE;

            MyCommonUtil.HtmlDecode(model.Detail); //2019-01-18 fix 中文編碼轉置問題（&#XXXXX;）
            rtn = View("Detail", model);
            return rtn;
        }

        /// <summary>SaveDetail</summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveDetail(StudOnlineViewModel model)
        {
            //LOG.Debug("GET EnterType/Detail");
            ActionResult rtn = View("Detail", model);
            SessionModel sm = SessionModel.Get();

            string UserIdno = (!string.IsNullOrEmpty(sm.UserID) ? sm.UserID.Trim().ToUpper() : "");
            string UserBirthday = (!string.IsNullOrEmpty(sm.Birthday) ? sm.Birthday : "");
            var D3 = model.Detail;
            var F3 = model.Form;
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            //取得目前(DB)系統時間
            var keyDao = new MyKeyMapDAO();
            var nowTime = keyDao.GetSysDateNow();

            try
            {
                var fg1 = D3.ZIPCODE1.HasValue;
                var fg2 = !string.IsNullOrEmpty(D3.ZIPCODE1_2W);
                var vZIPCODE1_6W = (fg1 && fg2) ? string.Concat(D3.ZIPCODE1, D3.ZIPCODE1_2W) : null;
                if (!string.IsNullOrEmpty(D3.ZIPCODE1_2W) && D3.ZIPCODE1_2W.Length > 2) { D3.ZIPCODE1_2W = null; }

                // 表單欄位檢核
                model.Valid(ModelState);

                if (ModelState.IsValid)
                {
                    ModelState.Clear();

                    var w1 = new TblSTUD_ENTERTEMP4 { IDNO = UserIdno };
                    var o1 = dao.GetRow(w1);
                    if (o1 != null)
                    {
                        var u2 = new TblSTUD_ENTERTEMP4
                        {
                            CONTACTPHONE = D3.CONTACTPHONE,
                            ZIPCODE1 = D3.ZIPCODE1,
                            ZIPCODE1_2W = D3.ZIPCODE1_2W,
                            ZIPCODE1_6W = vZIPCODE1_6W,
                            ZIPCODE1_N = D3.ZIPCODE1_N,
                            ADDRESS = D3.ADDRESS,
                            ISAGREE = D3.ISAGREE,
                            MODIFYACCT = sm.UserID,
                            MODIFYDATE = nowTime
                        };
                        var w2 = new TblSTUD_ENTERTEMP4 { ESETID4 = o1.ESETID4 };
                        dao.Update(u2, w2);
                    }
                    else
                    {
                        var iESETID4 = dao.GetNewId("STUD_ENTERTEMP4_ESETID4_SEQ,STUD_ENTERTEMP4,ESETID4").Value; //dao.GetAutoNum()
                        var i2 = new TblSTUD_ENTERTEMP4
                        {
                            ESETID4 = iESETID4,
                            IDNO = sm.ACID,
                            NAME = sm.UserName,
                            CONTACTPHONE = D3.CONTACTPHONE,
                            ZIPCODE1 = D3.ZIPCODE1,
                            ZIPCODE1_2W = D3.ZIPCODE1_2W,
                            ZIPCODE1_6W = vZIPCODE1_6W,
                            ZIPCODE1_N = D3.ZIPCODE1_N,
                            ADDRESS = D3.ADDRESS,
                            ISAGREE = D3.ISAGREE,
                            MODIFYACCT = sm.UserID,
                            MODIFYDATE = nowTime
                        };
                        dao.Insert(i2);
                    }

                    //sm.LastResultMessage = "資料儲存成功，請簽名";
                    //sm.RedirectUrlAfterBlock = "";
                    //rtn = RedirectToAction("Index", "ClassSearch");
                    rtn = submitGenerateQRCode(F3.ELNO, F3.SOCID, F3.OCID, F3.SID);
                }
            }
            catch (Exception ex)
            {
                string s_ErrorMessage = string.Concat("儲存失敗，儲存資料異常 或 資料庫異常，請重試!!<br>", "請再試一次，造成您不便之處，還請見諒。<br>", "(若持續出現此問題，請聯絡系統管理者)!!!");
                sm.LastErrorMessage = s_ErrorMessage;
                LOG.Error(string.Concat("#SaveDetail ex:", ex.Message, "\n", s_ErrorMessage), ex);
            }

            return rtn;
        }

        /// <summary>功能每日不開放查詢時段檢核</summary>
        /// <param name="nowTime"></param>
        private bool ChkTimeNoUse(DateTime nowTime)
        {
            string chkTime = nowTime.ToString("HH:mm");
            string timeNoUseS = ConfigModel.TimeNoUseS;
            string timeNoUseE = ConfigModel.TimeNoUseE;

            bool chkResult = false;  //預設不開放查詢

            if (!(string.Compare(timeNoUseS, chkTime) <= 0 && string.Compare(timeNoUseE, chkTime) >= 0))
                chkResult = false;

            // ChkTimeNoUse
            if (timeNoUseS.CompareTo(timeNoUseE) <= 0)
            {
                if (chkTime.CompareTo(timeNoUseS) <= 0)
                    chkResult = true;
                if (chkTime.CompareTo(timeNoUseE) >= 0)
                    chkResult = true;
            }

            return chkResult;
        }

        //字號做成加密字串
        public static string EncodeString(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.UTF8.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }

    }
}
