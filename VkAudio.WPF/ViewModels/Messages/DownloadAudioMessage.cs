namespace VkAudio.WPF.ViewModels.Messages
{
    internal class DownloadAudioMessage
    {
        public AudioViewModel AudioViewModel { get; }

        public DownloadAudioMessage(AudioViewModel audioViewModel)
        {
            AudioViewModel = audioViewModel;
        }
    }
}
