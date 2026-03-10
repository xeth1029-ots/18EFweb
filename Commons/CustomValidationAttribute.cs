using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using WDAIIP.WEB.Commons;
using Turbo.DataLayer;

namespace WDAIIP.WEB.Commons
{
    /// <summary>
    ///
    /// </summary>
    public class UnitNOAttribute : ValidationAttribute
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            //傳入為空時不在此檢核 (由必填觸發)            
            if (MyHelperUtil.IsEmpty(value))
            {
                return ValidationResult.Success;
            }


            //檢核            
            if (MyHelperUtil.IsUniformNo(value))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(validationContext.DisplayName + "輸入格式錯誤！");
        }
    }

    /// <summary>
    /// 身份證字號檢核
    /// </summary>
    public class IDNOAttribute : ValidationAttribute
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            //傳入為空時不在此檢核 (由必填觸發)            
            if (MyHelperUtil.IsEmpty(value))
            {
                return ValidationResult.Success;
            }


            //檢核            
            if (MyHelperUtil.IsIDNO(value))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(validationContext.DisplayName + "輸入格式錯誤！");
        }
    }

    /// <summary>
    /// 自訂數字格式檢核
    /// </summary>
    public class NumberValidtionAttribute : ValidationAttribute
    {
        private int DataPrecision = 0;
        private int DataScale = 0;

        public NumberValidtionAttribute(int dataPrecision, int dataScale)
        {
            DataPrecision = (int)dataPrecision;
            DataScale = (int)dataScale;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var numStr = MyHelperUtil.SafeTrim(value);

            //傳入為空時不在此檢核 (由必填觸發)

            if (MyHelperUtil.IsEmpty(numStr))
            {
                return ValidationResult.Success;
            }


            //整數            
            if (DataScale == 0)
            {
                //非數字檢核
                var a = 0;
                if (!int.TryParse(numStr, out a))
                {
                    return new ValidationResult(validationContext.DisplayName + "輸入格式錯誤，請輸入整數！");
                }

                if (numStr.Length > DataPrecision)
                {
                    return new ValidationResult(validationContext.DisplayName + "輸入錯誤，數字長度需小於等於[" + DataPrecision + "]位！");
                }
            }

            //小數
            if (DataScale > 0)
            {
                var b = new Decimal(0);
                //非數字檢核
                if (!Decimal.TryParse(numStr, out b))
                {
                }
                var numAry = numStr.Split('.');

                if (numAry[0].Length > (DataPrecision - DataScale))
                {
                    return new ValidationResult(validationContext.DisplayName + "輸入格式錯誤，整數部分長度需小於[" + (DataPrecision - DataScale) + "]位！");
                }

                if (numAry.Length > 1 && numAry[1].Length > DataScale)
                {
                    return new ValidationResult(validationContext.DisplayName + "輸入格式錯誤，小數部分長度不可超過[" + (DataScale) + "]位！");
                }
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// 用來檢核 字串欄位 值是否包含在指定的列表中
    /// </summary>
    public class CodeListAttribute : ValidationAttribute
    {
        // Fields
        private string[] _codeList;

        /// <summary>
        /// 用來檢核 字串欄位 值是否包含在指定的列表中
        /// </summary>
        /// <param name="codeList">字串代碼清單(變動個數)</param>
        public CodeListAttribute(params string[] codeList)
        {
            this._codeList = codeList;
            if (_codeList == null)
            {
                _codeList = new string[0];
            }

            base.ErrorMessage = "{0} 欄位值必須是下列之一: [{1}]";
        }

        // Methods
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool valid = false;
            List<string> codeNames = new List<string>();

            for (int i = 0; i < _codeList.Length; i++)
            {
                string codeStr = _codeList[i];
                if (codeStr != null)
                {
                    string[] part = codeStr.Split(':');
                    string code = part[0];
                    if (code.Equals(value))
                    {
                        valid = true;
                    }
                    codeNames.Add((part.Length > 1) ? part[1] : part[0]);
                }
                else
                {
                    if (string.IsNullOrEmpty((string)value))
                    {
                        valid = true;
                    }
                }
            }

            if (valid)
            {
                return ValidationResult.Success;
            }
            else
            {
                string errorMsg = String.Format(base.ErrorMessage, validationContext.DisplayName,
                    string.Join(", ", codeNames.ToArray()));
                return new ValidationResult(errorMsg);
            }
        }

    }

    /// <summary>
    /// 用來檢核 CodeMap 欄位值是否符合 KeyMapDAO.GetCodeMapList
    /// </summary>
    public class CodeMapAttribute : ValidationAttribute
    {
        // Fields
        private StaticCodeMap.CodeMap _codeMap;

        /// <summary>
        /// 用來檢核 CodeMap 欄位值是否符合 KeyMapDAO.GetCodeMapList
        /// </summary>
        /// <param name="codeMap">StaticCodeMap.CodeMap</param>
        public CodeMapAttribute(StaticCodeMap.CodeMap codeMap)
        {
            this._codeMap = codeMap;
            base.ErrorMessage = "{0} 欄位值必須是下列之一: [{1}]";
        }

        // Methods
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool valid = false;

            IList<KeyMapModel> list = (new KeyMapDAO()).GetCodeMapList(this._codeMap);
            List<string> codeNames = new List<string>();
            foreach (KeyMapModel item in list)
            {
                string code = item.CODE;
                string codeName = code + "." + item.TEXT;
                codeNames.Add(codeName);
                if (code.Equals(value))
                {
                    valid = true;
                }
            }

            if (valid)
            {
                return ValidationResult.Success;
            }
            else
            {
                var errorMsg = string.Format(base.ErrorMessage, validationContext.DisplayName,
                    string.Join(", ", codeNames));
                return new ValidationResult(errorMsg);
            }
        }
    }
}