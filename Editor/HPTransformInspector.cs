
using UnityEngine;
using UnityEditor;

namespace Unity.Geospatial.HighPrecision.Editor
{
    /// <summary>
    /// Class to display the <see cref="HPTransform"/> fields inside the Unity inspector
    /// </summary>
    [CustomEditor(typeof(HPTransform))]
    public class HPTransformInspector :
        HPNodeInspector
    {
        /// <summary>
        /// Custom IMGUI based GUI for the inspector for a given <see cref="HPTransform"/>.
        /// </summary>
        /// <param name="target">Target to draw the inspector for.</param>
        internal override void OnInspectorGUI(HPNode target)
        {
            base.OnInspectorGUI(target);

            //
            //  TODO - Uses internal editor tools, may not be stable
            //
            HPTransform hpTransform = target as HPTransform;

            if (hpTransform is { })
            {
                switch (PrefabUtility.GetPrefabAssetType(hpTransform.gameObject))
                {
                    case PrefabAssetType.NotAPrefab:
                        MoveToTop(hpTransform);
                        break;

                    case PrefabAssetType.Regular:
                    case PrefabAssetType.Variant:
                    case PrefabAssetType.Model:
                    case PrefabAssetType.MissingAsset:
                        //
                        //  Intentionally left blank
                        //
                        break;
                }
            }

            Tools.hidden = !hpTransform.IsSceneEditable;
        }

        /// <summary>
        /// This function is called when the scriptable object goes out of scope.
        /// </summary>
        public void OnDisable()
        {
            Tools.hidden = false;
        }

        /// <summary>
        /// Make sure the given <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see> is
        /// after the first component
        /// (usually after the <see href="https://docs.unity3d.com/ScriptReference/Transform.html">Transform</see> component).
        /// </summary>
        /// <param name="component">Component to move.</param>
        private static void MoveToTop(Component component)
        {
            int lastIndex;
            int index = GetIndex(component);
            do
            {
                lastIndex = index;
                UnityEditorInternal.ComponentUtility.MoveComponentUp(component);
                index = GetIndex(component);
            } while (index != lastIndex && index > 1);
        }

        /// <summary>
        /// Get the actual display order index of the given
        /// <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see>.
        /// </summary>
        /// <param name="component">Item to get the index for.</param>
        /// <returns>The display order index.</returns>
        internal static int GetIndex(Component component)
        {
            return System.Array.IndexOf(component.GetComponents<Component>(), component);
        }
    }
}
