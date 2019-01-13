using System;
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
        public static float MusicVolume { get; private set; }

        private static bool _initialized;
        private static SoundEffect[] _effects;

        private static SoundEffectInstance[] _playingEffects;
        private static SoundEffectInstance _song;

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

        public static void SetSFXVolume(HashKey key, float volume)
        {
            if (key.Validate("Sound.SetVolume"))
            {
                SFXVolume = volume;

                foreach (SoundEffectInstance effect in _playingEffects)
                {

                }
            }
        }

        public static void SetMusicVolume(HashKey key, float volume)
        {
            if (key.Validate("Sound.SetVolume"))
            {
                MusicVolume = volume;
            }
        }

        public static void PlaySong(SoundEffect effect)
        {
            StopSong();

            _song = effect.CreateInstance();
            _song.IsLooped = true;
            _song.Play();
        }

        public static void StopSong()
        {
            _song?.Stop();
            _song = null;
        }
    }
}
