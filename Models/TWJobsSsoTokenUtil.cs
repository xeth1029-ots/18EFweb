using System;
using System.Collections;
using System.Linq;
using System.Web;
using RSALibrary;
using log4net;

namespace WDAIIP.WEB.Models
{
    /// <summary>
    /// 就業通會員單一簽入 Token 工具
    /// </summary>
    public class TWJobsSsoTokenUtil
    {
        private static ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private RSAEncryption RSA = null;

        private string SidConf;

        //string encToken = null;

        private TWJobsSsoTokenUtil()
        {
        }

        public TWJobsSsoTokenUtil(string SidConf, string strPrivateKeyFile, string strPublicKeyFile)
        {
            if (string.IsNullOrEmpty(SidConf))
            {
                throw new ArgumentNullException("SidConf 不能為空");
            }

            this.SidConf = SidConf;

            try
            {
                RSA = new RSAEncryption(strPublicKeyFile, strPrivateKeyFile);
            }
            catch (Exception ex)
            {
                throw new Exception("Create RSAEncryption failed, " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 產生 SSO Login 所需的 Url 參數字串
        /// </summary>
        /// <param name="Rid"></param>
        /// <returns></returns>
        public string CreateLoginParms(string Rid)
        {
            if (string.IsNullOrEmpty(Rid))
            {
                throw new ArgumentNullException("Rid 不能為空");
            }

            // Token 明碼字串:  Sid=xxxxxxxx
            string token = this.RSA.EncryptData("Sid=" + SidConf);

            // 組Url參數串
            return "Sid=" + SidConf + "&Rid=" + Rid + "&Token=" + HttpUtility.UrlEncode(token);
        }

        /// <summary>
        /// 產生 SSO Status API 所需的 Url 參數字串
        /// </summary>
        /// <param name="Rid"></param>
        /// <param name="Uuid"></param>
        /// <returns></returns>
        public string CreateStatusParms(string Rid, string Uuid)
        {
            if (string.IsNullOrEmpty(Rid))
            {
                throw new ArgumentNullException("Rid 不能為空");
            }
            if (string.IsNullOrEmpty(Uuid))
            {
                throw new ArgumentNullException("Uuid 不能為空");
            }

            // Token 明碼字串:  Sid=xxxxxxxx,Uuid=xxxxxxxxxxxxxxx
            string token = this.RSA.EncryptData("Sid=" + SidConf + ",Uuid=" + Uuid);

            // 組Url參數串
            return "Sid=" + SidConf + "&Rid=" + Rid + "&Token=" + HttpUtility.UrlEncode(token);
        }

        /// <summary>
        /// 解析就業通SSO回傳的加密 Token 字串
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

            logger.Info("tokens: " + strToken);

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