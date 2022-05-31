using System;
using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace Unity.Geospatial.HighPrecision.Editor
{
    /// <summary>
    /// Change the default display of the
    /// <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> within the Unity inspector.
    /// </summary>
    [CustomEditor(typeof(Transform), true)]
    [CanEditMultipleObjects]
    public class TransformEditorOverride : UnityEditor.Editor
    {
        /// <summary>
        /// The factory <see href="https://docs.unity3d.com/ScriptReference/Editor.html">Editor</see> allowing
        /// to call the none overwritten methods.
        /// </summary>
        private UnityEditor.Editor m_DefaultEditor;

        /// <summary>
        /// <see langword="true"/> if the <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> group is expanded;
        /// <see langword="false"/> it is collapsed.
        /// </summary>
        private bool m_EnableEdit;

        /// <summary>
        /// Editor wrapper allowing to draw the widgets.
        /// This wrapper allow to execute Unit Tests via Moq.
        /// </summary>
        internal EditorGUILayoutWrapper Wrapper;

        /// <summary>
        /// This function is called when the object is loaded.
        /// </summary>
        private void OnEnable()
        {
            m_DefaultEditor = CreateEditor(targets, Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
        }

        /// <summary>
        /// This function is called when the scriptable object goes out of scope.
        /// </summary>
        private void OnDisable()
        {
            //
            //  TODO - Not sure this is necessary. (The destroy part yes, but not the disable method)
            //
            MethodInfo disableMethod = m_DefaultEditor == null
                ? null
                : m_DefaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (disableMethod != null)
                disableMethod.Invoke(m_DefaultEditor, null);

            DestroyImmediate(m_DefaultEditor);
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            Wrapper = new EditorGUILayoutWrapper();

            OnInspectorGUI(target as Transform);
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector for a given transform.
        /// </summary>
        /// <param name="target">Target to draw the inspector for.</param>
        internal virtual void OnInspectorGUI(Transform target)
        {
            HPTransform hpTransform = target == null ? null : target.GetComponent<HPTransform>();

            if (hpTransform == null)
                DrawDefault();

            else if (hpTransform.IsSceneEditable)
                DrawHighPrecision();
        }

        /// <summary>
        /// If no <see cref="HPTransform"/> is associated with the node, execute the factory method.
        /// </summary>
        private void DrawDefault()
        {
            if (m_DefaultEditor != null)
                m_DefaultEditor.OnInspectorGUI();
        }

        /// <summary>
        /// If a <see cref="HPTransform"/> is associated with the node, warn the user to use the <see cref="HPTransform"/>
        /// component instead of the <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> one.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the foldout is expanded;
        /// <see langword="false"/> if the foldout is collapsed.
        /// </returns>
        private bool DrawHighPrecision()
        {
            m_EnableEdit = Wrapper.BeginFoldoutHeaderGroup(m_EnableEdit, "Edit Transform");

            if (m_EnableEdit)
            {
                Wrapper.HelpBox("For most applications using the HP Transform, you should not be editing the Transform component.", MessageType.Warning);

                DrawDefault();
            }

            return m_EnableEdit;
        }
    }
}
