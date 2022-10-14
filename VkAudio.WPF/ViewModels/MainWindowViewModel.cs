using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkAudio.WPF.Collections;
using VkAudio.WPF.Models;
using VkAudio.WPF.Settings;
using VkAudio.WPF.Views.Helpers;
using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VkAudio.WPF.ViewModels
{
    [INotifyPropertyChanged]
    internal partial class MainWindowViewModel
    {
        private readonly IVkApi _vkApi;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly AppSettingsService _appSettingsService;

        [ObservableProperty]
        private bool _isAuthorized;

        [ObservableProperty]
        private Uri _photo100;

        [ObservableProperty]
        private string _userName;

        public ObservableCollectionDelayed<AudioViewModel> AudioViewModels { get; set; }

        private MainWindowViewModel()
        {
        }

        public MainWindowViewModel(
            IVkApi vkApi,
            ILogger<MainWindowViewModel> logger,
            AppSettingsService appSettingsService)
        {
            _vkApi = vkApi;
            _logger = logger;
            _appSettingsService = appSettingsService;
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
        }

        [RelayCommand]
        private async Task Loaded()
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
        }

        [RelayCommand]
        private async Task RefreshAudio()
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
                    Url = audio.url
                });
            }

            AudioViewModels = new ObservableCollectionDelayed<AudioViewModel>(audioVMs);
            OnPropertyChanged(nameof(AudioViewModels));
        }
    }
}
