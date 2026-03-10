using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.DataLayers;
using Turbo.Commons;
using System.Net.Security;
using WDAIIP.WEB.Commons;
using System.Net.Http;
using System.Configuration;
using Newtonsoft.Json;

namespace WDAIIP.WEB.Controllers
{
    public class PlanClassSearchController : BaseController
    {
        // GET: ClassSearchPlan
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            PlanClassSearchViewModel model = new PlanClassSearchViewModel();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;
            //model.Form.IsContainsOverEnter = "N"; //包含已截止報名課程(N:不含)
            model.Form.IsFirst = "Y";
            model.Form.PlanType = "2"; //預設查詢計畫選項
            return View("Index", model);
        }

        [HttpPost]
        public ActionResult Index(PlanClassSearchFormModel form)
        {
            if (form != null && !form.CheckArgument())
            {
                return new HttpStatusCodeResult(404);
            }

            //if (Form.PlanType != null)
            //{
            //    TempData["PlanType"] = Form.PlanType;
            //}
            //else
            //{
            //    Form.PlanType = TempData.ContainsKey("PlanType")? TempData["PlanType"].ToString(): "0";
            //}

            PlanClassSearchViewModel model = new PlanClassSearchViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            var ability = form.ABILITYS;
            //【就業通】職訓專長能力標籤斷字分詞
            ability = dao.GetSegmentkeyword(ability);

            model.Form = form;
            //model.Form.IsContainsOverEnter = "N"; //包含已截止報名課程(N:不含)
            model.Form.IsFirst = "N";
            model.Form.ABILITYS = ability;
            bLOG.Debug("PlanClassSearchController ; model.Form.ABILITYS: " + model.Form.ABILITYS);

            string x = model.Form.PlanType;
            //if (string.IsNullOrEmpty(x)) { return base.SetPageNotFound(); }

            ActionResult rtn = View(model);

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(model.Form.rid, model.Form.p);

                model.Grid1 = null;
                model.Grid2 = null;
                model.Grid3 = null;
                model.Grid4 = null;
                model.Grid5 = null;
                switch (model.Form.PlanType)
                {
                    case "1":
                        model.Form.TPLANID = "28";
                        model.Grid1 = dao.QueryPlanClassSearch(model.Form);
                        break;
                    case "2":
                        model.Form.TPLANID = "06";
                        model.Grid2 = dao.QueryPlanClassSearch_2(model.Form);
                        break;
                    case "3":
                        model.Form.TPLANID = "07";
                        model.Grid3 = dao.QueryPlanClassSearch_2(model.Form);
                        break;
                    case "4":
                        model.Form.TPLANID = "54";
                        model.Grid4 = dao.QueryPlanClassSearch(model.Form);
                        break;
                    case "5":
                        model.Form.TPLANID = "70";
                        model.Grid5 = dao.QueryPlanClassSearch_5(model.Form);
                        break;
                    default:
                        //return new HttpStatusCodeResult(404); //break;
                        return base.SetPageNotFound();
                }

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //rtn = PartialView("_GridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                //設定分頁元件(_PagingLink partial view)所需的資訊
                //base.SetPagingParams(model.Form, dao, "Index", "ajaxCallback");
                base.SetPagingParams(form, dao, "Index");
            }
            return rtn;
        }

        /// <summary>ability=專長能力名稱|關鍵字 1|關鍵字 2|…|關鍵字 </summary>
        /// <param name="ability">專長能力名稱|關鍵字 1|關鍵字 2|…|關鍵字</param>
        /// <param name="plantype">查詢類別("1":產業人才投資方案、"2":在職進修訓練、"3":充電起飛、"4":接受企業委託訓練、"5":區域產業據點職業訓練計畫(在職))</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Ability(string ability = null, string plantype = null)
        {
            bLOG.Debug("PlanClassSearchController ; Ability; ability: " + ability);
            bLOG.Debug("PlanClassSearchController ; Ability; plantype: " + plantype);

            SessionModel sm = SessionModel.Get();
            PlanClassSearchViewModel model = new PlanClassSearchViewModel();
            PlanClassSearchFormModel form = new PlanClassSearchFormModel();
            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu03;

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            form.PlanType = plantype;
            //【就業通】職訓專長能力標籤斷字分詞
            ability = dao.GetSegmentkeyword(ability);
            form.ABILITYS = ability;
            bLOG.Debug("PlanClassSearchController ; Ability; form.ABILITYS: " + form.ABILITYS);

            if (form != null && !form.CheckArgument2())
            {
                return new HttpStatusCodeResult(404);
            }

            model.Form = form;
            model.Form.IsFirst = "N";
            string x = model.Form.PlanType;
            //if (string.IsNullOrEmpty(x)) { return base.SetPageNotFound(); }

            ActionResult rtn = View("Index", model); // View(model);

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(model.Form.rid, model.Form.p);

                model.Grid1 = null;
                model.Grid2 = null;
                model.Grid3 = null;
                model.Grid4 = null;
                model.Grid5 = null;
                switch (model.Form.PlanType)
                {
                    case "1":
                        model.Form.TPLANID = "28";
                        model.Grid1 = dao.QueryPlanClassSearch(model.Form);
                        break;
                    case "2":
                        model.Form.TPLANID = "06";
                        model.Grid2 = dao.QueryPlanClassSearch_2(model.Form);
                        break;
                    case "3":
                        model.Form.TPLANID = "07";
                        model.Grid3 = dao.QueryPlanClassSearch_2(model.Form);
                        break;
                    case "4":
                        model.Form.TPLANID = "54";
                        model.Grid4 = dao.QueryPlanClassSearch(model.Form);
                        break;
                    case "5":
                        model.Form.TPLANID = "70";
                        model.Grid5 = dao.QueryPlanClassSearch_5(model.Form);
                        break;
                    default:
                        //return new HttpStatusCodeResult(404); //break;
                        return base.SetPageNotFound();
                }

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //rtn = PartialView("_GridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                //設定分頁元件(_PagingLink partial view)所需的資訊
                //base.SetPagingParams(model.Form, dao, "Index", "ajaxCallback");
                base.SetPagingParams(form, dao, "Index");
            }
            return rtn;



        }

    }
}