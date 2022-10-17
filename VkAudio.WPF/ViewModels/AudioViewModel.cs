using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
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
        private TimeSpan _duration;

        [ObservableProperty]
        private bool _isDownloading;

        [ObservableProperty]
        private int _percent;

        [ObservableProperty]
        private bool _isIndeterminate;

        [RelayCommand]
        private void DownloadAudio()
        {
            if (IsDownloading)
                return;

            StrongReferenceMessenger.Default.Send(new DownloadAudioMessage(this));
        }

        public override string ToString()
        {
            return $"{_artist} - {_title}";
        }
    }
}
