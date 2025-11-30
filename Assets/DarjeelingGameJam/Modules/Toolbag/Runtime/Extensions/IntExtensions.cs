namespace Modules.Toolbag.Extensions
{
    public static class IntExtensions
    {
        /// <summary>
        /// Returns true if value is between min and max, else returns false.
        /// </summary>
        /// <param name="self">self</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <param name="includeMax">include maximum</param>
        /// <returns></returns>
        public static bool InRange(this int self, int min, int max, bool includeMax = false)
        {
            return includeMax ? self >= min && self <= max : self >= min && self < max;
        }
    }
}
