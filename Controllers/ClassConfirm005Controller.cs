using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Commons;
using Turbo.Commons;

namespace WDAIIP.WEB.Controllers
{
    public class ClassConfirm005Controller : BaseController
    {
        // GET: ClassConfirm005
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();

            ClassConfirm005ViewModel model = new ClassConfirm005ViewModel();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu04;

            return View(model);
        }

        [HttpPost]
        [SecurityFilter]
        public ActionResult Index(ClassConfirm005FormModel form)
        {
            ClassConfirm005ViewModel model = new ClassConfirm005ViewModel();

            model.Form = form;

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            ActionResult rtn = View("Index", model);

            // 檢核查詢條件
            this.ValidFormInput(model.Form);

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                rtn = View("List", model);

                if (form.useCache > 0 || ModelState.IsValid)
                {
                    // 設定查詢分頁資訊
                    dao.SetPageInfo(model.Form.rid, model.Form.p);

                    model.Grid = dao.QueryTrainClassListEnroll(model.Form);

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
                }
                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(model.Form, dao, "Index");
            }
            return rtn;
        }

        [HttpPost]
        public ActionResult Detail(ClassConfirm005ClassFormModel form)
        {
            ClassConfirm005ViewModel model = new ClassConfirm005ViewModel();

            model.ClassForm = form;

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            //Class
            model.Detail = dao.GetQueryTrainClassUintEnroll(model.ClassForm);

            //Stud
            if (model.Detail != null)
            {
                model.Detail.UserGrid = dao.QueryTrainClassListEnrollStud(model.ClassForm);
            }
            return View("Detail", model);
        }

        /// <summary>
        /// 檢核查詢條件
        /// </summary>
        /// <param name="model"></param>
        public void ValidFormInput(ClassConfirm005FormModel model)
        {
            ModelState.Remove("ValidMsg");

            // 查詢條件須少填寫一項
            if (string.IsNullOrEmpty(model.OCID_TEXT) && string.IsNullOrWhiteSpace(model.CLASSCNAME) && string.IsNullOrWhiteSpace(model.DISTIDITEM))
            {
                ModelState.AddModelError("ValidMsg", "請至少輸入一項查詢條件");
            }
        }
    }
}