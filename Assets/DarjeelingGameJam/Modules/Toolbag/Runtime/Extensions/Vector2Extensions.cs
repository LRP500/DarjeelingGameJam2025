using UnityEngine;

namespace Modules.Toolbag.Extensions
{
	public static class Vector2Extensions
	{
		public static Vector3 ToVector3(this Vector2 source)
		{
			return new Vector3(source.x, source.y, 0f);
		}
	}
}