using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using ToroidalWorld.GameLogic.Entities.Components;
using ToroidalWorld.GameLogic.Physics.Components;

namespace ToroidalWorld.GameLogic.Systems
{
    public sealed class MovementAnimationSystem : EntityUpdateSystem
    {
        private ComponentMapper<AnimatedSprite> _spriteMapper;
        private ComponentMapper<MovementIntent> _intentMapper;
        private ComponentMapper<EntityFlagsComponent> _flagsMapper;

        public MovementAnimationSystem()
            : base(Aspect.All(typeof(AnimatedSprite), typeof(MovementIntent)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _spriteMapper = mapperService.GetMapper<AnimatedSprite>();
            _intentMapper = mapperService.GetMapper<MovementIntent>();
            _flagsMapper = mapperService.GetMapper<EntityFlagsComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                int entityId = ActiveEntities[i];

                var flags = _flagsMapper.Get(entityId);
                if (flags != null && (flags.Has(EntityFlags.Projectile) || flags.Has(EntityFlags.Pickup)))
                    continue;

                var sprite = _spriteMapper.Get(entityId);
                var intent = _intentMapper.Get(entityId);

                if (sprite == null || intent == null)
                    continue;

                bool isMoving = intent.Velocity.LengthSquared() > 0.001f;

                if (isMoving && sprite.CurrentAnimation == "Idle")
                    sprite.SetAnimation("Move");
                if (!isMoving && sprite.CurrentAnimation == "Move")
                    sprite.SetAnimation("Idle");
            }
        }
    }
}