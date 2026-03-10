using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace WDAIIP.WEB.Commons
{
    public class SecurityHelper
    {
        // Func<string, string> 代表一個接受 string 參數並返回 string 的函式
        public static Func<string, string> SanitizeHtml = (input) =>
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // 1. 移除 script 標籤
            input = Regex.Replace(input, @"<script.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 2. 移除所有 HTML 標籤
            // 這種方式比較激進，會移除所有 HTML 標籤。
            // 如果你需要保留部分 HTML，請使用更精細的過濾方法。
            // input = Regex.Replace(input, @"<[^>]+>", "");

            // 3. 轉義常見的 XSS 符號 // 將 <, >, &, ", ' 等特殊字元轉成 HTML 實體
            input = input.Replace("<", "&lt;");
            input = input.Replace(">", "&gt;");
            input = input.Replace("\"", "&quot;");
            input = input.Replace("'", "&#x27;");
            input = input.Replace("&", "&amp;");

            // 4. 移除常見的事件屬性 (如 onclick, onload) // 這裡使用更複雜的正規表示式來尋找並移除
            input = Regex.Replace(input, "on\\w+=\".*?\"", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "on\\w+='.*?'", "", RegexOptions.IgnoreCase);

            // 5. 移除常見的 JavaScript 協議 (如 javascript:)
            input = Regex.Replace(input, "javascript:", "", RegexOptions.IgnoreCase);

            return input;
        };

        /// <summary>
        /// 將字串中的特殊字元轉換為HTML實體，以防止XSS。
        /// </summary>
        /// <param name="input">要處理的原始字串。</param>
        /// <returns>已轉義的字串。</returns>
        public static string HtmlEncode(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            // 使用 ASP.NET 內建的 HttpUtility 進行編碼
            return HttpUtility.HtmlEncode(input);
        }

        /// <summary>
        /// 移除字串中的所有HTML標籤，只保留純文字。
        /// </summary>
        /// <param name="input">要處理的原始字串。</param>
        /// <returns>已移除HTML標籤的純文字字串。</returns>
        public static string StripHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            // 使用正規表示式移除所有 <...> 標籤
            return Regex.Replace(input, @"<[^>]*>", string.Empty);
        }

        /// <summary>
        /// 綜合性的XSS淨化方法，移除惡意腳本、事件屬性及協議。
        /// </summary>
        /// <param name="input">要處理的原始字串。</param>
        /// <returns>經過淨化處理的字串。</returns>
        public static string SanitizeForXss(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // 將所有 HTML 標籤轉為小寫以便正規表示式匹配
            string sanitized = input.ToLower();

            // 1. 移除 script 標籤 (不分大小寫)
            sanitized = Regex.Replace(sanitized, "<script.*?>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // 2. 移除所有事件屬性 (如 onclick, onmouseover 等)
            sanitized = Regex.Replace(sanitized, " on\\w+=\".*?\"", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, " on\\w+='.*?'", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, " on\\w+=\\w+", "", RegexOptions.IgnoreCase);

            // 3. 移除 JavaScript 協議 (如 javascript: 或 vbscript:)
            sanitized = Regex.Replace(sanitized, "javascript:", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, "vbscript:", "", RegexOptions.IgnoreCase);

            return sanitized;
        }

        /// <summary>執行完整且安全的淨化流程，建議優先使用此方法。</summary>
        /// <param name="input">要處理的原始字串。</param>
        /// <returns>最終安全的字串。</returns>
        public static string FullSanitize(string input)
        {
            // 1. 先移除惡意標籤和屬性
            string sanitized = SanitizeForXss(input);
            // 2. 再進行 HTML 編碼
            return HtmlEncode(sanitized);
        }

    }
}