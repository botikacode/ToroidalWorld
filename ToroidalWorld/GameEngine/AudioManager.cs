using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace ToroidalWorld.GameEngine
{
    public static class AudioManager
    {
        private static float _sfxVolume = 1f;
        private static float _musicVolume = 1f;

        private static readonly HashSet<SoundEffectInstance> _managedLoopInstances = new HashSet<SoundEffectInstance>();

        public static float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = MathHelper.Clamp(value, 0f, 1f);
                SoundEffect.MasterVolume = _sfxVolume;
            }
        }

        public static float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = MathHelper.Clamp(value, 0f, 1f);
                MediaPlayer.Volume = _musicVolume;
            }
        }

        public static void Initialize(float sfxVolume = 1f, float musicVolume = 1f)
        {
            SfxVolume = sfxVolume;
            MusicVolume = musicVolume;
        }

        public static bool TryPlaySoundEffect(string key, float volume = 1f, float pitch = 0f, float pan = 0f)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            if (!ResourceManager.HasSoundEffect(key))
                return false;

            volume = MathHelper.Clamp(volume, 0f, 1f);
            pitch = MathHelper.Clamp(pitch, -1f, 1f);
            pan = MathHelper.Clamp(pan, -1f, 1f);

            try
            {
                return ResourceManager.GetSoundEffect(key).Play(volume, pitch, pan);
            }
            catch
            {
                return false;
            }
        }

        public static SoundEffectInstance CreateSoundEffectInstance(string key, bool isLooped, float volume = 1f, float pitch = 0f, float pan = 0f)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            if (!ResourceManager.HasSoundEffect(key))
                return null;

            volume = MathHelper.Clamp(volume, 0f, 1f);
            pitch = MathHelper.Clamp(pitch, -1f, 1f);
            pan = MathHelper.Clamp(pan, -1f, 1f);

            try
            {
                var instance = ResourceManager.GetSoundEffect(key).CreateInstance();
                instance.IsLooped = isLooped;
                instance.Volume = volume;
                instance.Pitch = pitch;
                instance.Pan = pan;

                if (isLooped && instance != null)
                    _managedLoopInstances.Add(instance);

                return instance;
            }
            catch
            {
                return null;
            }
        }

        public static void StopManagedLoopingSoundEffects(bool disposeInstances)
        {
            if (_managedLoopInstances.Count == 0)
                return;

            foreach (var instance in _managedLoopInstances)
            {
                if (instance == null)
                    continue;

                try { instance.Stop(); } catch { }

                if (disposeInstances)
                {
                    try { instance.Dispose(); } catch { }
                }
            }

            if (disposeInstances)
                _managedLoopInstances.Clear();
        }

        public static bool TryPlaySong(string key, bool isRepeating = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            if (!ResourceManager.HasSong(key))
                return false;

            try
            {
                MediaPlayer.IsRepeating = isRepeating;
                MediaPlayer.Play(ResourceManager.GetSong(key));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void StopMusic()
        {
            try { MediaPlayer.Stop(); }
            catch { }
        }

        public static void PauseMusic()
        {
            try { MediaPlayer.Pause(); }
            catch { }
        }

        public static void ResumeMusic()
        {
            try { MediaPlayer.Resume(); }
            catch { }
        }

        public static bool IsMusicPlaying()
        {
            try { return MediaPlayer.State == MediaState.Playing; }
            catch { return false; }
        }
    }
}
