using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ToroidalWorld.GameLogic.Progress
{
    public sealed class PlayerProgressStore
    {
        private const int MaxBestRuns = 10;

        public static PlayerProgressStore Default { get; } = new PlayerProgressStore(GetDefaultFilePath());

        private readonly string _filePath;

        public PlayerProgressStore(string filePath)
        {
            _filePath = filePath;
        }

        public PlayerProgress Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new PlayerProgress();

                var base64 = File.ReadAllText(_filePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(base64))
                    return new PlayerProgress();

                var compressedBytes = Convert.FromBase64String(base64);
                var jsonBytes = Decompress(compressedBytes);
                var progress = JsonSerializer.Deserialize<PlayerProgress>(jsonBytes);

                return progress ?? new PlayerProgress();
            }
            catch
            {
                return new PlayerProgress();
            }
        }

        public void Save(PlayerProgress progress)
        {
            try
            {
                if (progress == null)
                    return;

                EnsureDirectoryExists();

                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(progress, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                var compressedBytes = Compress(jsonBytes);
                var base64 = Convert.ToBase64String(compressedBytes);

                File.WriteAllText(_filePath, base64, Encoding.UTF8);
            }
            catch
            {
            }
        }

        public void DeleteAll()
        {
            try
            {
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
            }
            catch
            {
            }
        }

        public PlayerProgress RegisterGameResult(int kills, float timeSeconds)
        {
            if (kills < 0)
                kills = 0;

            if (timeSeconds < 0f)
                timeSeconds = 0f;

            var points = CalculatePoints(timeSeconds, kills);

            var progress = Load();
            progress.TotalPoints += points;

            progress.BestRuns.Add(new GameRecord
            {
                UtcTimestamp = DateTime.UtcNow,
                Kills = kills,
                TimeSeconds = timeSeconds,
                Points = points
            });

            progress.BestRuns = progress.BestRuns
                .OrderByDescending(x => x.Points)
                .ThenByDescending(x => x.UtcTimestamp)
                .Take(MaxBestRuns)
                .ToList();

            Save(progress);
            return progress;
        }

        private static long CalculatePoints(float timeSeconds, int kills)
        {
            var raw = timeSeconds * kills;
            if (raw <= 0f)
                return 0;

            return (long)MathF.Floor(raw);
        }

        private void EnsureDirectoryExists()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrWhiteSpace(dir))
                return;

            Directory.CreateDirectory(dir);
        }

        private static string GetDefaultFilePath()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(root, "ToroidalWorld", "progress.dat");
        }

        private static byte[] Compress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
            {
                gzip.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }

        private static byte[] Decompress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }
}