using GARA.ShakeBalance;
using UnityEditor;
using UnityEngine;

namespace GARA.EditorTools
{
    // Test only works in Play mode — the runner is ticked from the tester's
    // Update() and judged against live Input System presses.
    [CustomEditor(typeof(ShakeBalanceVisualDriverTester))]
    public class ShakeBalanceVisualDriverTesterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var tester = (ShakeBalanceVisualDriverTester)target;

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button(tester.IsRunning ? "Restart Test" : "Test"))
                {
                    tester.RunTest();

                    // Clicking this button leaves keyboard focus on the Inspector, and the Input
                    // System only feeds keyboard input to the game while the Game view is focused.
                    // Deferred so the Inspector finishes its GUI pass before focus moves.
                    EditorApplication.delayCall += FocusGameView;
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play mode to run a test.", MessageType.Info);
            }
        }

        private static void FocusGameView()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
    }
}
