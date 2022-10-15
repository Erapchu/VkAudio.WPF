using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VkAudio.WPF.Models;
using VkAudio.WPF.Settings;
using VkAudio.WPF.Views;
using VkAudio.WPF.Views.Helpers;
using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Model;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Exceptions;

namespace VkAudio.WPF.ViewModels
{
    [INotifyPropertyChanged]
    internal partial class MainWindowViewModel
    {
        private readonly IVkApi _vkApi;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly AppSettingsService _appSettingsService;
        private readonly IServiceProvider _serviceProvider;
        [ObservableProperty]
        private bool _isAuthorized;

        [ObservableProperty]
        private Uri _photo100;

        [ObservableProperty]
        private string _userName;

        public ObservableCollection<AudioViewModel> AudioViewModels { get; set; } = new ObservableCollection<AudioViewModel>();

        private MainWindowViewModel()
        {
        }

        public MainWindowViewModel(
            IVkApi vkApi,
            ILogger<MainWindowViewModel> logger,
            AppSettingsService appSettingsService,
            IServiceProvider serviceProvider)
        {
            _vkApi = vkApi;
            _logger = logger;
            _appSettingsService = appSettingsService;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task Login()
        {
            try
            {
                var loginResult = await MaterialInputBox.ShowAsync(
                    "Login to VK",
                    "Login",
                    "Password",
                    DialogIdentifiers.MainWindowName);
                if (loginResult is null)
                    return;

                var apiAuthParams = new ApiAuthParams()
                {
                    Login = loginResult.Login,
                    Password = loginResult.Password,
                };
                await _vkApi.AuthorizeAsync(apiAuthParams);
                if (_vkApi.Token is not null)
                {
                    IsAuthorized = true;
                    _appSettingsService.UserId = _vkApi.UserId.GetValueOrDefault();
                    _appSettingsService.Token = _vkApi.Token;
                    await _appSettingsService.Save();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
        }

        [RelayCommand]
        private async Task Logout()
        {
            try
            {
                await _vkApi.LogOutAsync();
                IsAuthorized = false;
                _appSettingsService.UserId = 0;
                _appSettingsService.Token = null;
                await _appSettingsService.Save();
                AudioViewModels.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
        }

        [RelayCommand]
        private async Task Loaded()
        {
            try
            {
                var token = _appSettingsService.Token;
                if (token is not null)
                {
                    IsAuthorized = true;
                    var apiAuthParams = new ApiAuthParams()
                    {
                        AccessToken = token
                    };
                    await _vkApi.AuthorizeAsync(apiAuthParams);

                    _ = RefreshAudioCommand.ExecuteAsync(null);

                    var userInfo = await _vkApi.Users.GetAsync(new[] { _appSettingsService.UserId }, ProfileFields.Photo100 | ProfileFields.FirstName | ProfileFields.LastName);
                    if (userInfo is not null && userInfo.Count != 0)
                    {
                        var currentUser = userInfo[0];
                        Photo100 = currentUser.Photo100;
                        UserName = currentUser.FirstName + " " + currentUser.LastName;
                    }
                }

                var ffmpegPath = _appSettingsService.FFmpegPath;
                if (!string.IsNullOrWhiteSpace(ffmpegPath))
                    FFmpeg.SetExecutablesPath(ffmpegPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
        }

        [RelayCommand]
        private async Task RefreshAudio()
        {
            try
            {
                var parameters = new VkNet.Utils.VkParameters()
                {
                    {"v", "5.131" },
                    {"owner_id", _appSettingsService.UserId.ToString() },
                    {"need_blocks", "1" },
                };
                var getAudioResponse = await _vkApi.CallAsync("catalog.getAudio", parameters);
                var audioRoot = JsonConvert.DeserializeObject<Root>(getAudioResponse.RawJson);

                var audioVMs = new List<AudioViewModel>();
                foreach (var audio in audioRoot.response.audios)
                {
                    audioVMs.Add(new AudioViewModel()
                    {
                        Title = audio.title,
                        Url = audio.url,
                        Artist = audio.artist,
                    });
                }

                AudioViewModels = new ObservableCollection<AudioViewModel>(audioVMs);
                OnPropertyChanged(nameof(AudioViewModels));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
        }

        [RelayCommand]
        private async Task DownloadAudio(AudioViewModel audioViewModel)
        {
            var ffmpegNotFound = false;
            try
            {
                string savePath = null;
                if (string.IsNullOrWhiteSpace(_appSettingsService.DefaultSavePath))
                {
                    var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                    dialog.UseDescriptionForTitle = true;
                    dialog.Description = "Save audio to";
                    var dialogResult = dialog.ShowDialog();
                    savePath = dialog.SelectedPath;
                }
                else
                {
                    savePath = _appSettingsService.DefaultSavePath;
                }

                if (string.IsNullOrWhiteSpace(savePath))
                    return;

                var parameters = $"-protocol_whitelist file,http,https,tcp,tls,crypto -i \"{audioViewModel.Url}\" \"{savePath}\"";
                var conversion = FFmpeg.Conversions
                    .New()
                    .UseMultiThread(Environment.ProcessorCount);
                conversion.OnProgress += async (sender, args) =>
                {
                    await Console.Out.WriteLineAsync($"[{args.Duration}/{args.TotalLength}][{args.Percent}%]");
                };
                await conversion.Start(parameters);
            }
            catch (FFmpegNotFoundException ffnfe)
            {
                _logger.LogError(ffnfe, null);
                ffmpegNotFound = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }

            if (ffmpegNotFound)
            {
                var result = await MaterialMessageBox.ShowAsync(
                    "FFmpeg exception",
                    "FFmpeg executable files not found! Please, specify them in settings.",
                    MaterialMessageBoxButtons.OK,
                    DialogIdentifiers.MainWindowName,
                    PackIconKind.Error);
                //await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, selectedPath);
                //FFmpeg.SetExecutablesPath(selectedPath);
            }
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            var settingsView = _serviceProvider.GetService<SettingsView>();
            var result = await DialogHost.Show(settingsView, DialogIdentifiers.MainWindowName);
        }
    }
}
