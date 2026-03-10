using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Controllers
{
    public class ClassConfirm003Controller : BaseController
    {
        // GET: ClassConfirm003
        /// <summary>
        /// 充電起飛計畫
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            //充電起飛計畫
            SessionModel sm = SessionModel.Get();
            ClassConfirm003ViewModel model = new ClassConfirm003ViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu04;

            //接收分署清單-查詢結果
            model.DistNameGrid_list = dao.QueryClassConfirm003DistName();

            model.Form.IsFirst = "Y";

            if (model.DistNameGrid_list != null && model.DistNameGrid_list.Count > 0)
            {
                var item = model.DistNameGrid_list[0];
                model.Form.DISTID = item.DISTID;
                model.DISTID_TEXT = item.DISTNAME;
            }

            return View(model);
        }

        /// <summary>
        /// 查詢結果
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ClassConfirm003FormModel form)
        {
            if (form == null) { return new HttpStatusCodeResult(403); }
            //充電起飛計畫
            ClassConfirm003ViewModel model = new ClassConfirm003ViewModel();
            model.Form = form;
            ActionResult rtn = View("Index", model);

            //檢核查詢條件
            this.ValidFormInput(model.Form);

            if (!ModelState.IsValid) { return rtn; }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            ModelState.Clear();

            rtn = View("List", model);

            if (form.useCache > 0 || ModelState.IsValid)
            {
                // 設定查詢分頁資訊
                dao.SetPageInfo(model.Form.rid, model.Form.p);

                //接收課程清單-查詢結果
                model.Grid_list = dao.QueryClassConfirm003Class(model.Form);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //rtn = PartialView("_GridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    //model.Form.PagingInfo = dao.PaginationInfo; //更新 PagingResultsViewModel 設定分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }
            }

            // 設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model.Form, dao, "Index");

            return rtn;
        }

        /// <summary>
        /// 檢核查詢條件
        /// </summary>
        /// <param name="model"></param>
        public void ValidFormInput(ClassConfirm003FormModel model)
        {
            ModelState.Remove("ValidMsg");
            //充電起飛計畫
            //查詢條件須少填寫一項
            if (string.IsNullOrWhiteSpace(model.ORGNAME) && string.IsNullOrWhiteSpace(model.CLASSCNAME))
            {
                ModelState.AddModelError("ValidMsg", "請至少輸入一項查詢條件");
            }
        }

        /// <summary>
        /// 查詢結果-單位資訊/學生清單
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Detail(ClassConfirm003StudFormModel form)
        {
            ClassConfirm003ViewModel model = new ClassConfirm003ViewModel();
            //充電起飛計畫
            //紀錄查詢條件
            model.StudForm = form;
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            //單位資訊
            model.UnitGrid_list = dao.GetQueryClassConfirm003ClassUnit(form);
            //接收學生清單 - 查詢結果
            model.StudGrid_list = dao.QueryClassConfirm003Stud(form);
            //ViewModel回傳
            return View(model);
        }
    }
}