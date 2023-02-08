using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkAudio.WPF.Collections;
using VkAudio.WPF.Models.Catalog.GetAudio;
using VkAudio.WPF.Models.Catalog.GetSection;
using VkAudio.WPF.Services;
using VkAudio.WPF.Settings;
using VkAudio.WPF.ViewModels.Messages;
using VkAudio.WPF.Views;
using VkAudio.WPF.Views.Helpers;
using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;

namespace VkAudio.WPF.ViewModels
{
    internal partial class MainWindowViewModel : ObservableRecipient,
        IRecipient<DownloadAudioMessage>
    {
        private readonly IVkApi _vkApi;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly AppSettingsService _appSettingsService;
        private readonly IServiceProvider _serviceProvider;
        private bool _audioRefreshed;
        private string _nextFrom;
        private string _blockId;

        [ObservableProperty]
        private bool _isAuthorized;

        [ObservableProperty]
        private Uri _photo100;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private bool _downloadingNext;

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                _ = Search();
            }
        }

        public ObservableCollectionDelayed<AudioViewModel> AudioViewModels { get; set; } = new ObservableCollectionDelayed<AudioViewModel>();

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

        private async Task Search()
        {
            try
            {
                var parameters = new VkParameters
                {
                    { "v", "5.131" },
                    { "query", _searchText },
                    { "need_blocks", 1 },
                };
                var response = await _vkApi.CallAsync("catalog.getAudioSearch", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
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
                    _ = RefreshAudioCommand.ExecuteAsync(null);
                    _ = FillUserInfo();
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
                    _ = FillUserInfo();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
        }

        private async Task FillUserInfo()
        {
            var userInfo = await _vkApi.Users.GetAsync(new[] { _appSettingsService.UserId }, ProfileFields.Photo100 | ProfileFields.FirstName | ProfileFields.LastName);
            if (userInfo is not null && userInfo.Count != 0)
            {
                var currentUser = userInfo[0];
                Photo100 = currentUser.Photo100;
                UserName = currentUser.FirstName + " " + currentUser.LastName;
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
                var getAudio = JsonConvert.DeserializeObject<GetAudioWrapper>(getAudioResponse.RawJson);

                var audioVMs = new List<AudioViewModel>();
                foreach (var audio in getAudio.response.audios)
                {
                    audioVMs.Add(new AudioViewModel()
                    {
                        Title = audio.title,
                        Url = audio.url,
                        Artist = audio.artist,
                        Duration = TimeSpan.FromSeconds(audio.duration),
                    });
                }

                AudioViewModels = new ObservableCollectionDelayed<AudioViewModel>(audioVMs);
                OnPropertyChanged(nameof(AudioViewModels));

                var defaultSectionId = getAudio.response.catalog.default_section;
                var musicBlock = getAudio.response.catalog.sections
                    .Find(s => s.id == defaultSectionId && s.blocks?.Count > 0)
                    ?.blocks.Find(b => b.data_type == "music_audios");
                _nextFrom = musicBlock?.next_from;
                _blockId = musicBlock?.id;
                _audioRefreshed = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
        }

        [RelayCommand]
        private async Task GetNextAudios()
        {
            try
            {
                if (DownloadingNext
                    || !_audioRefreshed
                    || string.IsNullOrWhiteSpace(_nextFrom)
                    || string.IsNullOrWhiteSpace(_blockId))
                    return;

                DownloadingNext = true;
                var parameters = new VkNet.Utils.VkParameters()
                {
                    {"v", "5.131" },
                    {"start_from", _nextFrom },
                    {"section_id", _blockId },
                };
                var paginationResponse = await _vkApi.CallAsync("catalog.getSection", parameters);
                var getSection = JsonConvert.DeserializeObject<GetSectionWrapper>(paginationResponse.RawJson);

                _nextFrom = getSection.response.section.next_from;
                _blockId = getSection.response.section.id;

                using var delayed = AudioViewModels.DelayNotifications();
                foreach (var audio in getSection.response.audios)
                {
                    delayed.Add(new AudioViewModel()
                    {
                        Title = audio.title,
                        Url = audio.url,
                        Artist = audio.artist,
                        Duration = TimeSpan.FromSeconds(audio.duration),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
            finally
            {
                DownloadingNext = false;
            }
        }

        public async void Receive(DownloadAudioMessage message)
        {
            var audioViewModel = message.AudioViewModel;
            var ffmpegNotFound = false;
            try
            {
                string saveFolder = null;
                if (string.IsNullOrWhiteSpace(_appSettingsService.DefaultSavePath))
                {
                    var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                    dialog.UseDescriptionForTitle = true;
                    dialog.Description = "Save audio to";
                    var dialogResult = dialog.ShowDialog();
                    saveFolder = dialog.SelectedPath;
                }
                else
                {
                    saveFolder = _appSettingsService.DefaultSavePath;
                }

                if (string.IsNullOrWhiteSpace(saveFolder))
                    return;

                // Don't allow UI thread go deep than need
                await Task.Run(() => _serviceProvider.GetService<IAudioDownloaderService>().DownloadMP3(audioViewModel.Url));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
            finally
            {
                audioViewModel.IsDownloading = false;
            }

            if (ffmpegNotFound)
            {
                var result = await MaterialMessageBox.ShowAsync(
                    "FFmpeg exception",
                    "FFmpeg executable files not found! Please, specify them in settings.",
                    MaterialMessageBoxButtons.OK,
                    DialogIdentifiers.MainWindowName,
                    PackIconKind.Error);
            }
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            var settingsView = _serviceProvider.GetService<SettingsView>();
            var result = await DialogHost.Show(settingsView, DialogIdentifiers.MainWindowName);
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            StrongReferenceMessenger.Default.Register<DownloadAudioMessage>(this);
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            StrongReferenceMessenger.Default.Unregister<DownloadAudioMessage>(this);
        }
    }
}
