using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using VkAudio.WPF.Views.Helpers;

namespace VkAudio.WPF.ViewModels
{
    [INotifyPropertyChanged]
    public partial class MainWindowViewModel
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

        [RelayCommand]
        private async Task Login()
        {
            await MaterialInputBox.ShowAsync("Login to VK", "Login", "Password", DialogIdentifiers.MainWindowName);
        }
    }
}
