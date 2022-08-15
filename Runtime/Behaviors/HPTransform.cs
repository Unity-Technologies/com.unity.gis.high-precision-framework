using System.Diagnostics;

using UnityEngine;
using UnityEngine.Assertions;
using Unity.Mathematics;

namespace Unity.Geospatial.HighPrecision
{
    //
    //  TODO - Changing the scale of the root resets the HPTransform's values!!!
    //

    /// <summary>
    /// The HPTransform is the High-Precision Framework's primary class. It acts very
    /// similarly to an ordinary transform, however its position is in 64 bit precision
    /// rather than being in 32 bit precision. The HPTransform can be used with or without
    /// an HPRoot parent. However, in order to truly benefit from the 64 bit precision, it
    /// should be the child of an HPRoot.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("HighPrecision/HPTransform")]
    public class HPTransform : HPNode
    {
        /// <summary>
        /// <see langword="true"/> if all of the fields where set at least once;
        /// <see langword="false"/> if the fields needs to be set before used.
        /// </summary>
        [SerializeField]
        private bool m_IsInitialized;

        /// <inheritdoc cref="HPNode.LocalPosition"/>
        [SerializeField]
        private double3 m_LocalPosition = double3.zero;

        /// <inheritdoc cref="HPNode.LocalRotation"/>
        [SerializeField]
        private quaternion m_LocalRotation = quaternion.identity;

        /// <inheritdoc cref="HPNode.LocalScale"/>
        [SerializeField]
        private float3 m_LocalScale = new float3(1F);

        /// <summary>
        /// The HP hierarchy parent node of this instance.
        /// This node is used to calculate the <see cref="WorldMatrix"/>.
        /// </summary>
        private HPNode m_Parent;

        /// <summary>
        /// Cache Unity's <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> to reduce overhead of retrieving it everytime.
        /// </summary>
        private Transform m_CachedUnityTransform;

        /// <summary>
        /// Cache Unity's <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> to reduce overhead of retrieving it everytime.
        /// </summary>
        private Transform CachedUnityTransform
        {
            get
            {
                if (m_CachedUnityTransform == null)
                    m_CachedUnityTransform = transform;

                return m_CachedUnityTransform;
            }
        }

        /// <summary>
        /// <see langword="true"/> if the <see cref="m_CachedUniverseMatrix"/> value is still valid;
        /// <see langword="false"/> if it needs to be updated.
        /// </summary>
        private bool m_CachedUniverseMatrixIsValid;

        /// <summary>
        /// The computed <see cref="UniverseMatrix"/> value that was still valid at the previous frame.
        /// </summary>
        /// <remarks>
        /// If <see cref="m_CachedUniverseMatrixIsValid"/> is false, this value needs to be recalculated for the
        /// current frame.
        /// </remarks>
        private double4x4 m_CachedUniverseMatrix;

        /// <summary>
        /// <see langword="true"/> if the <see cref="m_CachedUniverseRotation"/> value is still valid;
        /// <see langword="false"/> if it needs to be updated.
        /// </summary>
        private bool m_CachedUniverseRotationIsValid;

        /// <summary>
        /// The computed <see cref="UniverseRotation"/> value that was still valid at the previous frame.
        /// </summary>
        /// <remarks>
        /// If <see cref="m_CachedUniverseRotationIsValid"/> is false, this value needs to be recalculated for the
        /// current frame.
        /// </remarks>
        private quaternion m_CachedUniverseRotation;

        /// <summary>
        /// <see langword="true"/> if the <see cref="m_CachedWorldMatrix"/> value is still valid;
        /// <see langword="false"/> if it needs to be updated.
        /// </summary>
        private bool m_CachedWorldMatrixIsValid;

        /// <summary>
        /// The computed <see cref="WorldMatrix"/> value that was still valid at the previous frame.
        /// </summary>
        /// <remarks>
        /// If <see cref="m_CachedWorldMatrixIsValid"/> is false, this value needs to be recalculated for the
        /// current frame.
        /// </remarks>
        private double4x4 m_CachedWorldMatrix;

        /// <summary>
        /// <see langword="true"/> if the <see cref="m_CachedLocalMatrix"/> value is still valid;
        /// <see langword="false"/> if it needs to be updated.
        /// </summary>
        private bool m_CachedLocalMatrixIsValid;

        /// <summary>
        /// The computed <see cref="LocalMatrix"/> value that was still valid at the previous frame.
        /// </summary>
        /// <remarks>
        /// If <see cref="m_CachedLocalMatrixIsValid"/> is false, this value needs to be recalculated for the
        /// current frame.
        /// </remarks>
        private double4x4 m_CachedLocalMatrix;

        /// <summary>
        /// <see langword="true"/> if the <see cref="m_CachedUnityTransform"/> values are still valid;
        /// <see langword="false"/> if they need to be updated.
        /// </summary>
        private bool m_UnityTransformIsValid;

        /// <summary>
        /// <see langword="true"/> if the current local transform values has changed;
        /// <see langword="false"/> if the local transform values are still the same as the previous frame.
        /// </summary>
        private bool m_LocalHasChanged;
        
        /// <summary>
        /// The position of the HPTransform relative to its parent HPRoot or HPTransform
        /// </summary>
        public override double3 LocalPosition
        {
            get { return m_LocalPosition; }
            set
            {
                AssertIsValid(value);

                m_LocalPosition = value;

                InvalidateLocalCache();
            }
        }

        /// <summary>
        /// Set the position of the HPTransform relative to its parent HPRoot or HPTransform
        /// </summary>
        /// <param name="position">Change the <see cref="LocalPosition"/> to this value.</param>
        public void SetLocalPosition(double3 position)
        {
            AssertIsValid(position);

            m_LocalPosition = position;

            InvalidateLocalCache();
        }

        /// <summary>
        /// The position of the HPTransform, in universe space.
        /// </summary>
        public override double3 UniversePosition
        {
            get
            {
                return m_Parent == null
                    ? m_LocalPosition
                    : m_Parent.UniverseMatrix.HomogeneousTransformPoint(m_LocalPosition);
            }
            set
            {
                AssertIsValid(value);

                m_LocalPosition = m_Parent == null
                    ? value
                    : math.inverse(m_Parent.UniverseMatrix).HomogeneousTransformPoint(value);

                InvalidateLocalCache();
            }
        }

        /// <summary>
        /// The rotation of the HPTransform relative to its parent HPTransform or HPRoot
        /// </summary>
        public override quaternion LocalRotation
        {
            get { return m_LocalRotation; }
            set
            {
                AssertIsValid(value);

                m_LocalRotation = value;

                InvalidateLocalCache();
            }
        }

        /// <summary>
        /// The rotation of the HPTransform, in universe space
        /// </summary>
        public override quaternion UniverseRotation
        {
            get
            {
                if (!m_CachedUniverseRotationIsValid)
                {
                    m_CachedUniverseRotation = m_Parent == null
                        ? m_LocalRotation
                        : math.mul(m_Parent.UniverseRotation, m_LocalRotation);

                    m_CachedUniverseRotationIsValid = true;
                }
                return m_CachedUniverseRotation;
            }
            set
            {
                AssertIsValid(value);

                m_LocalRotation = m_Parent == null
                    ? value
                    : math.mul(math.inverse(m_Parent.UniverseRotation), value);

                InvalidateLocalCache();
            }
        }
        
        //
        //  TODO - When scale is set to zero, quaternion method is printing messages in the console
        //
        /// <summary>
        /// The scale of the HPTransform, relative to its parent HPTransform or HPRoot. If the HPTransform is
        /// not a leaf node (i.e. if it contains another HPTransform) only uniform scales will be possible. Under
        /// these circumstances, only the x component of the scale will count towards the uniform scale.
        /// </summary>
        public override float3 LocalScale
        {
            get
            {
                if (ScaleType == ScaleTypes.Anisotropic)
                    return m_LocalScale;
                else
                {
                    float uniformScale = m_LocalScale.x;
                    return new float3(uniformScale, uniformScale, uniformScale);
                }
            }
            set
            {
                AssertIsValid(value);

                m_LocalScale = value;

                InvalidateLocalCache();
            }
        }

        /// <summary>
        /// Set or get the parent of the HPTransform. When changing the parent, the HPTransform's position in the world
        /// will be preserved.
        /// </summary>
        public Transform Parent
        {
            get { return CachedUnityTransform.parent; }
            set { CachedUnityTransform.parent = value; }
        }

        /// <summary>
        /// The HPTransform's forward vector, in universe space.
        /// </summary>
        public float3 Forward
        {
            get { return math.mul(UniverseRotation, math.forward()); }
        }

        /// <summary>
        /// The HPTransform's right vector, in universe space.
        /// </summary>
        public float3 Right
        {
            get { return math.mul(UniverseRotation, math.right()); }
        }

        /// <summary>
        /// The HPTransform's up vector, in universe space.
        /// </summary>
        public float3 Up
        {
            get { return math.mul(UniverseRotation, math.up()); }
        }

        /// <summary>
        /// The type of scale that is currently supported by the HPTransform. When in uniform scale,
        /// only the x component of the set scale will be considered for the uniform scale.
        /// </summary>
        public ScaleTypes ScaleType
        {
            get { return HasChildren
                            ? ScaleTypes.Isotropic
                            : ScaleTypes.Anisotropic; }
        }

        /// <inheritdoc cref="HPNode.LocalMatrix"/>
        public override double4x4 LocalMatrix
        {
            get
            {
                if (!m_CachedLocalMatrixIsValid)
                {
                    m_CachedLocalMatrix = HPMath.TRS(m_LocalPosition, m_LocalRotation, m_LocalScale);
                    m_CachedLocalMatrixIsValid = true;
                }
                return m_CachedLocalMatrix;
            }
        }

        /// <inheritdoc cref="HPNode.UniverseMatrix"/>
        public override double4x4 UniverseMatrix
        {
            get
            {
                if (!m_CachedUniverseMatrixIsValid)
                {
                    if (m_Parent == null)
                        m_CachedUniverseMatrix = LocalMatrix;
                    else
                        m_CachedUniverseMatrix = math.mul(m_Parent.UniverseMatrix, LocalMatrix);

                    m_CachedUniverseMatrixIsValid = true;
                }
                return m_CachedUniverseMatrix;
            }
        }

        /// <inheritdoc cref="HPNode.WorldMatrix"/>
        public override double4x4 WorldMatrix
        {
            get
            {
                if (!m_CachedWorldMatrixIsValid)
                {
                    if (m_Parent == null)
                        m_CachedWorldMatrix = LocalMatrix;
                    else
                        m_CachedWorldMatrix = math.mul(m_Parent.WorldMatrix, LocalMatrix);

                    m_CachedWorldMatrixIsValid = true;
                }
                return m_CachedWorldMatrix;
            }
        }

        /// <summary>
        /// <see langword="true"/> if <see cref="HPTransform"/> nodes have this instance as their parent;
        /// <see langword="false"/> otherwise.
        /// </summary>
        private bool HasChildren
        {
            get { return m_Children.Count > 0; }
        }

        /// <summary>
        /// <see langword="true"/> if it is possible to change the hierarchy underneath;
        /// <see langword="false"/> otherwise.
        /// </summary>
        internal bool IsSceneEditable
        {
            get { return m_Children.Count == 0; }
        }

        /// <summary>
        /// This method is called when the component or the GameObject is enabled.
        /// </summary>
        private void OnEnable()
        {
            Assert.IsNull(m_Parent);
            UpdateParentRelation();

            //
            //  TODO - Find a way of initializing the HPTransform from the Unity Transform without
            //         using serialized variables.
            //
            if (!m_IsInitialized)
                UpdateHPTransformFromUnityTransform();

            InvalidateLocalCache();
        }

        /// <summary>
        /// This method is called when the component or the GameObject is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (m_Parent != null)
            {
                m_Parent.UnregisterChild(this);
                m_Parent = null;
            }
        }

        /// <summary>
        /// This method is called whenever values in the inspector are changed by the user. By invalidating
        /// the local cache, the underlying Unity Transform is updated. This method also catches changes
        /// that do not necessarily go through the properties and directly to the private serialized fields.
        /// </summary>
        private void OnValidate()
        {
            InvalidateLocalCache();
        }

        /// <summary>
        /// This method is called when the component is reset in the inspector. By invalidating the local cache,
        /// the underlying Unity Transform is updated. This method is important since resetting a component 
        /// bypasses the properties and acts directly on the private serialized fields.
        /// </summary>
        private void Reset()
        {
            InvalidateLocalCache();
        }

        /// <summary>
        /// Updates m_HPRoot so that it points towards the corret HPRoot
        /// component. Returns true if it has changed and false otherwise.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the parenting relation has changed;
        /// <see langword="false"/> otherwise.
        /// </returns>
        private void UpdateParentRelation()
        {
            if (!isActiveAndEnabled)
                return;


            Transform parent = CachedUnityTransform.parent;
            
            HPNode hpNode = parent == null
                ? null
                : parent.GetComponentInParent<HPNode>();

            if (hpNode != m_Parent)
            {
                m_Parent?.UnregisterChild(this);
                m_Parent = hpNode;
                m_Parent?.RegisterChild(this);
            }
        }

        /// <summary>
        /// Update the translation, rotation and scaling values of the associated <see cref="CachedUnityTransform">Transform</see>.
        /// </summary>
        internal void UpdateUnityTransform()
        {
            if (HasChildren)
                UpdateUnityTransformIntermediateTransform();
            else
                UpdateUnityTransformWorldSpace();

            ClearUnityTransformChange();

            for (int i = 0; i < m_Children.Count; ++i)
                m_Children[i].UpdateUnityTransform();

            m_UnityTransformIsValid = true;
        }

        /// <summary>
        /// Flag the associated <see cref="CachedUnityTransform">Transform</see> to be up to date and doesn't need to be evaluated.
        /// </summary>
        internal void ClearUnityTransformChange()
        {
            CachedUnityTransform.hasChanged = false;

            for (int i = 0; i < m_Children.Count; ++i)
                m_Children[i].ClearUnityTransformChange();
        }

        /// <summary>
        /// Reset the translation, rotation and scaling values of the associated <see cref="CachedUnityTransform">Transform</see>
        /// allowing its children a valid position.
        /// </summary>
        private void UpdateUnityTransformIntermediateTransform()
        {
            Transform t = CachedUnityTransform;

            if (t.localPosition != Vector3.zero)
                t.localPosition = Vector3.zero;

            if (t.localRotation != Quaternion.identity)
                t.localRotation = Quaternion.identity;

            Vector3 scale = LocalScale;
            if (t.localScale != scale)
                t.localScale = scale;
        }

        /// <summary>
        /// Update the translation, rotation and scaling values of the associated <see cref="CachedUnityTransform">Transform</see>
        /// with the <see cref="WorldMatrix"/>.
        /// <remarks>This should be used when the <see cref="HPTransform"/> instance has no children.</remarks>
        /// </summary>
        private void UpdateUnityTransformWorldSpace()
        {
            double4x4 worldMatrix = WorldMatrix;

            // if (double.IsNaN(worldMatrix.m00))
            // {
            //     Debug.LogError("Invalid root matrix");
            //     return;
            // }

            worldMatrix.GetTRS(
                out double3 translation,
                out quaternion rotation,
                out float3 scale);

            CachedUnityTransform.SetPositionAndRotation(translation.ToVector3(), rotation); 

            // Since scale is impacted by translation and rotation in GetTRS(),
            // we do not directly use scale from GetTRS() to avoid loss of precision in values
            CachedUnityTransform.localScale = HPMath.CopyVector3Sign(scale, m_LocalScale);
        }

        /// <summary>
        /// Update the cache based on the changes from the <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> values.
        /// </summary>
        private void UpdateHPTransformFromUnityTransform()
        {
            double4x4 parentFromWorld = m_Parent == null
                ? double4x4.identity
                : math.inverse(m_Parent.WorldMatrix);

            double4x4 worldFromObject = HPMath.TRS(
                CachedUnityTransform.position.ToDouble3(),
                CachedUnityTransform.rotation,
                CachedUnityTransform.lossyScale);

            double4x4 parentFromObject = math.mul(parentFromWorld, worldFromObject);

            //
            //  TODO - Optimize this, we don't use scale.
            //
            parentFromObject.GetTRS(
                out double3 position,
                out quaternion rotation,
                out float3 scale);

            m_LocalPosition = position;
            m_LocalRotation = rotation;

            // Since scale is impacted by translation and rotation in GetTRS(),
            // we do not directly use scale from GetTRS() to avoid loss of precision in values
            m_LocalScale = HPMath.CopyVector3Sign(scale, CachedUnityTransform.localScale);

            InvalidateLocalCache();
        }

        /// <summary>
        /// LateUpdate is called every frame, if the Behaviour is enabled.
        /// LateUpdate is called after all Update functions have been called.
        /// </summary>
        private void LateUpdate()
        {
            if (m_Parent == null)
                UpdateFromParent();
        }

        /// <summary>
        /// This function is called when a direct or indirect parent of the transform of the GameObject has changed.
        /// </summary>
        private void OnTransformParentChanged()
        {
            double4x4 worldFromLocal = WorldMatrix;

            UpdateParentRelation();
            CachedUnityTransform.hasChanged = false;

            double4x4 worldFromParent = m_Parent == null ? double4x4.identity : m_Parent.WorldMatrix;
            double4x4 parentFromWorld = math.inverse(worldFromParent);
            double4x4 parentFromLocal = math.mul(parentFromWorld, worldFromLocal);

            parentFromLocal.GetTRS(out double3 translation, out quaternion rotation, out float3 scale);

            m_LocalPosition = translation;
            m_LocalRotation = rotation;
            m_LocalScale = scale;

            InvalidateLocalCache();
        }
        
        /// <summary>
        /// Update the Unity <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see>
        /// position, rotation and scaling value when the <see cref="m_Parent"/>
        /// <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> values changes.
        /// </summary>
        internal void UpdateFromParent()
        {
            //
            //  Give priority to changes made directly to the HPTransform over changes
            //      made to the CachedUnityTransform. This facilitates initializations.
            //
            if (!m_LocalHasChanged && CachedUnityTransform.hasChanged)
            {
                UpdateHPTransformFromUnityTransform();
                ClearUnityTransformChange();
            }

            if (!m_UnityTransformIsValid)
                UpdateUnityTransform();

            for (int i = 0; i < m_Children.Count; ++i)
                m_Children[i].UpdateFromParent();

            m_LocalHasChanged = false;
        }

        /// <inheritdoc cref="HPNode.RegisterChild"/>
        public override void RegisterChild(HPTransform child)
        {
            //
            //  When first child is added, scale type transitions from
            //      uniform to non-uniform.
            //
            if (m_Children.Count == 0)
                InvalidateLocalCache();

            base.RegisterChild(child);
            UpdateUnityTransform();
        }

        /// <inheritdoc cref="HPNode.UnregisterChild"/>
        public override void UnregisterChild(HPTransform child)
        {
            base.UnregisterChild(child);

            //
            //  When last child is removed, scale type transitions from
            //      uniform to non-uniform.
            //
            if (m_Children.Count == 0)
                InvalidateLocalCache();
        }

        /// <summary>
        /// Flag all caches related to local transforms to be recomputed.
        /// </summary>
        private void InvalidateLocalCache()
        {
            m_IsInitialized = true;
            m_LocalHasChanged = true;
            m_CachedLocalMatrixIsValid = false;
            InvalidateUniverseCache();
        }

        /// <summary>
        /// Flag all caches related to universe transforms to be recomputed.
        /// </summary>
        internal void InvalidateUniverseCache()
        {
            m_CachedUniverseRotationIsValid = false;
            m_CachedUniverseMatrixIsValid = false;
            InvalidateWorldCache();

            for (int i = 0; i < m_Children.Count; ++i)
                m_Children[i].InvalidateUniverseCache();
        }

        /// <summary>
        /// Flag all caches related to world transforms to be recomputed and execute the same invalidation to its children.
        /// </summary>
        internal void InvalidateWorldCacheRecursively()
        {
            InvalidateWorldCache();

            for (int i = 0; i < m_Children.Count; ++i)
                m_Children[i].InvalidateWorldCacheRecursively();
        }

        /// <summary>
        /// Flag all caches related to world transforms to be recomputed.
        /// </summary>
        private void InvalidateWorldCache()
        {
            m_CachedWorldMatrixIsValid = false;
            m_UnityTransformIsValid = false;
        }

        /// <summary>
        /// Validate the given double3 does not have infinite or NaN values.
        /// </summary>
        /// <param name="value">Struct to evaluate its content.</param>
        [Conditional("DEBUG")]
        private static void AssertIsValid(double3 value)
        {
            Assert.IsFalse(double.IsInfinity(value.x) || double.IsNaN(value.x), "Cannot set HP Transform with Null or Infinite values");
            Assert.IsFalse(double.IsInfinity(value.y) || double.IsNaN(value.y), "Cannot set HP Transform with Null or Infinite values");
            Assert.IsFalse(double.IsInfinity(value.z) || double.IsNaN(value.z), "Cannot set HP Transform with Null or Infinite values");
        }

        /// <summary>
        /// Validate the given float3 does not have infinite or NaN values.
        /// </summary>
        /// <param name="value">Struct to evaluate its content.</param>
        [Conditional("DEBUG")]
        private static void AssertIsValid(float3 value)
        {
            Assert.IsFalse(float.IsInfinity(value.x) || float.IsNaN(value.x), "Cannot set HP Transform with Null or Infinite values");
            Assert.IsFalse(float.IsInfinity(value.y) || float.IsNaN(value.y), "Cannot set HP Transform with Null or Infinite values");
            Assert.IsFalse(float.IsInfinity(value.z) || float.IsNaN(value.z), "Cannot set HP Transform with Null or Infinite values");
        }

        /// <summary>
        /// Validate the given quaternion does not have infinite or NaN values.
        /// </summary>
        /// <param name="value">Struct to evaluate its content.</param>
        [Conditional("DEBUG")]
        private static void AssertIsValid(quaternion value)
        {
            Assert.IsFalse(float.IsInfinity(value.value.x) || float.IsNaN(value.value.x), "Cannot set HP Transform with Null or Infinite values");
            Assert.IsFalse(float.IsInfinity(value.value.y) || float.IsNaN(value.value.y), "Cannot set HP Transform with Null or Infinite values");
            Assert.IsFalse(float.IsInfinity(value.value.z) || float.IsNaN(value.value.z), "Cannot set HP Transform with Null or Infinite values");
            Assert.IsFalse(float.IsInfinity(value.value.w) || float.IsNaN(value.value.w), "Cannot set HP Transform with Null or Infinite values");
        }
    }
}
