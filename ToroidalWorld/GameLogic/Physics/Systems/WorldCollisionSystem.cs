using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics.Components;

namespace ToroidalWorld.GameLogic.Physics.Systems
{
    public class WorldCollisionSystem : EntityProcessingSystem
    {
        private readonly MapData _mapData;
        private readonly WorldCollisionEvents _worldCollisionEvents;

        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<TerrainCollider> _colliderMapper;
        private ComponentMapper<MovementState> _stateMapper;

        public WorldCollisionSystem(MapData mapData, WorldCollisionEvents worldCollisionEvents)
            : base(Aspect.All(typeof(Transform2), typeof(TerrainCollider), typeof(MovementState)))
        {
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            _worldCollisionEvents = worldCollisionEvents ?? throw new ArgumentNullException(nameof(worldCollisionEvents));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _transformMapper = mapperService.GetMapper<Transform2>();
            _colliderMapper = mapperService.GetMapper<TerrainCollider>();
            _stateMapper = mapperService.GetMapper<MovementState>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = _transformMapper.Get(entityId);
            var collider = _colliderMapper.Get(entityId);
            var state = _stateMapper.Get(entityId);

            if (transform == null || collider == null || state == null)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 proposedMovement = state.Velocity * dt;
            Vector2 newPosition = transform.Position;

            float halfW = collider.Width * 0.5f;
            float halfH = collider.Height * 0.5f;

            if (!collider.Rotates)
            {
                var boundsX = new Rectangle(
                    (int)(newPosition.X + proposedMovement.X - halfW),
                    (int)(newPosition.Y - halfH),
                    collider.Width,
                    collider.Height);

                if (!TryGetCollisionTile(boundsX, out var hitTileX))
                {
                    newPosition.X += proposedMovement.X;
                }
                else
                {
                    _worldCollisionEvents.WorldCollisions.Add(new WorldCollisionEvent(entityId, hitTileX));
                    state.Velocity.X = 0f;
                }

                var boundsY = new Rectangle(
                    (int)(newPosition.X - halfW),
                    (int)(newPosition.Y + proposedMovement.Y - halfH),
                    collider.Width,
                    collider.Height);

                if (!TryGetCollisionTile(boundsY, out var hitTileY))
                {
                    newPosition.Y += proposedMovement.Y;
                }
                else
                {
                    _worldCollisionEvents.WorldCollisions.Add(new WorldCollisionEvent(entityId, hitTileY));
                    state.Velocity.Y = 0f;
                }

                state.ProposedPosition = newPosition;
                return;
            }

            // Rotating: resolve with “sliding” by projecting the movement against the tile
            float rotation = state.ProposedRotation;

            Vector2 oldPos = transform.Position;
            Vector2 desiredPos = oldPos + proposedMovement;

            if (!TryGetCollisionTileRotated(desiredPos, rotation, halfW, halfH, out var hitTile))
            {
                state.ProposedPosition = desiredPos;
                return;
            }

            _worldCollisionEvents.WorldCollisions.Add(new WorldCollisionEvent(entityId, hitTile));

            if (proposedMovement.LengthSquared() < 0.000001f)
            {
                state.ProposedPosition = oldPos;
                state.Velocity = Vector2.Zero;
                return;
            }

            Vector2 slideMove = ProjectMoveToSlide(oldPos, proposedMovement, hitTile);
            Vector2 slidePos = oldPos + slideMove;

            if (!TryGetCollisionTileRotated(slidePos, rotation, halfW, halfH, out _))
            {
                state.ProposedPosition = slidePos;
                state.Velocity = slideMove / Math.Max(dt, 0.0001f);
                return;
            }

            // Fallback to half movement, to allow sliding through narrow gaps
            slideMove *= 0.5f;
            slidePos = oldPos + slideMove;

            if (!TryGetCollisionTileRotated(slidePos, rotation, halfW, halfH, out _))
            {
                state.ProposedPosition = slidePos;
                state.Velocity = slideMove / Math.Max(dt, 0.0001f);
                return;
            }

            state.ProposedPosition = oldPos;
            state.Velocity = Vector2.Zero;
        }

        private Vector2 ProjectMoveToSlide(Vector2 entityPos, Vector2 move, Point hitTile)
        {
            int pixelSize = MapData.PixelSize;

            var tileCenter = new Vector2(
                (hitTile.X * pixelSize) + (pixelSize * 0.5f),
                (hitTile.Y * pixelSize) + (pixelSize * 0.5f));

            Vector2 normal = entityPos - tileCenter;

            float lenSq = normal.LengthSquared();
            if (lenSq < 0.0001f)
                normal = -Vector2.Normalize(move);
            else
                normal /= (float)Math.Sqrt(lenSq);

            float vn = Vector2.Dot(move, normal);

            // If moving towards the tile (vn < 0), nullify that component and keep the tangential
            if (vn < 0f)
                return move - (vn * normal);

            return move;
        }

        private Point WrapHitTile(int tileX, int tileY)
        {
            var wrappedWorldPx = _mapData.WrapWorldCoordinates(tileX, tileY);
            return new Point(wrappedWorldPx.X / MapData.PixelSize, wrappedWorldPx.Y / MapData.PixelSize);
        }

        private bool TryGetCollisionTile(Rectangle bounds, out Point hitMapPos)
        {
            int pixelSize = MapData.PixelSize;

            int minTileX = bounds.Left / pixelSize;
            int maxTileX = (bounds.Right - 1) / pixelSize;
            int minTileY = bounds.Top / pixelSize;
            int maxTileY = (bounds.Bottom - 1) / pixelSize;

            for (int y = minTileY; y <= maxTileY; y++)
            {
                for (int x = minTileX; x <= maxTileX; x++)
                {
                    if (!_mapData.IsSolidTile(x, y))
                        continue;

                    hitMapPos = WrapHitTile(x, y);
                    return true;
                }
            }

            hitMapPos = default;
            return false;
        }

        private bool TryGetCollisionTileRotated(Vector2 center, float rotation, float halfW, float halfH, out Point hitMapPos)
        {
            GetAxes(rotation, out var right, out var up);

            var p0 = center + (right * halfW) + (up * halfH);
            var p1 = center + (right * halfW) - (up * halfH);
            var p2 = center - (right * halfW) + (up * halfH);
            var p3 = center - (right * halfW) - (up * halfH);

            var p4 = center + (right * halfW);
            var p5 = center - (right * halfW);
            var p6 = center + (up * halfH);
            var p7 = center - (up * halfH);

            if (TryHitSolidTile(p0, out hitMapPos)) return true;
            if (TryHitSolidTile(p1, out hitMapPos)) return true;
            if (TryHitSolidTile(p2, out hitMapPos)) return true;
            if (TryHitSolidTile(p3, out hitMapPos)) return true;
            if (TryHitSolidTile(p4, out hitMapPos)) return true;
            if (TryHitSolidTile(p5, out hitMapPos)) return true;
            if (TryHitSolidTile(p6, out hitMapPos)) return true;
            if (TryHitSolidTile(p7, out hitMapPos)) return true;
            if (TryHitSolidTile(center, out hitMapPos)) return true;

            hitMapPos = default;
            return false;
        }

        private bool TryHitSolidTile(Vector2 worldPos, out Point hitMapPos)
        {
            int pixelSize = MapData.PixelSize;

            int tileX = (int)MathF.Floor(worldPos.X / pixelSize);
            int tileY = (int)MathF.Floor(worldPos.Y / pixelSize);

            if (_mapData.IsSolidTile(tileX, tileY))
            {
                hitMapPos = WrapHitTile(tileX, tileY);
                return true;
            }

            hitMapPos = default;
            return false;
        }

        private static void GetAxes(float rotation, out Vector2 right, out Vector2 up)
        {
            up = new Vector2(
                (float)Math.Cos(rotation - MathHelper.PiOver2),
                (float)Math.Sin(rotation - MathHelper.PiOver2));

            right = new Vector2(-up.Y, up.X);
        }
    }
}