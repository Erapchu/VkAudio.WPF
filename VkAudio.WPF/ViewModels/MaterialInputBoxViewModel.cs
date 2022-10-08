using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using System;
using VkAudio.WPF.Models;

namespace VkAudio.WPF.ViewModels
{
    [INotifyPropertyChanged]
    public partial class MaterialInputBoxViewModel
    {
        private static readonly Lazy<MaterialInputBoxViewModel> _lazy = new(() => new MaterialInputBoxViewModel());
        public static MaterialInputBoxViewModel DesignTimeInstance => _lazy.Value;

        public string DialogIdentifier { get; set; }

        [ObservableProperty]
        private string _textBoxHint;

        [ObservableProperty]
        private string _passwordBoxHint;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
        private string _login;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
        private string _password;

        [ObservableProperty]
        private string _header;

        public MaterialInputBoxViewModel()
        {

        }

        [RelayCommand(CanExecute = nameof(CanAccept))]
        private void Accept()
        {
            if (DialogHost.IsDialogOpen(DialogIdentifier))
                DialogHost.Close(DialogIdentifier, new LoginResult(Login, Password));
        }

        private bool CanAccept()
        {
            return !string.IsNullOrWhiteSpace(_login) && !string.IsNullOrWhiteSpace(_password);
        }

        [RelayCommand]
        private void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogIdentifier))
                DialogHost.Close(DialogIdentifier);
        }
    }
}
