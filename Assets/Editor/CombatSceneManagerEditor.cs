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
        }
    }
}
