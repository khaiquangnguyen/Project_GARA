using System;

namespace GARA.Countdowns
{
    /// <summary>
    /// An authored countdown spawn, timed relative to the runner's Start().
    /// </summary>
    [Serializable]
    public struct CountdownSpawn
    {
        public float spawnTime;
        public CountdownSpec spec;
    }
}
