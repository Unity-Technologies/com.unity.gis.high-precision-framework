using Unity.Mathematics;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// High Precision node allowing to get its local and universe position in double precision.
    /// </summary>
    /// <remarks>
    /// This interface is not intended to be implemented outside of this package.
    /// </remarks>
    public interface HPNode
    {
        /// <summary>
        /// Translation information relative to its direct parent.
        /// </summary>
        double3 LocalPosition { get; }

        /// <summary>
        /// Translation information relative to the center of the universe.
        /// <remarks>This value should not consider any rebasing system.</remarks>
        /// </summary>
        double3 UniversePosition { get; }

        /// <summary>
        /// Orientation information relative to its direct parent.
        /// </summary>
        quaternion LocalRotation { get; }

        /// <summary>
        /// Orientation information relative to the center of the universe.
        /// <remarks>This value should not consider any rebasing system.</remarks>
        /// </summary>
        quaternion UniverseRotation { get; }

        /// <summary>
        /// Scaling information relative to its direct parent.
        /// </summary>
        float3 LocalScale { get; }

        /// <summary>
        /// Translation, rotation and scaling information relative to its direct parent.
        /// </summary>
        double4x4 LocalMatrix { get; }

        /// <summary>
        /// Translation, rotation and scaling information relative to the center of the universe.
        /// <remarks>This value should not consider any rebasing system.</remarks>
        /// </summary>
        double4x4 UniverseMatrix { get; }

        /// <summary>
        /// Translation, rotation and scaling information relative to the Unity world center.
        /// </summary>
        double4x4 WorldMatrix { get; }

        /// <summary>
        /// Set a as a child of this instance.
        /// <remarks>
        /// This should always be in sync with the Unity hierarchy and serves as an acceleration structure for
        /// computing hierarchical transform.
        /// </remarks>
        /// </summary>
        /// <param name="child">Node to register</param>
        void RegisterChild(HPTransform child);

        /// <summary>
        /// Remove a <see cref="HPTransform"/> from the list of registered children.
        /// </summary>
        /// <param name="child">Node to unregister.</param>
        void UnregisterChild(HPTransform child);

    }
}
