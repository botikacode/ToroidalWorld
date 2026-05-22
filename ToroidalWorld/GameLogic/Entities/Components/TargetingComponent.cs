namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class TargetingComponent
    {
        public float MaxRange { get; set; }

        public EntityFlags TargetFlagsMask { get; set; } = EntityFlags.None;

        public int TargetEntityId { get; set; } = -1;
    }
}