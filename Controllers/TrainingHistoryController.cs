using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Services;


namespace WDAIIP.WEB.Controllers
{
    public class TrainingHistoryController : LoginBaseController
    {
        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            TrainingHistoryViewModel model = new TrainingHistoryViewModel();
            model.Form = new TrainingHistoryFormModel();

            WDAIIPWEBService serv = new WDAIIPWEBService();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            SessionModel sm = SessionModel.Get();
            string UserIdno = (!string.IsNullOrEmpty(sm.UserID) ? sm.UserID.Trim().ToUpper() : "");
            string UserBirthday = (!string.IsNullOrEmpty(sm.Birthday) ? sm.Birthday : "");

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu07;

            model.Form.IDNO = UserIdno;
            model.Form.BIRTHDAY_STR = UserBirthday;

            //取得目前(DB)系統時間
            MyKeyMapDAO keyDao = new MyKeyMapDAO();
            DateTime nowTime = keyDao.GetSysDateNow();

            //檢核是否在不提供查詢時段
            model.Form.CANVIEW = this.ChkTimeNoUse(nowTime);

            if (model.Form.CANVIEW)
            {
                //檢測是否停止報名
                //dao.StopEnterTempMsg();

                //依輸入條件進行查詢
                //model.Grid = dao.QueryTrainingHistory(model.Form);
                //顯示參訓歷史資料
                //serv.ShowHistoryDefStdCost(model);

                //2019-03-05 呼叫職前系統webservice，以顯示職前相關參訓記錄
                //資料顯示邏輯比照「學員動態管理>>招生管理>>e網報名審核」的
                serv.ShowTrainingHistory(model);

                //依輸入條件進行查詢(因webservice回傳資料已含產投，故只要再查自辦在職)
                /*model.Grid06 = dao.QueryTrainingHistory06(model.Form);
                
                //2019-02-20 改呼叫webservice（與後台系統共用）
                ojt0219ws1.GetOjt0219ws1 ws = new WEB.ojt0219ws1.GetOjt0219ws1();
                DataSet ds = new DataSet();
                DataTable dt = null;

                //撈取學員參訓歷史（產投+職前）
                ds = ws.GetDataS3(model.Form.IDNO);

                if (ds != null && ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];

                    //model.Grid = MyCommonUtil.ConvertToList<TrainingHistoryGridModel>(dt);
                    serv.StoreHistoryDefStdCost(model, dt);
                }
                
                //課程合併顯示（產投+職前（托育..）+自辦在職）
                serv.MergeHistoryDefStdCost(model);
                serv.ShowHistoryDefStdCost2(model,ws);
                */
            }
            else
            {
                string timeNoUseS = ConfigModel.TimeNoUseS;
                string timeNoUseE = ConfigModel.TimeNoUseE;
                sm.LastResultMessage = string.Format("為使民眾報名時更為順暢，本功能於每日 {0} 到 {1} 不開放查詢，請避開本時段再做查詢，謝謝。", timeNoUseS, timeNoUseE);
            }

            return View("Index", model);
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
    }
}