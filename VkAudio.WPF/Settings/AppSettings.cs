﻿namespace VkAudio.WPF.Settings
{
    internal class AppSettings : IAppSettings
    {
        public string Token { get; set; }
        public long UserId { get; set; }
        public string FFmpegPath { get; set; }
        public string DefaultSavePath { get; set; }
    }
}
