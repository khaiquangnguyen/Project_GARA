using System;
using GARA.Input;
using UnityEngine;

namespace GARA.Countdowns
{
    /// <summary>
    /// Authoring asset for a countdown stack run: the shared resolve button, spawn timeline,
    /// and default spec for runtime-spawned countdowns.
    /// </summary>
    [CreateAssetMenu(menuName = "GARA/Countdowns/Countdown Stack", fileName = "CountdownStack")]
    public class CountdownStackDefinition : ScriptableObject
    {
        public InputToken button;
        public CountdownPriority priority = CountdownPriority.SoonestToExpire;
        public CountdownSpawn[] authoredSpawns = Array.Empty<CountdownSpawn>();
        public CountdownSpec defaultSpec;
        public bool runUntilStopped;
        public float tailOut = 0.25f;

        private void OnValidate()
        {
            Array.Sort(authoredSpawns, (a, b) => a.spawnTime.CompareTo(b.spawnTime));

            foreach (var spawn in authoredSpawns)
            {
                if (spawn.spec.perfectWindow > spawn.spec.duration)
                {
                    Debug.LogWarning($"CountdownStackDefinition '{name}': a spawn has perfectWindow ({spawn.spec.perfectWindow}) greater than duration ({spawn.spec.duration}).", this);
                }
            }

            if (authoredSpawns.Length == 0 && !runUntilStopped)
            {
                Debug.LogWarning($"CountdownStackDefinition '{name}': no authored spawns and runUntilStopped is false, so this run would never complete on its own.", this);
            }
        }
    }
}
