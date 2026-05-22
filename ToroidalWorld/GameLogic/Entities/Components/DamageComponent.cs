namespace ToroidalWorld.GameLogic.Entities.Components
{
    public sealed class DamageComponent
    {
        public DamageComponent(int amount)
        {
            Damage = amount;
        }

        public int Damage { get; set; }

        public float AreaRadius { get; set; }

        public int AreaDamage { get; set; }

        public int TerrainExplosionRadiusInTiles { get; set; }
    }
}