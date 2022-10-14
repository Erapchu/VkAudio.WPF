using CommunityToolkit.Mvvm.ComponentModel;

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
    }
}
