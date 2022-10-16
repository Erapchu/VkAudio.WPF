using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkAudio.WPF.Settings;
using VkAudio.WPF.Views.Helpers;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace VkAudio.WPF.ViewModels
{
    [INotifyPropertyChanged]
    internal partial class SettingsViewModel : IProgress<ProgressInfo>
    {
        private readonly AppSettingsService _appSettingsService;
        private readonly ILogger<SettingsViewModel> _logger;
        private readonly Dictionary<double, double> _filesProgress = new();

        [ObservableProperty]
        private string _defaultSavePath;

        [ObservableProperty]
        private string _ffmpegPath;

        [ObservableProperty]
        private bool _ffmpegDownloading;

        [ObservableProperty]
        private double _ffmpegDownloadPercent;

        public SettingsViewModel(
            AppSettingsService appSettingsService,
            ILogger<SettingsViewModel> logger)
        {
            _appSettingsService = appSettingsService;
            _logger = logger;
            _defaultSavePath = _appSettingsService.DefaultSavePath;
            _ffmpegPath = _appSettingsService.FFmpegPath;
        }

        [RelayCommand]
        private async Task Apply()
        {
            if (DialogHost.IsDialogOpen(DialogIdentifiers.MainWindowName))
            {
                if (_appSettingsService.DefaultSavePath != DefaultSavePath
                    || _appSettingsService.FFmpegPath != FfmpegPath)
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

        [RelayCommand]
        private async Task DownloadFFmpeg()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.UseDescriptionForTitle = true;
            dialog.Description = "Download FFmpeg to";
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == true)
            {
                var selectedPath = dialog.SelectedPath;
                FfmpegPath = selectedPath;
                FfmpegDownloading = true;
                try
                {
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, selectedPath, this);
                    FFmpeg.SetExecutablesPath(selectedPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, null);
                }
                finally
                {
                    FfmpegDownloading = false;
                }
            }
        }

        public void Report(ProgressInfo value)
        {
            _filesProgress[value.TotalBytes] = value.DownloadedBytes;
            var newPercent = _filesProgress.Values.Sum() / _filesProgress.Keys.Sum() * 100;
            FfmpegDownloadPercent = newPercent;
        }
    }
}
