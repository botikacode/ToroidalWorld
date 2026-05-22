using Microsoft.Xna.Framework;

namespace ToroidalWorld.GameLogic.Components
{
    public sealed class ChildOfComponent
    {
        public ChildOfComponent(int parentEntityId, Vector2 localOffset, float localRotation = 0f)
        {
            ParentEntityId = parentEntityId;
            LocalOffset = localOffset;
            LocalRotation = localRotation;
        }

        public int ParentEntityId { get; }
        public Vector2 LocalOffset { get; set; }
        public float LocalRotation { get; set; }
    }
}