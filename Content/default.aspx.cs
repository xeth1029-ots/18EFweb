using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WDAIIP.WEB.Content
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Response.AppendHeader("Cache-Control", "private");
            //Response.Cache.AppendCacheExtension("no-cache, no-store, must-revalidate");
            Response.StatusCode = 404;
            Response.End();
        }
    }
}