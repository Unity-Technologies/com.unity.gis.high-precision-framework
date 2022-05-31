using Unity.Mathematics;

namespace Unity.Geospatial.HighPrecision.Editor
{
    //
    //  TODO - Clean this up
    //  TODO - Make labels draggable
    //

    /// <summary>
    /// Methods to draw <see cref="HPTransform"/> values in the <see href="https://docs.unity3d.com/Manual/UsingTheInspector.html">Inspector</see>
    /// </summary>
    internal static class HPTrsInspector
    {
        /// <summary>
        /// Draw translation, rotation and scaling fields.
        /// </summary>
        /// <param name="wrapper">The UI wrapper used to draw the component.</param>
        /// <param name="position">The translation values to draw.</param>
        /// <param name="rotation">The orientation values to draw.</param>
        /// <param name="scale">The scaling values to draw.</param>
        /// <returns>
        /// <see langword="true"/> ;
        /// <see langword="false"/> otherwise.
        /// </returns>
        internal static bool Draw(EditorGUILayoutWrapper wrapper, ref double3 position, ref quaternion rotation, ref float3 scale)
        {
            bool result = false;

            double3 oldPosition = position;
            position = wrapper.Double3Field("Position", position);
            if (!oldPosition.Equals(position))
                result = true;

            float3 vRotation = rotation.GetEulerDegrees();
            float3 oldRotation = vRotation;
            vRotation = wrapper.Float3Field("Rotation", vRotation);
            if (!oldRotation.Equals(vRotation))
            {
                rotation = HPMath.EulerZXYDegrees(vRotation);
                result = true;
            }

            float3 oldScale = scale;
            scale = wrapper.Float3Field("Scale", scale);
            if (!oldScale.Equals(scale))
                result = true;

            return result;
        }

        /// <summary>
        /// Draw translation and rotation fields.
        /// </summary>
        /// <param name="wrapper">The UI wrapper used to draw the component.</param>
        /// <param name="position">The translation values to draw.</param>
        /// <param name="rotation">The orientation values to draw.</param>
        /// <returns>
        /// <see langword="true"/> ;
        /// <see langword="false"/> otherwise.
        /// </returns>
        internal static bool Draw(EditorGUILayoutWrapper wrapper, ref double3 position, ref quaternion rotation)
        {
            bool result = false;

            double3 oldPosition = position;
            position = wrapper.Double3Field("Position", position);
            if (!oldPosition.Equals(position))
                result = true;

            float3 vRotation = rotation.GetEulerDegrees();
            float3 oldRotation = vRotation;
            vRotation = wrapper.Float3Field("Rotation", vRotation);
            if (!oldRotation.Equals(vRotation))
            {
                rotation = HPMath.EulerZXYDegrees(vRotation);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Draw translation, rotation and scaling fields where the scaling value is a uniform.
        /// </summary>
        /// <param name="wrapper">The UI wrapper used to draw the component.</param>
        /// <param name="position">The translation values to draw.</param>
        /// <param name="rotation">The orientation values to draw.</param>
        /// <param name="uniformScale">The scaling value to draw.</param>
        /// <returns>
        /// <see langword="true"/> ;
        /// <see langword="false"/> otherwise.
        /// </returns>
        internal static bool Draw(EditorGUILayoutWrapper wrapper, ref double3 position, ref quaternion rotation, ref float uniformScale)
        {
            bool result = false;

            double3 oldPosition = position;
            position = wrapper.Double3Field("Position", position);
            if (!oldPosition.Equals(position))
                result = true;

            float3 vRotation = rotation.GetEulerDegrees();
            float3 oldRotation = vRotation;
            vRotation = wrapper.Float3Field("Rotation", vRotation);
            if (!oldRotation.Equals(vRotation))
            {
                rotation = HPMath.EulerZXYDegrees(vRotation);
                result = true;
            }

            float oldScale = uniformScale;
            uniformScale = wrapper.Float1Field("Scale", uniformScale);
            if (!oldScale.Equals(uniformScale))
                result = true;


            return result;
        }
    }
}
