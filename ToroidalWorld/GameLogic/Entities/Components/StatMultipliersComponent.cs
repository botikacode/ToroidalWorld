namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class StatMultipliersComponent
    {
        public float DamageMultiplier { get; set; } = 1f;

        public float AreaDamageMultiplier { get; set; } = 1f;

        public float AreaRadiusMultiplier { get; set; } = 1f;

        public float MoveSpeedMultiplier { get; set; } = 1f;
        public float IcebreakerRateMultiplier { get; set; } = 1f;

        public float IcebreakerStrengthMultiplier { get; set; } = 1f;

        public float ExperienceGainMultiplier { get; set; } = 1f;

        public float ExperienceMagnetRangeMultiplier { get; set; } = 1f;

        public float TurretAimSpeedMultiplier { get; set; } = 1f;

        public float TurretCooldownRateMultiplier { get; set; } = 1f;

        public float TurretRangeMultiplier { get; set; } = 1f;
    }
}