namespace ToroidalWorld.GameLogic.Session
{
    public sealed class TurretPickupSelectionState
    {
        public int PickupEntityId { get; set; } = -1;

        public string TurretName { get; set; }
    }
}