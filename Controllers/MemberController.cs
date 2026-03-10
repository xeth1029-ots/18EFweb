using log4net;
using RSALibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using System.Web.Routing;
using WDAIIP.WEB.Models.Entities;
using System.Configuration;
using WDAIIP.WEB.Commons;

namespace WDAIIP.WEB.Controllers
{
    public class MemberController : BaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string strSID = ConfigModel.SSOSystemID;

        // GET: Member
        public ActionResult Index()
        {
            return RedirectToAction("MaintainMemInfo", "Member");
        }

        /// <summary>強制登入</summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LoginTestFunc1(string strRID, string rqTest)
        {
            /*[HttpGet] [HttpPost]*/
            LOG.Debug(string.Format("###LoginTestFunc1(strRID={0},rqTest={1})", strRID, rqTest));
            //select top 10 * from member where email = 'standbyme.wei@gmail.com'
            //and MODIFYDATE >= getdate() - 60 //ORDER BY MODIFYDATE DESC
            string CNFIG_STR_RID = ConfigurationManager.AppSettings["CNFIG_STR_RID"];
            if (!string.IsNullOrEmpty(CNFIG_STR_RID) && string.IsNullOrEmpty(strRID))
            {
                if (string.IsNullOrEmpty(strRID)) strRID = CNFIG_STR_RID;//"947750082";// null;
            }

            //ActionResult rtn = null;
            ActionResult rtn = RedirectToAction("Index", "Home");
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            //測試登入
            string strLoginTest = ConfigModel.LoginTest;
            if (!string.IsNullOrEmpty(rqTest) && string.IsNullOrEmpty(strLoginTest)) { strLoginTest = rqTest; }
            if (string.IsNullOrEmpty(strRID) || strLoginTest != "Y") { return rtn; }

            //存入member(登入導頁用)
            TWJobsMemberDataModel model = new TWJobsMemberDataModel();
            model.RID = Convert.ToDecimal(strRID);
            model.SID = strSID;

            string sid = model.SID;
            string rid = Convert.ToString(model.RID);
            string token = model.TOKEN;
            LOG.Debug(string.Format("###LoginTestFunc1 sid={0},rid={1},token={2} ", sid, rid, token));

            TWJobsMemberDataModel twMem = new TWJobsMemberDataModel();
            twMem = dao.QueryMember(rid, sid);
            if (twMem == null) { return rtn; }

            //記錄登入者資訊 (會員資料同步到 E_MEMBER 中)
            dao.saveToEMember(twMem);

            //LOG.Debug(string.Format("###LoginTestFunc1 twMem.MEMBER_USER_ID={0} ", twMem.MEMBER_USER_ID));
            //SessionModel sm = SessionModel.Get(); //Login Success
            sm.RID = Convert.ToString(model.RID);
            sm.SID = Convert.ToString(model.SID);
            sm.ACID = twMem.ACID; //IDNO;
            sm.MEMBER_USER_ID = Convert.ToString(twMem.MEMBER_USER_ID); //MEMBER_USER_ID;
            if (!string.IsNullOrEmpty(twMem.BIRTHDAY)) { twMem.BIRTHDAY = twMem.BIRTHDAY.Replace("-", "/"); }
            sm.Birthday = twMem.BIRTHDAY; //"yyyy/MM/dd";

            rtn = RedirectToAction("Index", "Home");

            return rtn;
        }

        #region "會員登入/登出"
        /// <summary>
        /// 會員登入-導向就業通登入頁(單一簽入)
        /// </summary>
        /// <returns></returns>
        public void Login()
        {
            SessionModel sm = SessionModel.Get();
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();

            //string strPrivateKeyFile = HttpContext.Server.MapPath(System.Configuration.ConfigurationManager.AppSettings["RSAPublicKeyFile"]);
            //string strPublicKeyFile = HttpContext.Server.MapPath(System.Configuration.ConfigurationManager.AppSettings["RSAPrivateKeyFile"]);
            string strPrivateKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPrivateKeyFile);
            string strPublicKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPublicKeyFile);
            string strLoginUrl = ConfigModel.TwJobsLogin_URL;
            string strEnParam = "";
            //產生rid
            string strRID = getRandomRID();
            //strRID = getRandomRID();
            string strTestLoginUrl = ConfigModel.MemberTestLogin;
            if (!string.IsNullOrEmpty(strTestLoginUrl))
            {
                //string strTLoginUrl = string.Concat("~/", strTestLoginUrl);
                HttpContext.Response.Redirect(strTestLoginUrl);
                return;
            }

            //存入member(登入導頁用)
            TWJobsMemberDataModel memInfo = new TWJobsMemberDataModel();

            if (sm.RedirectInfo != null)
            {
                memInfo.InjectFrom(sm.RedirectInfo);
            }

            int i_NG = 0;
            while (true)
            {
                try
                {
                    memInfo.RID = Convert.ToDecimal(strRID);
                    memInfo.SID = strSID;
                    dao.processMInfo(memInfo);
                    break;
                }
                catch (Exception) { /*throw;*/ }
                //Exception todo 再次檢核 rid 是否已被使用過(Member 找得到資料），有則需再重取一次rid
                strRID = getRandomRID();
                i_NG += 1;
                if (i_NG > 3 && memInfo.OCID.HasValue) { memInfo.OCID = null; }
                if (i_NG > ConfigModel.ProcessEnterWorkerMaxThreads) { throw new HttpException(404, "The resource cannot be found"); }
            }

            //產生token
            strEnParam = new RSAEncryption(strPublicKeyFile).EncryptData("Sid=" + strSID);

            //登入頁面URL
            strLoginUrl = string.Concat(ConfigModel.TwJobsLogin_URL, "?Sid=", strSID, "&Rid=", strRID, "&Token=", HttpContext.Server.UrlEncode(strEnParam));

            HttpContext.Response.Redirect(strLoginUrl);
        }

        /// <summary>會員登出-導向就業通登出頁(單一簽入)</summary>
        /// <returns></returns>
        public void LogoutTwJobs()
        {
            SessionModel sm = SessionModel.Get();
            string strPrivateKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPrivateKeyFile);
            string strPublicKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPublicKeyFile);
            string strRID = sm.RID;
            string strEnParam = "";
            //產生token
            strEnParam = new RSAEncryption(strPublicKeyFile).EncryptData(string.Concat("Sid=", strSID));

            //登入頁面URL
            string strLogoutUrl = ConfigModel.TwJobsLogout_URL;
            strLogoutUrl += string.Concat("?Sid=", strSID);
            strLogoutUrl += string.Concat("&Rid=", strRID);
            strLogoutUrl += string.Concat("&Token=", HttpContext.Server.UrlEncode(strEnParam));
            strLogoutUrl += "&PageUrl=/Member/Logout";
            HttpContext.Response.Redirect(strLogoutUrl);
        }

        /// <summary>會員登出</summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            //string strTestLoginUrl = ConfigModel.MemberTestLogin;
            //if (!string.IsNullOrEmpty(strTestLoginUrl)) { }
            SessionModel sm = SessionModel.Get();
            //LOG.Debug(string.Format("#Logout.1 SID={0},RID={1},MEMUSRID={2}", sm.SID, sm.RID, sm.MEMBER_USER_ID));
            LOG.Debug(string.Format("#Logout.1 SessionID={0}", sm.SessionID));
            LOG.Debug(string.Format("#Logout.1 sm={0}", sm.ToString()));
            Session.Clear();
            Session.RemoveAll();
            //Session.Abandon();
            //LOG.Debug(string.Format("#Logout.2 SID={0},RID={1},MEMUSRID={2}", sm.SID, sm.RID, sm.MEMBER_USER_ID));
            //sm = SessionModel.Get();
            LOG.Debug(string.Format("#Logout.2 SessionID={0}", sm.SessionID));
            LOG.Debug(string.Format("#Logout.2 sm={0}", sm.ToString()));
            return RedirectToAction("Index", "Home");
        }

        /// <summary>接收並parse登入頁回傳使用者登入資訊(minfo)</summary>
        /// <param name="sid">單一簽入系統代碼</param>
        /// <param name="rid">亂數抽籤號碼牌</param>
        /// <param name="minfo">xml本文</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MInfoReceiver(TWJobsMemberDataModel model)
        {
            string strPrivateKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPrivateKeyFile);
            string strPublicKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPublicKeyFile);
            LOG.Debug("MemberController.MInfoReceiver Begin......");

            RSAEncryption RSA = null;
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            //bool chkFlag = true;
            try
            {
                //比對金鑰
                RSA = new RSAEncryption(strPublicKeyFile, strPrivateKeyFile);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("#MInfoReceiver Create RSAEncryption failed Error: ", ex.Message), ex);
                //chkFlag = false;
                LOG.Error("#MInfoReceiver MemberController chkFlag:false!!");
                return base.SetPageNotFound(); //return new HttpStatusCodeResult(404); //return 404 not found
            }

            if (string.IsNullOrEmpty(model.SID) || string.IsNullOrEmpty(Convert.ToString(model.RID)) || string.IsNullOrEmpty(model.minfo))
            {
                LOG.Error(string.Concat("#MInfoReceiver Arguments invalid : (sid=", model.SID, ",rid=", model.RID, ",minfo=", model.minfo, ")"));
                //chkFlag = false;
                LOG.Error("#MInfoReceiver MemberController chkFlag:false!!");
                return base.SetPageNotFound(); //return new HttpStatusCodeResult(404); //return 404 not found
            }
            else if (!strSID.Equals(model.SID))
            {
                LOG.Error(string.Concat("#MInfoReceiver sid invalid : (sid=", model.SID, ")"));
                //chkFlag = false;
                LOG.Error("#MInfoReceiver MemberController chkFlag:false!!");
                return base.SetPageNotFound(); //return new HttpStatusCodeResult(404); //return 404 not found
            }

            try
            {
                TWJobsMemberDataModel memInfo = dao.HandleXmlData(model, RSA);
                if (memInfo != null)
                {
                    memInfo.SID = strSID;
                    dao.processMInfo(memInfo); //INSERT /UPDATE MEMBER
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("#MInfoReceiver failed Error: ", ex.Message), ex);
                //chkFlag = false;
                LOG.Error("#MInfoReceiver MemberController chkFlag:false!!");
                return base.SetPageNotFound(); //return new HttpStatusCodeResult(404); //return 404 not found
            }
            LOG.Debug("MemberController.MInfoReceiver End......");

            return Content("1");  //for success response
        }

        /// <summary> 成功登入處理 </summary>
        /// <param name="sid">單一簽入系統代碼</param>
        /// <param name="rid">亂數抽籤號碼牌</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public ActionResult LoginSuccess(TWJobsMemberDataModel model)
        {
            string str_METHOD = Request.QueryString["_method"];
            if (str_METHOD != null && !str_METHOD.Equals("")) { return base.SetPageNotFound(); }

            LOG.Debug("MemberController.LoginSuccess Begin......");

            //SessionModel sm = SessionModel.Get();,//ActionResult rtn = null;,//WDAIIPWEBDAO dao = new WDAIIPWEBDAO();,//Hashtable execParams = new Hashtable();,//TWJobsMemberDataModel twMem = null;

            string sid = model.SID;
            string rid = Convert.ToString(model.RID);
            string token = model.TOKEN;

            //rtn = RedirectToAction("Index", "Home");

            //return Content("sid 參數不存在");
            if (string.IsNullOrEmpty(sid)) { throw new ArgumentNullException("登入失敗：sid 參數不存在"); }
            //return Content("Rid 參數不存在");
            if (string.IsNullOrEmpty(rid)) { throw new ArgumentNullException("登入失敗：Rid 參數不存在"); }
            //return Content("token 參數不存在");
            if (string.IsNullOrEmpty(token)) { throw new ArgumentNullException("登入失敗：token 參數不存在"); }
            //return Content("sid '" + sid + "' 參數跟設定值不一致");
            if (!strSID.Equals(sid)) { throw new ArgumentException(string.Concat("登入失敗：sid '", sid, "' 參數跟設定值不一致")); }

            LOG.Debug(string.Concat("##LoginSuccess: sid=", sid, ", token=[", token, "]"));

            Hashtable tokens = null;
            try
            {
                tokens = new Commons.TokenPaser().parse(token);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("登入失敗：tokens 有誤!", ex.Message), ex);
                throw new ArgumentException("登入失敗：tokens 有誤!");
            }
            //LOG.Debug("LoginSuccess: decrypted tokens: " + JsonConvert.SerializeObject(tokens) );
            if (tokens != null)
            {
                string s_tokens = string.Empty;
                foreach (DictionaryEntry hV1 in tokens)
                {
                    if (!string.IsNullOrEmpty(s_tokens)) { s_tokens += ", "; }
                    s_tokens += string.Format("{0}={1}", hV1.Key, hV1.Value);
                    //Console.Write(hV1.Key); Console.WriteLine(hV1.Value); Console.WriteLine();
                }
                LOG.Debug(string.Concat("##LoginSuccess: tokens: ", s_tokens));
            }

            //return Content("明碼 sid 參數跟 token 中的值不一致");
            if (!sid.Equals(tokens["Sid"])) { throw new ArgumentException("登入失敗：明碼 sid 參數跟 token 中的值不一致"); }

            SessionModel sm = SessionModel.Get();
            ActionResult rtn = null;
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            TWJobsMemberDataModel twMem = null;

            rtn = RedirectToAction("Index", "Home");

            //TWJobsMemberDataModel twMem = dao.QueryMember(rid, "", (string)tokens["Mid"]);
            twMem = dao.QueryMember(rid, sid);
            if (twMem == null)
            {
                throw new ArgumentException(string.Concat("登入失敗：Rid '", rid, "', Mid '", tokens["Mid"], "' 對應的 member info 不存在"));
            }

            //twMem != null
            //if (twMem.MODIFYDATE.HasValue)
            //{
            //    DateTime date1 = DateTime.Now;//t1
            //    DateTime date2 = twMem.MODIFYDATE.Value.AddDays(1); //t2:加1天(1天後失效)
            //                                                        //登入失敗-date1 晚於 date12，回傳值:1
            //    if (DateTime.Compare(date1, date2) > 0) { throw new ArgumentException("登入失效：對應的 member info 不存在"); }
            //}

            //twMem != null
            //記錄登入者資訊 (會員資料同步到 E_MEMBER 中)
            dao.saveToEMember(twMem);

            //SessionModel sm = SessionModel.Get();
            sm.RID = Convert.ToString(model.RID);
            sm.SID = Convert.ToString(model.SID);

            string[] urlAry = { };
            if (!string.IsNullOrWhiteSpace(twMem.PAGEURL))
            {
                urlAry = twMem.PAGEURL.Split('/');
                if (urlAry.Length > 1)
                {
                    string controllerName = urlAry[1];
                    string actionName = urlAry[2];
                    rtn = RedirectToAction(urlAry[2], urlAry[1]);

                    if (twMem.OCID.HasValue)
                    {
                        rtn = RedirectToAction(actionName, controllerName, new { ocid = twMem.OCID, plantype = twMem.PLANTYPE });
                    }
                }
                //else { rtn = RedirectToAction("actionName", new RouteValueDictionary(new { controller = controllerName, action = actionName })); }
            }


            //2019-02-15 問題19：add 登入成功後檢查學員自訓後第1天到第21天是否有未填寫參訓學員意見調查表的狀況（任一門課），有則跳出提示訊息告知
            //ftdate + 1 ~ ftdate + 21
            IList<TblCLASS_CLASSINFO> chkQueList = dao.QueryQuestionFacNoWrite(twMem.ACID);
            if (chkQueList != null && chkQueList.Count > 0)
            {
                sm.LastResultMessage = "您尚有課後意見調查表未填，請至「會員專區」》「課後意見調查」填寫，感謝。";
            }

            return rtn;
        }

        /// <summary>測試傳入資料</summary>
        /// <returns></returns>
        public ActionResult ttMInfoReceiver()
        {
            //Minfo=%3CMEMBER_INFO%3E%0D%0A%20%20%3CSID%3ERA9UMnC8BjGk%2BiRo%2BmmRcgZvoK7x%2F5t0tsgPC1vduG8fCuK53oY9J5pWp1sv%2BaXebGKZAJJC%2BtIbRRW3IMKY5pcsn2XzNANTlody6LWZCWPzFRBAFyEckKDO1YqhTKDcssr%2BD4m%2FYBCy31NWVB3UCjwnVz1oQyJprT5RiBPQ9Uw%3D%3C%2FSID%3E%0D%0A%20%20%3CMEMBER_USER_ID%3EZ5oxwMEDITJo%2Fy9xrtHriqed7bzzbGYJlUBPI%2BzmK0cZ3uZ%2BD%2Bw%2BiE%2FvOCv1LoKp18tfn5d1gRL61F7sXxl%2FeKOcYCSBLDYGdcJab8wfy9XVF3s5ofBTCOcujShald3AIqshJOY7a57nMEdZGUsNdpJv0QlfP7oQIztIFdlByOY%3D%3C%2FMEMBER_USER_ID%3E%0D%0A%20%20%3CISFOREIGN%3Edf73RcLsKs%2FYMDqgMhzfysUg9O%2BBC3cMmbB9%2FWgIFWC5jSB57ZTTKytsqQ7Uhj6DEfMUsj6xiz%2FNp14jiPZ2fc7lBghm6l%2BgUAKrDDTplunwZHSxU654ndSbf8ahgurUNvqFYTb7LrzEqeiKuhjpeXyKy130HhVMmSjJdBKZ5GQ%3D%3C%2FISFOREIGN%3E%0D%0A%20%20%3CACID%3EkI8vM%2Bir7MAPUVZMkZ6Ua%2B1usK55oVPYEB9vH%2Ft6QVF0n%2BQsaAVkRm%2Bn6mfVb%2FxIfUV9aU%2F9um0subCtUiXZrkhlI3CK3z%2F3ALbjcuMgNjbMP18vsgg9ylL4XMoPubgJtFmMBJZ1xxdrlnvFwk2PJIxB8L5URZD4u1LCptcxxI4%3D%3C%2FACID%3E%0D%0A%20%20%3CPASSPORT%3Ef31x5km%2FvOwAdS2cCk2cx62VjfLdt4XG2mqbCRi2k0fKcYIz2903YivxthPISvwfjNt9P1d2I%2FepKT%2FxLJ62mIKrPYTG5kmPzuHdDyBaEM%2Bv8lDpTdJGFpWxJK5xnLK8D3juJWnDGcNPwR9Aw0XMb8IH0HkoSZOnBFrgad2YOVY%3D%3C%2FPASSPORT%3E%0D%0A%20%20%3CLANG%3ERuGfA6hWO3QHajG4vp5t4m6mwXRlaOzBLpmyWsZa1cSqlUg44c3M%2Fef4mRBh0J%2FKxiA8nZMWQbilL961cvrTuORrXJmtExFHl%2F%2BJjv3JAqufYLy1jprb7LkwC25zIEIGvtMeiG2ea6C2alHgffYxbCyvTZNbjlpkjqjpwZkpZGE%3D%3C%2FLANG%3E%0D%0A%20%20%3CNAME%3Eb14SGiMwRZRAJ5mCzDE1rEJrH0rR3BSNN%2FxK6KINH2y6E8RySyoWnfQhRoxKvypadIjilsw9A5WLidTF4SKnKEMVdEnEc6NRQ8MwMiNqhl46fLCluoal%2F7Z84BPek1G8oyu%2FCFR5UmDJYOdnMvdUGxYEzlfSAuJXKY7fpxBWah8%3D%3C%2FNAME%3E%0D%0A%20%20%3CBIRTHDAY%3EXqD6y2XQ7tb9%2FE5K7OzByNY6Ctg8t7JKS%2BTaBWw7kPbzqCFG1sr5LuKXcCq%2FW%2Bm2sQe3JatO82e2znIRGX1Nv39reii5unYjVgClUzaqi2SyVeTIHMxePcrzUKoz6FeAFc1Q1qjx%2BunadRX48OKNC%2FK9u%2Bcpm1ruXqLvdS%2FsS58%3D%3C%2FBIRTHDAY%3E%0D%0A%20%20%3CSEX%3EGbWmEPTCefxQEQGQgzjEIBPR6rNCBPnl1xfBl2EKyLJg8V5Xv444OgHP4u1cyfa1tGIJELD5qBq4T5bQaHjQ5YTEot0Tu%2BzvIEzr50wyftMqvNUVCLnWI1zYDZKEIa8yXzrQZHIYMDb9e7Y0E3HC0YhpV7fk%2FqUH8sza4N6%2B3ls%3D%3C%2FSEX%3E%0D%0A%20%20%3CEMAIL%3ELfKvOFNM9cCWrbM05uZFck0VEk98bqfl3kairyp8mR%2FJG0O227rS6TOoaq97Yxb9n94wthA5a7Edqw50pyRZtSeEbES9sVqqkhvBBaeExFoBOTqRrxObMDvTRAq5aVCogxB%2BvCpOJknBD3GZN4mfMbZdrO7guLJzqBXM4YYAwrA%3D%3C%2FEMAIL%3E%0D%0A%20%20%3CZIPCODE%3Ee3aMKT7dTL2tLpX1i4vE0m9cazNcr9oY6zgnYrsT6wnoRAGTxqixu6mIp6m4sKvLK0LGyVAAfy8OI9%2BFUqyNAE%2BRKZ6jUuU%2Bd6MI3i%2FhBVPcsWgXkdE7mIIFN1TuTQlPU09O4w8OyqbTwTRMDHesWf9VAZb3mpBR6KKGUSy44yw%3D%3C%2FZIPCODE%3E%0D%0A%20%20%3CADDR_CITY_1%3EXZxkVsrx3JYeeUKSx10%2Ftkpi8%2BvMH0zK0RCqKvCdxmYgvOPG8QRPlz%2FZM84NbEo3uNuQ90Fwv8VgwX4XAWugiYWXrXy%2BvvQFnZ6T9MxC1NQRadA7mGXnmDdl2BP5vdRN4ZOCO7Kofv0fV8K7l%2FBMuRwzu%2BIcq1iPQ9ljPuyFhNI%3D%3C%2FADDR_CITY_1%3E%0D%0A%20%20%3CADDR_CITY_2%3EEMhpwZ1BnVnsV03bBXS9CvOafkiOjNXPHGS634cxIYBrN14VLc7j7rfgqDJQ55MuKeS%2BRLTvtxgEKu4wz1t0P1uESojHmVW5vUw7hlEE%2B2oJr6o6XzC5mcErGEqxexuSRrFXsDOL8jflRlYxbVUA77WhOO%2BkYNhA70FNVxI%2FnuM%3D%3C%2FADDR_CITY_2%3E%0D%0A%20%20%3CADDR%3EtSvErlf9TsklJ9lHL87RBBqxPLEBgKuTLPqjCPXvRUobqIb%2FzLiY5oSRkFxkXkUDpMARoD540Sn61SjwlHCRFp5xHm8ZkbgRlgIk1GJZoCWkOY39meuldg%2F7FEVg3%2B%2FuVmPVgZUEOIAAdtG0Lz%2Byr6KC8DDxdxYguxtgG14tGA4ulaMPDhJzNP5SX0oe%2BUxfvHI%2BAG8umW0No0lYqmlmnbWmLqvk34QbdrXcImU2BPyM2KoYxjAR4Y6rCKO%2BFAZaNSpEn1JbxX%2FeFbweVncqLOEvOBzA725PxUDjoW3VoGi9HcYO9X4u%2BrFAZEQrknj4XnCEWabHxRSPnT4xDjV7Xg%3D%3D%3C%2FADDR%3E%0D%0A%20%20%3CTEL1%3EVMuAT82qEi1vS6JSwzylh9Ck3d%2BAC150BeP2kT90%2F0jtIk1YYTMKMYO6k2HzJZKf1RaXu1oLI7NP2GsQhZC9tnRYAOSXG%2B1L8WLDoCQtfCNfElxpg%2BL31ArlDVuGHtCzN7j9WIrsaxEZHmpqvqPnRFIX2VVL8wDRPo7gQ77ytCQ%3D%3C%2FTEL1%3E%0D%0A%20%20%3CTEL2%3ECb4tG6IdPCuaK%2FFaPt3LVt1HSu9PkLMA7h2XT48Ep2ZE6FtK132Fmrdnv1vTfD8eYXGmg1jx5M8go0cDA6Jf9Zv28V14DH79KvTpUbk4EZek%2BqkMvp8pM0J0jGB6HJKZr89lxfVhZ8eW5yikKk6K81%2FVCcEFIWSuMbcFeyZMPu4%3D%3C%2FTEL2%3E%0D%0A%20%20%3CMOBILE%3Enbl5R2qrmooaq791uGxc0VZxbhmjap%2FUAeN6wryZpPGgu%2FjrBfwmqsGKbATzgC%2FKTLXPVT%2BXNv1LSB0VboO190IahWiPdDY%2FZhnezDJkj52xsxB00oWwkMRQgjrR5doeamulyzzy48ymxnK%2FgZgw1CMf45BLiKy%2F4Tik0bxt8sk%3D%3C%2FMOBILE%3E%0D%0A%20%20%3CFAX%3EohYbpLdoY2lcRH%2Fm5Ff5HiSKwVIaIdXv8qSWMpkZ43JrA2sOOYra0gApJEKfZF9G11xGdd1j2euf2%2B%2F3lt4hYBnqyB8lRU1QKH5%2BHUCB9Gy7tuubLoa9PdvZ2yht7cdpomqltYNdiQoPbcbvArXyz%2BA%2BvZCPko5IUFHR33AcO0I%3D%3C%2FFAX%3E%0D%0A%20%20%3CEDU%3EtoGjJJK0NLJnpW%2Fs40y7ENaULvw%2FFOYATbQyDo4nJwMZrLB99Mi3g96N3i4vTx1trh5pg3qLN3UldOVGIUGP5iTZr5hEHlaIY9nz97uR3cRl28PjpK4DGSKZe35XQu8ZHjvlU4g2iu0XyyGityI8pJtFpIaFe5wz0XIkVpneock%3D%3C%2FEDU%3E%0D%0A%20%20%3CMARRI%3Ems4F%2BPbjzH2uQEJQU9WL2twLnTfP%2F5s7CML5jx9x7zy9BI%2FgwnJ39KI9PV6L%2FdvyAjq5F4hg494KQ%2BGRtqOIIZ0by%2F3O9PRgGkCE9BTAy8JfKDuMpxZzYZAT1WE0scolTNOqZd7sICqHzWVPchk%2FgmdnuuXqlMepk8Lur7Sf%2Bio%3D%3C%2FMARRI%3E%0D%0A%20%20%3CGRADU%3EDDluIdUI%2FcY2bUQv%2F57e4lSWhRoC4%2ByW4Fto%2F%2Fvm6b7KsdqPoUXG1bCBRcspWntsxVaSifHYzE%2FtYfsAzghRwY7ETgal5OGDnzvKRUKTwZffhsfFhmiBmJ42Gn%2F3L%2BJK919Yacca9MbHYmy1pRhThMXrn9UjDoLGc773KWHkx14%3D%3C%2FGRADU%3E%0D%0A%3C%2FMEMBER_INFO%3E&Sid=0022&Rid=1029118160
            TWJobsMemberDataModel model = new TWJobsMemberDataModel()
            {
                minfo = @"%3CMEMBER_INFO%3E%0D%0A%20%20%3CSID%3ERA9UMnC8BjGk%2BiRo%2BmmRcgZvoK7x%2F5t0tsgPC1vduG8fCuK53oY9J5pWp1sv%2BaXebGKZAJJC%2BtIbRRW3IMKY5pcsn2XzNANTlody6LWZCWPzFRBAFyEckKDO1YqhTKDcssr%2BD4m%2FYBCy31NWVB3UCjwnVz1oQyJprT5RiBPQ9Uw%3D%3C%2FSID%3E%0D%0A%20%20%3CMEMBER_USER_ID%3EZ5oxwMEDITJo%2Fy9xrtHriqed7bzzbGYJlUBPI%2BzmK0cZ3uZ%2BD%2Bw%2BiE%2FvOCv1LoKp18tfn5d1gRL61F7sXxl%2FeKOcYCSBLDYGdcJab8wfy9XVF3s5ofBTCOcujShald3AIqshJOY7a57nMEdZGUsNdpJv0QlfP7oQIztIFdlByOY%3D%3C%2FMEMBER_USER_ID%3E%0D%0A%20%20%3CISFOREIGN%3Edf73RcLsKs%2FYMDqgMhzfysUg9O%2BBC3cMmbB9%2FWgIFWC5jSB57ZTTKytsqQ7Uhj6DEfMUsj6xiz%2FNp14jiPZ2fc7lBghm6l%2BgUAKrDDTplunwZHSxU654ndSbf8ahgurUNvqFYTb7LrzEqeiKuhjpeXyKy130HhVMmSjJdBKZ5GQ%3D%3C%2FISFOREIGN%3E%0D%0A%20%20%3CACID%3EkI8vM%2Bir7MAPUVZMkZ6Ua%2B1usK55oVPYEB9vH%2Ft6QVF0n%2BQsaAVkRm%2Bn6mfVb%2FxIfUV9aU%2F9um0subCtUiXZrkhlI3CK3z%2F3ALbjcuMgNjbMP18vsgg9ylL4XMoPubgJtFmMBJZ1xxdrlnvFwk2PJIxB8L5URZD4u1LCptcxxI4%3D%3C%2FACID%3E%0D%0A%20%20%3CPASSPORT%3Ef31x5km%2FvOwAdS2cCk2cx62VjfLdt4XG2mqbCRi2k0fKcYIz2903YivxthPISvwfjNt9P1d2I%2FepKT%2FxLJ62mIKrPYTG5kmPzuHdDyBaEM%2Bv8lDpTdJGFpWxJK5xnLK8D3juJWnDGcNPwR9Aw0XMb8IH0HkoSZOnBFrgad2YOVY%3D%3C%2FPASSPORT%3E%0D%0A%20%20%3CLANG%3ERuGfA6hWO3QHajG4vp5t4m6mwXRlaOzBLpmyWsZa1cSqlUg44c3M%2Fef4mRBh0J%2FKxiA8nZMWQbilL961cvrTuORrXJmtExFHl%2F%2BJjv3JAqufYLy1jprb7LkwC25zIEIGvtMeiG2ea6C2alHgffYxbCyvTZNbjlpkjqjpwZkpZGE%3D%3C%2FLANG%3E%0D%0A%20%20%3CNAME%3Eb14SGiMwRZRAJ5mCzDE1rEJrH0rR3BSNN%2FxK6KINH2y6E8RySyoWnfQhRoxKvypadIjilsw9A5WLidTF4SKnKEMVdEnEc6NRQ8MwMiNqhl46fLCluoal%2F7Z84BPek1G8oyu%2FCFR5UmDJYOdnMvdUGxYEzlfSAuJXKY7fpxBWah8%3D%3C%2FNAME%3E%0D%0A%20%20%3CBIRTHDAY%3EXqD6y2XQ7tb9%2FE5K7OzByNY6Ctg8t7JKS%2BTaBWw7kPbzqCFG1sr5LuKXcCq%2FW%2Bm2sQe3JatO82e2znIRGX1Nv39reii5unYjVgClUzaqi2SyVeTIHMxePcrzUKoz6FeAFc1Q1qjx%2BunadRX48OKNC%2FK9u%2Bcpm1ruXqLvdS%2FsS58%3D%3C%2FBIRTHDAY%3E%0D%0A%20%20%3CSEX%3EGbWmEPTCefxQEQGQgzjEIBPR6rNCBPnl1xfBl2EKyLJg8V5Xv444OgHP4u1cyfa1tGIJELD5qBq4T5bQaHjQ5YTEot0Tu%2BzvIEzr50wyftMqvNUVCLnWI1zYDZKEIa8yXzrQZHIYMDb9e7Y0E3HC0YhpV7fk%2FqUH8sza4N6%2B3ls%3D%3C%2FSEX%3E%0D%0A%20%20%3CEMAIL%3ELfKvOFNM9cCWrbM05uZFck0VEk98bqfl3kairyp8mR%2FJG0O227rS6TOoaq97Yxb9n94wthA5a7Edqw50pyRZtSeEbES9sVqqkhvBBaeExFoBOTqRrxObMDvTRAq5aVCogxB%2BvCpOJknBD3GZN4mfMbZdrO7guLJzqBXM4YYAwrA%3D%3C%2FEMAIL%3E%0D%0A%20%20%3CZIPCODE%3Ee3aMKT7dTL2tLpX1i4vE0m9cazNcr9oY6zgnYrsT6wnoRAGTxqixu6mIp6m4sKvLK0LGyVAAfy8OI9%2BFUqyNAE%2BRKZ6jUuU%2Bd6MI3i%2FhBVPcsWgXkdE7mIIFN1TuTQlPU09O4w8OyqbTwTRMDHesWf9VAZb3mpBR6KKGUSy44yw%3D%3C%2FZIPCODE%3E%0D%0A%20%20%3CADDR_CITY_1%3EXZxkVsrx3JYeeUKSx10%2Ftkpi8%2BvMH0zK0RCqKvCdxmYgvOPG8QRPlz%2FZM84NbEo3uNuQ90Fwv8VgwX4XAWugiYWXrXy%2BvvQFnZ6T9MxC1NQRadA7mGXnmDdl2BP5vdRN4ZOCO7Kofv0fV8K7l%2FBMuRwzu%2BIcq1iPQ9ljPuyFhNI%3D%3C%2FADDR_CITY_1%3E%0D%0A%20%20%3CADDR_CITY_2%3EEMhpwZ1BnVnsV03bBXS9CvOafkiOjNXPHGS634cxIYBrN14VLc7j7rfgqDJQ55MuKeS%2BRLTvtxgEKu4wz1t0P1uESojHmVW5vUw7hlEE%2B2oJr6o6XzC5mcErGEqxexuSRrFXsDOL8jflRlYxbVUA77WhOO%2BkYNhA70FNVxI%2FnuM%3D%3C%2FADDR_CITY_2%3E%0D%0A%20%20%3CADDR%3EtSvErlf9TsklJ9lHL87RBBqxPLEBgKuTLPqjCPXvRUobqIb%2FzLiY5oSRkFxkXkUDpMARoD540Sn61SjwlHCRFp5xHm8ZkbgRlgIk1GJZoCWkOY39meuldg%2F7FEVg3%2B%2FuVmPVgZUEOIAAdtG0Lz%2Byr6KC8DDxdxYguxtgG14tGA4ulaMPDhJzNP5SX0oe%2BUxfvHI%2BAG8umW0No0lYqmlmnbWmLqvk34QbdrXcImU2BPyM2KoYxjAR4Y6rCKO%2BFAZaNSpEn1JbxX%2FeFbweVncqLOEvOBzA725PxUDjoW3VoGi9HcYO9X4u%2BrFAZEQrknj4XnCEWabHxRSPnT4xDjV7Xg%3D%3D%3C%2FADDR%3E%0D%0A%20%20%3CTEL1%3EVMuAT82qEi1vS6JSwzylh9Ck3d%2BAC150BeP2kT90%2F0jtIk1YYTMKMYO6k2HzJZKf1RaXu1oLI7NP2GsQhZC9tnRYAOSXG%2B1L8WLDoCQtfCNfElxpg%2BL31ArlDVuGHtCzN7j9WIrsaxEZHmpqvqPnRFIX2VVL8wDRPo7gQ77ytCQ%3D%3C%2FTEL1%3E%0D%0A%20%20%3CTEL2%3ECb4tG6IdPCuaK%2FFaPt3LVt1HSu9PkLMA7h2XT48Ep2ZE6FtK132Fmrdnv1vTfD8eYXGmg1jx5M8go0cDA6Jf9Zv28V14DH79KvTpUbk4EZek%2BqkMvp8pM0J0jGB6HJKZr89lxfVhZ8eW5yikKk6K81%2FVCcEFIWSuMbcFeyZMPu4%3D%3C%2FTEL2%3E%0D%0A%20%20%3CMOBILE%3Enbl5R2qrmooaq791uGxc0VZxbhmjap%2FUAeN6wryZpPGgu%2FjrBfwmqsGKbATzgC%2FKTLXPVT%2BXNv1LSB0VboO190IahWiPdDY%2FZhnezDJkj52xsxB00oWwkMRQgjrR5doeamulyzzy48ymxnK%2FgZgw1CMf45BLiKy%2F4Tik0bxt8sk%3D%3C%2FMOBILE%3E%0D%0A%20%20%3CFAX%3EohYbpLdoY2lcRH%2Fm5Ff5HiSKwVIaIdXv8qSWMpkZ43JrA2sOOYra0gApJEKfZF9G11xGdd1j2euf2%2B%2F3lt4hYBnqyB8lRU1QKH5%2BHUCB9Gy7tuubLoa9PdvZ2yht7cdpomqltYNdiQoPbcbvArXyz%2BA%2BvZCPko5IUFHR33AcO0I%3D%3C%2FFAX%3E%0D%0A%20%20%3CEDU%3EtoGjJJK0NLJnpW%2Fs40y7ENaULvw%2FFOYATbQyDo4nJwMZrLB99Mi3g96N3i4vTx1trh5pg3qLN3UldOVGIUGP5iTZr5hEHlaIY9nz97uR3cRl28PjpK4DGSKZe35XQu8ZHjvlU4g2iu0XyyGityI8pJtFpIaFe5wz0XIkVpneock%3D%3C%2FEDU%3E%0D%0A%20%20%3CMARRI%3Ems4F%2BPbjzH2uQEJQU9WL2twLnTfP%2F5s7CML5jx9x7zy9BI%2FgwnJ39KI9PV6L%2FdvyAjq5F4hg494KQ%2BGRtqOIIZ0by%2F3O9PRgGkCE9BTAy8JfKDuMpxZzYZAT1WE0scolTNOqZd7sICqHzWVPchk%2FgmdnuuXqlMepk8Lur7Sf%2Bio%3D%3C%2FMARRI%3E%0D%0A%20%20%3CGRADU%3EDDluIdUI%2FcY2bUQv%2F57e4lSWhRoC4%2ByW4Fto%2F%2Fvm6b7KsdqPoUXG1bCBRcspWntsxVaSifHYzE%2FtYfsAzghRwY7ETgal5OGDnzvKRUKTwZffhsfFhmiBmJ42Gn%2F3L%2BJK919Yacca9MbHYmy1pRhThMXrn9UjDoLGc773KWHkx14%3D%3C%2FGRADU%3E%0D%0A%3C%2FMEMBER_INFO%3E",
                SID = @"0022",
                RID = 1029118160
            };

            string strPrivateKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPrivateKeyFile);
            string strPublicKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPublicKeyFile);
            LOG.Debug("MemberController.MInfoReceiver Begin...");

            RSAEncryption RSA = null;
            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            //bool chkFlag = true;
            try
            {
                //比對金鑰
                RSA = new RSAEncryption(strPublicKeyFile, strPrivateKeyFile);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("#MInfoReceiver Create RSAEncryption failed Error: ", ex.Message), ex);
                //chkFlag = false;
                LOG.Error("#MInfoReceiver MemberController chkFlag:false!!");
                return base.SetPageNotFound(); //return new HttpStatusCodeResult(404); //return 404 not found
            }

            if (string.IsNullOrEmpty(model.SID) || string.IsNullOrEmpty(Convert.ToString(model.RID)) || string.IsNullOrEmpty(model.minfo))
            {
                LOG.Error(string.Concat("#MInfoReceiver Arguments invalid : (sid=", model.SID, ",rid=", model.RID, ",minfo=", model.minfo, ")"));
                //chkFlag = false;
                LOG.Error("#MInfoReceiver MemberController chkFlag:false!!");
                return base.SetPageNotFound(); //return new HttpStatusCodeResult(404); //return 404 not found
            }
            else if (!strSID.Equals(model.SID))
            {
                LOG.Error(string.Concat("#MInfoReceiver sid invalid : (sid=", model.SID, ")"));
                //chkFlag = false;
                LOG.Error("#MInfoReceiver MemberController chkFlag:false!!");
                return base.SetPageNotFound(); //return new HttpStatusCodeResult(404); //return 404 not found
            }

            try
            {
                TWJobsMemberDataModel memInfo = dao.HandleXmlData(model, RSA);
                if (memInfo != null)
                {
                    memInfo.SID = strSID;
                    dao.processMInfo(memInfo); //INSERT /UPDATE MEMBER
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Concat("#MInfoReceiver failed Error: ", ex.Message), ex);
                //chkFlag = false;
                LOG.Error("#MInfoReceiver MemberController chkFlag:false!!");
                return base.SetPageNotFound(); //return new HttpStatusCodeResult(404); //return 404 not found
            }
            LOG.Debug("MemberController.MInfoReceiver End...");

            return Content("1");  //for success response
        }

        /// <summary>產生亂數號碼</summary>
        /// <returns></returns>
        private string getRandomRID()
        {
            string strRet = "";
            Random rnd = new Random();
            strRet = Convert.ToString(rnd.Next());
            return strRet;
        }
        #endregion

        /// <summary> 取得url </summary>
        /// <param name="SEQNO"></param>
        /// <returns></returns>
        string get_url_1(string SEQNO)
        {
            string r_url = null;
            int i_SEQNO = -1;
            if (!int.TryParse(SEQNO, out i_SEQNO)) { return r_url; }
            LinkFormModel linkfm = new LinkFormModel();
            linkfm.FUNID = "010";
            linkfm.SEQNO = i_SEQNO; //Convert.ToInt32(SEQNO);

            WDAIIPWEBDAO dao = new WDAIIPWEBDAO();
            IList<LinkGridModel> lkGrid = dao.QueryLink2(linkfm);
            if (lkGrid == null) { return null; }//return base.SetPageNotFound();
            if (lkGrid.Count == 0) { return null; }//return base.SetPageNotFound();

            //string s_url_1 = string.Empty;
            foreach (LinkGridModel lkgm1 in lkGrid)
            {
                r_url = lkgm1.C_URL;
                return r_url; //break;
            }
            return r_url;
        }

        /// <summary> SsoRedirect SSO-重導 </summary>
        /// <param name="TID"></param>
        /// <param name="SEQNO"></param>
        /// <returns></returns>
        private ActionResult RedirectSso2(string TID, string SEQNO)
        {
            if (TID == null) { return base.SetPageNotFound(); }
            if (string.IsNullOrEmpty(TID)) { return base.SetPageNotFound(); }

            ArrayList TidSchAry = new ArrayList() { "1", "2" };
            if (!TidSchAry.Contains(TID))
            {
                LOG.Debug("[ALERT] MemberController-Url2()");
                return base.SetPageNotFound();
            }

            //string strPrivateKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPrivateKeyFile);
            string strPublicKeyFile = HttpContext.Server.MapPath(ConfigModel.RSAPublicKeyFile);
            //string strLoginUrl = ConfigModel.TwJobsLogin_URL;
            SessionModel sm = SessionModel.Get();

            string s_islogin = sm.IsLogin ? "true" : "false";
            LOG.Debug(string.Format("###Url2-s_islogin={0}", s_islogin));

            if (SEQNO == null) { return base.SetPageNotFound(); }
            if (string.IsNullOrEmpty(SEQNO)) { return base.SetPageNotFound(); }

            string s_Url_1 = string.Empty;
            s_Url_1 = get_url_1(SEQNO);
            if (s_Url_1 == null) { return base.SetPageNotFound(); }
            if (string.IsNullOrEmpty(s_Url_1)) { return base.SetPageNotFound(); }

            //沒有登入直接轉址 (有登入採用加密 SSO 重導)
            if (!sm.IsLogin) { return Redirect(s_Url_1); }

            string s_SsoTargetSid = string.Empty;
            //select * from TB_CONTENT where FUNID='010' and C_LINKURL3 is not null
            //1.青年職訓資源網 -0011
            //2.職前訓練網 -0111
            //0011 青年職訓資源網站 0111 職前訓練網
            //3.台灣就業通 - 找人才
            //4.台灣就業通 - 找工作
            switch (TID)
            {
                case "1":
                    s_SsoTargetSid = "0011";
                    break;
                case "2":
                    s_SsoTargetSid = "0111";
                    break;
                case "3":
                    s_SsoTargetSid = string.Empty;//TID;
                    break;
                case "4":
                    s_SsoTargetSid = string.Empty;//TID;
                    break;
                default:
                    return base.SetPageNotFound();
            }
            LOG.Debug(string.Format("###Url2-Sid={0},Tid={1},Rid={2},Mid={3}", sm.SID, s_SsoTargetSid, sm.RID, sm.MEMBER_USER_ID));

            //我需要這些網站的 tid資訊
            //文件中並沒有提到這些網站的tid資訊
            if (string.IsNullOrEmpty(s_SsoTargetSid)) { return base.SetPageNotFound(); }

            //string s_Url_1 = "https://ojt.wda.gov.tw/";
            string s_SsoRedirectUrl = "https://sso.taiwanjobs.gov.tw/member/SsoRedirect.aspx";
            string s_Redirect = string.Concat("Sid=", sm.SID, ",Mid=", sm.MEMBER_USER_ID, ",Url=", s_Url_1);
            string s_Enpama = new RSAEncryption(strPublicKeyFile).EncryptData(s_Redirect);
            string s_url_3 = string.Concat(s_SsoRedirectUrl, "?Sid=", sm.SID, "&Tid=", s_SsoTargetSid, "&Rid=", sm.RID, "&Token=", Server.UrlEncode(s_Enpama));
            //Response.Redirect(url); //轉址 
            return Redirect(s_url_3);
        }

        /// <summary> SsoRedirect SSO-重導 </summary>
        /// <param name="TID"></param>
        /// <param name="SEQNO"></param>
        /// <returns></returns>
        public ActionResult Url2(string TID, string SEQNO)
        {
            return RedirectSso2(TID, SEQNO);
        }

    }
}