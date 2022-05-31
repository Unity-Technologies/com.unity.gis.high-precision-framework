
using UnityEditor;

namespace Unity.Geospatial.HighPrecision.Editor
{
    /// <summary>
    /// Class to display the <see cref="HPRoot"/> fields inside the Unity inspector.
    /// </summary>
    [CustomEditor(typeof(HPRoot))]
    public class HPRootInspector:
        HPNodeInspector
    {
    }
}
