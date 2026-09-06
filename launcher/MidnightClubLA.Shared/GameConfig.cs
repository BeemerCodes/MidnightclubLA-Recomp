using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MidnightClubLA
{
    /// <summary>
    /// Simple key=value config for Midnight Club LA launcher preferences.
    /// Stored as mcla_launcher.ini next to the launcher executable.
    /// </summary>
    public class GameConfig
    {
        private readonly Dictionary<string, string> _values =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly string _path;

        public GameConfig(string path)
        {
            _path = path ?? string.Empty;
        }

        public static GameConfig Load(string path)
        {
            var cfg = new GameConfig(path);
            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    int eq = trimmed.IndexOf('=');
                    if (eq <= 0) continue;

                    string key   = trimmed.Substring(0, eq).Trim();
                    string value = trimmed.Substring(eq + 1).Trim();
                    cfg._values[key] = value;
                }
            }
            return cfg;
        }

        public void Save()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Midnight Club LA Launcher settings");
            sb.AppendLine("# Auto-generated — do not edit while the launcher is running");
            sb.AppendLine();

            foreach (var kv in _values.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (kv.Value != null)
                    sb.AppendLine(string.Format("{0}={1}", kv.Key, kv.Value));
            }

            string dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, sb.ToString(), Encoding.UTF8);
        }

        // ----- raw accessor -----

        public string this[string key]
        {
            get { return _values.TryGetValue(key, out var v) ? v : null; }
            set { _values[key] = value ?? string.Empty; }
        }

        private T Get<T>(string key, T defaultValue) where T : IConvertible
        {
            if (!_values.TryGetValue(key, out var raw) || raw == null)
                return defaultValue;
            try
            {
                return (T)Convert.ChangeType(raw, typeof(T), CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        private void Set<T>(string key, T value) where T : IConvertible
        {
            _values[key] = value != null
                ? Convert.ToString(value, CultureInfo.InvariantCulture)
                : string.Empty;
        }

        // ----- typed properties -----

        /// <summary>Game data root folder (relative or absolute).</summary>
        public string GameDataRoot
        {
            get { return this["game_data_root"] ?? "game"; }
            set { this["game_data_root"] = value; }
        }

        /// <summary>Draw-resolution scale multiplier (1 = native, 2 = 2x for 1440p/4K).</summary>
        public int ResolutionScale
        {
            get { return Get("resolution_scale", 2); }
            set { Set("resolution_scale", Math.Max(1, Math.Min(8, value))); }
        }

        /// <summary>Whether FXAA post-processing is enabled.</summary>
        public bool UseFxaa
        {
            get { return Get("use_fxaa", true); }
            set { Set("use_fxaa", value); }
        }

        /// <summary>Path to the last chosen ISO file (for the extraction UI).</summary>
        public string LastIsoPath
        {
            get { return this["last_iso_path"] ?? string.Empty; }
            set { this["last_iso_path"] = value; }
        }
    }
}
