using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ToroidalWorld.GameLogic.Components
{
    public sealed class WaterWakeTrailComponent
    {
        public struct Stamp
        {
            public Vector2 Position;
            public float Rotation;
            public float AgeSeconds;
        }

        public Color Color { get; set; } = new Color(210, 245, 255);
        public float StartAlpha { get; set; } = 0.25f;
        public float EndAlpha { get; set; } = 0f;

        public Vector2 StartScale { get; set; } = new Vector2(0.2f, 0.8f);
        public Vector2 EndScale { get; set; } = new Vector2(1.4f, 0.55f);

        public float LifeSeconds { get; set; } = 0.15f;
        public float MinDistancePixels { get; set; } = 6f;
        public float BehindOffsetPixels { get; set; } = 10f;

        public int MaxStamps { get; set; } = 80;

        public List<Stamp> Stamps { get; } = new List<Stamp>(capacity: 96);

        public bool HasLastStampPosition { get; set; }
        public Vector2 LastStampPosition { get; set; }
    }
}