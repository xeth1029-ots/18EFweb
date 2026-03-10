using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// /News 最新消息/焦點消息
    /// </summary>
    public class NewsController : ContentController
    {
        // GET: News
        /// <summary>
        ///  取得類別項目代碼
        /// </summary>
        /// <returns></returns>           
        public override string GetFunID()
        {
            return "001";
        }

        /// <summary>
        ///  取得子類別代碼
        /// </summary>
        /// <returns></returns>
        public override string GetSubFunID()
        {
            return "1"; /*焦點消息*/
        }
    }
}