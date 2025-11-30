using UnityEngine;

namespace Modules.Toolbag.Extensions
{
    /// <summary>
    /// Extension methods for UnityEngine.Float
    /// </summary>
    public static class FloatExtensions
    {
        /// <summary>
        /// Return true if float is almost equal to value, return false otherwise. 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool AlmostEqual(this float self, float value, float tolerance)
        {
            return Mathf.Abs(self - value) < tolerance;
        }

        /// <summary>
        /// Return true if value is between min and max, return false otherwise
        /// </summary>
        /// <param name="self">self</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <param name="include">include bounds</param>
        /// <returns></returns>
        public static bool InRange(this float self, float min, float max, bool include = true)
        {
            return include ? (self >= min && self <= max) : (self > min && self < max);
        }

        /// <summary>
        /// Remap value from range A to range B.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minA"></param>
        /// <param name="maxA"></param>
        /// <param name="minB"></param>
        /// <param name="maxB"></param>
        /// <returns></returns>
        public static float Remap(this float value, float minA, float maxA, float minB, float maxB)
        {
            var normal = Mathf.InverseLerp(minA, maxA, value);
            return Mathf.Lerp(minB, maxB, normal);
        }

		/// <summary>
		/// Clamp value between two values.
		/// </summary>
		/// <param name="self"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static float Clamp(this float self, float min, float max)
		{
			return Mathf.Clamp(self, min, max);
		}

		/// <summary>
		/// Clamp value between 0 and 1.
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static float Clamp01(this float self)
		{
			return Mathf.Clamp01(self);
		}
    }
}