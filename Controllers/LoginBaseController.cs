using Turbo.DataLayer;
using WDAIIP.WEB.DataLayers;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Turbo.Commons;
using WDAIIP.WEB.Commons.Filter;


namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// 有權限控管的共用 Controller 基底類, 需要 登入/授權 才能使用的功能, 一律繼承這個基底類
    /// </summary>
    [LoginRequired]
    public class LoginBaseController : BaseController
    {
    }
}