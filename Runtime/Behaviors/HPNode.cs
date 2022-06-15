using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// High Precision node allowing to get its local and universe position in double precision.
    /// </summary>
    /// <remarks>
    /// This interface is not intended to be implemented outside of this package.
    /// </remarks>
    public abstract class HPNode:
        MonoBehaviour
    {
        /// <summary>
        /// The type of scale that the HPNode's current configuration allows. When it is 
        /// a leaf node, it will accept non-uniform scales. Otherwise, it will only apply
        /// a uniform scale.
        /// </summary>
        public enum ScaleTypes
        {
            /// <summary>
            /// A non uniform scale, which has an x, y and z component
            /// </summary>
            Anisotropic,

            /// <summary>
            /// A uniform scale, where only the x component is considered
            /// </summary>
            Isotropic
        }
        
        /// <summary>
        /// List of the child nodes of this instance. Those nodes will multiply their <see cref="LocalMatrix"/> by this
        /// instance <see cref="UniverseMatrix"/> to get their <see cref="UniverseMatrix"/> and same thing for the
        /// <see cref="WorldMatrix"/>.
        /// </summary>
        protected readonly List<HPTransform> m_Children = new List<HPTransform>();
        
        /// <summary>
        /// Translation information relative to its direct parent.
        /// </summary>
        public abstract double3 LocalPosition { get; set; }

        /// <summary>
        /// Orientation information relative to its direct parent.
        /// </summary>
        public abstract quaternion LocalRotation { get; set; }

        /// <summary>
        /// Scaling information relative to its direct parent.
        /// </summary>
        public abstract float3 LocalScale { get; set; }

        /// <summary>
        /// Translation, rotation and scaling information relative to its direct parent.
        /// </summary>
        public abstract double4x4 LocalMatrix { get; }

        /// <summary>
        /// Translation information relative to the center of the universe.
        /// <remarks>This value should not consider any rebasing system.</remarks>
        /// </summary>
        public abstract double3 UniversePosition { get; set; }

        /// <summary>
        /// Orientation information relative to the center of the universe.
        /// <remarks>This value should not consider any rebasing system.</remarks>
        /// </summary>
        public abstract quaternion UniverseRotation { get; set; }

        /// <summary>
        /// Translation, rotation and scaling information relative to the center of the universe.
        /// <remarks>This value should not consider any rebasing system.</remarks>
        /// </summary>
        public abstract double4x4 UniverseMatrix { get; }

        /// <summary>
        /// Translation, rotation and scaling information relative to the Unity world center.
        /// </summary>
        public abstract double4x4 WorldMatrix { get; }

        /// <summary>
        /// Set a as a child of this instance.
        /// <remarks>
        /// This should always be in sync with the Unity hierarchy and serves as an acceleration structure for
        /// computing hierarchical transform.
        /// </remarks>
        /// </summary>
        /// <param name="child">Node to register</param>
        public virtual void RegisterChild(HPTransform child)
        {
            m_Children.Add(child);
        }

        /// <summary>
        /// Remove a <see cref="HPTransform"/> from the list of registered children.
        /// </summary>
        /// <param name="child">Node to unregister.</param>
        public virtual void UnregisterChild(HPTransform child)
        {
            m_Children.Remove(child);
        }

    }
}
