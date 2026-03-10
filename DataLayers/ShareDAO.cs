using log4net;
using Omu.ValueInjecter;
using RSALibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;
using Turbo.DataLayer;

namespace WDAIIP.WEB.DataLayers
{
    public class ShareDAO : BaseDAO
    {
        //protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> 郵遞區號資料選取對話框使用 </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public IList<ZipGridModel> QueryZip(ZipFormModel form)
        {
            IList<ZipGridModel> rst = null;
            if (form == null) { return rst; }

            string s_mentId = (!string.IsNullOrEmpty(form.POSTTYPE1) && form.POSTTYPE1.Equals("3")) ? "Share.queryZip6" : "Share.queryZip";
            LOG.DebugFormat("#form.POSTTYPE1 : {0}", form.POSTTYPE1);
            LOG.DebugFormat("#s_mentId : {0}", s_mentId);
            rst = base.QueryForListAll<ZipGridModel>(s_mentId, form);

            return rst;
        }
    }
}