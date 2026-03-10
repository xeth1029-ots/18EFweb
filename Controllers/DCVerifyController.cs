using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models;
using WDAIIP.WEB.Models.Entities;

namespace WDAIIP.WEB.Controllers
{
    public class DCVerifyController : BaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: DCVerify //BaseController,Controller 線上查驗
        public ActionResult Index(string UD1 = null)
        {
            SessionModel sm = SessionModel.Get();
            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu09;

            if (!string.IsNullOrEmpty(UD1)) { LOG.Debug("#UD1: " + UD1); }

            var model = new DigiCertViewModel();
            var dao = new WDAIIPWEBDAO();
            var form1 = new DigiCertFormModel();
            var guid1 = (UD1 != null && !string.IsNullOrEmpty(UD1.Trim())) ? UD1.Trim().ToUpper() : "";

            if (!string.IsNullOrEmpty(guid1))
            {
                if (guid1.Length < 26 || !MyCommonUtil.IsGuidValid(guid1))
                {
                    var LastErrmsg = string.Concat("檢查驗證碼有誤!", NonceGenerator.GenerateRandomStr(5)); LOG.Warn("#Errmsg: " + LastErrmsg);
                    sm.LastResultMessage = LastErrmsg;
                    return HttpNotFound(); //資安調整-Often Misused: HTTP Method Override ( 11534 )  //return View(model);
                }
                LOG.Debug("#GUID1: " + guid1);

                var wdc1 = new TblSTUD_DIGICERTAPPLY() { GUID1 = guid1 };
                var dc1 = dao.GetRow(wdc1);
                if (dc1 != null)
                {
                    form1.DCANO = dc1.DCANO;
                    form1.DCASENO = dc1.DCASENO;
                    form1.IDNO = dc1.IDNO;
                    form1.CNAME = dc1.CNAME;
                    form1.PURID = dc1.PURID;
                    form1.UAGID = dc1.UAGID;
                    form1.EMAIL = dc1.EMAIL;
                    form1.EMVCODE = dc1.EMVCODE;
                    form1.GUID1 = dc1.GUID1;
                    form1.APPLNACCT = dc1.APPLNACCT;
                    form1.APPLNDATE = dc1.APPLNDATE;
                    form1.MODIFYACCT = dc1.MODIFYACCT;
                    form1.MODIFYDATE = dc1.MODIFYDATE;
                    //return form1;
                    model.Form = form1;
                    return View(model); //return View();
                }
            }

            if (guid1 != null && !string.IsNullOrEmpty(guid1) && !form1.DCANO.HasValue) sm.LastResultMessage = "查無資料!";

            model.Form = form1;
            return View(model); //return View();
        }

        [HttpPost]
        public ActionResult Index(DigiCertViewModel model)
        {
            if (model == null || model.Form == null)
            {
                LOG.Warn(string.Concat("#HttpNotFound!", NonceGenerator.GenerateRandomStr(5)));
                return HttpNotFound(); //異常
            }

            SessionModel sm = SessionModel.Get();
            if (string.IsNullOrEmpty(model.Form.GUID1))
            {
                var LastErrmsg = string.Concat("檢查驗證碼有誤,不可為空!", NonceGenerator.GenerateRandomStr(5)); LOG.Warn("#Errmsg: " + LastErrmsg);
                sm.LastResultMessage = LastErrmsg;
                return HttpNotFound(); //資安調整-Often Misused: HTTP Method Override ( 11534 )  //return View(model); 
            }

            //設定所在主功能表位置
            sm.ACTIVEFUNCTION = ConfigModel.MainMenu09;

            var dao = new WDAIIPWEBDAO();
            var form1 = model.Form; // new DigiCertFormModel();
            var guid1 = (form1.GUID1 != null && !string.IsNullOrEmpty(form1.GUID1.Trim())) ? form1.GUID1.Trim().ToUpper() : "";

            if (!string.IsNullOrEmpty(guid1))
            {
                if (guid1.Length < 26 || !MyCommonUtil.IsGuidValid(guid1))
                {
                    var LastErrmsg = string.Concat("檢查驗證碼有誤!", NonceGenerator.GenerateRandomStr(5)); LOG.Warn("#Errmsg: " + LastErrmsg);
                    sm.LastResultMessage = LastErrmsg;
                    return HttpNotFound(); //資安調整-Often Misused: HTTP Method Override ( 11534 )  //return View(model); 
                }
                LOG.Debug("#GUID1: " + model.Form.GUID1);

                var wdc1 = new TblSTUD_DIGICERTAPPLY() { GUID1 = guid1 };
                var dc1 = dao.GetRow(wdc1);
                if (dc1 != null)
                {
                    form1.DCANO = dc1.DCANO;
                    form1.DCASENO = dc1.DCASENO;
                    form1.IDNO = dc1.IDNO;
                    form1.CNAME = dc1.CNAME;
                    form1.PURID = dc1.PURID;
                    form1.UAGID = dc1.UAGID;
                    form1.EMAIL = dc1.EMAIL;
                    form1.EMVCODE = dc1.EMVCODE;
                    form1.GUID1 = dc1.GUID1;
                    form1.APPLNACCT = dc1.APPLNACCT;
                    form1.APPLNDATE = dc1.APPLNDATE;
                    form1.MODIFYACCT = dc1.MODIFYACCT;
                    form1.MODIFYDATE = dc1.MODIFYDATE;
                    //return form1;
                    model.Form = form1;
                    return View("Index", model); // View();
                }
            }

            if (guid1 != null && !string.IsNullOrEmpty(guid1) && !form1.DCANO.HasValue)
            {
                var LastErrmsg = string.Concat("檢查驗證碼,查無資料!", NonceGenerator.GenerateRandomStr(5)); LOG.Warn("#Errmsg: " + LastErrmsg);
                sm.LastResultMessage = LastErrmsg;
                return HttpNotFound(); //資安調整-Often Misused: HTTP Method Override ( 11534 )  
            }

            //model.Form = form1;  //return View("Index", model); // View();
            return HttpNotFound(); //資安調整-Often Misused: HTTP Method Override ( 11534 ) 
        }
    }
}
