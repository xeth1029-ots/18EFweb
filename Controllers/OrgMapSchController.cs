using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Commons;
using Turbo.Commons;
using log4net;
using Newtonsoft.Json;
using System.Collections;

namespace WDAIIP.WEB.Controllers
{
    public class OrgMapSchController : BaseController
    {
        //using log4net;
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: OrgMapSch
        public ActionResult Index()
        {
            OrgMapSchViewModel model = new OrgMapSchViewModel();
            //return View();//return View("Index", model);
            return Index(model);
        }

        public void GetTidydata(OrgMapSchViewModel model)
        {
            //OrgMapSchViewModel model = new OrgMapSchViewModel();
            OrgMapSchFormModel form = model.Form;

            string s_msg = "\n #Tidydata ";
            s_msg += string.Format("\n,form.CTID:{0}", form.CTID ?? "(null)");
            s_msg += string.Format("\n,form.ZIPCODE:{0}", form.ZIPCODE ?? "(null)");

            s_msg += "\n #Tidydata.form: ";
            var dict = form.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(form, null));
            foreach (var kvp in dict)
            {
                s_msg += string.Format("\n,{0}:{1}", kvp.Key, kvp.Value ?? "[null]");
            }
            logger.Debug(s_msg);

            //form.PlanType = "1";
            if (!string.IsNullOrEmpty(form.CTID)) { form.hidCityID = form.CTID; }

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            model.Grid1 = dao.QueryOrgMapSchList_G1(form);

            ArrayList data = new ArrayList();
            if (model.Grid1 != null)
            {
                // 找出座標分佈的邊界
                //jobBounds = new MapSearchBounds();
                //jobBounds.left = double.MaxValue;
                //jobBounds.right = 0;
                //jobBounds.top = 0;
                //jobBounds.bottom = double.MaxValue;
                foreach (var rItem in model.Grid1)
                {
                    double x = rItem.REAL_TWD97_X.HasValue ? rItem.REAL_TWD97_X.Value : 0;
                    double y = rItem.REAL_TWD97_Y.HasValue ? rItem.REAL_TWD97_Y.Value : 0;
                    //jobBounds.left = Math.Min(jobBounds.left, x);
                    //jobBounds.right = Math.Max(jobBounds.right, x);
                    //jobBounds.bottom = Math.Min(jobBounds.bottom, y);
                    //jobBounds.top = Math.Max(jobBounds.top, y);

                    Hashtable station = new Hashtable
                    {
                        { "TB_ORGID", rItem.TB_ORGID },
                        { "TB_CTID", rItem.CTID },
                        { "TB_ZIPCODE", rItem.ZIPCODE },
                        { "TB_ADDRESS1", rItem.TB_ADDRESS1 },
                        { "TB_ORGNAME", rItem.TB_ORGNAME },
                        { "TB_PHONE", rItem.PHONE },
                        { "TB_TYPE", rItem.TB_TYPE },
                        { "MAP_X", x },//Convert.ToString(dr["MAP_X"]));
                        { "MAP_Y", y }//Convert.ToString(dr["MAP_Y"]));
                    };
                    data.Add(station);
                }

                //轉成 JSON 字串 //model.Form.hidJobBounds = Newtonsoft.Json.JsonConvert.SerializeObject(jobBounds);
                model.Form.liSDM = JsonConvert.SerializeObject(data);
            }

        }

        [HttpPost]
        public ActionResult Index(OrgMapSchViewModel model)
        {
            SessionModel sm = SessionModel.Get();

            //整理 資料
            GetTidydata(model);

            return View("Index", model);
        }


    }
}