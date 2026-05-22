namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class HealthComponent
    {
        public HealthComponent(int maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public HealthComponent(int maxHealth, int currentHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth > maxHealth ? maxHealth : currentHealth;
        }

        public int MaxHealth { get; set; }

        public int CurrentHealth { get; set; }

        public bool IsDead => CurrentHealth <= 0;
    }
}