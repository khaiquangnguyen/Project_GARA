using GARA.Combat;
using UnityEditor;
using UnityEngine;

namespace GARA.EditorTools
{
    // Applies to every SpineAnimationState subclass (SpecialAState, IdleState,
    // ...) via editorForChildClasses. Preview only works while the target
    // is part of a live scene/prefab-editing context — its SkeletonAnimation
    // needs to actually be ticking (Spine components run ExecuteAlways).
    [CustomEditor(typeof(SpineAnimationState), true)]
    public class SpineAnimationStateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var state = (SpineAnimationState)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Preview Animation"))
            {
                state.PreviewAnimation();
            }
        }
    }
}
