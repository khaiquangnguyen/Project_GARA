using GARA.Combat;
using UnityEditor;
using UnityEngine;

namespace GARA.EditorTools
{
    [CustomEditor(typeof(CombatSceneManager))]
    public class CombatSceneManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Open Combat Scene Development"))
            {
                CombatSceneDevelopmentWindow.Open();
            }

            // Play mode only: the view shake's display is switched on at runtime.
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                // Uses the Combat Scene Development window's View Shake settings.
                if (GUILayout.Button("Test View Shake"))
                {
                    CombatSceneDevelopmentWindow.TestViewShake();
                }
            }
        }
    }
}
