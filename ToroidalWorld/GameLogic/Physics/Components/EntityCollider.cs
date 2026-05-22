using System;
using Microsoft.Xna.Framework;

namespace ToroidalWorld.GameLogic.Physics.Components
{
    public class EntityCollider
    {
        public int Height;
        public int Width;
        public CollisionLayer Layer;
        public CollisionLayer CollidesWith;
        public bool IsSolid;

        public bool Rotates;

        public EntityCollider(int size, CollisionLayer layer, CollisionLayer collidesWith, bool isSolid, bool rotates = false)
        {
            Height = size;
            Width = size;
            Layer = layer;
            CollidesWith = collidesWith;
            IsSolid = isSolid;
            Rotates = rotates;
        }
        public EntityCollider(int height, int width, CollisionLayer layer, CollisionLayer collidesWith, bool isSolid, bool rotates = false)
        {
            Height = height;
            Width = width;
            Layer = layer;
            CollidesWith = collidesWith;
            IsSolid = isSolid;
            Rotates = rotates;
        }
    }
}
