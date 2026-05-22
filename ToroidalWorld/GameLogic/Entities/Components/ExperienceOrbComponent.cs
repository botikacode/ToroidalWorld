namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class ExperienceOrbComponent
    {
        public int Amount { get; set; }

        public ExperienceOrbComponent()
        {
        }

        public ExperienceOrbComponent(int amount)
        {
            Amount = amount;
        }
    }
}