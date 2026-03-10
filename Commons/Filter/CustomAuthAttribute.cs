using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;
using Turbo.Commons;
using WDAIIP.WEB.DataLayers;


namespace WDAIIP.WEB.Commons.Filter
{
    /// <summary>
    /// 判斷 登入狀態 的 AuthorizeAttribute
    /// </summary>
    public class LoginRequired : AuthorizeAttribute
    {
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 預設系統登入頁
        /// </summary>
        public static string LOGIN_PAGE = "~/Member/Login";

        /// <summary>
        /// LoginRequired 登入 Session 檢核 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string actionPath = ControllerContextHelper.GetActionPath(filterContext);
            string verb = filterContext.HttpContext.Request.HttpMethod;
            //LOG.Debug("#####actionPath_1 = [" + actionPath + "]");
            SessionModel sm = SessionModel.Get();

            sm.LastActionPath = actionPath;
            //sm.LastActionFunc = null;

            if (!sm.IsLogin && ConfigModel.StressTestMode)
            {
               if( (new WDAIIPWEBDAO()).StressTestRandomUser() )
                {
                    LOG.Info("StressTestMode enabled !!");
                }
            }

            string userNo = sm.ToString();
            HttpContext context = HttpContext.Current;
            string str_UserHostIp = MyCommonUtil.GetIpAddress(HttpContext.Current);
            string s_LOG_msg= string.Format("OnAuthorization[{0}] {1} {2} From {3}/{4}", userNo, verb, actionPath , str_UserHostIp, context.Request.UserHostAddress);
            LOG.Info(s_LOG_msg);


            if (!sm.IsLogin)
            {
                string loginPage = LOGIN_PAGE;
                LOG.Info("OnAuthorization: redirect to Login page: " + loginPage);
                filterContext.Result = new RedirectResult(loginPage);
            }

            #region (目前不使用)

            //if (string.IsNullOrEmpty(userNo))
            //{
            //    // SessionModel 中若不存在 UserInfo 則視為 "未登入", 導向登入頁

            //    // 根據 LoginRequired.Roles 決定, 登入頁面
            //    string loginPage = LOGIN_PAGE;

            //    LOG.Info("OnAuthorization: redirect to Login page: " + loginPage);
            //    filterContext.Result = new RedirectResult(loginPage);
            //}
            //else
            //{
            //    // 比對系統 TlbE_FUN.FUN_PAGE2 以取得功能名稱
            //    // 要把 actionPath 中的 action method 部份去掉
            //    // 只留下 Area/Controller
            //    int p = actionPath.LastIndexOf("/");
            //    if (p > -1)
            //    {
            //        if (actionPath.IndexOf("/") == 0 && p > 1)
            //        {
            //            actionPath = actionPath.Substring(1, p - 1); //  /Area/Controller
            //        }
            //        else
            //        {
            //            actionPath = actionPath.Substring(0, p);//  Area/Controller
            //        }
            //    }

            //    LOG.Debug("#####actionPath_2 = ["+ actionPath + "]");
            //    // 取得系統中已啟用的全部 Action Function 定義
            //    // 以比對取得當前 action path 對應的 功能名稱 並記錄在 SessionModel.LastActionFunc 
            //    // 比對範圍: 以 area/controller 為準, 同一個 controller 下不再區分子功能
            //    IList<TblE_FUN> allFuncs = ApplicationModel.GeteYVTRmngFuncsAll();

            //    LOG.Debug("#####allFuncs.count = [" + allFuncs.Count + "]");
            //    for (int i = 0; i < allFuncs.Count; i++)
            //    {
            //        TblE_FUN item = allFuncs[i];

            //        LOG.Debug("@@@@@@@@ actionPath = [" + actionPath + "],item = [" +  item.FUN_PAGE2 + "]");
            //        if (actionPath.Equals(item.FUN_PAGE2))
            //        {
            //            /* 找到 action 對應功能, 
            //             * 包括同一Controller中的相關子功能, 
            //             * 例: AM/SE11/Index, AM/SE11/Modify 
            //             *    都對應至 AM/SE11 這個功能
            //             */
            //            sm.LastActionFunc = item;
            //            break;
            //        }
            //    }
            //}

            #endregion

            //base.OnAuthorization(filterContext);
            return;
        }
    }

    /// <summary>
    /// 同時判斷 登入狀態 及 角色執行權限 的 AuthorizeAttribute
    /// </summary>
    public class AuthorizeRequired : LoginRequired 
    {
        //private static string ROLE_PAGE = "~/Login/Role";
        //private static string UNAUTH_PAGE = "~/ErrorPage/UnAuth";
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /*
            web.config appSettings  設定是否停用 Authorize 權限檢核 (測試開發用):
            2:  當系統管理者角色時停用, AuthorizeRequired 沒作用直接 bypass
            1:  全部停用, AuthorizeRequired 沒作用直接 bypass
            0:  未停用
        */
        private string disabled = System.Configuration.ConfigurationManager.AppSettings["DisableAuthorize"];

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.Result is RedirectResult)
            {
                //在 LoginRequired 中, 已經進行了 Redirect 處理, 不用再處理
                return;
            }

            SessionModel sm = SessionModel.Get();
            string verb = filterContext.HttpContext.Request.HttpMethod;
            string lastController = sm.LastActionController;    // 最後執行有授權功能的 controller name (不含 action 部份)

            #region (目前不使用)

            // 角色權限檢核
            /*
            IList<eYVTRmngRoleFunc> funcs = sm.RoleFuncs;
            bool isAuth = false;
            if (funcs != null)
            {
                TblE_FUN func = sm.LastActionFunc;
                for (int i = 0; func != null && i < funcs.Count; i++)
                {
                    eYVTRmngRoleFunc item = funcs[i];
                    if (string.IsNullOrWhiteSpace(item.FUN_PAGE2))
                    {
                        continue;
                    }
                    if(item.FUN_PAGE2.Equals(func.FUN_PAGE2))
                    {
                        isAuth = true;
                        break;
                    }
                }
            }

            if (!isAuth)
            {
                if ("1".Equals(disabled)
                    || ("2".Equals(disabled) && ConfigModel.Admin.Equals(sm.UserInfo.UserNo)))
                {
                    //停用權限檢核
                    LOG.Info("OnAuthorization[" + sm.UserInfo.UserNo + "] " + verb + " " + sm.LastActionPath + ", -- BYPASS --");
                }
                else
                {
                    // 使用者試圖執行未授權的 Action, 導向 UnAuth 頁面
                    LOG.Info("OnAuthorization[" + sm.UserInfo.UserNo + "] " + verb + " " + sm.LastActionPath + ", redirect to UnAuth page");
                    filterContext.Result = new RedirectResult(UNAUTH_PAGE);
                }

            }
            */

            #endregion
        }
    }
}