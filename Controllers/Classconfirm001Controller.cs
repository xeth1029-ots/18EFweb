using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.DataLayers;
using Turbo.Commons;

namespace WDAIIP.WEB.Controllers
{
    public class ClassConfirm001Controller : BaseController
    {
        // GET: ClassConfirm001
        /// <summary>
        /// 查詢頁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            ClassConfirm001ViewModel model = new ClassConfirm001ViewModel();
            ModelState.Clear();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu04;

            return View(model);
        }

        /// <summary>
        /// 查詢結果
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ClassConfirm001FormModel form)
        {
            ClassConfirm001ViewModel model = new ClassConfirm001ViewModel();
            model.Form = form;
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            ActionResult rtn = View("Index", model);
 
            //檢核查詢條件
            this.ValidFormInput(model.Form);

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                rtn = View("List", model);

                if (form.useCache > 0 || ModelState.IsValid)
                {
                    // 設定查詢分頁資訊
                    dao.SetPageInfo(model.Form.rid, model.Form.p);

                    model.Grid = dao.QueryClassConfirm001(model.Form);

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
            }

            return rtn;
        }

        /// <summary>
        /// Detail查詢結果
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Detail(ClassConfirm0012FormModel form)
        {
            ClassConfirm001ViewModel model = new ClassConfirm001ViewModel();
            model.Form2 = form;
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            model.Detail = dao.GetClassConfirm001Detail(model.Form2);
            if(model.Detail != null)
            {
                model.Detail.UserGrid= dao.QueryClassConfirm0012(model.Form2);
            }           

            return View("Detail", model);
        }

        /// <summary>
        /// 檢核查詢條件
        /// </summary>
        /// <param name="model"></param>
        public void ValidFormInput(ClassConfirm001FormModel model)
        {
            ModelState.Remove("ValidMsg");

            // 查詢條件須少填寫一項
            if (string.IsNullOrEmpty(model.OCID_TEXT) && string.IsNullOrWhiteSpace(model.ORGNAME) && string.IsNullOrWhiteSpace(model.CLASSCNAME))
            {
                ModelState.AddModelError("ValidMsg","請至少輸入一項查詢條件");
            }
        }
    }
}