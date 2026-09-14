using GARA.Characters;
using UnityEditor;
using UnityEngine;

namespace GARA.EditorTools
{
    // Applies to CharacterDefinition and every subclass (e.g. Dancer) via
    // editorForChildClasses.
    [CustomEditor(typeof(CharacterDefinition), true)]
    public class CharacterDefinitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Open Character Manager"))
            {
                CharacterManagerWindow.OpenFor((CharacterDefinition)target);
            }
        }
    }
}
