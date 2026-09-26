namespace GARA.Characters
{
    // An effect a live skill step applies on its hit. Implemented in code
    // per effect; authored data is only what each implementation exposes.
    public interface ISkillEffect
    {
        void ApplyEffect(in SkillEffectContext context);
    }
}
