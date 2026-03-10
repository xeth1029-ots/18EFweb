using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;


namespace WDAIIP.WEB.Controllers
{
    public abstract class LinkController : BaseController
    {
        public abstract string GetFunID();

        public abstract string GetSubFunID();

        /// <summary>查詢</summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            LinkViewModel model = new LinkViewModel();

            string t_FunID = GetFunID();
            string t_SubFunID = GetSubFunID();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu05;

            LinkFormModel form = new LinkFormModel();
            form.FUNID = t_FunID;
            form.SUB_FUNID = t_SubFunID;

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            model.Grid = dao.QueryLink(form);
            return View(model);
        }
    }
}