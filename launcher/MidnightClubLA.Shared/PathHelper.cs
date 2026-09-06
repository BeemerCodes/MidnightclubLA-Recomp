using System;
using System.IO;
using System.Reflection;

namespace MidnightClubLA
{
    public static class PathHelper
    {
        /// <summary>
        /// Directory that contains the running launcher executable.
        /// Launcher is compiled to: launcher\DantesInfernoLauncher\bin\Release\
        /// </summary>
        public static string ExecutableDirectory
        {
            get
            {
                string path = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Resolves a path relative to the launcher executable's directory.
        /// Uses Path.GetFullPath so ".." navigation works correctly.
        /// </summary>
        public static string Resolve(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(ExecutableDirectory, relativePath));
        }

        /// <summary>
        /// Full path to midnight_club_la.exe.
        /// Relative from  launcher/DantesInfernoLauncher/bin/Release/
        ///   -> ../../../../out/build/win-amd64-release/midnight_club_la.exe
        /// </summary>
        public static string GetGameExecutablePath()
        {
            return Resolve(@"..\..\..\..\out\build\win-amd64-release\midnight_club_la.exe");
        }

        /// <summary>
        /// Full path to the launcher preferences file (mcla_launcher.ini).
        /// Stored next to the launcher exe so it stays with the install.
        /// </summary>
        public static string GetGameConfigPath()
        {
            return Path.Combine(ExecutableDirectory, "mcla_launcher.ini");
        }

        /// <summary>
        /// Absolute path to the repository root (4 levels above the launcher bin).
        /// Used as WorkingDirectory when launching the game.
        /// </summary>
        public static string GetRepositoryRoot()
        {
            return Resolve(@"..\..\..\..");
        }

        /// <summary>
        /// Absolute path to game\ data folder (relative to repo root).
        /// </summary>
        public static string GetGameDataPath()
        {
            return Path.Combine(GetRepositoryRoot(), "game");
        }

        /// <summary>
        /// Absolute path to tools\extract-xiso\extract-xiso.exe.
        /// </summary>
        public static string GetExtractXisoPath()
        {
            return Path.Combine(GetRepositoryRoot(), @"tools\extract-xiso\extract-xiso.exe");
        }

        /// <summary>
        /// Absolute path to the cover/banner image.
        /// The launcher .csproj copies launcher/banner.jpg → bin output as banner.jpg.
        /// For MCLA we ship cover.jpg; the csproj references it as cover.jpg.
        /// </summary>
        public static string GetCoverImagePath()
        {
            // Try cover.jpg first (MCLA), fall back to banner.jpg (legacy)
            string cover = Path.Combine(GetRepositoryRoot(), "assets", "cover.jpg");
            if (File.Exists(cover)) return cover;
            return Path.Combine(ExecutableDirectory, "banner.jpg");
        }

        // ---------- legacy overloads kept for Installer compatibility ----------

        public static string GetGameExecutablePath(string installDirectory)
        {
            return Path.Combine(installDirectory, "midnight_club_la.exe");
        }

        public static string GetGameConfigPath(string installDirectory)
        {
            return Path.Combine(installDirectory, "midnight_club_la.toml");
        }

        public static string GetGameDataPath(string installDirectory)
        {
            return Path.Combine(installDirectory, "game");
        }

        public static string GetVersionPath(string installDirectory)
        {
            return Path.Combine(installDirectory, "version.txt");
        }

        public static string GetLogsPath(string installDirectory)
        {
            return Path.Combine(installDirectory, "logs");
        }
    }
}
