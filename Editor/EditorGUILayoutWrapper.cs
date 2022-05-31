
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Unity.Geospatial.HighPrecision.Editor
{
    /// <summary>
    /// UI library used to display the High Precision <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see> fields.
    /// </summary>
    public class EditorGUILayoutWrapper
    {
        /// <summary>
        /// Make a label with a foldout arrow to the left of it.
        /// This is useful for folder-like structures, where child objects only appear if you've unfolded the parent folder.
        /// This control cannot be nested in another BeginFoldoutHeaderGroup.
        /// To use multiple of these foldouts, you must end each method with <see cref="EndFoldoutHeaderGroup"/>.
        /// </summary>
        /// <param name="foldout">The shown foldout state.</param>
        /// <param name="content">The label to show.</param>
        /// <returns>The foldout state selected by the user.
        /// <see langword="true"/> if you should render sub-objects.;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public virtual bool BeginFoldoutHeaderGroup(bool foldout, string content)
        {
            return EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content);
        }

        /// <summary>
        /// Make a text field for entering a single double value.
        /// </summary>
        /// <param name="value">The value to edit.</param>
        /// <returns>The value entered by the user.</returns>
        private static void Double(ref double value)
        {
            value = EditorGUILayout.DoubleField(
                value,
                GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// Make a text field for entering 3 fields containing each a double value.
        /// </summary>
        /// <param name="label">Optional label to display in front of the fields.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Text to display in front of the first float field.</param>
        /// <param name="labelY">Text to display in front of the second float field.</param>
        /// <param name="labelZ">Text to display in front of the third float field.</param>
        /// <returns>The value entered by the user.</returns>
        public virtual double3 Double3Field(string label, double3 value, string labelX = "X", string labelY = "Y", string labelZ = "Z")
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(label, GUILayout.Width(150.0f));

            Label(labelX);
            Double(ref value.x);
            Label(labelY);
            Double(ref value.y);
            Label(labelZ);
            Double(ref value.z);

            EditorGUILayout.EndHorizontal();

            return value;
        }
        
        /// <summary>
        /// Closes a group started with <see cref="BeginFoldoutHeaderGroup"/>.
        /// </summary>
        public virtual void EndFoldoutHeaderGroup()
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Make a text field for entering a single float value.
        /// </summary>
        /// <param name="value">The value to edit.</param>
        /// <returns>The value entered by the user.</returns>
        private static void Float(ref float value)
        {
            value = EditorGUILayout.FloatField(
                value,
                GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// Make a text field for entering a single float value.
        /// </summary>
        /// <param name="label">Optional label to display in front of the fields.</param>
        /// <param name="value">The value to edit.</param>
        /// <returns>The value entered by the user.</returns>
        public virtual float Float1Field(string label, float value)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(label, GUILayout.Width(150.0f));

            Float(ref value);

            EditorGUILayout.EndHorizontal();

            return value;
        }

        /// <summary>
        /// Make a text field for entering 3 fields containing each a float value.
        /// </summary>
        /// <param name="label">Optional label to display in front of the fields.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="labelX">Text to display in front of the first float field.</param>
        /// <param name="labelY">Text to display in front of the second float field.</param>
        /// <param name="labelZ">Text to display in front of the third float field.</param>
        /// <returns>The value entered by the user.</returns>
        public virtual float3 Float3Field(string label, float3 value, string labelX = "X", string labelY = "Y", string labelZ = "Z")
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(label, GUILayout.Width(150.0f));

            Label(labelX);
            Float(ref value.x);
            Label(labelY);
            Float(ref value.y);
            Label(labelZ);
            Float(ref value.z);

            EditorGUILayout.EndHorizontal();

            return value;
        }

        /// <summary>
        /// Make a help box with a message to the user.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="type">The type of message.</param>
        /// <param name="wide">
        /// <see langword="true"/> the box will cover the whole width of the window;
        /// <see langword="false"/> otherwise it will cover the controls part only.
        /// </param>
        public virtual void HelpBox(string message, MessageType type, bool wide = true)
        {
            EditorGUILayout.HelpBox(message, type, wide);
        }

        /// <summary>
        /// Make an auto-layout label.
        /// Labels have no user interaction, do not catch mouse clicks and are always rendered in normal style. 
        /// </summary>
        /// <param name="str">Text to display on the label.</param>
        public virtual void Label(string str)
        {
            GUIContent labelContent = new GUIContent(str);
            float width = GUI.skin.GetStyle("Label").CalcSize(labelContent).x;
            GUILayout.Label(labelContent, GUILayout.Width(width));
        }

        /// <summary>
        /// Make a generic popup selection field.
        /// </summary>
        /// <param name="label">Optional label in front of the field.</param>
        /// <param name="selectedIndex">The index of the option the field shows.</param>
        /// <param name="displayedOptions">An array with the options shown in the popup.</param>
        /// <returns>
        /// The index of the option that has been selected by the user.
        /// </returns>
        public virtual int Popup(string label, int selectedIndex, string[] displayedOptions)
        {
            return EditorGUILayout.Popup(label, selectedIndex, displayedOptions);
        }
    }
}
