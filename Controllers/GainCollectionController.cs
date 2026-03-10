using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>GainCollection 最新消息/成果集錦</summary>
    public class GainCollectionController : ContentController
    {
        /// <summary>
        ///  取得類別項目代碼
        /// </summary>
        /// <returns></returns>
        public override string GetFunID()
        {
            return "001";
        }

        /// <summary>
        /// 取得子類別項目代碼
        /// </summary>
        /// <returns></returns>
        public override string GetSubFunID()
        {
            return "3"; /*成果集錦*/
        }
    }
}