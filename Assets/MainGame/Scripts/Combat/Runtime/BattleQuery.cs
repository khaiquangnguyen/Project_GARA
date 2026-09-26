using System.Collections.Generic;
using System.Linq;
using GARA.Characters;

namespace GARA.Combat
{
    // IBattleQuery adapter scoped to one participant, built on demand rather
    // than stored on BattleContext — keeps BattleContext free of "who's
    // asking" state.
    public class BattleQuery : IBattleQuery
    {
        private readonly BattleContext _battle;
        private readonly CombatParticipant _self;

        public BattleQuery(BattleContext battle, CombatParticipant self)
        {
            _battle = battle;
            _self = self;
        }

        public ICombatTarget Self => _self;

        public IEnumerable<ICombatTarget> Allies =>
            _battle.AllParticipants.Where(p => p != _self && p.Allegiance == _self.Allegiance);

        public IEnumerable<ICombatTarget> Enemies =>
            _battle.AllParticipants.Where(p => p.Allegiance != _self.Allegiance);

        public NoirWorld NoirWorld => _battle.NoirWorld;
    }
}
