using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WDAIIP.WEB.Controllers
{
    public class ClassMatchPlan70Controller : ClassMatchListController
    {
        public override string GetPlanType()
        {
            return "5";
        }
    }
}