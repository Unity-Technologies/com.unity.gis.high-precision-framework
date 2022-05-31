using System;
using System.Globalization;
using System.Runtime.CompilerServices;

using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// Represents an axis aligned bounding box with values stored as doubles.
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    [Serializable]
    public readonly struct DoubleBounds :
        IEquatable<DoubleBounds>,
        IFormattable
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="center">The location of the origin of the Bounds.</param>
        /// <param name="size">The dimensions of the Bounds.</param>
        public DoubleBounds(double3 center, double3 size) :
            this(center, 0.5 * size, false)
        {
            Assert.IsTrue(size.x >= 0, $"{nameof(DoubleBounds)} size.x cannot be a negative value.");
            Assert.IsTrue(size.y >= 0, $"{nameof(DoubleBounds)} size.y cannot be a negative value.");
            Assert.IsTrue(size.z >= 0, $"{nameof(DoubleBounds)} size.z cannot be a negative value.");
        }

        /// <summary>
        /// Construct a new <see cref="DoubleBounds"/> instance with the extents already calculated.
        /// </summary>
        /// <param name="center">The location of the origin of the Bounds.</param>
        /// <param name="extents">Distance between the center and the edges of the bounds.</param>
        /// <param name="isEmpty">
        /// <see langword="true"/> if the <paramref name="center"/> and <paramref name="extents"/> should not be considered;
        /// <see langword="false"/> otherwise.
        /// </param>
        private DoubleBounds(double3 center, double3 extents, bool isEmpty)
        {
            this.Center = center;
            this.Extents = extents;
            IsEmpty = isEmpty;
        }

        /// <summary>
        /// Get a <see cref="DoubleBounds"/> instance that is considered empty (no position, no size).
        /// </summary>
        /// <remarks>The empty bounds is preferred to the default when no content is required.</remarks>
        public static DoubleBounds Empty
        {
            get
            {
                return new DoubleBounds(
                    double3.zero,
                    new double3(-1),
                    true);
            }
        }

        /// <summary>
        /// The center of the bounding box.
        /// </summary>
        [SerializeField]
        public readonly double3 Center;

        /// <summary>
        /// Distance between the center and the edges of the bounds.
        /// </summary>
        [SerializeField]
        public readonly double3 Extents;

        /// <summary>
        /// <see langword="true"/> if the <see cref="Center"/> and <see cref="Extents"/> should not be considered;
        /// <see langword="false"/> otherwise.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// Validate both <see cref="DoubleBounds"/> have the same values.
        /// </summary>
        /// <param name="lhs">First instance to compare with.</param>
        /// <param name="rhs">Compare <paramref name="lhs"/> with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(DoubleBounds lhs, DoubleBounds rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Validate both <see cref="DoubleBounds"/> have the different values.
        /// </summary>
        /// <param name="lhs">First instance to compare with.</param>
        /// <param name="rhs">Compare <paramref name="lhs"/> with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if at least one value is different on both instances;
        /// <see langword="false"/> if both instance have the same values.
        /// </returns>
        public static bool operator !=(DoubleBounds lhs, DoubleBounds rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Validate <paramref name="obj"/> is a <see cref="DoubleBounds"/> instance and have the same values as this instance.
        /// </summary>
        /// <param name="obj">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is DoubleBounds bounds && Equals(bounds);
        }

        /// <summary>
        /// Validate an other <see cref="DoubleBounds"/> has the same values as this instance.
        /// </summary>
        /// <param name="other">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(DoubleBounds other)
        {
            return Center.Equals(other.Center)
                   && Extents.Equals(other.Extents)
                   && IsEmpty == other.IsEmpty;
        }

        /// <summary>
        /// Compute a hash code for the object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// * You should not assume that equal hash codes imply object equality.
        /// * You should never persist or use a hash code outside the application domain in which it was created,
        ///   because the same object may hash differently across application domains, processes, and platforms.
        /// </remarks>
        public override int GetHashCode()
        {
            int hashCenter = Center.GetHashCode() << 2;

            int hashExtents = Extents.GetHashCode() << 3;

            int hashEmpty = IsEmpty.GetHashCode();

            return hashEmpty ^ hashCenter ^ hashExtents;
        }

        /// <summary>
        /// The minimal point of the box. This is always equal to center-extents.
        /// </summary>
        public double3 Min
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Center - Extents; }
        }

        /// <summary>
        /// The maximal point of the box. This is always equal to center+extents.
        /// </summary>
        public double3 Max
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Center + Extents; }
        }

        /// <summary>
        /// The total size of the box. This is always twice as large as the extents.
        /// </summary>
        public double3 Size
        {
            get { return Extents * 2; }
        }

        /// <summary>
        /// Does another bounding box intersect with an other bounding box?
        /// Check if the bounding box comes into contact with another bounding box.
        /// This returns a Boolean that is set to <see langword="true"/> if there is an intersection between bounds.
        /// Two bounds are intersecting if there is at least one point which is contained by both bounds.
        /// Points on the min and max limits (corners and edges) of the bounding box are considered inside.
        /// </summary>
        /// <param name="bounds">The other <see cref="DoubleBounds"/> to validate the intersection with.</param>
        /// <returns>
        /// <see langword="true"/> If the <paramref name="bounds"/> intersects.;
        /// <see langword="false"/> if it doesn't touch it.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(DoubleBounds bounds)
        {
            if (IsEmpty || bounds.IsEmpty)
                return false;

            double3 ourMin = Center - Extents;
            double3 ourMax = Center + Extents;

            double3 theirMin = bounds.Center - bounds.Extents;
            double3 theirMax = bounds.Center + bounds.Extents;

            return
                ourMin.x <= theirMax.x && theirMin.x <= ourMax.x &&
                ourMin.y <= theirMax.y && theirMin.y <= ourMax.y &&
                ourMin.z <= theirMax.z && theirMin.z <= ourMax.z;
        }

        /// <summary>
        /// Is the given <paramref name="bounds"/> limits are within the limits of this instance.
        /// </summary>
        /// <param name="bounds">The <see cref="DoubleBounds"/> that must be within this instance.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="bounds"/> is inside this instance;
        /// <see langword="false"/> if it is not entirely within this instance limits.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(DoubleBounds bounds)
        {
            if (IsEmpty || bounds.IsEmpty)
                return false;

            double3 ourMin = Center - Extents;
            double3 ourMax = Center + Extents;

            double3 theirMin = bounds.Center - bounds.Extents;
            double3 theirMax = bounds.Center + bounds.Extents;

            return
                ourMin.x <= theirMin.x &&
                ourMin.y <= theirMin.y &&
                ourMin.z <= theirMin.z &&
                ourMax.x >= theirMax.x &&
                ourMax.y >= theirMax.y &&
                ourMax.z >= theirMax.z;
        }

        /// <summary>
        /// Convert a single precision Unity <see href="https://docs.unity3d.com/ScriptReference/Bounds.html">Bounds</see>
        /// instance to a new <see cref="DoubleBounds"/> instance.
        /// </summary>
        /// <param name="bounds">The instance to convert.</param>
        /// <returns>A new <see cref="DoubleBounds"/> instance.</returns>
        public static explicit operator DoubleBounds(Bounds bounds)
        {
            return new DoubleBounds(
                bounds.center.ToDouble3(),
                bounds.extents.ToDouble3(),
                false);
        }

        /// <summary>
        /// Convert a <see cref="DoubleBounds"/> instance to a new single precision Unity
        /// <see href="https://docs.unity3d.com/ScriptReference/Bounds.html">Bounds</see> instance.
        /// </summary>
        /// <param name="bounds">The instance to convert.</param>
        /// <returns>A new <see href="https://docs.unity3d.com/ScriptReference/Bounds.html">Bounds</see> instance.</returns>
        public static explicit operator Bounds(DoubleBounds bounds)
        {
            Bounds result = default;

            result.center = bounds.Center.ToVector3();
            result.extents = bounds.Extents.ToVector3();

            return result;
        }

        /// <summary>
        /// Join two <see cref="DoubleBounds"/> instance into a single instance resulting in the maximum size of both.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DoubleBounds Union(DoubleBounds a, DoubleBounds b)
        {
            if (!a.Intersects(b))
                return Empty;

            double3 min = math.max(a.Min, b.Min);
            double3 max = math.min(a.Max, b.Max);

            return new DoubleBounds(0.5 * (min + max), max - min);
        }

        /// <summary>
        /// Transform a <see cref="DoubleBounds"/> instance by a matrix allowing to translate / turn / scale it.
        /// </summary>
        /// <param name="bounds">Instance to transform.</param>
        /// <param name="transformationMatrix">Translate / Rotate / Resize the bounds by this matrix.</param>
        /// <returns>The result of the transformation.</returns>
        public static DoubleBounds Transform3x4(DoubleBounds bounds, double4x4 transformationMatrix)
        {
            //
            //  TODO - Implement optimized version of this
            //
            return Transform(bounds, transformationMatrix, default, false);
        }

        /// <summary>
        /// Apply a <see cref="DoublePlane"/> to the original <see cref="DoubleBounds"/> then
        /// translate / rotate / scale it according to the given <paramref name="transformMatrix"transformation matrix</paramref>.
        /// </summary>
        /// <param name="bounds">Instance to transform.</param>
        /// <param name="transformMatrix">Translate / Rotate / Resize the bounds by this matrix.</param>
        /// <param name="clipPlane">Cut the resulting <see cref="DoubleBounds"/> by this plane.</param>
        /// <returns>The result of the transformation.</returns>
        public static DoubleBounds Transform(DoubleBounds bounds, double4x4 transformMatrix, DoublePlane clipPlane)
        {
            return Transform(bounds, transformMatrix, clipPlane, true);
        }

        /// <summary>
        /// Transform a <see cref="DoubleBounds"/> instance by a matrix allowing to translate / turn / scale it and
        /// make sure it doesn't go beyond the given <paramref name="clipPlane"/>.
        /// </summary>
        /// <param name="bounds">Instance to transform.</param>
        /// <param name="transformationMatrix">Translate / Rotate / Resize the bounds by this matrix.</param>
        /// <param name="clipPlane">Cut the resulting <see cref="DoubleBounds"/> by this plane.</param>
        /// <param name="useClipPlane">
        /// <see langword="true"/> to cut the resulting <see cref="DoubleBounds"/> by the <paramref name="clipPlane"/>;
        /// <see langword="false"/> otherwise.
        /// </param>
        /// <returns>The result of the transformation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe DoubleBounds Transform(DoubleBounds bounds, double4x4 transformationMatrix, DoublePlane clipPlane, bool useClipPlane)
        {
            if (bounds.IsEmpty)
                return Empty;

            double3* universeVertices = stackalloc double3[8];

            double3 extents = bounds.Extents;
            double3 center = bounds.Center;

            double extentX = extents.x;
            double extentY = extents.y;
            double extentZ = extents.z;

            //
            //  Compute corner vertices
            //
            universeVertices[0] = center + new double3(-extentX, -extentY, -extentZ);
            universeVertices[1] = center + new double3(-extentX, -extentY, extentZ);
            universeVertices[2] = center + new double3(-extentX, extentY, -extentZ);
            universeVertices[3] = center + new double3(-extentX, extentY, extentZ);

            universeVertices[4] = center + new double3(extentX, -extentY, -extentZ);
            universeVertices[5] = center + new double3(extentX, -extentY, extentZ);
            universeVertices[6] = center + new double3(extentX, extentY, -extentZ);
            universeVertices[7] = center + new double3(extentX, extentY, extentZ);

            bool writeOne = false;
            double3 min = new double3(double.MaxValue, double.MaxValue, double.MaxValue);
            double3 max = new double3(double.MinValue, double.MinValue, double.MinValue);

            for (int i = 0; i < 8; i++)
            {
                double3 v = universeVertices[i];

                if (!useClipPlane || clipPlane.GetSide(v))
                {
                    double3 clip = transformationMatrix.HomogeneousTransformPoint(v);
                    min = math.min(min, clip);
                    max = math.max(max, clip);
                    writeOne = true;
                }
                else
                {
                    int a = i ^ 0x01;
                    int b = i ^ 0x02;
                    int c = i ^ 0x04;

                    if (clipPlane.GetSide(universeVertices[a]))
                    {
                        double3 raycast = clipPlane.Raycast(v, universeVertices[a]);
                        double3 clip = transformationMatrix.HomogeneousTransformPoint(raycast);
                        min = math.min(min, clip);
                        max = math.max(max, clip);
                        writeOne = true;
                    }

                    if (clipPlane.GetSide(universeVertices[b]))
                    {
                        double3 raycast = clipPlane.Raycast(v, universeVertices[b]);
                        double3 clip = transformationMatrix.HomogeneousTransformPoint(raycast);
                        min = math.min(min, clip);
                        max = math.max(max, clip);
                        writeOne = true;
                    }

                    if (clipPlane.GetSide(universeVertices[c]))
                    {
                        double3 raycast = clipPlane.Raycast(v, universeVertices[c]);
                        double3 clip = transformationMatrix.HomogeneousTransformPoint(raycast);
                        min = math.min(min, clip);
                        max = math.max(max, clip);
                        writeOne = true;
                    }
                }
            }

            return writeOne
                ? new DoubleBounds(0.5 * (min + max), max - min)
                : Empty;
        }

        /// <summary>
        /// Returns a formatted string for the bounds.
        /// </summary>
        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        /// Returns a formatted string for the bounds.
        /// </summary>
        /// <param name="format">A numeric format string.</param>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        /// Returns a formatted string for the bounds.
        /// </summary>
        /// <param name="format">A numeric format string.</param>
        /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F1";

            return string.Format(
                CultureInfo.InvariantCulture.NumberFormat,
                "Center: {0}, Extents: {1}",
                Center.ToString(format, formatProvider),
                Extents.ToString(format, formatProvider));
        }
    }
}
