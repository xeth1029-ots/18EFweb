using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.DataLayer;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Commons;
using Turbo.Commons;

namespace WDAIIP.WEB.Controllers
{
    public class ZipController : BaseController
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);  //記錄Log
        SessionModel sm = SessionModel.Get();

        /// <summary> 使用popupDialog，首次進入不查詢 </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(string onclickHandler)
        {
            ZipViewModel model = new ZipViewModel();
            model.Form.POSTTYPE1 = "3";
            model.Form.onclickHandler = onclickHandler;
            return PartialView("ZipPopupDialog", model);
        }

        /// <summary> 查詢 </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SearchZip(ZipFormModel form)
        {
            ZipViewModel model = new ZipViewModel();
            model.Form = form;

            //處理查詢
            ShareDAO dao = new ShareDAO();

            // 設定查詢分頁資訊
            dao.SetPageInfo(form.rid, form.p);

            //var tmp= dao.QueryZip(form);
            model.Grid = dao.QueryZip(form);

            ActionResult rtn = PartialView("_ZipGridRows", model);

            return rtn;
        }

        /// <summary>
        /// Ajax 取得縣市代碼所對應的鄉鎮市區域郵遞區號清單
        /// </summary>
        /// <param name="CTID">縣市代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetZipCode(string CTID)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCountyZipList(CTID);
            list.Insert(0, new KeyMapModel { CODE = "", TEXT = "請選擇" });
            return MyCommonUtil.BuildOptionHtmlAjaxResult(list, null, null);
        }

        //public ActionResult test2(string str1) { return Content("OK"); }
        //[HttpPost]
        //public ActionResult Json(string str1) { return Content("OK"); }
        public ActionResult Test1EAT(string str1)
        {
            //"EAT", "ATE", "TAE"  TEST
            string out_x = MyCommonUtil.Test1EAT(str1);
            string txt = string.Concat("out_x:<BR>", out_x);
            return Content(txt);
        }

    }
}