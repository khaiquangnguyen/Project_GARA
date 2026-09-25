using System.Collections.Generic;

namespace GARA.Characters.Gambler
{
    // Mutable per-resolution state a GambleRigDefinition can tweak before a
    // GamblingGameDefinition rolls (BeforeResolve), and the same knobs a game
    // reads while rolling.
    public sealed class GambleContext
    {
        public float Manipulate;
        public bool ConsolationOnFail;
        public Dictionary<string, float> Knobs = new Dictionary<string, float>();
    }
}
