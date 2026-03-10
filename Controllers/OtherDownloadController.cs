using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;


namespace WDAIIP.WEB.Controllers
{
    public class OtherDownloadController : DownloadController
    {
        public override string GetKindID()
        {
            return "2";  //ref:TB_DLFILE.KINDID
        }

        public override string GetPlanID()
        {
            return "";  //ref:TB_DLFILE.PLANID
        }
    }
}