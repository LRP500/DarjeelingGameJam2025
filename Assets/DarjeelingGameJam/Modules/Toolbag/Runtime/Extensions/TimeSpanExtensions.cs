using System;

namespace Modules.Toolbag.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToVerbose(this TimeSpan span)
        {
            var duration = span.Duration();
            var days = duration.Days > 0 ? $"{span.Days:0}d " : string.Empty;
            var hours = duration.Hours > 0 ? $"{span.Hours:00}h " : string.Empty;
            var minutes = duration.Minutes > 0 ? $"{span.Minutes:00}m " : string.Empty;
            var seconds = duration.Seconds > 0 ? $"{span.Seconds:00}s" : string.Empty;
            var formatted = $"{days}{hours}{minutes}{seconds}";

            if (formatted.EndsWith(", "))
            {
                formatted = formatted[..^2];
            }

            if (string.IsNullOrEmpty(formatted))
            {
                formatted = "NaN";
            }

            return formatted;
        }
        
        public static string ToReadable(this TimeSpan span)
        {
            var formatted = span.ToString(@"dd\.hh\:mm\:ss");
            
            if (string.IsNullOrEmpty(formatted))
            {
                formatted = "NaN";
            }
            
            return formatted;
        }
    }
}