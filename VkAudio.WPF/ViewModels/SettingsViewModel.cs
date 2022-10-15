using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using VkAudio.WPF.Settings;
using VkAudio.WPF.Views.Helpers;

namespace VkAudio.WPF.ViewModels
{
    [INotifyPropertyChanged]
    internal partial class SettingsViewModel
    {
        private readonly AppSettingsService _appSettingsService;

        [ObservableProperty]
        private string _defaultSavePath;

        [ObservableProperty]
        private string _ffmpegPath;

        public SettingsViewModel(AppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;
            _defaultSavePath = _appSettingsService.DefaultSavePath;
            _ffmpegPath = _appSettingsService.FFmpegPath;
        }

        [RelayCommand]
        private async Task Apply()
        {
            if (DialogHost.IsDialogOpen(DialogIdentifiers.MainWindowName))
            {
                if (_appSettingsService.DefaultSavePath != DefaultSavePath
                    && _appSettingsService.FFmpegPath != FfmpegPath)
                {
                    _appSettingsService.DefaultSavePath = DefaultSavePath;
                    _appSettingsService.FFmpegPath = FfmpegPath;

                    await _appSettingsService.Save();
                }

                DialogHost.Close(DialogIdentifiers.MainWindowName);
            }
        }
    }
}
