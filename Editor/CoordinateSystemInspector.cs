using System;

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Unity.Geospatial.HighPrecision.Editor
{
    /// <summary>
    /// Base class to tells an Editor class how to display the values of a custom <see cref="LocalCoordinateSystem"/>.
    /// </summary>
    public abstract class CoordinateSystemInspector
    {
        /// <summary>
        /// Specify how to draw the scaling field.
        /// </summary>
        public enum ScaleTypes
        {
            /// <summary>
            /// The <see cref="LocalCoordinateSystem"/> does not support scaling.
            /// </summary>
            None,

            /// <summary>
            /// Uniform scaling where x, y and z values are the same.
            /// </summary>
            Isotropic,

            /// <summary>
            /// None-uniform scaling.
            /// </summary>
            Anisotropic
        }

        /// <summary>
        /// The title of the action to appear in the undo history (i.e. visible in the undo menu) once a value has changed.
        /// </summary>
        private const string k_UndoString = "Inspector";

        /// <summary>
        /// The UI wrapper used to draw the component in the inspector.
        /// </summary>
        internal EditorGUILayoutWrapper GUILayoutWrapper { get; set; }

        /// <summary>
        /// Unique name to display allowing to differentiate the related <see cref="LocalCoordinateSystem"/> from others.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector for a given <see cref="HPNode"/>.
        /// </summary>
        /// <param name="target">Target to draw the inspector for.</param>
        public abstract void OnInspectorGUI(HPNode target);

        /// <summary>
        /// Get the <see cref="ScaleTypes"/> of the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">Get the <see cref="ScaleTypes"/> values for this node.</param>
        /// <returns>The <see cref="ScaleTypes"/> for the given <paramref name="target"/>.</returns>
        protected ScaleTypes GetScaleType(HPNode target)
        {
            if (target is HPTransform transform && target != null)
            {
                return transform.ScaleType == HPTransform.ScaleTypes.Isotropic ? ScaleTypes.Isotropic : ScaleTypes.Anisotropic;
            }
            else
            {
                return ScaleTypes.None;
            }
        }

        /// <summary>
        /// Get the actual translation, rotation and scale values of the <paramref name="target"/>.
        /// </summary>
        /// <param name="target">Get the transform values for this node.</param>
        /// <param name="translation">Returns the position value of the <paramref name="target"/>.</param>
        /// <param name="rotation">Returns the orientation value of the <paramref name="target"/>.</param>
        /// <param name="scale">Returns the scaling value of the <paramref name="target"/>.</param>
        /// <exception cref="InvalidOperationException">If the <paramref name="target"/> will be disposed.</exception>
        /// <exception cref="NotSupportedException">
        /// If the <paramref name="target"/> is not supported by this class.
        /// Custom <see cref="HPNode"/> implementations are not supported.
        /// </exception>
        protected void GetTRS(HPNode target, out double3 translation, out quaternion rotation, out float3 scale)
        {
            if (target == null)
                throw new InvalidOperationException(
                    "Coordinate System Inspector cannot be instantiated with null values");

            if (target is HPTransform transform)
            {
                translation = transform.LocalPosition;
                rotation = transform.LocalRotation;
                scale = transform.LocalScale;
                return;
            }

            if (target is HPRoot root)
            {
                translation = root.RootUniversePosition;
                rotation = root.RootUniverseRotation;
                scale = new float3(1F);
                return;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Change the position and the orientation of the <paramref name="target"/>.
        /// <remarks>This will set the scaling to a uniform value of 1.</remarks>
        /// </summary>
        /// <param name="target">Set the transform values to this node.</param>
        /// <param name="translation">Change the <paramref name="target"/> position to this value.</param>
        /// <param name="rotation">Change the <paramref name="target"/> orientation to this value.</param>
        /// <exception cref="InvalidOperationException">If the <paramref name="target"/> will be disposed.</exception>
        /// <exception cref="NotSupportedException">
        /// If the <paramref name="target"/> is not supported by this class.
        /// Custom <see cref="HPNode"/> implementations are not supported.
        /// </exception>
        protected void SetTRS(HPNode target, double3 translation, quaternion rotation)
        {
            SetTRS(target, translation, rotation, new float3(1F));
        }

        /// <summary>
        /// Change the position, orientation and the scaling of the <paramref name="target"/>.
        /// </summary>
        /// <param name="target">Set the transform values to this node.</param>
        /// <param name="translation">Change the <paramref name="target"/> position to this value.</param>
        /// <param name="rotation">Change the <paramref name="target"/> orientation to this value.</param>
        /// <param name="scale">Change the <paramref name="target"/> size to this value.</param>
        /// <exception cref="InvalidOperationException">If the <paramref name="target"/> will be disposed.</exception>
        /// <exception cref="NotSupportedException">
        /// If the <paramref name="target"/> is not supported by this class.
        /// Custom <see cref="HPNode"/> implementations are not supported.
        /// </exception>
        protected void SetTRS(HPNode target, double3 translation, quaternion rotation, float3 scale)
        {
            if (target == null)
                throw new InvalidOperationException(
                    "Coordinate System Inspector cannot be instantiated with null values");

            if (target is HPTransform)
                SetTRSTransform(target, translation, rotation, scale);

            else if (target is HPRoot)
                SetTRSRoot(target, translation, rotation, scale);

            else
                throw new NotSupportedException();
        }

        /// <summary>
        /// Change the position, orientation and the scaling of the <paramref name="target"/>.
        /// </summary>
        /// <remarks><paramref name="target"/> must be a <see cref="HPTransform"/> instance.</remarks>
        /// <param name="target">Set the transform values to this node.</param>
        /// <param name="translation">Change the <paramref name="target"/> position to this value.</param>
        /// <param name="rotation">Change the <paramref name="target"/> orientation to this value.</param>
        /// <param name="scale">Change the <paramref name="target"/> size to this value.</param>
        private void SetTRSTransform(HPNode target, double3 translation, quaternion rotation, float3 scale)
        {
            HPTransform transform = target as HPTransform;

            Undo.RecordObject(transform, k_UndoString);
            Undo.RecordObject(transform.transform, k_UndoString);
            foreach (Transform child in transform.transform)
                Undo.RecordObject(child, k_UndoString);

            transform.LocalPosition = translation;
            transform.LocalRotation = rotation;
            transform.LocalScale = scale;

            EditorUtility.SetDirty(transform);
        }

        /// <summary>
        /// Change the position, orientation and the scaling of the <paramref name="target"/>.
        /// </summary>
        /// <remarks><paramref name="target"/> must be a <see cref="HPRoot"/> instance.</remarks>
        /// <param name="target">Set the transform values to this node.</param>
        /// <param name="translation">Change the <paramref name="target"/> position to this value.</param>
        /// <param name="rotation">Change the <paramref name="target"/> orientation to this value.</param>
        /// <param name="scale">Change the <paramref name="target"/> size to this value.</param>
        private void SetTRSRoot(HPNode target, double3 translation, quaternion rotation, float3 scale)
        {
            HPRoot root = target as HPRoot;

            Undo.RecordObject(root, k_UndoString);
            Undo.RecordObject(root.transform, k_UndoString);
            foreach (Transform child in root.transform)
                Undo.RecordObject(child, k_UndoString);

            root.SetRootTR(translation, rotation);

            EditorUtility.SetDirty(root);
        }
    }
}
