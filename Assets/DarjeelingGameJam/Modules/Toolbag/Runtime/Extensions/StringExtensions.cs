using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Modules.Toolbag.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhitespace(this string str)
        {
            return string.IsNullOrEmpty(str) || str.All(char.IsWhiteSpace);
        }
        
        public static string Humanize(this string source)
        {
            var returnValue = source;
            returnValue = Regex.Replace(returnValue, "^_", "").Trim(); 
            returnValue = Regex.Replace(returnValue, "([a-z])([A-Z])", "$1 $2").Trim(); 
            returnValue = Regex.Replace(returnValue, "([A-Z])([A-Z][a-z])", "$1 $2").Trim(); 
            return returnValue;
        }
        
        public static string Colorize(this string source, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{source}</color>";
        }
    }
}