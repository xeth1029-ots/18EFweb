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

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// EnterClassDel 會員專區/已取消報名查詢   
    /// </summary>
    public class EnterClassDelController : LoginBaseController
    {
        //using log4net;
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: EnterClassDel
        /// <summary>
        /// 取消報名 記錄清單頁（產投+在職 已報名且未開訓）
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            EnterClassDelViewModel model = new EnterClassDelViewModel();
            WDAIIPWEBService serv = new WDAIIPWEBService();
            string idno = sm.ACID;
            DateTime birth = MyHelperUtil.TransToDateTime(sm.Birthday).Value;

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu07;

            serv.StopEnterTempMsg();

            serv.ShowEnterClassDel(ref model, idno, birth);

            return View("Index", model);
        }

    }
}