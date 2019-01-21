using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Andromedroids
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed class ShipAI : Attribute
    {
        public string MenuName { get; private set; }
        public bool Quickstart { get; private set; }

        public ShipAI(string menuName, bool quickstart = false)
        {
            MenuName = menuName.Length > 12 ? menuName.Substring(0, 12) : menuName;
            Quickstart = quickstart;
        }
    }

    /// <summary>Contains the player itself, and a bunch of information to be used in an AI.
    /// Remmber that changing these will bear no effect.</summary>
    public abstract class ShipPlayer : ManagedWorldObject
    {
        public const float
            HITRADIUS = 1.0f;

        public int OpponentHealth { get; set; }
        public float OpponentShield { get; set; }
        public Vector2 OpponentPosition { get; set; }
        public float OpponentRotation { get; set; }
        public Vector2 OpponentVelocity { get; set; }
        public WeaponType[] OpponentWeapons { get; set; }
        /// <summary>Opponent's currently active powerups</summary>
        public PowerupInfo[] OpponentActivePowerups { get; set; }

        public BulletInfo[] OpponentBullets { get; set; }
        public BulletInfo[] FriendlyBullets { get; set; }

        public bool FirstFrame { get; set; }
        public float TotalPower { get; set; }
        public float UnusedPower { get; set; }

        /// <summary>The type of the given weapon</summary>
        public WeaponType[] WeaponType { get; set; }
        /// <summary>How much power the weapon needs to work at maximum capacity</summary>
        public float[] WeaponPowerMax { get; set; }
        /// <summary>If the weapon is ready to fire</summary>
        public bool[] WeaponReady { get; set; }

        /// <summary>Currently active powerups on player</summary>
        public PowerupInfo[] ActivePowerups { get; set; }

        /// <summary>Powerups currently on the map</summary>
        public PowerupWorldInfo[] Powerups { get; set; }

        public abstract StartupConfig GetConfig();
        public abstract void Initialize();
        public abstract Configuration Update(float deltaTime);
        public abstract int ReplaceWeapon(WeaponType weaponType);
        public abstract void PowerupActivation(Powerup powerupType);
    }

    public enum Powerup
    {
        /// <summary>Thruster speed multiplier</summary>
        Speed,

        /// <summary>Direct damage reduction</summary>
        Armor,

        /// <summary>Shield strength multiplier</summary>
        Shield,

        /// <summary>Maximum power increase</summary>
        Power,
    }

    public struct PowerupInfo
    {
        public Powerup Powerup { get; set; }
        public float RemainingTime { get; set; }
    }

    public struct WeaponInfo
    {
        public WeaponType Type { get; set; }
        public float WeaponPowerMax { get; set; }
    }

    public struct PowerupWorldInfo
    {
        public Powerup Powerup { get; set; }
        public Vector2 Position { get; set; }
        public float RemainingDespawnTime { get; set; }
    }

    public struct Configuration
    {
        public float targetRotation, thrusterPower, rotationPower, shieldPower;
        public float[] weaponPower;
        public bool[] weaponFire;

        public static Configuration Empty => new Configuration()
        {
            weaponFire = new bool[6],
            weaponPower = new float[6]
        };
    }
}
