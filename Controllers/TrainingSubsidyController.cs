using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Services;
using System.Collections;
using System.Data;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using log4net;

namespace WDAIIP.WEB.Controllers
{
    public class TrainingSubsidyController : LoginBaseController
    {

        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool ValidateServerCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //using System.Net.Security;
            //using System.Security.Cryptography.X509Certificates;
            return true;
        }

        // GET: TrainingSubsidy
        [HttpGet]
        public ActionResult Index()
        {
            TrainingSubsidyViewModel model = new TrainingSubsidyViewModel();

            model.Form = new TrainingSubsidyFormModel();

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            SessionModel sm = SessionModel.Get();

            WDAIIPWEBService serv = new WDAIIPWEBService();

            string UserIdno = (!string.IsNullOrEmpty(sm.UserID) ? sm.UserID.Trim().ToUpper() : "");
            string UserBirthday = (!string.IsNullOrEmpty(sm.Birthday) ? sm.Birthday : "");
            string ocids = string.Empty;
            string idno = string.Empty;
            string birth = string.Empty;

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu07;

            model.Form.IDNO = UserIdno;
            model.Form.BIRTHDAY_TEXT = UserBirthday;

            idno = UserIdno;
            birth = UserBirthday;

            //取得系統時間（db）
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            DateTime today = keyDao.GetSysDateNow();

            //檢核是否在不提供查詢時段
            //bool i = this.ChkTimeNoUse(today);
            model.Form.CANVIEW = this.ChkTimeNoUse(today);
            if (model.Form.CANVIEW)
            {
                //先檢核是否有報名資料
                this.CheckShowStudEnterType2(idno, birth);

                //檢測是否停止報名(alert)
                //serv.StopEnterTempMsg();

                //資料查詢作業
                //model.QType = "1";

                model.NearYearDetail = new NearYearTrainDetailModel();

                //處理查詢作業
                //serv.ProcessNearYearTrain(model.NearYearDetail, idno, birth, ref ocids);

                //顯示近3年參訓課程查詢結果
                //serv.ShowNearYearTrain(model, ocids);

                //WebRequest物件如何忽略憑證問題
                System.Net.ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
                //TLS 1.2-基礎連接已關閉: 傳送時發生未預期的錯誤 
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;//3072
                //2019-02-20 改呼叫webservice（與後台系統共用）
                ojt0219ws1.GetOjt0219ws1 ws = new WEB.ojt0219ws1.GetOjt0219ws1();
                DataSet ds = new DataSet();

                //撈取學員參訓歷史（產投+職前）
                string s_err1 = string.Format("\n##ws.GetDataS3 \n ds = ws.GetDataS3(model.Form.IDNO), {0}", model.Form.IDNO);
                //logger.Debug(s_log1);
                try
                {
                    ds = ws.GetDataS3(model.Form.IDNO);
                }
                catch (Exception ex)
                {
                    s_err1 += string.Format("\n{0}", ex.Message);
                    logger.Error(s_err1, ex);
                    throw ex;
                }

                DataTable dt = null;
                if (ds != null && ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                    //model.Grid = MyCommonUtil.ConvertToList<TrainingHistoryGridModel>(dt);
                    serv.StoreSubsidyDefStdCost(model, dt); //datatable to modle
                }

                //處理補助歷程相關欄位資料顯示內容(代碼轉中文描述)
                serv.ShowNearYearTrain2(model, ws);

            }
            else
            {
                string timeNoUseS = ConfigModel.TimeNoUseS;
                string timeNoUseE = ConfigModel.TimeNoUseE;
                sm.LastResultMessage = string.Format("為使民眾報名時更為順暢，本功能於每日 {0} 到 {1} 不開放查詢，請避開本時段再做查詢，謝謝。", timeNoUseS, timeNoUseE);
            }

            return View("Index", model);
        }


        #region 檢核資料用
        /// <summary>
        /// 近3年-依身份證號與birth yyyy/MM/dd 查詢 線上報名查詢
        /// ref SHOW_StudEnterType2
        /// </summary>
        /// <param name="idno"></param>
        /// <param name="birth"></param>
        public void CheckShowStudEnterType2(string idno, string birth)
        {
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            IList<Hashtable> studEnterType2 = null;

            //ModelState.Remove("chkStudEnterType2");

            studEnterType2 = dao.QueryStudEnterType2(idno, birth);

            if (studEnterType2 == null || studEnterType2.Count == 0)
            {
                ModelState.AddModelError("chkStudEnterType2", "您沒有任何報名資料!!");
            }
        }

        /// <summary>
        /// 功能每日不開放查詢時段檢核
        /// </summary>
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
        #endregion
    }
}