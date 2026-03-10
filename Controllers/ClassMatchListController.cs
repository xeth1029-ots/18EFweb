using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Controllers
{
    public abstract class ClassMatchListController : LoginBaseController
    {
        public abstract string GetPlanType();

        // GET: ClassMatchList
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            ClassMatchListViewModel model = new ClassMatchListViewModel();
            ClassMatchListFormModel form = model.Form;
            ActionResult rtn = View("Index", model);

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu07;

            form.PlanType = this.GetPlanType();
            form.ProvideLocation = "N";

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                TblTB_MEMSEARCH memSearch = dao.GetMemSearch(sm.MemSN);
                if (memSearch == null)
                {
                    sm.LastResultMessage = "尚未設定速配條件!";
                }
                else
                {
                    // 依輸入條件進行查詢
                    switch (form.PlanType)
                    {
                        case "1": //產投
                            //有設定速配條件才做查詢
                            if (string.IsNullOrEmpty(memSearch.TMID) && string.IsNullOrEmpty(memSearch.CTID))
                            {
                                sm.LastResultMessage = "尚未設定產業人才投資方案課程速配條件!";
                            }
                            else
                            {
                                form.TMID = memSearch.TMID;
                                form.CTID = memSearch.CTID;

                                model.Grid1 = dao.QueryClassMatchList_1(form);
                            }
                            break;
                        case "2": //在職
                            //有設定速配條件才做查詢
                            if (string.IsNullOrEmpty(memSearch.DISTID) && string.IsNullOrEmpty(memSearch.CJOBNO))
                            {
                                sm.LastResultMessage = "尚未設定分署自辦在職訓練課程速配條件";
                            }
                            else
                            {
                                form.DISTID = memSearch.DISTID;
                                form.CJOBUNKEY = memSearch.CJOBNO;
                                
                                model.Grid2 = dao.QueryClassMatchList_2(form);
                            }
                            
                            break;
                    }
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");

            }

            return rtn;
        }

        [HttpPost]
        public ActionResult Index(ClassMatchListFormModel form)
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            ClassMatchListViewModel model = new ClassMatchListViewModel();
            model.Form = form;

            ActionResult rtn = View("Index", model);

            model.Form.PlanType = this.GetPlanType();

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                TblTB_MEMSEARCH memSearch = dao.GetMemSearch(sm.MemSN);
                if (memSearch == null)
                {
                    sm.LastResultMessage = "尚未設定速配條件!";
                }
                else
                {
                    // 依輸入條件進行查詢
                    switch (form.PlanType)
                    {
                        case "1": //產投
                            //有設定速配條件才做查詢
                            if (string.IsNullOrEmpty(memSearch.TMID) && string.IsNullOrEmpty(memSearch.CTID))
                            {
                                sm.LastResultMessage = "尚未設定產業人才投資方案課程速配條件!";
                            }
                            else
                            {
                                model.Form.TMID = memSearch.TMID;
                                model.Form.CTID = memSearch.CTID;

                                model.Grid1 = dao.QueryClassMatchList_1(model.Form);
                            }
                            break;
                        case "2": //在職
                            //有設定速配條件才做查詢
                            if (string.IsNullOrEmpty(memSearch.DISTID) && string.IsNullOrEmpty(memSearch.CJOBNO))
                            {
                                sm.LastResultMessage = "尚未設定分署自辦在職訓練課程速配條件";
                            }
                            else
                            {
                                model.Form.DISTID = memSearch.DISTID;
                                model.Form.CJOBUNKEY = memSearch.CJOBNO;

                                model.Grid2 = dao.QueryClassMatchList_2(model.Form);
                            }

                            break;
                    }
                }

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //rtn = PartialView("_NewsGridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");

            }

            return rtn;
        }

    }
}