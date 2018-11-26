﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;

namespace Andromedroids
{
    public enum SFX
    {
        EscapeLong, EscapeShort,
        ExplodeEcho, ExplodeLong, ExplodeMedium, ExplodeShort,
        HitHard, HitImpact, HitMedium,
        MenuBlipBack, MenuBlipClick, MenuBlipExit, MenuBlipNeutral, MenuBlipStart,
        PickupLong, PickupLong2,
        PowerupElectric, PowerupMedium, PowerupMinor, PowerupWobble,
        Success
    }

    public static class Sound
    {
        public static float SFXVolume { get; private set; }

        private static bool _initialized;
        private static SoundEffect[] _effects;

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _effects = ContentController.GetCollection<SoundEffect>("SFX");
        }

        public static void PlayEffect(SFX sfx)
        {
            _effects[(int)sfx].Play(SFXVolume, 0.0f, 0.0f);
        }

        public static void SetVolume(HashKey key, float volume)
        {
            if (key.Validate("Sound.SetVolume"))
            {
                SFXVolume = volume;
            }
        }
    }
}