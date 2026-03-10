using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Turbo.Helpers
{
    /// <summary>
    /// HTML CheckBox 輸入框產生輔助方法類別
    /// </summary>
    public static class CheckBoxExtension
    {
        /// <summary>HTML 勾選輸入框產生方法。（本方法用來解決 ASP.NET MVC CheckBoxFor 在設定 disabled 屬性後總是提交 false 值給後端問題）</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        /// <param name="isChecked">是否勾選。（null: 依據 Model 欄位值自動設定，true: 勾選，false: 不勾選）。預設 null。</param>
        public static MvcHtmlString CheckBoxForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, object htmlAttributes = null, bool enabled = true, bool? isChecked = null)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();
            
            //HTML 標籤的 id 與 name 屬性值
            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            var checkboxTag = new TagBuilder("input");
            checkboxTag.GenerateId(name);
            checkboxTag.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.CheckBox));
            checkboxTag.MergeAttribute("name", name);
            checkboxTag.MergeAttribute("value", value);

            //當勾選框選取狀態變化時，同時連動隱藏欄位值。
            StringBuilder sb = new StringBuilder();
            sb.Append("var _el=$('input#");
            sb.Append(checkboxTag.Attributes["id"]);
            sb.Append("');var _sta=$(this).prop('checked');_el.val((_sta==true));");
            checkboxTag.MergeAttribute("onchange", sb.ToString());

            //判斷真實的「勾選狀態」
            bool tagIsChecked = false;
            switch (isChecked.HasValue)
            {
                case false: tagIsChecked = (value == "true"); break;
                case true: tagIsChecked = (isChecked == true); break;
            }
            if (tagIsChecked) checkboxTag.Attributes.Add("checked", "checked");

            //若有設定「禁用狀態」時
            if (enabled == false) checkboxTag.Attributes.Add("disabled", "disabled");
            
            //若有指定額外的 HTML 屬性時
            if (htmlAttributes != null)
            {
                RouteValueDictionary attr = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                checkboxTag.MergeAttributes(attr);
            }

            //當勾選框被禁用或是若勾選框沒有勾選時，一律強迫瀏覽器傳回隱藏欄位值。
            var hiddenTag = new TagBuilder("input");
            hiddenTag.GenerateId(name);
            hiddenTag.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.Hidden));
            hiddenTag.MergeAttribute("name", name);
            hiddenTag.MergeAttribute("value", value);

            //產生最終的 CheckBox HTML
            var divId = string.Concat("checkbox_", propertyId);
            sb.Clear();
            sb.Append("<div class=\"checkbox\" id=\"");
            sb.Append(divId);
            sb.Append("\"><label for=\"");
            sb.Append(propertyId);
            sb.Append("\">");
            sb.Append(checkboxTag.ToString(TagRenderMode.SelfClosing));
            sb.Append("<i class=\"fa fa-");
            if (tagIsChecked) sb.Append("check-");
            sb.Append("square-o");
            sb.Append("\" aria-hidden=\"true\"></i>");
            sb.Append("</label>");
            sb.Append(hiddenTag.ToString(TagRenderMode.SelfClosing));

            //20171229 判斷是否使用 Html.Partial() 方式輸出 HTML
            if (htmlHelper.IsPartialOutput()) {
                sb.Append("<script>if (bindEventCheckBoxListChange) bindEventCheckBoxListChange(\"");
                sb.Append(divId);
                sb.Append("\");</script>");
            }

            sb.Append("</div>");
            return MvcHtmlString.Create(sb.ToString());

            //var name = ExpressionHelper.GetExpressionText(expression);
            //var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            //var value = Convert.ToString(metadata.Model).ToLower();

            //var checkboxTag = new TagBuilder("input");
            //checkboxTag.GenerateId(name);
            //checkboxTag.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.CheckBox));
            //checkboxTag.MergeAttribute("name", name);
            //checkboxTag.MergeAttribute("value", value);

            ////當勾選框選取狀態變化時，同時連動隱藏欄位值。
            //var onChangeEvent = string.Concat("var _el=$('input#", checkboxTag.Attributes["id"], "');_el.val($(this).prop('checked'));");
            //checkboxTag.MergeAttribute("onchange", onChangeEvent);

            //switch (isChecked.HasValue)
            //{
            //    case false:
            //        if (value == "true") checkboxTag.Attributes.Add("checked", "checked");
            //        break;
            //    case true:
            //        if (isChecked == true) checkboxTag.Attributes.Add("checked", "checked");
            //        break;
            //}

            //if (enabled == false) checkboxTag.Attributes.Add("disabled", "disabled");
            //if (htmlAttributes != null)
            //{
            //    RouteValueDictionary attr = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            //    checkboxTag.MergeAttributes(attr);
            //}

            ////當勾選框被禁用或是若勾選框沒有勾選時，一律強迫瀏覽器傳回隱藏欄位值。
            //var hiddenTag = new TagBuilder("input");
            //hiddenTag.GenerateId(name);
            //hiddenTag.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.Hidden));
            //hiddenTag.MergeAttribute("name", name);
            //hiddenTag.MergeAttribute("value", value);

            ////checkbox + hidden 輸出
            //string result = checkboxTag.ToString(TagRenderMode.SelfClosing) + hiddenTag.ToString(TagRenderMode.SelfClosing);
            //return MvcHtmlString.Create(result);
        }

        /// <summary>HTML 勾選輸入框產生方法。（本方法用來解決 ASP.NET MVC CheckBoxFor 在設定 disabled 屬性後總是提交 false 值給後端問題）</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="checkedValue">（非必填）代表「勾選狀態」的值字串（請區分大小寫英文）。此參數請依照實際狀況來設定適當值（例："1"、"Y" 代表要勾選）。</param>
        /// <param name="uncheckedValue">（非必填）代表「不勾選狀態」的值字串（請區分大小寫英文）。此參數請依照實際狀況來設定適當值（例：""、"N" 代表不勾選）。</param>
        /// <param name="htmlAttributes">（非必填）要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">（非必填）是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        /// <param name="isChecked">（非必填）是否勾選。（null: 依據 Model 欄位值自動設定，true: 勾選，false: 不勾選）。預設 null。</param>
        public static MvcHtmlString CheckBoxForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, string>> expression, string checkedValue, string uncheckedValue, object htmlAttributes = null, bool enabled = true, bool? isChecked = null)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model);
            
            //HTML 標籤的 id 與 name 屬性值
            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            var checkboxTag = new TagBuilder("input");
            checkboxTag.GenerateId(name);
            checkboxTag.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.CheckBox));
            checkboxTag.MergeAttribute("name", name);
            checkboxTag.MergeAttribute("value", value);

            //當勾選框選取狀態變化時，同時連動隱藏欄位值。
            StringBuilder sb = new StringBuilder();
            sb.Append("var _el=$('input#");
            sb.Append(checkboxTag.Attributes["id"]);
            sb.Append("');var _sta=$(this).prop('checked');_el.val((_sta==true) ? '");
            sb.Append(checkedValue);
            sb.Append("' : '");
            sb.Append(uncheckedValue);
            sb.Append("');");
            checkboxTag.MergeAttribute("onchange", sb.ToString());

            //判斷真實的「勾選狀態」
            bool tagIsChecked = false;
            switch (isChecked.HasValue)
            {
                case false: tagIsChecked = (value == checkedValue); break;
                case true: tagIsChecked = (isChecked == true); break;
            }
            if (tagIsChecked) checkboxTag.Attributes.Add("checked", "checked");

            //若有設定「禁用狀態」時
            if (enabled == false) checkboxTag.Attributes.Add("disabled", "disabled");
            
            //若有指定額外的 HTML 屬性時
            if (htmlAttributes != null)
            {
                RouteValueDictionary attr = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                checkboxTag.MergeAttributes(attr);
            }

            //當勾選框被禁用或是若勾選框沒有勾選時，一律強迫瀏覽器傳回隱藏欄位值。
            var hiddenTag = new TagBuilder("input");
            hiddenTag.GenerateId(name);
            hiddenTag.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.Hidden));
            hiddenTag.MergeAttribute("name", name);
            hiddenTag.MergeAttribute("value", value);

            //產生最終的 CheckBox HTML
            var divId = string.Concat("checkbox_", propertyId);
            sb.Clear();
            sb.Append("<div class=\"checkbox\" id=\"");
            sb.Append(divId);
            sb.Append("\"><label for=\"");
            sb.Append(propertyId);
            sb.Append("\">");
            sb.Append(checkboxTag.ToString(TagRenderMode.SelfClosing));
            sb.Append("<i class=\"fa fa-");
            if (tagIsChecked) sb.Append("check-");
            sb.Append("square-o");
            sb.Append("\" aria-hidden=\"true\"></i>");
            sb.Append("</label>");
            sb.Append(hiddenTag.ToString(TagRenderMode.SelfClosing));

            //20171229 判斷是否使用 Html.Partial() 方式輸出 HTML
            if (htmlHelper.IsPartialOutput()) {
                sb.Append("<script>if (bindEventCheckBoxListChange) bindEventCheckBoxListChange(\"");
                sb.Append(divId);
                sb.Append("\");</script>");
            }

            sb.Append("</div>");
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}