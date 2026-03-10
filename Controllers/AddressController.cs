using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Web.Mvc;
using log4net;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Commons.Filter;
using WDAIIP.WEB.Models;
using Turbo.DataLayer;
using Turbo.Commons;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// 這個類集中放置一些 Ajax 動作會用的的下拉代碼清單控制 action  
    /// </summary>
    /// [LoginRequired]
    public class AddressController : WDAIIP.WEB.Controllers.BaseController
    {
        /// <summary>
        /// Ajax查詢手KEY縣鎮市區代碼，回縣市代碼
        /// </summary>
        /// <param name="ZIP">縣鎮市區代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetCityCode(string ZIP)
        {
            AjaxResultStruct result = new AjaxResultStruct();

            MyKeyMapDAO dao = new MyKeyMapDAO();

            result.data = dao.GetCityCode(ZIP);


            return Content(result.Serialize(), "applycation/json");
        }

        /// <summary>
        /// Ajax查詢 鄉鎮市區域 郵遞區號清單
        /// </summary>
        /// <param name="CountryCode"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetZip(string CountryCode)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCountyZipList(CountryCode);
            return PartialView("_SelectOptions", list);
        }
    }
}