namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class WorldPersistentComponent
    {
        public WorldPersistentComponent(EntityType type, string archetype)
        {
            Type = type;
            Archetype = archetype;
        }

        public EntityType Type { get; }

        public string Archetype { get; }
    }
}