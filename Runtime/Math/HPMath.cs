using System.Runtime.CompilerServices;

using Unity.Burst;
using Unity.Mathematics;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// Mathematics methods used for easily converting data between
    /// geodetic formats and Unity default expected formats.
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public static class HPMath
    {
        /// <summary>
        /// Returns a quaternion representing a rotation around a unit axis by an angle in degrees.
        /// The rotation direction is clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <returns>The quaternion representing a rotation around an axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion AxisAngleDegrees(float3 axis, float angle)
        {
            return quaternion.AxisAngle(axis, math.radians(angle));
        }

        /// <summary>
        /// Make sure <paramref name="dst"/> has the sign as <paramref name="src"/> on all of its dimensions.
        /// Other than the sign, the scalar values of <paramref name="dst"/> are preserved.
        /// </summary>
        /// <param name="src">Copy the negative sign from the x, y, z value of this point.</param>
        /// <param name="dst">Paste the negative sign from to the x, y, z value of this point.</param>
        /// <returns>The converted <paramref name="dst"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float3 CopyVector3Sign(float3 src, float3 dst)
        {
            float x = src.x * dst.x >= 0 ? dst.x : -dst.x;
            float y = src.y * dst.y >= 0 ? dst.y : -dst.y;
            float z = src.z * dst.z >= 0 ? dst.z : -dst.z;

            return new float3(x, y, z);
        }

        /// <summary>
        /// Returns a quaternion constructed by first performing a rotation around the z-axis, then the x-axis and finally the y-axis.
        /// All rotation angles are in degrees and clockwise when looking along the rotation axis towards the origin.
        /// This is the default order rotation order in Unity.
        /// </summary>
        /// <param name="xyz">A float3 vector containing the rotation angles around the x-, y- and z-axis measures in degrees.</param>
        /// <returns>The quaternion representing the Euler angle rotation in z-x-y order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion EulerZXYDegrees(float3 xyz)
        {
            return quaternion.EulerZXY(math.radians(xyz));
        }

        /// <summary>
        /// Returns a double4x4 matrix representing a combined scale-, rotation- and translation transform.
        /// Equivalent to mul(translationTransform, mul(rotationTransform, scaleTransform)).
        /// </summary>
        /// <param name="translation">The translation vector.</param>
        /// <param name="rotation">The quaternion rotation.</param>
        /// <param name="scale">The scaling factors of each axis.</param>
        /// <returns>The double4x4 matrix representing the translation, rotation, and scale by the inputs.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double4x4 TRS(double3 translation, quaternion rotation, float3 scale)
        {
            float3x3 r = math.float3x3(rotation);
            return math.double4x4(
                math.double4(r.c0 * scale.x, 0.0f),
                math.double4(r.c1 * scale.y, 0.0f),
                math.double4(r.c2 * scale.z, 0.0f),
                math.double4(translation, 1.0f));
        }

        /// <summary>Returns a double4x4 translation matrix given a double3 translation vector.</summary>
        /// <param name="vector">The translation vector.</param>
        /// <returns>The double4x4 translation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double4x4 Translate(double3 vector)
        {
            return new double4x4(
                new double4(1.0, 0.0, 0.0, 0.0),
                new double4(0.0, 1.0, 0.0, 0.0),
                new double4(0.0, 0.0, 1.0, 0.0),
                new double4(vector.x, vector.y, vector.z, 1.0));
        }

        /// <summary>
        /// Transform the given <paramref name="point"/> by returning a new projective coordinate.
        /// </summary>
        /// <param name="matrix">The coordinate to transform.</param>
        /// <param name="point">Multiply the <paramref name="matrix"/> by this point.</param>
        /// <returns>The computed projective coordinate.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double3 HomogeneousTransformPoint(this double4x4 matrix, double3 point)
        {
            double4 result = matrix.c0 * point.x + matrix.c1 * point.y + matrix.c2 * point.z + matrix.c3;
            double w = 1.0 / result.w;
            return new double3(w * result.x, w * result.y, w * result.z);
        }

        /// <summary>
        /// Returns the double4 column vector result of a matrix multiplication between a double4x4 matrix and
        /// a double3 column vector with a 0 as the fourth value.
        /// </summary>
        /// <param name="matrix">Left hand side argument of the matrix multiply.</param>
        /// <param name="vector">Right hand side argument of the matrix multiply.</param>
        /// <returns>The computed matrix multiplication.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double3 HomogeneousTransformVector(this double4x4 matrix, double3 vector)
        {
            double4 result = math.mul(matrix, new double4(vector, 0));

            return new double3(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get the <paramref name="translation"/>, <paramref name="rotation"/> and <paramref name="scale">scaling</paramref> part of the given <paramref name="matrix"/>.
        /// </summary>
        /// <param name="matrix">The matrix requested to get its <paramref name="translation"/>, <paramref name="rotation"/> and <paramref name="scale"/> from.</param>
        /// <param name="translation">Returns the position part.</param>
        /// <param name="rotation">Returns the orientation part.</param>
        /// <param name="scale">Returns the resize part.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetTRS(this double4x4 matrix, out double3 translation, out quaternion rotation, out float3 scale)
        {
            double4 col0x4 = matrix.c0;
            double4 col1x4 = matrix.c1;
            double4 col2x4 = matrix.c2;
            double4 col3 = matrix.c3;

            float3 col0 = new float3((float)col0x4.x, (float)col0x4.y, (float)col0x4.z);
            float3 col1 = new float3((float)col1x4.x, (float)col1x4.y, (float)col1x4.z);
            float3 col2 = new float3((float)col2x4.x, (float)col2x4.y, (float)col2x4.z);

            rotation = quaternion.LookRotationSafe(col2, col1);

            scale = new float3(
                math.dot(col0, math.mul(rotation, math.right())),
                math.dot(col1, math.mul(rotation, math.up())),
                math.dot(col2, math.mul(rotation, math.forward())));

            translation = new double3(col3.x, col3.y, col3.z);
        }

        /// <summary>
        /// Returns the Euler angle representation of the quaternion following the ZYX rotation order.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="rotation">The quaternion to convert to Euler angles.</param>
        /// <returns>The Euler angle representation of the quaternion in ZYX order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetEulerZXY(this quaternion rotation)
        {
            const float epsilon = 1e-6f;
            const float cutoff = (1f - 2f * epsilon) * (1f - 2f * epsilon);

            float4 qv = rotation.value;
            float4 d1 = qv * qv.wwww * math.float4(2f); //xw, yw, zw, ww
            float4 d2 = qv * qv.yzxw * math.float4(2f); //xy, yz, zx, ww
            float4 d3 = qv * qv;
            float3 euler;

            float y1 = d2.y - d1.x;
            if (y1 * y1 < cutoff)
            {
                float x1 = d2.x + d1.z;
                float x2 = d3.y + d3.w - d3.x - d3.z;
                float z1 = d2.z + d1.y;
                float z2 = d3.z + d3.w - d3.x - d3.y;
                euler = new float3(math.atan2(x1, x2), -math.asin(y1), math.atan2(z1, z2));
            }
            else //zxz
            {
                y1 = math.clamp(y1, -1.0f, 1.0f);
                float4 abcd = new float4(d2.z, d1.y, d2.y, d1.x);
                float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                float x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                euler = new float3(math.atan2(x1, x2), -math.asin(y1), 0.0f);
            }

            return euler.yzx;
        }

        /// <summary>
        /// Returns the Euler angle representation of the quaternion following the ZYX rotation order.
        /// All rotation angles are in radians and clockwise when looking along the rotation axis towards the origin.
        /// </summary>
        /// <param name="rotation">The quaternion to convert to Euler angles.</param>
        /// <returns>The Euler angle representation of the quaternion in ZYX order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetEulerDegrees(this quaternion rotation)
        {
            return math.degrees(GetEulerZXY(rotation));
        }

        /// <summary>
        /// Get the rotation part of a matrix as a quaternion expressed in radians.
        /// </summary>
        /// <param name="matrix">Matrix to extract the rotation from.</param>
        /// <returns>The rotation result.</returns>
        public static quaternion GetRotation(this double4x4 matrix)
        {
            double4 col1x4 = matrix.c1;
            double4 col2x4 = matrix.c2;

            float3 col1 = new float3((float)col1x4.x, (float)col1x4.y, (float)col1x4.z);
            float3 col2 = new float3((float)col2x4.x, (float)col2x4.y, (float)col2x4.z);

            return quaternion.LookRotationSafe(col2, col1);
        }

        /// <summary>
        /// Get the scaling part of a matrix.
        /// </summary>
        /// <param name="matrix">Matrix to extract the scaling from.</param>
        /// <returns>The scaling result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetScale(this double4x4 matrix)
        {
            double4 col0x4 = matrix.c0;
            double4 col1x4 = matrix.c1;
            double4 col2x4 = matrix.c2;

            float3 col0 = new float3((float)col0x4.x, (float)col0x4.y, (float)col0x4.z);
            float3 col1 = new float3((float)col1x4.x, (float)col1x4.y, (float)col1x4.z);
            float3 col2 = new float3((float)col2x4.x, (float)col2x4.y, (float)col2x4.z);

            quaternion rotation = quaternion.LookRotationSafe(col2, col1);

            return new float3(
                math.dot(new float3(col0.x, col0.y, col0.z), math.mul(rotation, new float3(1F, 0F, 0F))),
                math.dot(new float3(col1.x, col1.y, col1.z), math.mul(rotation, new float3(0F, 1F, 0F))),
                math.dot(new float3(col2.x, col2.y, col2.z), math.mul(rotation, new float3(0F, 0F, 1F))));
        }

        /// <summary>
        /// Get the position part of a matrix.
        /// </summary>
        /// <param name="matrix">Matrix to extract the position from.</param>
        /// <returns>The position result.</returns>
        public static double3 GetTranslation(this double4x4 matrix)
        {
            double4 col = matrix.c3;
            return new double3(col.x, col.y, col.z);
        }
    }
}
