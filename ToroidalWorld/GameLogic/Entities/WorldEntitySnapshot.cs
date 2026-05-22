using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using ToroidalWorld.GameLogic.Entities.Components;

namespace ToroidalWorld.GameLogic.Entities
{
    public sealed class WorldEntitySnapshot
    {
        private readonly float _x;
        private readonly float _y;
        private readonly float _rotation;
        private readonly string _type;
        private readonly string _archetype;

        public WorldEntitySnapshot(Transform2 transform, WorldPersistentComponent persistent)
        {
            _x = transform.Position.X;
            _y = transform.Position.Y;
            _rotation = transform.Rotation;
            _type = persistent.Type.ToString();
            _archetype = persistent.Archetype;
        }

        public Transform2 GetTransform()
        {
            return new Transform2(new Vector2(_x, _y), _rotation);
        }

        public EntityType GetEntityType()
        {
            return Enum.Parse<EntityType>(_type);
        }

        public string GetArchetype()
        {
            return _archetype;
        }
    }
}