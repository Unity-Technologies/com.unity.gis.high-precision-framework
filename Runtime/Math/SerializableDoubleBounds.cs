using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// This struct should only be used for serialization purposes within Unity, either
    /// in MonoBehaviours, such that it can appear in the inspector, or for JSON
    /// serialization. For any purpose that does not require serialization, please use
    /// <see cref="DoubleBounds"/>. Cast as quickly as you can to the DoubleBounds struct
    /// which is immutable.
    /// </summary>
    [Serializable]
    public struct SerializableDoubleBounds
    {
        /// <summary>
        /// The center of the bounds
        /// </summary>
        public double3 Center;

        /// <summary>
        /// The extent of the bounds, or the vector that can run from the center of the bounds 
        /// to each of the corners.
        /// </summary>
        public double3 Extents;

        /// <summary>
        /// Default constructor, using a <see cref="DoubleBounds"/>.
        /// </summary>
        /// <param name="bounds">The bounds which should be copied into the serializable version</param>
        public SerializableDoubleBounds(DoubleBounds bounds)
        {
            Center = bounds.Center;
            Extents = bounds.Extents;
        }

        /// <summary>
        /// Cast to <see cref="DoubleBounds"/>
        /// </summary>
        /// <param name="bounds">The <see cref="SerializableDoubleBounds"/> to be casted from</param>
        public static explicit operator DoubleBounds(SerializableDoubleBounds bounds)
        {
            return new DoubleBounds(bounds.Center, 2.0 * bounds.Extents);
        }

        /// <summary>
        /// Cast from <see cref="DoubleBounds"/>
        /// </summary>
        /// <param name="bounds">The <see cref="DoubleBounds"/> to be casted from</param>
        public static explicit operator SerializableDoubleBounds(DoubleBounds bounds)
        {
            return new SerializableDoubleBounds(bounds);
        }
    }
}
