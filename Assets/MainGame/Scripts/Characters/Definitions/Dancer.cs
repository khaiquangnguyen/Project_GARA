namespace GARA.Characters
{
    // Dancer's own character definition. Subclassing CharacterDefinition
    // (rather than using it directly) gives this specific character a place
    // to grow unique personality/trait data and behavior overrides later,
    // without needing a generic "personality" system on the base class.
    public class Dancer : CharacterDefinition
    {
    }
}
