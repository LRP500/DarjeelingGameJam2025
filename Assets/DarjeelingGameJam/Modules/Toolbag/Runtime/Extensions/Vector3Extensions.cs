using System.Collections.Generic;
using UnityEngine;

namespace Modules.Toolbag.Extensions
{
    /// <summary>
    /// Extension methods for UnityEngine.Vector3
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Finds the position closest to the given one
        /// </summary>
        /// <param name="position">World position.</param>
        /// <param name="otherPositions">Other world positions.</param>
        /// <returns>Closest position.</returns>
        public static Vector3 GetClosest(this Vector3 position, IEnumerable<Vector3> otherPositions)
        {
            var closest = Vector3.zero;
            var shortestDistance = Mathf.Infinity;
            
            foreach (var otherPosition in otherPositions)
            {
                var distance = (position - otherPosition).sqrMagnitude;
                
                if (!(distance < shortestDistance))
                {
                    continue;
                }
                
                closest = otherPosition;
                shortestDistance = distance;
            }
            
            return closest;
        }

        /// <summary>
        /// Check equality with floating point imprecision tolerance
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool AlmostEqual(this Vector3 v1, Vector3 v2, float tolerance)
        {
            var equal = !(Mathf.Abs(v1.x - v2.x) > tolerance);
            
            if (Mathf.Abs(v1.y - v2.y) > tolerance)
            {
                equal = false;
            }

            if (Mathf.Abs(v1.z - v2.z) > tolerance)
            {
                equal = false;
            }
            
            return equal;
        }

        /// <summary>
        /// Check equality with floating point imprecision tolerance
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool AlmostEqualXZ(this Vector3 v1, Vector3 v2, float tolerance)
        {
            var equal = !(Mathf.Abs(v1.x - v2.x) > tolerance);
            
            if (Mathf.Abs(v1.z - v2.z) > tolerance)
            {
                equal = false;
            }
            
            return equal;
        }

        /// <summary>
        /// Check equality with floating point imprecision tolerance
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool AlmostEqualXY(this Vector3 v1, Vector3 v2, float tolerance)
        {
            var equal = !(Mathf.Abs(v1.x - v2.x) > tolerance);
            
            if (Mathf.Abs(v1.y - v2.y) > tolerance)
            {
                equal = false;
            }
            
            return equal;
        }

        /// <summary>
        /// Sets vector's x value
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="x">Value of x.</param>
        public static Vector3 SetX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }

        /// <summary>
        /// Sets vector's y value
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="y">Value of x.</param>
        public static Vector3 SetY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }

        /// <summary>
        /// Sets vector's z value
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="z">Value of x.</param>
        public static Vector3 SetZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// Set vector's x and y values
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static Vector3 SetXY(this Vector3 vector, float x, float y)
        {
            return new Vector3(x, y, vector.z);
        }

        /// <summary>
        /// Set vector's x and z values
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public static Vector3 SetXZ(this Vector3 vector, float x, float z)
        {
            return new Vector3(x, vector.y, z);
        }

        /// <summary>
        /// Set vector's y and z values
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static Vector3 SetYZ(this Vector3 vector, float y, float z)
        {
            return new Vector3(vector.x, y, z);
        }

        /// <summary>
        /// Return true if world point is within camera frustum limits, returns false otherwise
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static bool IsWorldPointVisible(this Vector3 worldPos, Camera camera)
        {
            var screenPos = camera.WorldToScreenPoint(worldPos);
            return screenPos.IsScreenPointVisible();
        }

        /// <summary>
        /// Return true if screen point is within screen limits, returns false otherwise
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        public static bool IsScreenPointVisible(this Vector3 screenPos)
        {
            return screenPos.x.InRange(0, Screen.width) && 
                   screenPos.y.InRange(0, Screen.height);
        }
        
        /// <summary>
        /// Returns a Vector2 with x and y values of the Vector3.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector2 ToVector2(this Vector3 source)
        {
            return new Vector2(source.x, source.y);
        }
    }
}