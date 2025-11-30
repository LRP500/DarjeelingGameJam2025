using UnityEngine;

namespace Modules.Toolbag.Extensions
{
	public static class Vector2Extensions
	{
		public static Vector3 ToVector3(this Vector2 source)
		{
			return new Vector3(source.x, source.y, 0f);
		}
		
		public static bool AlmostEqual(this Vector2 v1, Vector2 v2, float tolerance)
		{
			var equal = !(Mathf.Abs(v1.x - v2.x) > tolerance);
            
			if (Mathf.Abs(v1.y - v2.y) > tolerance)
			{
				equal = false;
			}
            
			return equal;
		}
	}
}