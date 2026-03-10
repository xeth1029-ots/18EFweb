using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// /Plan 最新消息/計畫公告
    /// </summary>
    public class PlanController : ContentController
    {
        // GET: Plan
        /// <summary>
        /// 取得類別代碼
        /// </summary>
        /// <returns></returns>
        public override string GetFunID()
        {
            return "001"; //ref:tb_content.funid
        }

        /// <summary>
        /// 取得子類別代碼
        /// </summary>
        /// <returns></returns>
        public override string GetSubFunID()
        {
            return "2"; //ref:tb_content.sub_funid /*計畫公告*/
        }
    }
}