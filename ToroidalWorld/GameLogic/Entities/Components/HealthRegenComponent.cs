namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class HealthRegenComponent
    {
        public float RegenPerSecond { get; set; }

        public float AccumulatedHealing { get; set; }
    }
}