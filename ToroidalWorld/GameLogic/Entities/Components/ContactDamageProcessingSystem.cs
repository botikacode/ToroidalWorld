namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class DamageTakenCooldownComponent
    {
        public float RemainingSeconds { get; set; }

        public float CooldownSeconds { get; set; } = 0.25f;
    }
}