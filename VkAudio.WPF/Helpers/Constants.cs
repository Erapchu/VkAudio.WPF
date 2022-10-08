using System;
using System.IO;

namespace VkAudio.WPF.Helpers
{
    internal static class Constants
    {
        public const string CommonSettingsFileName = "commonSettings.json";
        public const string AppName = "VkAudio";

        public static string LocalAppDataDirectoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
        public static string CommonSettingsFilePath { get; } = Path.Combine(LocalAppDataDirectoryPath, CommonSettingsFileName);
    }
}
