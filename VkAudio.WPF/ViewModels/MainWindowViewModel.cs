using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using VkAudio.WPF.Collections;
using VkAudio.WPF.Models;
using VkAudio.WPF.Settings;
using VkAudio.WPF.Views.Helpers;
using VkNet.Abstractions;
using VkNet.Model;

namespace VkAudio.WPF.ViewModels
{
    [INotifyPropertyChanged]
    internal partial class MainWindowViewModel
    {
        #region Design time instance
        private static readonly Lazy<MainWindowViewModel> _lazy = new(GetDesignTimeVM);
        public static MainWindowViewModel DesignTimeInstance => _lazy.Value;

        private static MainWindowViewModel GetDesignTimeVM()
        {
            var vm = new MainWindowViewModel();
            return vm;
        }
        #endregion

        private readonly IVkApi _vkApi;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly AppSettingsService _appSettingsService;
        
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
                if (_appSettingsService.Token is null)
                {
                    var loginResult = await MaterialInputBox.ShowAsync("Login to VK", "Login", "Password", DialogIdentifiers.MainWindowName);
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
                        _appSettingsService.Token = _vkApi.Token;
                        await _appSettingsService.Save();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    var apiAuthParams = new ApiAuthParams()
                    {
                        AccessToken = _appSettingsService.Token
                    };
                    await _vkApi.AuthorizeAsync(apiAuthParams);
                }

                var parameters = new VkNet.Utils.VkParameters()
                {
                    {"v", "5.131" },
                    {"owner_id", "67912281" },
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
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
        }
    }
}
