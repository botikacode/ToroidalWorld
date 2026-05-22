using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameLogic.Physics.Components
{
    public class MovementState
    {
        public Vector2 OldPosition;
        public Vector2 ProposedPosition;
        public Vector2 Velocity;
        public float ProposedRotation;

        public MovementState()
        {
            OldPosition = Vector2.Zero;
            ProposedPosition = Vector2.Zero;
            Velocity = Vector2.Zero;
            ProposedRotation = 0f;
        }

        public MovementState(Transform2 transform)
        {
            OldPosition = transform.Position;
            ProposedPosition = transform.Position;
            Velocity = Vector2.Zero;
            ProposedRotation = transform.Rotation;
        }

        public MovementState(Vector2 oldPosition, Vector2 proposedPosition, Vector2 velocity, float proposedRotation)
        {
            OldPosition = oldPosition;
            ProposedPosition = proposedPosition;
            Velocity = velocity;
            ProposedRotation = proposedRotation;
        }
    }
}
