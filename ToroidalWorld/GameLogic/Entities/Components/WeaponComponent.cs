namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class WeaponComponent
    {
        public WeaponComponent(string projectileArchetype, float cooldownSeconds, string shootSoundKey = null)
        {
            ProjectileArchetype = projectileArchetype;
            CooldownSeconds = cooldownSeconds;
            ShootSoundKey = shootSoundKey;
        }

        public string ProjectileArchetype { get; }

        public float RemainingSeconds { get; set; }

        public float CooldownSeconds { get; set; } = 0.5f;

        public string ShootSoundKey { get; }
    }
}