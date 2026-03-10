using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using WDAIIP.WEB.Commons;
using WDAIIP.WEB.DataLayers;
using WDAIIP.WEB.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Turbo.Commons;
using WDAIIP.WEB.Services;

namespace WDAIIP.WEB.Models
{
    #region SignUpViewModel
    public class SignUpViewModel
    {
        public SignUpViewModel()
        {
            this.Form = new SignUpFormModel();
        }

        /// <summary>
        /// 會員資料 FormModel
        /// </summary>
        public SignUpFormModel Form { get; set; }

        /// <summary>
        /// 個人報名基本資料 DetailModel
        /// </summary>
        public SignUpDetailModel Detail { get; set; }

        /// <summary>
        /// 要報名的課程基本資料
        /// <para>根據報名課程 OCID 由資料庫中載入</para>
        /// </summary>
        public ClassClassInfoExtModel ClassInfo { get; set; }

        /// <summary>
        /// 報名時的系統時間
        /// </summary>
        public DateTime? SignUpTime { get; set; }

        /// <summary>
        /// 產投報名結果序號
        /// </summary>
        public long? SIGNNO { get; set; }

        ///// <summary>
        ///// 是否成功報名(取得產投報名結果序號)
        ///// </summary>
        //public bool IsUpdateType2OK { get; set; }

        /// <summary>
        /// 報名結果錯誤訊息
        /// </summary>
        public string SignUpErrMsg { get; set; }


        /// <summary>
        /// 查詢計畫類型 清單來源
        /// 2018-12-26 add區域據點(tplanid=70)
        /// </summary>
        public IList<SelectListItem> PlanType_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "2","分署自辦在職訓練" },
                    { "1","產業人才投資方案" },
                    { "5","區域產業據點" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 身分別 清單來源
        /// </summary>
        public IList<SelectListItem> PASSPORTNO_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "1","本國"},
                    { "2","外籍(含大陸人士)" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 性別 清單來源
        /// </summary>
        public IList<SelectListItem> SEX_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "M","男"},
                    { "F","女" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 最高學歷 清單來源
        /// </summary>
        public IList<SelectListItem> DEGREEID_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetDegreeIDList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 畢業狀況 清單來源
        /// </summary>
        public IList<SelectListItem> GRADID_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetGraduateStatusList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 婚姻狀況 清單來源
        /// </summary>
        public IList<SelectListItem> MARITALSTATUS_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "1","已婚"},
                    { "2","未婚" },
                    { "3","暫不提供" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>主要參訓身份別_下拉選單。</summary>
        public IEnumerable<SelectListItem> MIdentityID_list
        {
            get
            {
                IList<KeyMapModel> list = (new MyKeyMapDAO()).GetMIdentityIDList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>服務部門_下拉選單。</summary>
        public IEnumerable<SelectListItem> Servdept_list
        {
            get
            {
                IList<KeyMapModel> list = (new MyKeyMapDAO()).GetServdeptList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>投保類別_下拉選單。</summary>
        public IList<SelectListItem> ActType_List()
        {
            //new SelectListItem { Value = "3", Text = "漁" }
            IList<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "勞" },
                new SelectListItem { Value = "2", Text = "農" },
                new SelectListItem { Value = "4", Text = "軍" }
            };
            return list;
        }

        /// <summary>職稱_下拉選單。</summary>
        public IEnumerable<SelectListItem> JobTitle_list
        {
            get
            {
                IList<KeyMapModel> list = (new MyKeyMapDAO()).GetJobTitleList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 婚姻狀況 清單來源
        /// </summary>
        public IList<SelectListItem> YesNo_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "1","是"},
                    { "0","否" },
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>服務單位行業別_下拉選單。</summary>
        public IEnumerable<SelectListItem> Q4_list
        {
            get
            {
                IList<KeyMapModel> list = (new MyKeyMapDAO()).GetTradeList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary> 儲存欄位檢核 </summary>
        /// <param name="modelState"></param>
        public void Valid(ModelStateDictionary modelState)
        {
            string msg = string.Empty;
            string sDate = string.Empty;
            //bool blCheckIDNO = true;

            #region 調整欄位值(trim, 全型半型轉換...)
            if (!string.IsNullOrEmpty(this.Detail.NAME))
            {
                this.Detail.NAME = HttpUtility.HtmlDecode(this.Detail.NAME.Trim());
            }

            if (!string.IsNullOrEmpty(this.Detail.PHONED))
            {
                this.Detail.PHONED = this.Detail.PHONED.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.PHONEN))
            {
                this.Detail.PHONEN = this.Detail.PHONEN.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.CELLPHONE))
            {
                this.Detail.CELLPHONE = this.Detail.CELLPHONE.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.ADDRESS))
            {
                this.Detail.ADDRESS = this.Detail.ADDRESS.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.HOUSEHOLDADDRESS))
            {
                this.Detail.HOUSEHOLDADDRESS = this.Detail.HOUSEHOLDADDRESS.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.EMAIL))
            {
                this.Detail.EMAIL = this.Detail.EMAIL.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.UNAME))
            {
                this.Detail.UNAME = this.Detail.UNAME.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.ACTNAME))
            {
                this.Detail.ACTNAME = this.Detail.ACTNAME.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.ACTNO))
            {
                this.Detail.ACTNO = this.Detail.ACTNO.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.ACTTEL))
            {
                this.Detail.ACTTEL = this.Detail.ACTTEL.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.INTAXNO))
            {
                this.Detail.INTAXNO = this.Detail.INTAXNO.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.SERVDEPT))
            {
                this.Detail.SERVDEPT = this.Detail.SERVDEPT.Trim();
            }

            if (!string.IsNullOrEmpty(this.Detail.ACTADDRESS))
            {
                this.Detail.ACTADDRESS = this.Detail.ACTADDRESS.Trim();
            }

            //相關輸入欄位字母全型轉半型
            this.Detail.IDNO = MyHelperUtil.ChangeIDNO(this.Detail.IDNO);
            this.Detail.PHONED = MyHelperUtil.ChangeIDNO(this.Detail.PHONED);
            this.Detail.PHONEN = MyHelperUtil.ChangeIDNO(this.Detail.PHONEN);
            this.Detail.CELLPHONE = MyHelperUtil.ChangeIDNO(this.Detail.CELLPHONE);
            this.Detail.ACCTNO = MyHelperUtil.ChangeIDNO(this.Detail.ACCTNO);
            this.Detail.INTAXNO = MyHelperUtil.ChangeIDNO(this.Detail.INTAXNO);
            this.Detail.ACTNO = MyHelperUtil.ChangeIDNO(this.Detail.ACTNO);
            this.Detail.ACTTEL = MyHelperUtil.ChangeIDNO(this.Detail.ACTTEL);
            #endregion

            #region 會員基本資料欄位檢核
            if (string.IsNullOrEmpty(this.Detail.NAME))
            {
                modelState.AddModelError("", "請輸入姓名");
            }

            if (!this.Detail.PASSPORTNO.HasValue)
            {
                modelState.AddModelError("", "請選擇身份別");
            }

            if (string.IsNullOrWhiteSpace(this.Detail.IDNO))
            {
                //blCheckIDNO = false;
                modelState.AddModelError("", "請輸入身分證號");
            }
            else
            {
                switch (this.Detail.PASSPORTNO)
                {
                    case 2: //外國籍
                        break;
                    case 1: //本國籍
                        //檢核身分證格式
                        if (!MyHelperUtil.IsIDNO(this.Detail.IDNO))
                        {
                            //blCheckIDNO = false;
                            //modelState.AddModelError("", "身分證號碼錯誤(如果有此身分證號碼，請聯絡系統管理者)!");
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(this.Detail.SEX) || !("M".Equals(this.Detail.SEX) || "F".Equals(this.Detail.SEX)))
            {
                //blCheckIDNO = false;
                modelState.AddModelError("", "請選擇性別");
            }

            //if (blCheckIDNO)
            //{
            //    if (!MyHelperUtil.CheckMemberSex(this.Detail.IDNO, this.Detail.SEX))
            //    {
            //        modelState.AddModelError("", "依身分證號判斷 性別選項 不正確，請確認");
            //    }
            //}

            if (!this.Detail.BIRTHDAY.HasValue)
            {
                modelState.AddModelError("", "請填入出生日期");
            }
            else
            {
                //報名時多加判斷參訓年齡資格
                ClassSignUpService serv = new ClassSignUpService();
                if (this.Detail.IsSignUp && !serv.CheckYearsOld15(this.Detail.BIRTHDAY.Value, this.Detail.STDATE))
                {
                    modelState.AddModelError("", "學員資格 年齡不滿15歲 不符合可參訓條件！");
                }
            }

            if (string.IsNullOrEmpty(this.Detail.DEGREEID))
            {
                modelState.AddModelError("", "請選擇學歷");
            }

            if (string.IsNullOrEmpty(this.Detail.GRADID))
            {
                modelState.AddModelError("", "請選擇畢業狀況");
            }

            if (string.IsNullOrWhiteSpace(this.Detail.SCHOOLNAME))
            {
                modelState.AddModelError("", "請輸入學校名稱");
            }

            if (string.IsNullOrWhiteSpace(this.Detail.DEPARTMENT))
            {
                modelState.AddModelError("", "請輸入科系名稱");
            }

            if (!this.Detail.MARITALSTATUS.HasValue)
            {
                modelState.AddModelError("", "請選擇婚姻狀況");
            }

            if (string.IsNullOrEmpty(this.Detail.HASMOBILE))
            {
                modelState.AddModelError("", "請選擇是否有行動電話");
            }
            else
            {
                if ("Y".Equals(this.Detail.HASMOBILE))
                {
                    if (string.IsNullOrWhiteSpace(this.Detail.CELLPHONE))
                    {
                        modelState.AddModelError("", "有行動電話 請輸入行動電話");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(this.Detail.PHONED) && string.IsNullOrWhiteSpace(this.Detail.PHONEN))
                    {
                        modelState.AddModelError("", "請輸入聯絡電話(日)或電話(夜)");
                    }

                    if (!string.IsNullOrWhiteSpace(this.Detail.CELLPHONE))
                    {
                        modelState.AddModelError("", "有輸入行動電話,請選擇有行動電話");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(Detail.PHONED))
            {
                if (!MyCommonUtil.IsValidPhoneNumber(Detail.PHONED)) modelState.AddModelError("", "聯絡電話(日)僅可輸入數字或插入「-」符號");
            }
            if (!string.IsNullOrWhiteSpace(Detail.PHONEN))
            {
                if (!MyCommonUtil.IsValidPhoneNumber(Detail.PHONEN)) modelState.AddModelError("", "聯絡電話(夜)僅可輸入數字或插入「-」符號");
            }
            if (!string.IsNullOrWhiteSpace(Detail.CELLPHONE))
            {
                if (!MyCommonUtil.IsValidMobileNumber10(Detail.CELLPHONE) || !MyCommonUtil.IsValidMobileNumber10B(Detail.CELLPHONE))
                {
                    modelState.AddModelError("", "行動電話僅可輸入數字10碼");
                }
            }

            string errmsg_ZIPCODE = MyCommonUtil.CHK_ZIPCODE(this.Detail.ZIPCODE, this.Detail.ZIPCODE_2W, "通訊地址(縣市) ", false);
            if (!string.IsNullOrEmpty(errmsg_ZIPCODE)) { modelState.AddModelError("ZIPCODE", errmsg_ZIPCODE); }
            else if (!string.IsNullOrEmpty(this.Detail.ZIPCODE))
            {
                string s_zipname = (new MyKeyMapDAO()).GetZipName(this.Detail.ZIPCODE);
                if (string.IsNullOrEmpty(s_zipname)) { modelState.AddModelError("ZIPCODE", string.Concat("通訊地址(縣市)", "郵遞區號前3碼", "鍵詞範圍有誤!")); }
            }

            if (string.IsNullOrWhiteSpace(this.Detail.ADDRESS)) { modelState.AddModelError("", "請輸入通訊地址 資料"); }


            if (string.IsNullOrEmpty(this.Detail.HASEMAIL))
            {
                modelState.AddModelError("", "請選擇是否有電子郵件");
            }
            else if ("Y".Equals(this.Detail.HASEMAIL))
            {
                if ("Y".Equals(this.Detail.HASEMAIL))
                {
                    if (string.IsNullOrWhiteSpace(this.Detail.EMAIL))
                    {
                        modelState.AddModelError("", "有電子郵件 請輸入電子郵件");
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(this.Detail.EMAIL) && !"無".Equals(this.Detail.EMAIL))
                    {
                        modelState.AddModelError("", "有輸入電子郵件,請選擇有電子郵件");
                    }
                }
            }

            if (string.IsNullOrEmpty(this.Detail.MIDENTITYID))
            {
                modelState.AddModelError("", "請選擇主要參訓身份別");
            }

            string errmsg_ZIPCODE2 = MyCommonUtil.CHK_ZIPCODE(this.Detail.ZIPCODE2, this.Detail.ZIPCODE2_2W, "戶籍地址(縣市)", true);
            if (!string.IsNullOrEmpty(errmsg_ZIPCODE2)) { modelState.AddModelError("ZIPCODE2", errmsg_ZIPCODE2); }
            else if (!string.IsNullOrEmpty(this.Detail.ZIPCODE2))
            {
                string s_zipname = (new MyKeyMapDAO()).GetZipName(this.Detail.ZIPCODE2);
                if (string.IsNullOrEmpty(s_zipname)) { modelState.AddModelError("ZIPCODE2", string.Concat("戶籍地址(縣市)", "郵遞區號前3碼", "鍵詞範圍有誤!")); }
            }

            if (string.IsNullOrEmpty(this.Detail.HOUSEHOLDADDRESS)) { modelState.AddModelError("", "請輸入戶籍地址 資料"); }

            if (string.IsNullOrWhiteSpace(this.Detail.UNAME)) { modelState.AddModelError("", "請輸入服務單位"); }

            if (!string.IsNullOrWhiteSpace(this.Detail.INTAXNO))
            {
                if (this.Detail.INTAXNO.Length >= 10)
                {
                    modelState.AddModelError("", "統一編號 長度超過系統範圍(10)");
                }
            }

            if (string.IsNullOrEmpty(this.Detail.SERVDEPTID))
            {
                modelState.AddModelError("", "請選擇服務部門");
            }

            if (string.IsNullOrWhiteSpace(this.Detail.ACTNAME))
            {
                modelState.AddModelError("", "請輸入投保單位名稱");
            }

            //投保類別 (1.勞 2.農 3.漁 4.軍)
            if (string.IsNullOrEmpty(this.Detail.ACTTYPE))
            {
                modelState.AddModelError("", "請輸入投保類別");
            }

            if (string.IsNullOrWhiteSpace(this.Detail.ACTNO))
            {
                //modelState.AddModelError("", "請輸入投保單位保險證號");
            }
            else
            {
                if (!(this.Detail.ACTNO.Substring(0, 1) == "0" || "1".Equals(this.Detail.ACTNO.Substring(0, 1))))
                {
                    modelState.AddModelError("", "投保單位保險證號，第1碼應為0或1");
                }
                else if (!MyHelperUtil.IsACTNO(this.Detail.ACTNO))
                {
                    modelState.AddModelError("", "投保單位保險證號輸入格式錯誤");
                }

                if (this.Detail.ACTNO.Length >= 20)
                {
                    modelState.AddModelError("", "投保單位保險證號 長度超過系統範圍(20)");
                }

                if (this.Detail.ACTNO.Length >= 2 && "09".Equals($"{this.Detail.ACTNO}".Substring(0, 2)))
                {
                    modelState.AddModelError("", "學員資格 投保單位保險證號 為09開頭者為(訓)字保 不符合可參訓條件！");
                }
            }

            if (!string.IsNullOrEmpty(this.Detail.ACTTEL))
            {
                int intLen = MyCommonUtil.LenB($"{this.Detail.ACTTEL}");
                if (intLen >= 30)
                {
                    modelState.AddModelError("", "投保單位電話長度超過系統範圍(30)");
                }
                else if (!string.IsNullOrWhiteSpace(this.Detail.ACTTEL))
                {
                    if (!MyCommonUtil.IsValidPhoneNumber(this.Detail.ACTTEL)) modelState.AddModelError("", "投保單位電話僅可輸入數字或插入「-」符號");
                }
            }

            string errmsg_ZIPCODE3 = MyCommonUtil.CHK_ZIPCODE(this.Detail.ZIPCODE3, this.Detail.ZIPCODE3_2W, "投保單位地址(縣市)", false);
            if (!string.IsNullOrEmpty(errmsg_ZIPCODE3)) { modelState.AddModelError("ZIPCODE3", errmsg_ZIPCODE3); }
            else if (!string.IsNullOrEmpty(this.Detail.ZIPCODE3))
            {
                string s_zipname = (new MyKeyMapDAO()).GetZipName(this.Detail.ZIPCODE3);
                if (string.IsNullOrEmpty(s_zipname)) { modelState.AddModelError("ZIPCODE3", string.Concat("投保單位地址(縣市)", "郵遞區號前3碼", "鍵詞範圍有誤!")); }
            }

            if (string.IsNullOrEmpty(this.Detail.JOBTITLEID))
            {
                modelState.AddModelError("", "請選擇職務");
            }
            #endregion

            #region 產投報名資料欄位檢核(stud_entertemp3)
            if ("Y".Equals(this.Detail.ISPLAN28))
            {

                if (!(string.IsNullOrEmpty(this.Detail.PRIORWORKPAY)))
                {
                    if (!MyCommonUtil.isUnsignedInt(this.Detail.PRIORWORKPAY))
                    {
                        modelState.AddModelError("", "受訓前薪資 請填寫整數數字");
                    }

                    //Boolean bolPay = true;
                    //try
                    //{
                    //    Int32 intOut = Int32.Parse(this.Detail.PRIORWORKPAY);
                    //}
                    //catch
                    //{
                    //    bolPay = false;

                    //    modelState.AddModelError("", "受訓前薪資 請填寫整數數字");
                    //}

                    //if (bolPay)
                    //{
                    //    if (this.Detail.PRIORWORKPAY.IndexOf('.') > -1)
                    //    {
                    //        modelState.AddModelError("", "受訓前薪資 請填寫整數數字(非天文數字)");
                    //    }
                    //}
                }

                if (!this.Detail.Q1.HasValue)
                {
                    modelState.AddModelError("", "請選擇參訓資料背景-是否由公司推薦參訓");
                }

                if (string.IsNullOrEmpty(Convert.ToString(this.Detail.Q2_1))
                    && string.IsNullOrEmpty(Convert.ToString(this.Detail.Q2_2))
                    && string.IsNullOrEmpty(Convert.ToString(this.Detail.Q2_3))
                    && string.IsNullOrEmpty(Convert.ToString(this.Detail.Q2_4)))
                {
                    modelState.AddModelError("", "請選擇參訓資料背景-參訓動機");
                }

                if (string.IsNullOrEmpty(Convert.ToString(this.Detail.Q3)))
                {
                    modelState.AddModelError("", "請選擇參訓資料背景-結訓後之計畫");
                }


                if (string.IsNullOrEmpty(Convert.ToString(this.Detail.Q4)))
                {
                    modelState.AddModelError("", "請選擇參訓資料背景-服務單位行業別");
                }

                if (string.IsNullOrEmpty(Convert.ToString(this.Detail.Q5)))
                {
                    modelState.AddModelError("", "請選擇參訓資料背景-服務單位是否屬於中小企業");
                }
                else
                {
                    if (!"1".Equals(Convert.ToString(this.Detail.Q5)) && !"0".Equals(Convert.ToString(this.Detail.Q5)))
                    {
                        modelState.AddModelError("", "服務單位是否屬於中小企業只能輸入 Y 或 N");
                    }
                }

                if (string.IsNullOrWhiteSpace(Convert.ToString(this.Detail.Q61)))
                {
                    modelState.AddModelError("", "請輸入個人工作年資");
                }
                else
                {
                    double chkVal = 0;

                    if (double.TryParse(this.Detail.Q61, out chkVal))
                    {
                        if ((Convert.ToDouble(this.Detail.Q61) % 0.5) != 0)
                        {
                            modelState.AddModelError("", "個人工作年資 開放小數點填寫，但必須以0.5為最小單位");
                        }
                    }
                    else
                    {
                        modelState.AddModelError("", "個人工作年資必須為數字");
                    }
                }

                if (string.IsNullOrWhiteSpace(Convert.ToString(this.Detail.Q62)))
                {
                    modelState.AddModelError("", "請輸入在這家公司的年資");
                }
                else
                {
                    double chkVal = 0;

                    if (double.TryParse(this.Detail.Q62, out chkVal))
                    {
                        if ((Convert.ToDouble(this.Detail.Q62) % 0.5) != 0)
                        {
                            modelState.AddModelError("", "在這家公司的年資 開放小數點填寫，但必須以0.5為最小單位");
                        }
                    }
                    else
                    {
                        modelState.AddModelError("", "在這家公司的年資必須為數字");
                    }
                }

                if (string.IsNullOrWhiteSpace(Convert.ToString(this.Detail.Q63)))
                {
                    modelState.AddModelError("", "請輸入在這職位的年資");
                }
                else
                {
                    double chkVal = 0;
                    if (double.TryParse(this.Detail.Q63, out chkVal))
                    {
                        if ((Convert.ToDouble(this.Detail.Q63) % 0.5) != 0)
                        {
                            modelState.AddModelError("", "在這職位的年資 開放小數點填寫，但必須以0.5為最小單位");
                        }
                    }
                    else
                    {
                        modelState.AddModelError("", "在這職位的年資必須為數字");
                    }
                }

                if (string.IsNullOrWhiteSpace(Convert.ToString(this.Detail.Q64)))
                {
                    modelState.AddModelError("", "請輸入最近升遷離本職幾年");
                }
                else
                {
                    double chkVal = 0;
                    if (double.TryParse(this.Detail.Q64, out chkVal))
                    {
                        if ((Convert.ToDouble(this.Detail.Q64) % 0.5) != 0)
                        {
                            modelState.AddModelError("", "最近升遷離本職幾年 開放小數點填寫，但必須以0.5為最小單位");
                        }
                    }
                    else
                    {
                        modelState.AddModelError("", "最近升遷離本職幾年必須為數字");
                    }
                }

                if (!"Y".Equals($"{this.Detail.ISCHECK}"))
                {
                    modelState.AddModelError("", "(下方)您未選擇或未確認 請選擇「確認」於開訓當日為具就業保險、勞工保險或農民保險被保險人身分之在職勞工");
                }

                if (string.IsNullOrEmpty(this.Detail.ISEMAIL))
                {
                    modelState.AddModelError("", "(下方)請選擇是否希望 定期收到產業人才投資方案最新課程資訊");
                }
                else if ("Y".Equals($"{this.Detail.ISEMAIL}") && !"Y".Equals($"{this.Detail.HASEMAIL}"))
                {
                    modelState.AddModelError("", "希望收到最新課程資訊; 電子郵件 請選擇「有」並填寫有效資料");
                }
            }
            #endregion

            if (!"Y".Equals($"{this.Detail.ISAGREE}"))
            {
                modelState.AddModelError("", "(最下方)若您不同意本署所屬分署暨相關訓練單位於合理範圍內蒐集、處理及利用您的個人資料，將無法受理報名！");
            }

            if (!"Y".Equals($"{this.Detail.ISCHECK2}"))
            {
                modelState.AddModelError("", "(最下方)您未選擇或未確認 請選擇「確認」填寫 個人最新及正確資料");
            }
        }
    }
    #endregion

    #region SignUpFormModel
    public class SignUpFormModel
    {
        /// <summary>
        /// 類別("1"28:產業人才投資方案、"2"06:自辦在職、"5"70:區域產業據點)
        /// </summary>
        [Display(Name = "類別：")]
        public string PlanType { get; set; }

        /// <summary>課程代碼</summary>
        public Int64? OCID { get; set; }

        /// <summary> 課程代碼加密 </summary>
        public string DB3D0C { get; set; }

        /// <summary> 提供上課位置距離("Y":是、"N":否) </summary>
        public string ProvideLocation { get; set; } = "N";

        /// <summary>姓名</summary>
        [Display(Name = "姓名")]
        public string NAME { get; set; }
        public string NAME_MK
        {
            get
            {
                if (string.IsNullOrEmpty(this.NAME)) { return ""; }
                if (this.NAME.Length < 3) { return string.Concat(NAME.Substring(0, 1), "○"); }
                var s_Star = MyCommonUtil.GenerateOneStars(NAME.Length - 2, "○");
                return string.Concat(NAME.Substring(0, 1), s_Star, NAME.Substring(NAME.Length - 1, 1));
            }
        }

        /// <summary>
        /// 出生日期
        /// </summary>
        [Display(Name = "出生日期")]
        public string BIRTHDAY
        {
            get
            {
                string result = "";
                if (!string.IsNullOrEmpty(this.BIRTH_YEAR) && !string.IsNullOrEmpty(this.BIRTH_MON) && !string.IsNullOrEmpty(this.BIRTH_DAY))
                {
                    int tmp = -1;
                    if (int.TryParse(this.BIRTH_YEAR, out tmp) && int.TryParse(this.BIRTH_MON, out tmp) && int.TryParse(this.BIRTHDAY, out tmp))
                    {
                        result = this.BIRTH_YEAR + "/" + this.BIRTH_MON + "/" + this.BIRTH_DAY;
                    }
                }

                return result;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string[] date = value.Split('/');
                    if (date.Length == 3)
                    {
                        int tmp = -1;
                        if (int.TryParse(date[0], out tmp) && int.TryParse(date[1], out tmp) && int.TryParse(date[2], out tmp))
                        {
                            this.BIRTH_YEAR = (Convert.ToInt32(date[0]) - 1911).ToString();
                            this.BIRTH_MON = date[1];
                            this.BIRTH_DAY = date[2];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 出生日期-民國年
        /// </summary>
        public string BIRTH_YEAR { get; set; }

        /// <summary>
        /// 出生日期-月
        /// </summary>
        public string BIRTH_MON { get; set; }
        /// <summary>
        /// 出生日期-日
        /// </summary>
        public string BIRTH_DAY { get; set; }
        public string DOUBLESTAR { get { return "**"; } }
        /// <summary>身分證號</summary>
        [Display(Name = "身分證號")]
        public string IDNO { get; set; }

        /// <summary>
        /// 身分證號隱碼(前2後2)
        /// </summary>
        public string IDNO_MK
        {
            get
            {
                if (string.IsNullOrEmpty(this.IDNO)) { return ""; }
                if (this.IDNO.Length < 10) { return "*******"; }
                return string.Concat(IDNO.Substring(0, 2), "******", IDNO.Substring(8, 2));
            }
        }
    }
    #endregion

    #region SignUpDetailModel
    public class SignUpDetailModel
    {
        public SignUpDetailModel()
        {
            this.IsSignUp = false;
            this.IsShowSelEditPlan28 = "Y";
            this.ISPLAN28 = "N";

        }

        /// <summary>
        /// 是否為報名作業的資料確認維護作業(true 是 , false 否)
        /// </summary>
        public bool IsSignUp { get; set; }

        /// <summary>
        /// 是否為產投用報名資料(Y/NULL 是, N 否)
        /// </summary>
        public string ISPLAN28 { get; set; }

        /// <summary>
        /// 是否顯示「是否維護產投報名資料」選項欄位 (Y 是, N 否)
        /// </summary>
        public string IsShowSelEditPlan28 { get; set; }

        /// <summary>
        /// 是否維護產投報名資料 (Y 是, N 否)
        /// </summary>
        public string IsEditPlan28 { get; set; }

        /// <summary>
        /// 資料異動模式 (CREATE 新增, UPDATE 修改)
        /// </summary>
        public string DB_ACTION { get; set; }

        /// <summary>
        /// e網報名資料序號(Stud_EnterType2 PK)
        /// </summary>
        public Int64? ESERNUM { get; set; }

        /// <summary>
        /// 流水號 (ref stud_entertemp.setid)
        /// </summary>
        public Int64? SETID { get; set; }

        /// <summary>
        /// e網報名暫存資料key值(ref STUD_ENTERTEMP2.ESETID)
        /// </summary>
        public Int64? ESETID { get; set; }

        /// <summary>
        /// e網報名資料序號（stud_entertype2.sernum）
        /// </summary>
        public Int64? SERNUM { get; set; }

        /// <summary>
        /// 線上報名資料序號 (ref stud_entertrain2.seid)
        /// </summary>
        public Int64? SEID { get; set; }

        /// <summary>
        /// 姓名(stud_entertemp3.name)
        /// </summary>
        public string NAME3 { get; set; }

        /// <summary> 課程代碼 </summary>
        public Int64? OCID { get; set; }

        /// <summary> 課程代碼加密 </summary>
        public string DB3D0C { get; set; }

        /// <summary>
        /// 課程名稱
        /// </summary>
        [Display(Name = "課程名稱")]
        public string CLASSCNAME { get; set; }

        /// <summary>
        /// 開訓日期
        /// </summary>
        public DateTime? STDATE { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "中文姓名")]
        //[Required]
        public string NAME { get; set; }

        /// <summary>
        /// 身份別 (1 本國籍, 2 外國籍)
        /// </summary>
        [Display(Name = "身分別")]
        //[Required]
        public Int64? PASSPORTNO { get; set; }

        /// <summary>
        /// 身份證字號
        /// </summary>
        [Display(Name = "身份證字號")]
        //[Required]
        public string IDNO { get; set; }

        /// <summary>
        /// epi：IDNO_encode
        /// </summary>
        public string Epi { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        [Display(Name = "性別")]
        //[Required]
        public string SEX { get; set; }

        /// <summary>修正性別再多判斷一次</summary>
        public string GET_SEX_MF
        {
            get
            {
                //2019-02-28 修正性別再多判斷一次
                if (this.SEX == "1") { return "M"; }
                if (this.SEX == "2") { return "F"; }
                return this.SEX;
            }
        }

        /// <summary>
        /// 出生日期
        /// </summary>
        [Display(Name = "出生日期")]
        //[Required]
        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 出生日期 (民國年 yyy/MM/dd)
        /// </summary>
        [Display(Name = "出生日期")]
        //[Required]
        public string BIRTHDAY_TW
        {
            get { return MyHelperUtil.DateTimeToTwString(this.BIRTHDAY); }
        }

        /// <summary>
        /// 最高學歷
        /// </summary>
        [Display(Name = "最高學歷")]
        //[Required]
        public string DEGREEID { get; set; }

        /// <summary>
        /// 畢業狀況
        /// </summary>
        [Display(Name = "畢業狀況")]
        //[Required]
        public string GRADID { get; set; }

        /// <summary>
        /// 學校名稱
        /// </summary>
        [Display(Name = "學校名稱")]
        //[Required]
        public string SCHOOLNAME { get; set; }

        /// <summary>
        /// 科系名稱
        /// </summary>
        [Display(Name = "科系名稱")]
        //[Required]
        public string DEPARTMENT { get; set; }

        /// <summary>
        /// 婚姻狀況(1.已婚;2.未婚 3.暫不提供(預設))
        /// </summary>
        [Display(Name = "婚姻狀況")]
        public Int64? MARITALSTATUS { get; set; }

        /// <summary>
        /// 聯絡電話(日)
        /// </summary>
        [Display(Name = "日間連絡電話")]
        public string PHONED { get; set; }

        /// <summary>
        /// 聯絡電話(夜)
        /// </summary>
        [Display(Name = "夜間連絡電話")]
        public string PHONEN { get; set; }

        /// <summary> 是否有行動電話("Y":有、"N":無) </summary>
        [Display(Name = "是否有行動電話")]
        //[Required]
        public string HASMOBILE { get; set; }

        /// <summary> 行動電話 </summary>
        [Display(Name = "行動電話")]
        public string CELLPHONE { get; set; }

        /// <summary> 通訊地址-郵遞區號 </summary>
        public string ZIPCODE { get; set; }

        //public string ZIPCODE_2W { get { return MyCommonUtil.GET_ZIPCODE2W(ZIPCODE_6W, null); } set { ZIPCODE_6W = MyCommonUtil.GET_ZIPCODE6W(ZIPCODE, value); } }
        /// <summary> 通訊地址-郵遞區號_後兩碼 </summary>  
        public string ZIPCODE_2W { get; set; }
        /// <summary> 通訊地址-郵遞區號6碼 </summary>
        public string ZIPCODE_6W { get; set; }

        /// <summary> 通訊地址 </summary>
        [Display(Name = "通訊地址")]
        //[Required]
        public string ADDRESS { get; set; }

        /// <summary> 戶籍地址-郵遞區號 </summary>
        public string ZIPCODE2 { get; set; }

        //public string ZIPCODE2_2W { get { return MyCommonUtil.GET_ZIPCODE2W(ZIPCODE2_6W, null); } set { ZIPCODE2_6W = MyCommonUtil.GET_ZIPCODE6W(ZIPCODE2, value); } }
        /// <summary> 戶籍地址-郵遞區號_後兩碼 </summary>  
        public string ZIPCODE2_2W { get; set; }
        /// <summary> 戶籍地址-郵遞區號_6碼 </summary>
        public string ZIPCODE2_6W { get; set; }

        /// <summary>
        /// 戶籍地址
        /// </summary>
        [Display(Name = "戶籍地址")]
        //[Required]
        public string HOUSEHOLDADDRESS { get; set; }
        /// <summary>
        /// 是否有電子郵件("Y":是、"N":否)
        /// </summary>
        public string HASEMAIL { get; set; }
        /// <summary>
        /// 電子郵件
        /// </summary>
        [Display(Name = "電子郵件")]
        //[Required]
        public string EMAIL { get; set; }
        /// <summary>
        ///
        /// </summary>
        public Int64? ESETID3 { get; set; }
        /// <summary>
        /// 主要參訓身份別
        /// </summary>
        [Display(Name = "主要參訓身份別")]
        //[Required]
        public string MIDENTITYID { get; set; }
        /// <summary>
        /// 受訓前薪資
        /// </summary>
        [Display(Name = "受訓前薪資")]
        public string PRIORWORKPAY { get; set; }
        /// <summary>
        /// 郵政/銀行帳號 (0 郵局帳號, 1 銀行帳號 )
        /// </summary>
        [Display(Name = "郵政/銀行帳號")]
        public Int64? ACCTMODE { get; set; }
        /// <summary>
        /// 局號
        /// </summary>
        [Display(Name = "局號")]
        public string POSTNO { get; set; }
        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        public string ACCTNO { get; set; }
        /// <summary>
        /// 郵局帳號
        /// </summary>
        [Display(Name = "帳號")]
        public string POST_ACCTNO { get; set; }
        /// <summary>
        /// 銀行帳號
        /// </summary>
        [Display(Name = "帳號")]
        public string BANK_ACCTNO { get; set; }
        /// <summary>
        /// 總行名稱
        /// </summary>
        [Display(Name = "總行名稱")]
        public string BANKNAME { get; set; }
        /// <summary>
        /// 總行代號
        /// </summary>
        [Display(Name = "總行代號")]
        public string ACCTHEADNO { get; set; }
        /// <summary>
        /// 分行名稱
        /// </summary>
        [Display(Name = "分行名稱")]
        public string EXBANKNAME { get; set; }
        /// <summary>
        /// 分行代號
        /// </summary>
        [Display(Name = "分行代號")]
        public string ACCTEXNO { get; set; }
        /// <summary>
        /// 服務單位
        /// </summary>
        [Display(Name = "服務單位")]
        //[Required]
        public string UNAME { get; set; }
        /// <summary>
        /// 服務單位統一編號
        /// </summary>
        [Display(Name = "統一編號")]
        //[Required]
        public string INTAXNO { get; set; }
        /// <summary>
        /// 服務部門
        /// </summary>
        [Display(Name = "服務部門")]
        //[Required]
        public string SERVDEPT { get; set; }
        /// <summary>
        /// 服務部門
        /// </summary>
        [Display(Name = "服務部門")]
        //[Required]
        public string SERVDEPTID { get; set; }
        /// <summary>
        /// 投保單位名稱
        /// </summary>
        [Display(Name = "投保單位名稱")]
        //[Required]
        public string ACTNAME { get; set; }
        /// <summary>
        /// 投保類別 (1.勞 2.農 3.漁 4.軍)
        /// </summary>
        [Display(Name = "投保類別")]
        //[Required]
        public string ACTTYPE { get; set; }
        /// <summary>
        /// 投保單位保險證號
        /// </summary>
        [Display(Name = "投保單位保險證號")]
        public string ACTNO { get; set; }
        /// <summary>
        /// 投保單位電話
        /// </summary>
        [Display(Name = "投保單位電話")]
        public string ACTTEL { get; set; }
        /// <summary> 投保單位郵遞區號前三碼 </summary>
        public string ZIPCODE3 { get; set; }
        //public string ZIPCODE3_2W { get { return MyCommonUtil.GET_ZIPCODE2W(ZIPCODE3_6W, null); } set { ZIPCODE3_6W = MyCommonUtil.GET_ZIPCODE6W(ZIPCODE3, value); } }
        /// <summary> 投保單位郵遞區號後兩碼 </summary>
        public string ZIPCODE3_2W { get; set; }
        /// <summary> 投保單位郵遞區號6碼 </summary>
        public string ZIPCODE3_6W { get; set; }
        /// <summary>
        /// 投保單位地址
        /// </summary>
        [Display(Name = "投保單位地址")]
        public string ACTADDRESS { get; set; }
        /// <summary>
        /// 職務
        /// </summary>
        [Display(Name = "職務")]
        //[Required]
        public string JOBTITLE { get; set; }
        /// <summary>
        /// 職務代碼
        /// </summary>
        [Display(Name = "職務")]
        //[Required]
        public string JOBTITLEID { get; set; }
        /// <summary>
        /// 推薦參訓
        /// </summary>
        [Display(Name = "是否由公司推薦參訓")]
        public Int64? Q1 { get; set; }
        /// <summary>
        /// 參訓動機_2_1
        /// </summary>
        public string Q2_1
        {
            get { return (this.Q2_1_CHECKED ? "1" : ""); }
            set
            {
                this.Q2_1_CHECKED = "1".Equals(value) ? true : false;
            }
        }
        /// <summary>
        /// 參訓動機_2_2
        /// </summary>
        public string Q2_2
        {
            get { return (this.Q2_2_CHECKED ? "1" : ""); }
            set
            {
                this.Q2_2_CHECKED = "1".Equals(value) ? true : false;
            }
        }
        /// <summary>
        /// 參訓動機_2_3
        /// </summary>
        public string Q2_3
        {
            get { return (this.Q2_3_CHECKED ? "1" : ""); }
            set
            {
                this.Q2_3_CHECKED = "1".Equals(value) ? true : false;
            }
        }
        /// <summary>
        /// 參訓動機_2_4
        /// </summary>
        public string Q2_4
        {
            get { return (this.Q2_4_CHECKED ? "1" : ""); }
            set
            {
                this.Q2_4_CHECKED = "1".Equals(value) ? true : false;
            }
        }
        /// <summary>
        ///
        /// </summary>
        [NotDBField]
        public bool Q2_1_CHECKED { get; set; }
        /// <summary>
        ///
        /// </summary>
        [NotDBField]
        public bool Q2_2_CHECKED { get; set; }
        /// <summary>
        ///
        /// </summary>
        [NotDBField]
        public bool Q2_3_CHECKED { get; set; }
        /// <summary>
        ///
        /// </summary>
        [NotDBField]
        public bool Q2_4_CHECKED { get; set; }
        /// <summary>
        /// 結訓後之計畫
        /// </summary>
        [Display(Name = "結訓後之計畫")]
        //[Required]
        public Int64? Q3 { get; set; }
        /// <summary>
        /// 結訓後計畫其它說明
        /// </summary>
        public string Q3_OTHER { get; set; }
        /// <summary>
        /// 服務單位行業別
        /// </summary>
        [Display(Name = "服務單位行業別")]
        //[Required]
        public string Q4 { get; set; }
        /// <summary>
        /// 服務單位是否屬於中小企業
        /// </summary>
        [Display(Name = "服務單位是否屬於中小企業")]
        //[Required]
        public Int64? Q5 { get; set; }
        /// <summary>
        /// 個人工作年資
        /// </summary>
        [Display(Name = "個人工作年資")]
        //[Required]
        public string Q61 { get; set; }
        /// <summary>
        /// 在這家公司的年資
        /// </summary>
        [Display(Name = "在這家公司的年資")]
        //[Required]
        public string Q62 { get; set; }
        /// <summary>
        /// 在這職位的年資
        /// </summary>
        [Display(Name = "在這職位的年資")]
        //[Required]
        public string Q63 { get; set; }
        /// <summary>
        /// 最近升遷離本職幾年
        /// </summary>
        [Display(Name = "最近升遷離本職幾年")]
        //[Required]
        public string Q64 { get; set; }
        /// <summary>
        /// 是否願意收到職訓通知
        /// </summary>
        //[Required]
        public string ISEMAIL { get; set; }
        /// <summary>
        /// 就業保險相關資訊是否確認(Y / N)
        /// </summary>
        //[Required]
        public string ISCHECK { get; set; }
        /// <summary>
        /// 同意否
        /// </summary>
        //[Required]
        public string ISAGREE { get; set; }
        /// <summary>
        /// 確認是否為最新及正確資料
        /// </summary>
        //[Required]
        public string ISCHECK2 { get; set; }
        /// <summary>
        /// 兵役狀態
        /// </summary>
        public string MILITARYID { get; set; }
        /// <summary>
        /// 異動者帳號
        /// </summary>
        public string MODIFYACCT { get; set; }
        /// <summary>
        /// 異動日期(西元年 yyyy/MM/dd)
        /// </summary>
        public string MODIFYDATE { get; set; }
        /// <summary>
        /// 異動日期2(西元年 yyyy/MM/dd)
        /// </summary>
        public string MODIFYDATE2 { get; set; }

        /// <summary>上傳銀行存摺驗證</summary>
        public bool ISUSE_IMG1 { get; set; }
        /// <summary>上傳身分證件驗證</summary>
        public bool ISUSE_IMG2 { get; set; }
        /// <summary>上傳銀行存摺驗證-資料有誤</summary>
        public bool ERR_IMG1 { get; set; }
        /// <summary>上傳身分證件驗證-資料有誤</summary>
        public bool ERR_IMG2 { get; set; }

    }
    #endregion
}

