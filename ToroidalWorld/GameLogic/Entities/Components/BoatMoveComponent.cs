namespace ToroidalWorld.GameLogic.Physics.Components
{
    public sealed class BoatMoveComponent
    {
        public float WaterDrag { get; set; } = 0.995f;
        public float LateralFriction { get; set; } = 4f * 0.05f;
    }
}