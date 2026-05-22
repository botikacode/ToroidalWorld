namespace ToroidalWorld.GameLogic.Entities.Definitions
{
    public sealed class EnemyDefinition
    {
        public string Name { get; set; }

        public string DeathVfx { get; set; }

        public int ColliderSize { get; set; }

        public float MoveSpeed { get; set; }

        public int Health { get; set; }

        public int Damage { get; set; }

        public int ExperienceDrop { get; set; }
    }
}