using Geo.Grid.Common.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WDAIIP.WEB.Controllers
{
    public class SharedController : BaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Shared
        public ActionResult Index()
        {
            //bLOG.Error(string.Concat("##Index: ", ex.Message), ex); //Utl_StatusCode404();
            return HttpNotFound(); //return View();
        }

        public ActionResult UploadAndParseImage()
        {
            List<string> ocrResults = new List<string>();
            var result = new Result<List<string>>()
            {
                Success = false,
                Data = ocrResults,
                Message = "message",
            };
            return Content(JsonConvert.SerializeObject(result));
        }

    }
}