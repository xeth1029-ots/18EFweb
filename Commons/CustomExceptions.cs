using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WDAIIP.WEB.Commons
{
    /// <summary>
    /// 用來代表登入失敗的 Exception
    /// </summary>
    public class LoginExceptions : Exception
    {
        public LoginExceptions(string message) : base(message) { }
    }

    class DigiCertExceptions : Exception
    {
        public DigiCertExceptions(string message) : base(message) { }
    }
}