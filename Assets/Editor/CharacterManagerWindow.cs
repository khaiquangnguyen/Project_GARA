using GARA.Characters;
using GARA.Combat;
using UnityEditor;
using UnityEngine;

namespace GARA.EditorTools
{
    // Lists every CharacterState under a chosen character (prefab or scene
    // instance), with a Preview button for the ones that can preview
    // themselves (currently: SpineAnimationState subclasses).
    public class CharacterManagerWindow : EditorWindow
    {
        private CharacterDefinition _character;
        private Vector2 _scroll;

        [MenuItem("GARA/Characters/Character Manager")]
        public static void Open()
        {
            GetWindow<CharacterManagerWindow>("Character Manager");
        }

        public static void OpenFor(CharacterDefinition character)
        {
            var window = GetWindow<CharacterManagerWindow>("Character Manager");
            window._character = character;
            window.Repaint();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (_character != null || Selection.activeGameObject == null)
            {
                return;
            }

            var found = Selection.activeGameObject.GetComponentInParent<CharacterDefinition>();
            if (found != null)
            {
                _character = found;
                Repaint();
            }
        }

        private void OnGUI()
        {
            _character = (CharacterDefinition)EditorGUILayout.ObjectField(
                "Character", _character, typeof(CharacterDefinition), true);

            if (_character == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a character (prefab or scene instance with a CharacterDefinition) to see its states.",
                    MessageType.Info);
                return;
            }

            var states = _character.GetComponentsInChildren<CharacterState>(true);
            EditorGUILayout.LabelField($"States ({states.Length})", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var state in states)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.ObjectField(state, typeof(CharacterState), true);

                using (new EditorGUI.DisabledScope(state is not SpineAnimationState))
                {
                    if (GUILayout.Button("Preview", GUILayout.Width(80)))
                    {
                        ((SpineAnimationState)state).PreviewAnimation();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
