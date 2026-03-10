using log4net;
using RSALibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WDAIIP.WEB.Models;

namespace WDAIIP.WEB.Commons
{
    /// <summary>
    /// TokenPaser 的摘要描述
    /// </summary>
    [Serializable]
    public class TokenPaser
    {
        private static ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string strSID = ConfigModel.SSOSystemID;
        string strPrivateKeyFile = HttpContext.Current.Server.MapPath(ConfigModel.RSAPrivateKeyFile);
        string strPublicKeyFile = HttpContext.Current.Server.MapPath(ConfigModel.RSAPublicKeyFile);

        RSAEncryption RSA = null;
        //string encToken = null;

        public TokenPaser()
        {
            try
            {
                RSA = new RSAEncryption(strPublicKeyFile, strPrivateKeyFile);
            }
            catch (Exception ex)
            {
                throw new Exception("Create RSAEncryption failed, " + ex.Message);
            }
        }

        /// <summary>
        /// 解密-數據
        /// </summary>
        /// <param name="encToken"></param>
        /// <returns></returns>
        public Hashtable parse(string encToken)
        {
            if (string.IsNullOrEmpty(encToken))
                return null;

            string strToken = null;
            try
            {
                strToken = RSA.DecryptData(encToken);
                if (strToken == null)
                    throw new Exception("DecryptData return null");
            }
            catch (Exception ex)
            {
                throw new Exception("RSA Decrypt failed, " + ex.Message, ex);
            }

            LOG.Info("tokens: " + strToken);

            Hashtable parms = new Hashtable();
            int p0 = 0;
            int p = strToken.IndexOf(",");
            int d;
            string pair = "";
            while (p > -1)
            {
                pair = strToken.Substring(p0, p - p0);
                d = pair.IndexOf("=");
                if (d > -1)
                {
                    string key = pair.Substring(0, d);
                    string value = pair.Substring(d + 1);
                    parms.Add(key, value);
                }

                p0 = p + 1;
                p = strToken.IndexOf(",", p0);
            }

            // last pair
            pair = strToken.Substring(p0);
            d = pair.IndexOf("=");
            if (d > -1)
            {
                string key = pair.Substring(0, d);
                string value = pair.Substring(d + 1);
                parms.Add(key, value);
            }


            return parms;
        }
    }
}