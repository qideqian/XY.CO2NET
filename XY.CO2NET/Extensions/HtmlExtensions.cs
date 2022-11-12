using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XY.CO2NET.Extensions
{
    /// <summary>
    /// Html扩展类
    /// </summary>
    public static class HtmlExtensions
    {
        /// <summary>
        /// Html转化为文本模式
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlToPlainText(this String html)
        {
            if (string.IsNullOrEmpty(html))
                return html;
            //matches one or more (white space or line breaks) between '>' and '<'
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";
            //match any character between '<' and '>', even when end tag is missing
            const string stripFormatting = @"<[^>]*(>|$)";
            const string lineBreak = @"<(br|BR|p|P)\s{0,1}\/{0,1}>";
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);
            string text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);
            //Remove the nbsp and amp
            text = text.Replace("&nbsp;", "").Replace("&amp;", "");
            return text;
        }
    }
}
