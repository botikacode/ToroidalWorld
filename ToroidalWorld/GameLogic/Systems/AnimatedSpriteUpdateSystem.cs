using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameLogic.Systems
{
    public class AnimatedSpriteUpdateSystem : EntityUpdateSystem
    {
        private ComponentMapper<AnimatedSprite> _spriteMapper;

        public AnimatedSpriteUpdateSystem()
            : base(Aspect.All(typeof(AnimatedSprite)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _spriteMapper = mapperService.GetMapper<AnimatedSprite>();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var entityId in ActiveEntities)
            {
                var animatedSprite = _spriteMapper.Get(entityId);
                animatedSprite.Update(gameTime);
            }
        }
    }
}
