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
            MenuName = menuName;
            Quickstart = quickstart;
        }
    }

    /// <summary>Contains the player itself, and a bunch of information to be used in an AI.
    /// Remmber that changing these will bear no effect.</summary>
    public abstract class ShipPlayer : ManagedWorldObject
    {
        public const float
            HITRADIUS = 1.0f;

        public PlayerManager Manager { get; private set; }

        public int OpponentHealth { get; set; }
        public float OpponentShield { get; set; }
        public Vector2 OpponentPosition { get; set; }
        public float OpponentRotation { get; set; }
        public Vector2 OpponentVelocity { get; set; }
        public Weapon.Type[] OpponentWeapons { get; set; }
         
        public BulletInfo[] OpponentBullets { get; set; }
        public BulletInfo[] FriendlyBullets { get; set; }

        public bool FirstFrame { get; set; }
        public int TotalPower { get; set; }
        public int UnusedPower { get; set; }

        /// <summary>The type of the given weapon</summary>
        public Weapon.Type[] WeaponType { get; set; }
        /// <summary>The time needed between shots for given weapon</summary>
        public float[] WeaponCooldown { get; set; }
        /// <summary>Remaining cooldown for given weapon</summary>
        public float[] WeaponCooldownRemaining { get; set; }
        /// <summary>How much power the weapon needs to function normally (most efficiently)</summary>
        public int[] WeaponPowerNeeded { get; set; }
        /// <summary>How much power the weapon needs to work at maximum capacity</summary>
        public int[] WeaponPowerMax { get; set; }
        /// <summary>If the weapon is ready to fire</summary>
        public bool[] WeaponReady { get; set; }

        /// <summary>Currently active powerups</summary>
        public PowerupInfo[] ActivePowerups { get; set; }

        public abstract StartupConfig GetConfig();
        public abstract void Initialize();
        public abstract Configuration Update(float deltaTime);
        public abstract int ReplaceWeapon(Weapon.Type weaponType);
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

    public struct Configuration
    {
        public int targetRotation, thrusterPower, rotationPower, shieldPower;
        public int[] weaponPower;
        public bool[] weaponFire;
    }
}
