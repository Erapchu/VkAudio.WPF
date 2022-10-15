using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using VkAudio.WPF.ViewModels.Messages;

namespace VkAudio.WPF.ViewModels
{
    [INotifyPropertyChanged]
    internal partial class AudioViewModel
    {
        [ObservableProperty]
        private string _url;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _artist;

        [ObservableProperty]
        private bool _isDownloading;

        [ObservableProperty]
        private int _percent;

        [RelayCommand]
        private void DownloadAudio()
        {
            if (IsDownloading)
                return;

            StrongReferenceMessenger.Default.Send(new DownloadAudioMessage(this));
        }
    }
}
