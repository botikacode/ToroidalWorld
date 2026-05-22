using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using ToroidalWorld.GameLogic.Map;
using ToroidalWorld.GameLogic.Physics.Components;

namespace ToroidalWorld.GameLogic.Physics.Systems
{
    public class EntityCollisionSystem : EntityUpdateSystem
    {
        private readonly MapData _mapData;
        private readonly EntityCollisionEvents _collisionEvents;

        private ComponentMapper<MovementState> _stateMapper;
        private ComponentMapper<Transform2> _transformMapper;
        private ComponentMapper<EntityCollider> _colliderMapper;

        public EntityCollisionSystem(MapData mapData, EntityCollisionEvents collisionEvents)
            : base(Aspect.All(typeof(MovementState), typeof(EntityCollider)))
        {
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            _collisionEvents = collisionEvents ?? throw new ArgumentNullException(nameof(collisionEvents));
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _stateMapper = mapperService.GetMapper<MovementState>();
            _transformMapper = mapperService.GetMapper<Transform2>();
            _colliderMapper = mapperService.GetMapper<EntityCollider>();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var entityAId in ActiveEntities)
            {
                var stateA = _stateMapper.Get(entityAId);
                var colliderA = _colliderMapper.Get(entityAId);

                if (stateA == null || colliderA == null)
                    continue;

                Vector2 posA = stateA.ProposedPosition;
                float rotA = colliderA.Rotates ? stateA.ProposedRotation : 0f;

                Point chunkId = _mapData.GetChunkIdFromWorldCoords((int)posA.X, (int)posA.Y);

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Point neighborChunk = _mapData.WrapChunkCoordinates(chunkId.X + dx, chunkId.Y + dy);

                        foreach (int entityBId in _mapData.GetEntitiesInChunk(neighborChunk))
                        {
                            if (entityBId == entityAId)
                                continue;

                            var colliderB = _colliderMapper.Get(entityBId);
                            if (colliderB == null)
                                continue;

                            var stateB = _stateMapper.Get(entityBId);
                            bool bIsDynamic = stateB != null;

                            if (bIsDynamic && entityBId < entityAId)
                                continue;

                            if (!ShouldCollide(colliderA, colliderB))
                                continue;

                            if (!TryGetCollisionPose(entityBId, stateB, colliderB, out var posB, out var rotB))
                                continue;

                            if (!AreColliding(posA, rotA, colliderA, posB, rotB, colliderB))
                                continue;

                            _collisionEvents.EntityCollisions.Add(new EntityCollisionEvent(entityAId, entityBId));

                            if (colliderA.IsSolid && colliderB.IsSolid)
                            {
                                if (bIsDynamic)
                                    ResolveDynamicDynamic(stateA, colliderA, stateB, colliderB);
                                else
                                    ResolveDynamicStatic(stateA, colliderA, posB, colliderB);
                            }
                        }
                    }
                }
            }
        }

        private bool TryGetCollisionPose(int entityId, MovementState state, EntityCollider collider, out Vector2 position, out float rotation)
        {
            if (state != null)
            {
                position = state.ProposedPosition;
                rotation = collider.Rotates ? state.ProposedRotation : 0f;
                return true;
            }

            var transform = _transformMapper.Get(entityId);
            if (transform == null)
            {
                position = Vector2.Zero;
                rotation = 0f;
                return false;
            }

            position = transform.Position;
            rotation = collider.Rotates ? transform.Rotation : 0f;
            return true;
        }

        private static bool ShouldCollide(EntityCollider a, EntityCollider b)
        {
            return (a.CollidesWith & b.Layer) != 0;
        }

        private static bool AreColliding(
            Vector2 posA,
            float rotA,
            EntityCollider colliderA,
            Vector2 posB,
            float rotB,
            EntityCollider colliderB)
        {
            float halfAx = colliderA.Width * 0.5f;
            float halfAy = colliderA.Height * 0.5f;

            float halfBx = colliderB.Width * 0.5f;
            float halfBy = colliderB.Height * 0.5f;

            if (!colliderA.Rotates && !colliderB.Rotates)
            {
                float minAx = posA.X - halfAx;
                float maxAx = posA.X + halfAx;
                float minAy = posA.Y - halfAy;
                float maxAy = posA.Y + halfAy;

                float minBx = posB.X - halfBx;
                float maxBx = posB.X + halfBx;
                float minBy = posB.Y - halfBy;
                float maxBy = posB.Y + halfBy;

                return !(maxAx <= minBx || minAx >= maxBx || maxAy <= minBy || minAy >= maxBy);
            }

            return IntersectsOrientedRectangles(
                centerA: posA, rotationA: rotA, halfAx: halfAx, halfAy: halfAy, rotatesA: colliderA.Rotates,
                centerB: posB, rotationB: rotB, halfBx: halfBx, halfBy: halfBy, rotatesB: colliderB.Rotates);
        }

        private static bool IntersectsOrientedRectangles(
            Vector2 centerA, float rotationA, float halfAx, float halfAy, bool rotatesA,
            Vector2 centerB, float rotationB, float halfBx, float halfBy, bool rotatesB)
        {
            var d = centerB - centerA;

            GetAxes(rotationA, rotatesA, out var aRight, out var aUp);
            GetAxes(rotationB, rotatesB, out var bRight, out var bUp);

            if (IsSeparated(d, aRight, aRight, aUp, halfAx, halfAy, bRight, bUp, halfBx, halfBy)) return false;
            if (IsSeparated(d, aUp, aRight, aUp, halfAx, halfAy, bRight, bUp, halfBx, halfBy)) return false;
            if (IsSeparated(d, bRight, aRight, aUp, halfAx, halfAy, bRight, bUp, halfBx, halfBy)) return false;
            if (IsSeparated(d, bUp, aRight, aUp, halfAx, halfAy, bRight, bUp, halfBx, halfBy)) return false;

            return true;
        }

        private static bool IsSeparated(
            Vector2 centerDelta,
            Vector2 axis,
            Vector2 aRight, Vector2 aUp, float halfAx, float halfAy,
            Vector2 bRight, Vector2 bUp, float halfBx, float halfBy)
        {
            float dist = MathF.Abs(Vector2.Dot(centerDelta, axis));

            float ra = (halfAx * MathF.Abs(Vector2.Dot(aRight, axis))) +
                       (halfAy * MathF.Abs(Vector2.Dot(aUp, axis)));

            float rb = (halfBx * MathF.Abs(Vector2.Dot(bRight, axis))) +
                       (halfBy * MathF.Abs(Vector2.Dot(bUp, axis)));

            return dist > (ra + rb);
        }

        private static void GetAxes(float rotation, bool rotates, out Vector2 right, out Vector2 up)
        {
            if (!rotates)
            {
                right = Vector2.UnitX;
                up = Vector2.UnitY;
                return;
            }

            up = new Vector2(
                (float)Math.Cos(rotation - MathHelper.PiOver2),
                (float)Math.Sin(rotation - MathHelper.PiOver2));

            right = new Vector2(-up.Y, up.X);
        }

        private static void ResolveDynamicDynamic(
            MovementState stateA,
            EntityCollider colliderA,
            MovementState stateB,
            EntityCollider colliderB)
        {
            ResolveEntityCollision(stateA, colliderA, stateB, colliderB);
        }

        private static void ResolveDynamicStatic(
            MovementState dynamicState,
            EntityCollider dynamicCollider,
            Vector2 staticPos,
            EntityCollider staticCollider)
        {
            Vector2 posA = dynamicState.ProposedPosition;
            Vector2 posB = staticPos;

            float halfAx = dynamicCollider.Width * 0.5f;
            float halfAy = dynamicCollider.Height * 0.5f;

            float halfBx = staticCollider.Width * 0.5f;
            float halfBy = staticCollider.Height * 0.5f;

            float centerAx = posA.X;
            float centerAy = posA.Y;
            float centerBx = posB.X;
            float centerBy = posB.Y;

            float penX = (halfAx + halfBx) - Math.Abs(centerBx - centerAx);
            float penY = (halfAy + halfBy) - Math.Abs(centerBy - centerAy);

            if (penX <= 0f || penY <= 0f)
                return;

            if (penX < penY)
            {
                float direction = Math.Sign(centerBx - centerAx);
                if (direction == 0f)
                    direction = 1f;

                float moveX = penX * direction;

                dynamicState.ProposedPosition.X -= moveX;
                dynamicState.Velocity.X = 0f;
            }
            else
            {
                float direction = Math.Sign(centerBy - centerAy);
                if (direction == 0f)
                    direction = 1f;

                float moveY = penY * direction;

                dynamicState.ProposedPosition.Y -= moveY;
                dynamicState.Velocity.Y = 0f;
            }
        }

        private static void ResolveEntityCollision(
            MovementState stateA,
            EntityCollider colliderA,
            MovementState stateB,
            EntityCollider colliderB)
        {
            // Resolución aproximada por ejes (igual que antes, pero usando Width/Height)
            Vector2 posA = stateA.ProposedPosition;
            Vector2 posB = stateB.ProposedPosition;

            float halfAx = colliderA.Width * 0.5f;
            float halfAy = colliderA.Height * 0.5f;

            float halfBx = colliderB.Width * 0.5f;
            float halfBy = colliderB.Height * 0.5f;

            float centerAx = posA.X;
            float centerAy = posA.Y;
            float centerBx = posB.X;
            float centerBy = posB.Y;

            float penX = (halfAx + halfBx) - Math.Abs(centerBx - centerAx);
            float penY = (halfAy + halfBy) - Math.Abs(centerBy - centerAy);

            if (penX <= 0f || penY <= 0f)
                return;

            if (penX < penY)
            {
                float direction = Math.Sign(centerBx - centerAx);
                if (direction == 0f)
                    direction = 1f;

                float moveX = penX * 0.5f * direction;

                stateA.ProposedPosition.X -= moveX;
                stateB.ProposedPosition.X += moveX;

                stateA.Velocity.X = 0f;
                stateB.Velocity.X = 0f;
            }
            else
            {
                float direction = Math.Sign(centerBy - centerAy);
                if (direction == 0f)
                    direction = 1f;

                float moveY = penY * 0.5f * direction;

                stateA.ProposedPosition.Y -= moveY;
                stateB.ProposedPosition.Y += moveY;

                stateA.Velocity.Y = 0f;
                stateB.Velocity.Y = 0f;
            }
        }
    }
}