namespace VkAudio.WPF.Settings
{
    public interface IAppSettings
    {
        public string Token { get; set; }
        public long UserId { get; set; }
    }
}
