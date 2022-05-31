using Unity.Mathematics;

namespace Unity.Geospatial.HighPrecision.Editor
{
    /// <summary>
    /// Tells an Editor class how to display the <see cref="LocalCoordinateSystem"/> values.
    /// </summary>
    public class DefaultCoordinateSystemInspector : CoordinateSystemInspector
    {
        /// <summary>
        /// Value to use by the <see cref="Name"/> property.
        /// </summary>
        internal const string DefaultName = "Unity Default";

        /// <inheritdoc cref="CoordinateSystemInspector.Name"/>
        public override string Name
        {
            get { return DefaultName; }
        }

        /// <inheritdoc cref="CoordinateSystemInspector.OnInspectorGUI(HPNode)"/>
        public override void OnInspectorGUI(HPNode target)
        {
            switch (GetScaleType(target))
            {
                case ScaleTypes.None:
                    DrawTRSNoScale(target);
                    break;

                case ScaleTypes.Isotropic:
                    DrawTRSUniformScale(target);
                    break;

                case ScaleTypes.Anisotropic:
                    DrawTRSNonUniformScale(target);
                    break;
            }
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector when the scaling is non uniform.
        /// </summary>
        /// <param name="target">Draw the IMGUI for this component.</param>
        private void DrawTRSNonUniformScale(HPNode target)
        {
            GetTRS(target, out double3 position, out quaternion rotation, out float3 scale);

            if (HPTrsInspector.Draw(GUILayoutWrapper, ref position, ref rotation, ref scale))
                SetTRS(target, position, rotation, scale);
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector when the scaling is uniform.
        /// </summary>
        /// <param name="target">Draw the IMGUI for this component.</param>
        private void DrawTRSUniformScale(HPNode target)
        {
            GetTRS(target, out double3 position, out quaternion rotation, out float3 vScale);
            float scale = vScale.x;

            if (HPTrsInspector.Draw(GUILayoutWrapper, ref position, ref rotation, ref scale))
                SetTRS(target, position, rotation, scale * new float3(1F));
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector when there is no scaling values.
        /// </summary>
        /// <param name="target">Draw the IMGUI for this component.</param>
        private void DrawTRSNoScale(HPNode target)
        {
            GetTRS(target, out double3 position, out quaternion rotation, out float3 _);

            if (HPTrsInspector.Draw(GUILayoutWrapper, ref position, ref rotation))
                SetTRS(target, position, rotation, new float3(1F));
        }
    }
}
