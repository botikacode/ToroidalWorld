using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using ToroidalWorld.GameLogic.Entities.Definitions;

namespace ToroidalWorld.GameEngine
{
    public static class ResourceManager
    {
        private static readonly Dictionary<string, Texture2D> _textures = new();
        private static readonly Dictionary<string, SpriteSheet> _spriteSheets = new();
        private static readonly Dictionary<string, BoatDefinition> _boatDefinitions = new();
        private static readonly Dictionary<string, EnemyDefinition> _enemyDefinitions = new();
        private static readonly Dictionary<string, EnemyBaseDefinition> _enemyBaseDefinitions = new();
        private static readonly Dictionary<string, StageDefinition> _stageDefinitions = new();
        private static readonly Dictionary<string, TurretDefinition> _turretDefinitions = new();
        private static readonly Dictionary<string, ProjectileDefinition> _projectileDefinitions = new();

        private static readonly List<ExperienceOrbDefinition> _experienceOrbDefinitions = new();

        private static readonly Dictionary<string, SoundEffect> _soundEffects = new();
        private static readonly Dictionary<string, Song> _songs = new();

        private static readonly JsonSerializerOptions _configOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly string[] _extensions =
        {
            ".png", ".jpg", ".jpeg"
        };

        private static readonly string[] _soundEffectExtensions =
        {
            ".wav"
        };

        private static readonly string[] _musicExtensions =
        {
            ".mp3", ".ogg"
        };

        public static void LoadTextures(GraphicsDevice graphicsDevice)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Textures");
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException(folderPath);

            foreach (string file in Directory.GetFiles(folderPath))
            {
                Debug.WriteLine($"File: {file}");
                string ext = Path.GetExtension(file).ToLower();
                if (!IsValidExtension(ext))
                    continue;

                string key = Path.GetFileNameWithoutExtension(file);

                if (_textures.ContainsKey(key))
                    continue;

                using FileStream stream = File.OpenRead(file);
                Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);

                _textures.Add(key, texture);
                Debug.WriteLine($"Load texture: {key} ({texture.Width}x{texture.Height})");
            }
        }

        public static void LoadSprites(GraphicsDevice graphicsDevice)
        {
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Sprites");
            if (!Directory.Exists(basePath))
                throw new DirectoryNotFoundException(basePath);

            foreach (string file in Directory.GetFiles(basePath))
            {
                string ext = Path.GetExtension(file).ToLower();
                if (!IsValidExtension(ext))
                    continue;

                string key = Path.GetFileNameWithoutExtension(file);
                if (_spriteSheets.ContainsKey(key))
                    continue;

                using FileStream stream = File.OpenRead(file);
                Texture2D spriteTexture = Texture2D.FromStream(graphicsDevice, stream);

                string configPath = Path.Combine(basePath, $"{key}.json");
                if (!File.Exists(configPath))
                    throw new FileNotFoundException(configPath);

                SpriteData spriteData = JsonSerializer.Deserialize<SpriteData>(File.ReadAllText(configPath), _configOptions);
                Debug.WriteLine($"Json obtained: {File.ReadAllText(configPath)}");

                SpriteSheet spriteSheet = CreateSpriteSheet(spriteTexture, spriteData, key);
                _spriteSheets.Add(key, spriteSheet);
                Debug.WriteLine($"Loaded sprite: {key}");
                Debug.WriteLine($"Animations added: {_spriteSheets[key].AnimationCount.ToString()}");
            }
        }

        private static SpriteSheet CreateSpriteSheet(Texture2D texture, SpriteData data, string key)
        {
            Debug.WriteLine($"Width: {data.FrameWidth} and Height: {data.FrameHeight}");
            Texture2DAtlas atlas = Texture2DAtlas.Create($"atlas//{key}", texture, data.FrameWidth, data.FrameHeight);
            SpriteSheet spriteSheet = new SpriteSheet($"spritesheet//{key}", atlas);

            foreach (AnimationData anim in data.Animations)
            {
                AddAnimationCycle(spriteSheet, anim.Name, anim.Frames, anim.IsLooping, anim.FrameDuration);
            }
            return spriteSheet;
        }

        private static void AddAnimationCycle(SpriteSheet spriteSheet, string name, int[] frames, bool isLooping = true, float frameDuration = 0.1f)
        {
            spriteSheet.DefineAnimation(name, builder =>
            {
                builder.IsLooping(isLooping);
                for (int i = 0; i < frames.Length; i++)
                {
                    builder.AddFrame(frames[i], TimeSpan.FromSeconds(frameDuration));
                }
            });
        }

        public static SpriteSheet GetSpriteSheet(string key)
        {
            return _spriteSheets[key];
        }

        public static Texture2D GetTexture(string key)
        {
            return _textures[key];
        }

        public static void LoadExperienceOrbDefinitions()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", "ExperienceOrbs.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            string json = File.ReadAllText(configPath);

            var defs = JsonSerializer.Deserialize<List<ExperienceOrbDefinition>>(json, _configOptions);
            if (defs == null)
                return;

            _experienceOrbDefinitions.Clear();

            foreach (var def in defs)
            {
                if (def == null)
                    continue;

                if (def.MinAmount < 1)
                    continue;

                if (def.MaxAmount != -1 && def.MaxAmount < def.MinAmount)
                    continue;

                if (string.IsNullOrWhiteSpace(def.SpriteSheet))
                    continue;

                _experienceOrbDefinitions.Add(def);
            }

            Debug.WriteLine($"Loaded experience orb definitions: {_experienceOrbDefinitions.Count}");
        }

        public static string ResolveExperienceOrbSpriteKey(int amount)
        {
            if (amount <= 0)
                return null;

            ExperienceOrbDefinition best = null;

            for (int i = 0; i < _experienceOrbDefinitions.Count; i++)
            {
                var def = _experienceOrbDefinitions[i];

                bool inRange = amount >= def.MinAmount && (def.MaxAmount < 0 || amount <= def.MaxAmount);
                if (!inRange)
                    continue;

                if (best == null)
                {
                    best = def;
                    continue;
                }

                if (def.MinAmount > best.MinAmount)
                {
                    best = def;
                    continue;
                }

                if (def.MinAmount == best.MinAmount)
                {
                    int bestMax = best.MaxAmount < 0 ? int.MaxValue : best.MaxAmount;
                    int defMax = def.MaxAmount < 0 ? int.MaxValue : def.MaxAmount;

                    if (defMax < bestMax)
                        best = def;
                }
            }

            return best?.SpriteSheet;
        }

        public static void LoadSoundEffects()
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "SoundEffects");
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException(folderPath);

            foreach (string file in Directory.GetFiles(folderPath))
            {
                string ext = Path.GetExtension(file).ToLower();
                if (!IsValidSoundEffectExtension(ext))
                    continue;

                string key = Path.GetFileNameWithoutExtension(file);
                if (_soundEffects.ContainsKey(key))
                    continue;

                try
                {
                    using FileStream stream = File.OpenRead(file);
                    SoundEffect sfx = SoundEffect.FromStream(stream);
                    _soundEffects.Add(key, sfx);
                    Debug.WriteLine($"Loaded SFX: {key}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading SFX '{file}': {ex.Message}");
                }
            }
        }

        public static void LoadMusic()
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Music");
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException(folderPath);

            foreach (string file in Directory.GetFiles(folderPath))
            {
                string ext = Path.GetExtension(file).ToLower();
                if (!IsValidMusicExtension(ext))
                    continue;

                string key = Path.GetFileNameWithoutExtension(file);
                if (_songs.ContainsKey(key))
                    continue;

                try
                {
                    string fullPath = Path.GetFullPath(file);
                    var uri = new Uri(fullPath);

                    Song song = Song.FromUri(key, uri);
                    _songs.Add(key, song);
                    Debug.WriteLine($"Loaded Song: {key}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading Song '{file}': {ex.Message}");
                }
            }
        }

        public static SoundEffect GetSoundEffect(string key)
        {
            return _soundEffects[key];
        }

        public static Song GetSong(string key)
        {
            return _songs[key];
        }

        public static bool HasSoundEffect(string key)
        {
            return _soundEffects.ContainsKey(key);
        }

        public static bool HasSong(string key)
        {
            return _songs.ContainsKey(key);
        }

        public static void LoadBoatDefinitions()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", "Boats.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            string json = File.ReadAllText(configPath);

            var boats = JsonSerializer.Deserialize<List<BoatDefinition>>(json, _configOptions);
            if (boats == null)
                return;

            _boatDefinitions.Clear();

            foreach (var boat in boats)
            {
                if (boat == null)
                    continue;

                if (string.IsNullOrWhiteSpace(boat.Name))
                    continue;

                _boatDefinitions[boat.Name] = boat;
                Debug.WriteLine($"Loaded boat definition: {boat.Name}");
            }
        }

        public static BoatDefinition GetBoatDefinition(string name)
        {
            return _boatDefinitions[name];
        }

        public static IReadOnlyList<BoatDefinition> GetBoatDefinitions()
        {
            return _boatDefinitions.Values
                .OrderBy(b => b.RequiredPoints)
                .ThenBy(b => b.Name)
                .ToList();
        }

        public static void LoadEnemyBaseDefinitions()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", "EnemyBases.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            string json = File.ReadAllText(configPath);

            var bases = JsonSerializer.Deserialize<List<EnemyBaseDefinition>>(json, _configOptions);
            if (bases == null)
                return;

            _enemyBaseDefinitions.Clear();

            foreach (var enemyBase in bases)
            {
                if (enemyBase == null)
                    continue;

                if (string.IsNullOrWhiteSpace(enemyBase.Name))
                    continue;

                _enemyBaseDefinitions[enemyBase.Name] = enemyBase;
                Debug.WriteLine($"Loaded enemy base definition: {enemyBase.Name}");
            }
        }

        public static EnemyBaseDefinition GetEnemyBaseDefinition(string name)
        {
            return _enemyBaseDefinitions[name];
        }

        public static void LoadStageDefinitions()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", "Stages.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            string json = File.ReadAllText(configPath);

            var stages = JsonSerializer.Deserialize<List<StageDefinition>>(json, _configOptions);
            if (stages == null)
                return;

            _stageDefinitions.Clear();

            foreach (var stage in stages)
            {
                if (stage == null || string.IsNullOrWhiteSpace(stage.Id))
                    continue;

                _stageDefinitions[stage.Id] = stage;
                Debug.WriteLine($"Loaded stage: {stage.Id} ({stage.Name})");
            }
        }

        public static StageDefinition GetStageDefinition(string id)
        {
            return _stageDefinitions[id];
        }

        private static bool IsValidExtension(string ext)
        {
            foreach (string valid in _extensions)
                if (ext == valid)
                    return true;

            return false;
        }

        private static bool IsValidSoundEffectExtension(string ext)
        {
            foreach (string valid in _soundEffectExtensions)
                if (ext == valid)
                    return true;

            return false;
        }

        private static bool IsValidMusicExtension(string ext)
        {
            foreach (string valid in _musicExtensions)
                if (ext == valid)
                    return true;

            return false;
        }

        public static bool HasTexture(string key)
        {
            return _textures.ContainsKey(key);
        }

        public static void LoadEnemyDefinitions()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", "Enemies.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            string json = File.ReadAllText(configPath);

            var enemies = JsonSerializer.Deserialize<List<EnemyDefinition>>(json, _configOptions);
            if (enemies == null)
                return;

            _enemyDefinitions.Clear();

            foreach (var enemy in enemies)
            {
                if (enemy == null)
                    continue;

                if (string.IsNullOrWhiteSpace(enemy.Name))
                    continue;

                _enemyDefinitions[enemy.Name] = enemy;
                Debug.WriteLine($"Loaded enemy definition: {enemy.Name}");
            }
        }

        public static EnemyDefinition GetEnemyDefinition(string name)
        {
            return _enemyDefinitions[name];
        }

        public static void LoadTurretDefinitions()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", "Turrets.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            string json = File.ReadAllText(configPath);

            var turrets = JsonSerializer.Deserialize<List<TurretDefinition>>(json, _configOptions);
            if (turrets == null)
                return;

            _turretDefinitions.Clear();

            foreach (var turret in turrets)
            {
                if (turret == null)
                    continue;

                if (string.IsNullOrWhiteSpace(turret.Name))
                    continue;

                _turretDefinitions[turret.Name] = turret;
                Debug.WriteLine($"Loaded turret definition: {turret.Name}");
            }
        }

        public static TurretDefinition GetTurretDefinition(string name)
        {
            return _turretDefinitions[name];
        }

        public static void LoadProjectileDefinitions()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Config", "Projectiles.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            string json = File.ReadAllText(configPath);

            var projectiles = JsonSerializer.Deserialize<List<ProjectileDefinition>>(json, _configOptions);
            if (projectiles == null)
                return;

            _projectileDefinitions.Clear();

            foreach (var projectile in projectiles)
            {
                if (projectile == null)
                    continue;

                if (string.IsNullOrWhiteSpace(projectile.Name))
                    continue;

                _projectileDefinitions[projectile.Name] = projectile;
                Debug.WriteLine($"Loaded projectile definition: {projectile.Name}");
            }
        }

        public static ProjectileDefinition GetProjectileDefinition(string name)
        {
            return _projectileDefinitions[name];
        }

        public static void Unload()
        {
            foreach (Texture2D texture in _textures.Values)
                texture.Dispose();

            foreach (var spriteSheet in _spriteSheets.Values)
                spriteSheet.TextureAtlas.Texture.Dispose();

            foreach (var sfx in _soundEffects.Values)
                sfx.Dispose();

            _textures.Clear();
            _spriteSheets.Clear();
            _boatDefinitions.Clear();
            _enemyDefinitions.Clear();
            _enemyBaseDefinitions.Clear();
            _stageDefinitions.Clear();
            _turretDefinitions.Clear();
            _projectileDefinitions.Clear();
            _soundEffects.Clear();
            _songs.Clear();
        }
    }
}