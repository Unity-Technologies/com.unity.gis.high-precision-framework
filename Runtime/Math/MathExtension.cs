
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// Extend Unity mathematics structs.
    /// </summary>
    public static class MathExtension
    {
        /// <summary>
        /// Convert a double3 instance to a Unity <see href="https://docs.unity3d.com/ScriptReference/Vector3.html">Vector3</see> instance.
        /// </summary>
        /// <param name="vector">Point to convert.</param>
        /// <returns>The convert point.</returns>
        public static Vector3 ToVector3(this double3 vector)
        {
            return new Vector3((float)vector.x, (float)vector.y, (float)vector.z);
        }

        /// <summary>
        /// Convert a Unity <see href="https://docs.unity3d.com/ScriptReference/Vector3.html">Vector3</see> to a double3 instance.
        /// </summary>
        /// <param name="vector">Point to convert.</param>
        /// <returns>The convert point.</returns>
        public static double3 ToDouble3(this Vector3 vector)
        {
            return new double3(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Convert a double4x4 instance to a Unity <see href="https://docs.unity3d.com/ScriptReference/Matrix4x4.html">Vector3</see> instance.
        /// </summary>
        /// <param name="matrix">Matrix to convert.</param>
        /// <returns>The convert matrix.</returns>
        public static Matrix4x4 ToMatrix4x4(this double4x4 matrix)
        {
            return new Matrix4x4((float4)matrix.c0, (float4)matrix.c1, (float4)matrix.c2, (float4)matrix.c3);
        }
    }
}
