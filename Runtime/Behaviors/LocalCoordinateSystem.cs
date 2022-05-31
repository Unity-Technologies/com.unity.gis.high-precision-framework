using Unity.Mathematics;
using UnityEngine;

namespace Unity.Geospatial.HighPrecision
{
    /// <summary>
    /// The LocalCoordinateSystem component causes any HPTransform to be defined as 
    /// the center of interest within the universe space. This will maximize rendering
    /// precision around this object by essentially keeping it in the center of the
    /// scene. For many applications, the camera or the player makes for a good candidate
    /// as the origin HPTransform.
    /// 
    /// This component is meant as an example of how to implement a very simple dynamic
    /// rebasing scheme. However, many other schemes can and should be considered, such
    /// as rebasing only when getting a predefined distance from the origin or event triggered
    /// rebasing when the user performs an action.
    /// </summary>
    [RequireComponent(typeof(HPRoot))]
    public class LocalCoordinateSystem : MonoBehaviour
    {
        /// <summary>
        /// The object who's position and rotation will correspond to the origin of the scene.
        /// It's position, in world space will be zero, and it's rotation will be identity.
        /// </summary>
        [SerializeField]
        private HPTransform m_Origin;

        /// <summary>
        /// The <see cref="HPRoot"/> node this instance is linked with.
        /// </summary>
        private HPRoot m_Root;

        /// <summary>
        /// Position of the <see cref="Origin"/> at the last stored frame.
        /// </summary>
        private double3 m_LastPosition;

        /// <summary>
        /// The object who's position and rotation will correspond to the origin of the scene.
        /// It's position, in world space will be zero, and it's rotation will be identity.
        /// </summary>
        public HPTransform Origin
        {
            get { return m_Origin; }
            set { m_Origin = value; }
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            m_Root = GetComponent<HPRoot>();
        }

        /// <summary>
        /// LateUpdate is called every frame, if the Behaviour is enabled.
        /// LateUpdate is called after all Update functions have been called.
        /// </summary>
        private void LateUpdate()
        {
            if (m_Origin != null && !m_LastPosition.Equals(m_Origin.UniversePosition))
            {
                m_LastPosition = m_Origin.UniversePosition;
                m_Root.RootUniversePosition = m_LastPosition;
            }
        }
    }
}
