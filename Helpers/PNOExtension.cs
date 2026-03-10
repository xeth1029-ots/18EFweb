using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Turbo.Helpers;
using WDAIIP.WEB.Services;

namespace Turbo.Helpers
{
    /// <summary>
    /// HTML PNO 職類輸入框產生輔助方法類別
    /// </summary>
    public static class PNOExtension
    {
        /// <summary>HTML 職類單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expressionPNO">PNO(職類代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionPNO_TEXT">PNO_TEXT(職類名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString PNOForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, string>> expressionPNO,
            Expression<Func<TModel, string>> expressionPNO_TEXT,
            object htmlAttributes = null, bool enabled = true)
        { 
            //PNO HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expressionPNO);
            var pnoname = name.ToSplit('.')[1];
            var metadata = ModelMetadata.FromLambdaExpression(expressionPNO, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();            
           
            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            //PNO_TEXT HTML 標籤的 id 與 name 屬性值
            var name_text = ExpressionHelper.GetExpressionText(expressionPNO_TEXT);
            var metadata_text = ModelMetadata.FromLambdaExpression(expressionPNO_TEXT, htmlHelper.ViewData);
            var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_text = Convert.ToString(metadata.Model).ToLower();

            var propertyName_text = templateInfo.GetFullHtmlFieldName(name_text);
            var propertyId_text = templateInfo.GetFullHtmlFieldId(propertyName_text);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append(htmlHelper.TextBoxFor(expressionPNO, new { @class = "form-control", size = 5, maxlength = 5, placeholder = "代碼" }).ToHtmlString());
            sb.Append("<div class='input-group'>");
            sb.Append(htmlHelper.TextBoxFor(expressionPNO_TEXT, new { size = "40", @class = "form-control formbar-bg", placeholder = "職類名稱", @readonly = "readonly" }).ToHtmlString());
            sb.Append("<span class='input-group-btn'>");
            sb.Append("<button type='button' class='btn btn-info' onclick='do"+ pnoname + "Select()'>");
            sb.Append("<i class='fa fa-ellipsis-h' aria-hidden='true'></i>");
            sb.Append("</button>");
            sb.Append("</span>");
            sb.Append("</div>");


            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#"+ propertyId + "').on('blur', do"+ pnoname + "NameGet);");
            sb_script.Append("do"+ pnoname + "NameGet();");
            sb_script.Append("});");
            //先取得資料代碼對應的顯示名稱，再將顯示名稱設定到唯讀文字框。
            //data 參數物件必須要有 url、msg、arg、box 四個屬性。
            sb_script.Append("function loadDialogCodeMapName(data) {");
            sb_script.Append("ajaxLoadMore(data.url, data.arg, function(resp) {");
            sb_script.Append("if (resp === undefined) data.box.val('');");
            sb_script.Append("else {");
            sb_script.Append("if (resp.data != '') data.box.val(resp.data);");
            sb_script.Append("else {");
            sb_script.Append("data.box.val('');");
            sb_script.Append("blockAlert(data.msg);");
            sb_script.Append("}");
            sb_script.Append("}");
            sb_script.Append("});");
            sb_script.Append("};");
            //顯示職類代碼(PNO)資料選取對話框
            sb_script.Append("function do"+ pnoname + "Select() {");
            sb_script.Append("var url = '"+ url.Action("Index", "PROF", new { area = "Share" }) +"?onclickHandler=on"+ pnoname + "Select';");
            sb_script.Append("var title = '職類資料查詢';");
            sb_script.Append("var arrBtn = '';");
            sb_script.Append("popupDialog(url, title, arrBtn, 800);");
            sb_script.Append("};");
            //處理職類代碼(PNO)資料選取對話框傳回的資料。
            sb_script.Append("function on"+ pnoname + "Select(id, name) {");
            sb_script.Append("$('#"+ propertyId + "').val(id);");
            sb_script.Append("$('#" + propertyId_text + "').val(name);");
            sb_script.Append("}");
            //取得職類代碼對應的職類名稱。
            sb_script.Append("function do"+ pnoname + "NameGet() {");
            sb_script.Append("var code = $('#" + propertyId + "');");
            sb_script.Append("var data = { 'url': '"+url.Action("GetPnoName", "Ajax", new { area = "" }) + "',");
            sb_script.Append("'msg': '查無該職類資訊!!',");
            sb_script.Append("'arg': { 'PNO': code.val().toUpperCase() },");
            sb_script.Append("'box': $('#" + propertyId_text + "') };");
            sb_script.Append(" if ($.trim(data.arg.PNO) == '') data.box.val('');");
            sb_script.Append(" else {");
            sb_script.Append("code.val(data.arg.PNO);");
            sb_script.Append("loadDialogCodeMapName(data);");
            sb_script.Append("}");
            sb_script.Append("};");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}