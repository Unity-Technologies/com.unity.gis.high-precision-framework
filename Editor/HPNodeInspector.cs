
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace Unity.Geospatial.HighPrecision.Editor
{
    //
    //  TODO - Support Multi Object Editing
    //

    /// <summary>
    /// Base class with methods allowing to display the coordinate fields in the Unity Inspector.
    /// </summary>
    public abstract class HPNodeInspector:
        UnityEditor.Editor
    {
        /// <summary>
        /// Name of the <see href="https://docs.unity3d.com/ScriptReference/EditorPrefs.html">EditorPrefs</see> used
        /// to get the available <see cref="LocalCoordinateSystem"/>.
        /// </summary>
        internal const string CoordinateSystemPreference = "Unity.HighPrecision.CoordinateSystem";

        /// <summary>
        /// Each <see cref="CoordinateSystemInspector"/> instantiated for each inspected <see cref="HPNode"/>.
        /// </summary>
        private static List<System.Func<HPNode, CoordinateSystemInspector>> s_CoordinateSystemConstructors;

        /// <summary>
        /// List of all the instantiated <see cref="CoordinateSystemInspector"/>.
        /// </summary>
        private List<CoordinateSystemInspector> m_Inspectors;

        /// <summary>
        /// Editor wrapper allowing to draw the widgets.
        /// This wrapper allow to execute Unit Tests via Moq.
        /// </summary>
        internal EditorGUILayoutWrapper GUILayoutWrapper;

        /// <summary>
        /// This function is called when the object is loaded.
        /// </summary>
        public virtual void OnEnable()
        {
            if (s_CoordinateSystemConstructors is null)
            {
                //
                //  TODO - Don't look through every assembly, might get really long
                //
                System.Type[] constructorTypes = { };
                s_CoordinateSystemConstructors = System.AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(type => type.IsPublic)
                    .Where(type => type.IsSubclassOf(typeof(CoordinateSystemInspector)))
                    .Select(type => type.GetConstructor(constructorTypes))
                    .Select<ConstructorInfo, System.Func<HPNode, CoordinateSystemInspector>>(c =>
                    {
                        return target =>
                        {
                            if (c == null)
                                return null;

                            CoordinateSystemInspector inspector = (CoordinateSystemInspector)c.Invoke(null);
                            return inspector;
                        };
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUILayoutWrapper = new EditorGUILayoutWrapper();

            OnInspectorGUI(target as HPNode);
        }

        /// <summary>
        /// Custom IMGUI based GUI for the inspector for a given <see cref="HPNode"/>.
        /// </summary>
        /// <param name="target">Target to draw the inspector for.</param>
        internal virtual void OnInspectorGUI(HPNode target)
        {
            int index = CoordinateSystemPopup(target);

            CoordinateSystemInspector inspector = m_Inspectors[index];

            inspector.GUILayoutWrapper = GUILayoutWrapper;
            inspector.OnInspectorGUI(target);
        }

        private void PopulateCoordinateSystemInspectors(HPNode target)
        {
            m_Inspectors = s_CoordinateSystemConstructors
                .Select(c => c.Invoke(target))
                .OrderByDescending(i => i.Name == DefaultCoordinateSystemInspector.DefaultName)
                .ThenBy(i => i.Name)
                .ToList();
        }

        internal int CoordinateSystemPopup(HPNode target)
        {
            PopulateCoordinateSystemInspectors(target);

            int oldCoordinateSystemIndex = EditorPrefs.GetInt(CoordinateSystemPreference);
            int coordinateSystemIndex = GUILayoutWrapper.Popup(
                "Coordinate System",
                oldCoordinateSystemIndex,
                m_Inspectors.Select(i => i.Name).ToArray());

            if (oldCoordinateSystemIndex != coordinateSystemIndex)
                EditorPrefs.SetInt(CoordinateSystemPreference, coordinateSystemIndex);

            return coordinateSystemIndex;
        }
    }
}
