using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;


namespace WDAIIP.WEB.Controllers
{
    public class TrainingLinkController : LinkController
    {
        public override string GetFunID()
        {
            return "010";  //ref:TB_CONTENT.FUNID
        }

        public override string GetSubFunID()
        {
            return "1";  //ref:TB_CONTENT.SUB_FUNID
        }
    }
}