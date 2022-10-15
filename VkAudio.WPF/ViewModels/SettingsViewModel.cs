using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using VkAudio.WPF.Settings;
using VkAudio.WPF.Views.Helpers;
using Xabe.FFmpeg;

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

        [RelayCommand]
        private void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogIdentifiers.MainWindowName))
                DialogHost.Close(DialogIdentifiers.MainWindowName);
        }

        [RelayCommand]
        private void SetFFmpegPath()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.UseDescriptionForTitle = true;
            dialog.Description = "Set path to FFmpeg executables";
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == true)
            {
                var selectedPath = dialog.SelectedPath;
                FfmpegPath = selectedPath;
                FFmpeg.SetExecutablesPath(selectedPath);
            }
        }

        [RelayCommand]
        private void SetDefaultSavePath()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.UseDescriptionForTitle = true;
            dialog.Description = "Set path to default save folder";
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == true)
            {
                var selectedPath = dialog.SelectedPath;
                DefaultSavePath = selectedPath;
            }
        }
    }
}
