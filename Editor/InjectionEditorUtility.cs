using UnityEditor;
using UnityEngine;
using WhiteSparrow.Shared.DependencyInjection.Containers;

namespace WhiteSparrow.Shared.DependencyInjection
{
    public static class InjectionEditorUtility
    {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.ExitingEditMode)
            {
                DestroyAllInstances();
            }
            else if (stateChange == PlayModeStateChange.ExitingPlayMode)
            {
                DestroyAllInstances();
            }
        }

        private static void DestroyAllInstances()
        {
            var allInjectionContainers = Resources.FindObjectsOfTypeAll<InjectionContainer>();
            foreach (var container in allInjectionContainers)
            {
                container.Destroy();
            }
        }
    }
}