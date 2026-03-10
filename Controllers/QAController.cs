using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Controllers
{
    /// <summary>
    /// /QA  Q&A/常見問題
    /// </summary>
    public class QAController : BaseController
    {
        // GET: QA
        /// <summary>
        /// 查詢頁
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            QAViewModel model = new QAViewModel();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu08;

            //查詢常見問題類別清單(啟用中)
            model.QATYPE_list = dao.QueryQAType();
            model.TYPEID_TEXT = string.Empty;
            model.Form.IsFirst = "Y";

            if (model.QATYPE_list != null && model.QATYPE_list.Count > 0)
            {
                var item = model.QATYPE_list[0];
                model.Form.TYPEID = item.CODE;
                model.TYPEID_TEXT = item.DESCR;

                if (model.QATYPE_list.Count >= 2)
                {
                    item = new QATYPEGridModel
                    {
                        CODE = "99",
                        DESCR = "全部資料"
                    };
                    model.QATYPE_list.Add(item);
                }
            }

            // 設定查詢分頁資訊
            dao.SetPageInfo(model.Form.rid, model.Form.p);

            model.Grid = dao.QueryQA(model.Form);

            // 設定分頁元件(_PagingLink partial view)所需的資訊            
            base.SetPagingParams(model.Form, dao, "Index", "ajaxCallback");

            return View(model);
        }

        /// <summary>
        /// 查詢結果
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(QAFormModel form)
        {
            if (form == null) { return base.SetPageNotFound(); }

            //return new HttpStatusCodeResult(403);
            if (!form.CheckArgument()) { return base.SetPageNotFound(); }

            QAViewModel model = new QAViewModel();

            model.Form = form;
            model.Form.IsFirst = "N";

            ActionResult rtn = View(model);
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            //查詢常見問題類別清單(啟用中)
            model.QATYPE_list = dao.QueryQAType();

            if (model.QATYPE_list != null && model.QATYPE_list.Count > 0)
            {
                if (!string.IsNullOrEmpty(model.Form.TYPEID) && !"99".Equals(model.Form.TYPEID))
                {
                    //model.TYPEID_TEXT = model.QATYPE_list[Convert.ToInt32(model.Form.TYPEID) - 1].DESCR;
                    //int i_qry = model.QATYPE_list.Where(i => i.CODE == model.Form.TYPEID).Count();
                    model.TYPEID_TEXT = model.QATYPE_list.Where(i => i.CODE == model.Form.TYPEID).First().DESCR;
                }
                else
                {
                    model.TYPEID_TEXT = "全部資料";
                }

                if (model.QATYPE_list.Count >= 2)
                {
                    QATYPEGridModel item = new QATYPEGridModel
                    {
                        CODE = "99",
                        DESCR = "全部資料"
                    };
                    model.QATYPE_list.Add(item);
                }
            }

            if (form.useCache > 0 || ModelState.IsValid)
            {
                ModelState.Clear();

                // 設定查詢分頁資訊
                dao.SetPageInfo(model.Form.rid, model.Form.p);

                model.Grid = dao.QueryQA(model.Form);

                // 從 cache 中取回 或 從 DB 中查詢 (自動根據 rid 判斷)
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    //rtn = PartialView("_GridRows", model);

                    // for 更動分頁筆數下拉時需再重新補傳分頁資訊
                    model.Form.PagingInfo = dao.PaginationInfo; //更新 PagingResultsViewModel 設定分頁資訊
                    PagingViewModel pagingModel = new PagingViewModel();
                    pagingModel.Result = ControllerContextHelper.RenderRazorPartialViewToString(ControllerContext, "_GridRows", model).ToString();
                    pagingModel.PagingInfo = dao.PaginationInfo;
                    rtn = Json(pagingModel);
                }
                else
                {
                    //有輸入關鍵字進行查詢時，要寫入關鍵字Log資料表
                    TblTB_KEYWORD_LOG log = null;
                    IList<TblTB_KEYWORD_LOG> logList = new List<TblTB_KEYWORD_LOG>();
                    IList<string> keyWordAry = new List<string>();

                    if (!string.IsNullOrWhiteSpace(form.KEYWORD))
                    {
                        //多組關鍵字要以空白區隔
                        keyWordAry = form.KEYWORD.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string item in keyWordAry)
                        {
                            log = new TblTB_KEYWORD_LOG
                            {
                                KWLTYPE = "1",
                                SUB_TYPEID = form.TYPEID,
                                KEYWORD = item,
                                USERIP = Request.UserHostAddress,
                                SEARCHDATE = DateTime.Now
                            };

                            logList.Add(log);
                        }

                        //新增關鍵字查詢Log紀錄
                        dao.InsertKeyWordLog(logList);
                    }
                }

                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(model.Form, dao, "Index", "ajaxCallback");
            }


            return rtn;
        }
    }
}