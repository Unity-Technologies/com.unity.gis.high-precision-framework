
using UnityEngine;
using Unity.Mathematics;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// Extension methods for Unity <see href="https://docs.unity3d.com/ScriptReference/Matrix4x4.html">Matrix4x4</see>.
    /// </summary>
    public static class MatrixExtension
    {
        /// <summary>
        /// Convert a unity Matrix4x4 instance to a new Unity mathematics double4x4 instance.
        /// </summary>
        /// <param name="matrix">The instance to convert.</param>
        /// <returns>A new double4x4 instance.</returns>
        public static double4x4 ToDouble4x4(this Matrix4x4 matrix)
        {
            Vector4 col0 = matrix.GetColumn(0);
            Vector4 col1 = matrix.GetColumn(1);
            Vector4 col2 = matrix.GetColumn(2);
            Vector4 col3 = matrix.GetColumn(3);

            return new double4x4(
                col0.x, col1.x, col2.x, col3.x,
                col0.y, col1.y, col2.y, col3.y,
                col0.z, col1.z, col2.z, col3.z,
                col0.w, col1.w, col2.w, col3.w);
        }

        /// <summary>
        /// Get the <paramref name="translation"/>, <paramref name="rotation"/> and <paramref name="scale">scaling</paramref> part of the given <paramref name="matrix"/>.
        /// </summary>
        /// <param name="matrix">The matrix requested to get its <paramref name="translation"/>, <paramref name="rotation"/> and <paramref name="scale"/> from.</param>
        /// <param name="translation">Returns the position part.</param>
        /// <param name="rotation">Returns the orientation part.</param>
        /// <param name="scale">Returns the resize part.</param>
        public static void GetTRS(this Matrix4x4 matrix, out double3 translation, out quaternion rotation, out float3 scale)
        {
            matrix.ToDouble4x4().GetTRS(out translation, out rotation, out scale);
        }
    }
}
