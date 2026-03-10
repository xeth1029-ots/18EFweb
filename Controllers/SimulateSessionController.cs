using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Controllers
{
    public class SimulateSessionController : Controller
    {
        public ActionResult Index()
        {
            return new HttpNotFoundResult();
        }

        /// <summary>
        /// 用來模擬指定IDNO的會員登入(在 E_MEMBER 中必須有值)
        /// <para>此功能只有在本機環境才能運作</para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Login(string id)
        {
            string iXdX = SecurityHelper.FullSanitize(id);
            if (string.IsNullOrEmpty(iXdX) || !Request.IsLocal)
            {
                return new HttpNotFoundResult();
            }

            (new WDAIIPWEBDAO()).SimulateUserSession(iXdX);

            SessionModel sm = SessionModel.Get();

            if (string.IsNullOrEmpty(sm.UserID))
            {
                return Content($"Member ID: {iXdX} Not Exists.");
            }
            else
            {
                return Content($"Member ID: {iXdX} Logon Simulated.");
            }
        }

    }
}