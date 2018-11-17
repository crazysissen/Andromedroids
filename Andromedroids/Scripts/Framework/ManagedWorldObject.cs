using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Andromedroids
{
    class ManagedWorldObject
    {
        public float Rotation { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }

        public void SetPosition(HashKey key, Vector2 position)
        {
            if (key.Validate("ManagedWorldObject.SetPosition"))
            {
                Position = position;
            }
        }

        public void SetVelocity(HashKey key, Vector2 velocity)
        {
            if (key.Validate("ManagedWorldObject.SetVelocity"))
            {
                Position = velocity;
            }
        }

        public void SetRotation(HashKey key, float rotation)
        {
            if (key.Validate("ManagedWorldObject.SetRotation"))
            {
                Rotation = rotation;
            }
        }
    }
}
