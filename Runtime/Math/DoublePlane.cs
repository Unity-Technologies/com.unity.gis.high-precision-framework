using System;
using System.Globalization;
using System.Runtime.CompilerServices;

using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// Representation of a plane in 3D space storing values as doubles.
    /// </summary>
    [BurstCompile(CompileSynchronously = true)]
    public readonly struct DoublePlane :
        IEquatable<DoublePlane>,
        IFormattable
    {
        /// <summary>
        /// Normal vector of the plane.
        /// </summary>
        public double3 Normal { get; }

        /// <summary>
        /// The distance measured from the Plane to the origin, along the Plane's normal.
        /// </summary>
        public double Distance { get; }

        /// <summary>
        /// Construct a plane using an arbitrary <paramref name="point"/> on the plane's surface and the plane's <paramref name="normal"/>.
        /// </summary>
        /// <param name="normal">
        /// The plane's normal vector which is perpendicular to the plane's surface.
        /// This is expected to be a unit vector.
        /// </param>
        /// <param name="point">An arbitrary point which is included in the plane's domain.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DoublePlane(double3 normal, double3 point)
        {
            Normal = math.normalizesafe(normal);
            Distance = -math.dot(Normal, point);
        }

        /// <summary>
        /// Construct a <see cref="DoublePlane"/> instance using its <paramref name="normal"/> and its
        /// <paramref name="distance"/> from the origin.
        /// </summary>
        /// <param name="normal">
        /// The plane's normal vector which is perpendicular to the plane's surface.
        /// This is expected to be a unit vector.
        /// </param>
        /// <param name="distance">
        /// The distance the plane is from the origin.
        /// This distance can be easily determined by extending the <paramref name="normal"/> vector from the origin
        /// until it intersects the plane. The <paramref name="distance"/> can be negative to allow forward and
        /// back facing planes.
        /// </param>
        public DoublePlane(double3 normal, double distance) :
            this(normal, distance, true) { }

        /// <summary>
        /// Construct a <see cref="DoublePlane"/> instance using three arbitrary points on the plane.
        /// The only constraint is that these points cannot be collinear.
        /// </summary>
        /// <param name="a">First point on the plane.</param>
        /// <param name="b">Second point on the plane.</param>
        /// <param name="c">Third point on the plane.</param>
        public DoublePlane(double3 a, double3 b, double3 c) :
            this(math.cross(b - a, c - a), a) { }

        /// <summary>
        /// Construct a <see cref="DoublePlane"/> instance with the distance already calculated and a possibly
        /// normalized normal.
        /// </summary>
        /// <param name="normal">
        /// The plane's normal vector which is perpendicular to the plane's surface.
        /// This is expected to be a unit vector.
        /// </param>
        /// <param name="distance">The distance measured from the Plane to the origin, along the Plane's normal.</param>
        /// <param name="normalize">
        /// <see langword="true"/> to normalize the given <paramref name="normal"/>;
        /// <see langword="false"/> otherwise.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DoublePlane(double3 normal, double distance, bool normalize)
        {
            Normal = normalize ? math.normalizesafe(normal) : normal;
            Distance = distance;
        }

        /// <summary>
        /// Convert a <see cref="DoublePlane"/> instance to a new single precision Unity
        /// <see href="https://docs.unity3d.com/ScriptReference/Plane.html">Plane</see> instance.
        /// </summary>
        /// <param name="plane">The instance to convert.</param>
        /// <returns>A new <see href="https://docs.unity3d.com/ScriptReference/Plane.html">Plane</see> instance.</returns>
        public static explicit operator Plane(DoublePlane plane)
        {
            Plane result = default;

            result.distance = (float)plane.Distance;
            result.normal = plane.Normal.ToVector3();

            return result;
        }

        /// <summary>
        /// Convert a single precision Unity <see href="https://docs.unity3d.com/ScriptReference/Plane.html">Plane</see>
        /// instance to a new <see cref="DoublePlane"/> instance.
        /// </summary>
        /// <param name="plane">The instance to convert.</param>
        /// <returns>A new <see cref="DoublePlane"/> instance.</returns>
        public static explicit operator DoublePlane(Plane plane)
        {
            Vector3 normal = plane.normal;
            return new DoublePlane(normal.ToDouble3(), plane.distance, false);
        }

        /// <summary>
        /// Validate both <see cref="DoublePlane"/> have the same values.
        /// </summary>
        /// <param name="lhs">First instance to compare with.</param>
        /// <param name="rhs">Compare <paramref name="lhs"/> with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(DoublePlane lhs, DoublePlane rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Validate both <see cref="DoublePlane"/> have the different values.
        /// </summary>
        /// <param name="lhs">First instance to compare with.</param>
        /// <param name="rhs">Compare <paramref name="lhs"/> with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if at least one value is different on both instances;
        /// <see langword="false"/> if both instance have the same values.
        /// </returns>
        public static bool operator !=(DoublePlane lhs, DoublePlane rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Validate <paramref name="obj"/> is a <see cref="DoublePlane"/> instance and have the same values as this instance.
        /// </summary>
        /// <param name="obj">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is DoublePlane plane && Equals(plane);
        }

        /// <summary>
        /// Validate an other <see cref="DoublePlane"/> have the same values as this instance.
        /// </summary>
        /// <param name="other">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(DoublePlane other)
        {
            return Normal.Equals(other.Normal)
                   && Distance.Equals(other.Distance);
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
            int hashNormal = Normal.GetHashCode() << 2;

            int hashDistance = Distance.GetHashCode();

            return hashDistance ^ hashNormal;
        }

        /// <summary>
        /// For a given point returns the closest point on the plane.
        /// </summary>
        /// <param name="point">The point to project onto the plane.</param>
        /// <returns>
        /// A point on the plane that is closest to point.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double3 ClosestPointOnPlane(double3 point)
        {
            double num = math.dot(Normal, point) + Distance;
            return point - Normal * num;
        }

        /// <summary>
        /// Returns a copy of the plane that faces in the opposite direction.
        /// </summary>
        public DoublePlane Flipped
        {
            get { return new DoublePlane(-Normal, -Distance); }
        }

        /// <summary>
        /// Returns a signed distance from plane to point.
        /// </summary>
        /// <param name="point">Get the distance from this point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetDistanceToPoint(double3 point)
        {
            return math.dot(Normal, point) + Distance;
        }

        /// <summary>
        /// Get on which side a the plane a given <paramref name="point"/> is.
        /// </summary>
        /// <param name="point">Position to get its relative side.</param>
        /// <returns>
        /// <see langword="true"/> if the point is on the positive side of the plane;
        /// <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetSide(double3 point)
        {
            double3 origin = -Distance * Normal;
            return math.dot(point - origin, Normal) >= 0.0;
        }

        /// <summary>
        /// Intersects a ray with the plane.
        /// </summary>
        /// <param name="p1">One point positioned on the projected ray.</param>
        /// <param name="p2">A second point positioned on the projected ray.</param>
        /// <returns>The distance along the ray, where it intersects the plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double3 Raycast(double3 p1, double3 p2)
        {
            double3 origin = -Distance * Normal;
            double proj1 = math.dot(p1 - origin, Normal);
            double proj2 = math.dot(p2 - origin, Normal);
            double k = proj1 / (proj1 - proj2);
            return math.lerp(p1, p2, k);
        }

        /// <summary>
        /// Are two points on the same side of the plane?
        /// </summary>
        /// <param name="point0">First point to evaluate.</param>
        /// <param name="point1">Get if this second point is on the same side as <paramref name="point0"/>.</param>
        public bool SameSide(double3 point0, double3 point1)
        {
            double distanceToPoint1 = GetDistanceToPoint(point0);
            double distanceToPoint2 = GetDistanceToPoint(point1);
            return distanceToPoint1 > 0.0 && distanceToPoint2 > 0.0 || distanceToPoint1 <= 0.0 && distanceToPoint2 <= 0.0;
        }

        /// <summary>
        /// Returns a copy of the plane that is moved in space by the given translation.
        /// </summary>
        /// <param name="translation">The offset in space to move the plane with.</param>
        /// <returns>
        /// The translated plane.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DoublePlane Translate(double3 translation)
        {
            return new DoublePlane(Normal, Distance + math.dot(Normal, translation));
        }


        /// <summary>
        ///   <para>Returns a formatted string for the plane.</para>
        /// </summary>
        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        ///   <para>Returns a formatted string for the plane.</para>
        /// </summary>
        /// <param name="format">A numeric format string.</param>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        ///   <para>Returns a formatted string for the plane.</para>
        /// </summary>
        /// <param name="format">A numeric format string.</param>
        /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F1";

            return string.Format(
                CultureInfo.InvariantCulture.NumberFormat,
                "Normal: {0}, Distance: {1}",
                Normal.ToString(format, formatProvider),
                Distance.ToString(format, formatProvider));
        }
    }
}
