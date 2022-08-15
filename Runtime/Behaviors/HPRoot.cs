using System;

using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// The HPRoot determines how the universe space will be converted into world
    /// space. It defines the coordinate in universe space which corresponds to
    /// the position of the GameObject, in world space.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("HighPrecision/HPRoot")]
    public class HPRoot : HPNode
    {
        /// <inheritdoc cref="HPNode.LocalPosition"/>
        [SerializeField]
        private double3 m_LocalPosition = double3.zero;

        /// <inheritdoc cref="HPNode.LocalRotation"/>
        [SerializeField]
        private quaternion m_LocalRotation = quaternion.identity;

        /// <summary>
        /// <see langword="true"/> if the <see cref="WorldMatrix"/> is already calculated and stored;
        /// <see langword="false"/> if the <see cref="WorldMatrix"/> needs to be reevaluated because of an updated value.
        /// </summary>
        private bool m_CachedWorldMatrixIsValid;

        /// <summary>
        /// The last known <see cref="WorldMatrix"/> value.
        /// </summary>
        private double4x4 m_CachedWorldMatrix;

        /// <summary>
        /// Performance marker used for profiling <see cref="LateUpdate"/> code block.
        /// </summary>
        private static ProfilerMarker s_LateUpdateMarker = new ProfilerMarker("HPRoot.LateUpdate");

        /// <inheritdoc cref="HPNode.LocalPosition"/>
        public override double3 LocalPosition
        {
            get { return double3.zero; }
            set { throw new NotSupportedException($"{nameof(HPRoot)}.{nameof(LocalPosition)} cannot be changed."); }
        }

        /// <inheritdoc cref="HPNode.UniversePosition"/>
        public override double3 UniversePosition
        {
            get { return double3.zero; }
            set { throw new NotSupportedException($"{nameof(HPRoot)}.{nameof(UniversePosition)} cannot be changed."); }
        }

        /// <summary>
        /// Set the position in universe space which corresponds to the HPRoot's
        /// position in the scene.
        /// </summary>
        public double3 RootUniversePosition
        {
            get { return m_LocalPosition; }
            set
            {
                m_LocalPosition = value;
                InvalidateWorldCache();
            }
        }

        /// <summary>
        /// Set the rotation of the universe space which corresponds to the HPRoot's
        /// rotation in the scene.
        /// </summary>
        public quaternion RootUniverseRotation
        {
            get { return m_LocalRotation; }
            set
            {
                m_LocalRotation = value;
                InvalidateWorldCache();
            }
        }

        /// <summary>
        /// Simultaneously set root universe position and root universe rotation
        /// in a single call, updating underlying transforms only once.
        /// </summary>
        /// <param name="position">The position in universe space which corresponds to the HPRoot's position in the scene</param>
        /// <param name="rotation">The position in universe space which corresponds to the HPRoot's rotation in the scene</param>
        public void SetRootTR(double3 position, quaternion rotation)
        {
            m_LocalPosition = position;
            m_LocalRotation = rotation;
            InvalidateWorldCache();
        }

        /// <inheritdoc cref="HPNode.LocalRotation"/>
        public override quaternion LocalRotation
        {
            get { return quaternion.identity; }
            set { throw new NotSupportedException($"{nameof(HPRoot)}.{nameof(LocalRotation)} cannot be changed."); }
        }

        /// <inheritdoc cref="HPNode.UniverseRotation"/>
        public override quaternion UniverseRotation
        {
            get { return quaternion.identity; }
            set { throw new NotSupportedException($"{nameof(HPRoot)}.{nameof(UniverseRotation)} cannot be changed."); }
        }

        /// <inheritdoc cref="HPNode.LocalScale"/>
        public override float3 LocalScale
        {
            get { return new float3(1F); }
            set { throw new NotSupportedException($"{nameof(HPRoot)}.{nameof(LocalScale)} cannot be changed."); }
        }

        /// <inheritdoc cref="HPNode.LocalMatrix"/>
        public override double4x4 LocalMatrix
        {
            get { return double4x4.identity; }
        }

        /// <inheritdoc cref="HPNode.UniverseMatrix"/>
        public override double4x4 UniverseMatrix
        {
            get { return double4x4.identity; }
        }

        /// <inheritdoc cref="HPNode.WorldMatrix"/>
        public override double4x4 WorldMatrix
        {
            get
            {
                if (!m_CachedWorldMatrixIsValid || transform.hasChanged)
                {
                    double4x4 worldFromRoot = transform.localToWorldMatrix.ToDouble4x4();
                    double4x4 universeFromRoot = HPMath.TRS(m_LocalPosition, m_LocalRotation, LocalScale);
                    m_CachedWorldMatrix = math.mul(worldFromRoot, math.inverse(universeFromRoot));
                    m_CachedWorldMatrixIsValid = true;
                    transform.hasChanged = false;
                }
                return m_CachedWorldMatrix;
            }
        }

        /// <summary>
        /// Transforms position from universe space to world space
        /// </summary>
        internal double3 TransformPoint(double3 universePosition)
        {
            return WorldMatrix.HomogeneousTransformPoint(universePosition);
        }

        /// <summary>
        /// Transforms direction from universe space to world space, the returned vector is an unit vector
        /// </summary>
        internal double3 TransformDirection(double3 universeDirection)
        {
            double3 worldDirection = WorldMatrix.HomogeneousTransformVector(universeDirection);
            return math.normalizesafe(worldDirection);
        }

        /// <summary>
        /// Transforms rotation from universe space to world space
        /// </summary>
        internal quaternion TransformRotation(quaternion universeRotation)
        {
            return math.mul(WorldMatrix.GetRotation(), universeRotation);
        }

        /// <summary>
        /// Transforms position from world space to universe space
        /// </summary>
        internal double3 InverseTransformPoint(double3 worldPosition)
        {
            return math.inverse(WorldMatrix).HomogeneousTransformPoint(worldPosition);
        }

        /// <summary>
        /// Transforms direction from world space to universe space.
        /// </summary>
        /// <param name="worldDirection">
        /// The direction to transform the <see cref="WorldMatrix"/>.
        /// If the input vector is not a unit vector, the result will also be scaled accordingly.
        /// </param>
        internal double3 InverseTransformDirection(double3 worldDirection)
        {
            double3 universeDirection = math.inverse(WorldMatrix).HomogeneousTransformVector(worldDirection);
            return math.normalizesafe(universeDirection);
        }

        /// <summary>
        /// Transforms rotation from world space to universe space
        /// </summary>
        internal quaternion InverseTransformRotation(quaternion worldRotation)
        {
            quaternion worldRotationInverted = math.inverse(WorldMatrix.GetRotation());
            return math.mul(worldRotationInverted, worldRotation);
        }

        /// <summary>
        /// Update the Unity <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see>
        /// position, rotation and scaling value for each of this instance children.
        /// </summary>
        private void UpdateTransforms()
        {
            foreach (var child in m_Children)
                child.UpdateFromParent();
        }

        /// <summary>
        /// Flag the world matrix of this instance as invalid and force its recalculation the next time it is required.
        /// </summary>
        private void InvalidateWorldCache()
        {
            m_CachedWorldMatrixIsValid = false;
            foreach (var child in m_Children)
                child.InvalidateWorldCacheRecursively();
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            InvalidateWorldCache();
        }

        /// <summary>
        /// LateUpdate is called every frame, if the Behaviour is enabled.
        /// LateUpdate is called after all Update functions have been called.
        /// </summary>
        private void LateUpdate()
        {
            s_LateUpdateMarker.Begin();

            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                foreach (var child in m_Children)
                    child.ClearUnityTransformChange();
            }

            UpdateTransforms();

            s_LateUpdateMarker.End();
        }
    }
}
