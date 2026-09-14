using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace GARA.EditorTools
{
    // Thin wrapper so callers that shouldn't/can't reference Spine.Unity
    // directly (e.g. a dynamically-compiled tooling script with a limited
    // assembly reference set) can still build a skeleton GameObject by path.
    public static class SpineGameObjectFactory
    {
        public static GameObject CreateSkeletonAnimationGameObject(string skeletonDataAssetPath)
        {
            var skeletonData = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(skeletonDataAssetPath);
            if (skeletonData == null)
            {
                return null;
            }

            var components = SkeletonAnimation.NewSkeletonAnimationGameObject(skeletonData);
            return components.skeletonAnimation.gameObject;
        }

        // Re-points every GARA.Combat.SpineAnimationState under root at the
        // SkeletonRenderer found anywhere under root — used to backfill
        // skeletonRendererRef after it was added as a field, or after a
        // state got reparented away from the renderer's GameObject.
        public static int BackfillSkeletonRendererRef(GameObject root)
        {
            var skeletonRenderer = root.GetComponentInChildren<SkeletonRenderer>();
            var states = root.GetComponentsInChildren<GARA.Combat.SpineAnimationState>(true);

            var updated = 0;
            foreach (var state in states)
            {
                var so = new SerializedObject(state);
                var prop = so.FindProperty("skeletonRendererRef");
                if (prop.objectReferenceValue == skeletonRenderer)
                {
                    continue;
                }

                prop.objectReferenceValue = skeletonRenderer;
                so.ApplyModifiedPropertiesWithoutUndo();
                updated++;
            }

            return updated;
        }
    }
}
