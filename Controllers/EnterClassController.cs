using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using WDAIIP.WEB.Services;
using log4net;
using System.Collections;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// /EnterClass 會員專區/報名查詢及取消
    /// </summary>
    public class EnterClassController : LoginBaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: EnterClass
        /// <summary>
        /// 報名記錄清單頁（產投+在職 已報名且未開訓）
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            EnterClassViewModel model = new EnterClassViewModel();
            WDAIIPWEBService serv = new WDAIIPWEBService();
            string idno = sm.ACID;
            DateTime birth = MyHelperUtil.TransToDateTime(sm.Birthday).Value;

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu07;

            serv.StopEnterTempMsg();

            serv.ShowEnterClass(ref model, idno, birth);

            return View("Index", model);
        }

        /// <summary>
        /// 取消報名
        /// </summary>
        /// <param name="esernum"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveCancel(string tplanid, Int64? esetid, Int64? esernum, Int64? ocid1)
        {
            int tplanidnum = 0;

            if (string.IsNullOrEmpty(tplanid)) { throw new ArgumentNullException("tplanid 不可為null"); }

            if (!int.TryParse(tplanid, out tplanidnum)) { throw new ArgumentException("tplanid 格式錯誤"); }

            if (!esetid.HasValue) { throw new ArgumentNullException("esetid 不可為null"); }

            if (!esernum.HasValue) { throw new ArgumentNullException("esernum 不可為null"); }

            if (!ocid1.HasValue) { throw new ArgumentNullException("ocid1 不可為null"); }

            ActionResult rtn = null;
            SessionModel sm = SessionModel.Get();
            string idno = sm.ACID;
            WDAIIPWEBService serv = new WDAIIPWEBService();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            TblSTUD_ENTERTYPE2 enterType2Info = null;
            TblSTUD_ENTERTEMP2 enterTemp2data = null;

            rtn = Index();

            //再確認一次 報名資料
            enterTemp2data = dao.GetEnterTemp2ByeSETID(esetid.Value);
            if (enterTemp2data == null)
            {
                string exMsg1 = "查無報名資料，取消報名失敗！";
                Exception ex = new Exception(exMsg1);
                LOG.Error(exMsg1, ex);
                sm.LastErrorMessage = exMsg1;//取消報名失敗
                return rtn;
            }
            //再確認一次 報名資訊
            enterType2Info = dao.GetEnterType2ByESERNUM(esetid.Value, esernum.Value, ocid1.Value);
            if (enterType2Info == null)
            {
                string exMsg1 = "查無報名資訊，取消報名失敗！";
                Exception ex = new Exception(exMsg1);
                LOG.Error(exMsg1, ex);
                sm.LastErrorMessage = exMsg1;//取消報名失敗
                return rtn;
            }
            //再確認一次 報名狀態 true 可刪除 false 不可刪除
            bool fg_CAN_DEL = (enterType2Info.SIGNUPSTATUS.Value == 0) ? true : (enterType2Info.SIGNUPSTATUS.Value == 4) ? true : false;
            if (!fg_CAN_DEL)
            {
                string exMsg1 = "報名狀態已改變，取消報名失敗！";
                Exception ex = new Exception(exMsg1);
                LOG.Error(exMsg1, ex);
                sm.LastErrorMessage = exMsg1;//取消報名失敗
                return rtn;
            }

            //再確認一次 班級學員資料
            TblSTUD_STUDENTINFO studinfo = dao.QueryStudentinfo(new TblSTUD_STUDENTINFO() { IDNO = idno });
            if (studinfo != null)
            {
                bool flag_isStudent = dao.chkStudent1(ocid1.Value, studinfo.SID);
                if (flag_isStudent)
                {
                    string exMsg1 = "班級學員資料已錄取，取消報名失敗！";
                    Exception ex = new Exception(exMsg1);
                    LOG.Error(exMsg1, ex);
                    sm.LastErrorMessage = exMsg1;//取消報名失敗
                    return rtn;
                }
            }

            bool flag_cancel_ok = false;//取消報名失敗／成功

            //刪除現場報名資料 (有值的話)
            if (enterType2Info.SETID.HasValue && enterType2Info.ENTERDATE.HasValue && enterType2Info.SERNUM.HasValue)
            {
                //Hashtable parms = new Hashtable();
                decimal setid = enterType2Info.SETID.Value;
                DateTime enterdate = enterType2Info.ENTERDATE.Value;
                decimal sernum = enterType2Info.SERNUM.Value;
                TblSTUD_ENTERTYPE enterTypeInfo = dao.GetEnterTypeBySETID(setid, enterdate, sernum);
                if (enterTypeInfo != null && enterTypeInfo.OCID1.HasValue && enterTypeInfo.ESERNUM.HasValue && enterTypeInfo.ESETID.HasValue
                    && enterType2Info.OCID1.Value == enterTypeInfo.OCID1.Value && enterType2Info.ESERNUM.Value == enterTypeInfo.ESERNUM.Value
                    && enterType2Info.ESETID.Value == enterTypeInfo.ESETID.Value)
                {
                    //比對班級,比對報名序號 無誤，啟動刪除
                    object mutex = new object(); //lock (mutex) {}
                    lock (mutex)
                    {
                        serv.procCancelEnter(dao, tplanid, setid, enterdate, sernum, ocid1.Value);
                    }
                    flag_cancel_ok = true;
                }
            }

            try
            {
                //刪除E網報名資料
                object mutex = new object(); //lock (mutex) {}
                lock (mutex)
                {
                    if (enterType2Info != null) { serv.procCancelEnter2(dao, tplanid, esetid.Value, esernum.Value, ocid1.Value); }
                }
                flag_cancel_ok = true;
            }
            catch (Exception ex)
            {
                //異常
                string exMsg1 = "取消報名失敗！";
                string str_error = "EnterClassController.SaveCancel: ";
                str_error += $"\n exMsg1:[{exMsg1}]";
                str_error += $"\n esernum:[{esernum.ToString()}]";
                str_error += $"\n ocid1:[{ocid1.ToString()}]";
                str_error += $"\n ex:{ex.Message}";
                LOG.Error(str_error, ex);
                sm.LastErrorMessage = exMsg1;
            }

            if (flag_cancel_ok)
            {
                sm.LastResultMessage = "取消報名成功";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "EnterClass");
            }

            return rtn;
        }
    }
}