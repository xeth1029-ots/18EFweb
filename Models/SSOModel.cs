using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Models
{
    public class SSOModel
    {
        public string strSID { get; set; }
        public string strPrivateKeyFile { get; set; }
        public string strPublicKeyFile { get; set; }
        public string strSsoUrl { get; set; }
        public string strLoginUrl { get; set; }
        public string strSsoStatusUrl { get; set; }
    }
}