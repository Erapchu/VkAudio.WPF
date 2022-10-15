using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using VkAudio.WPF.Enums;
using VkAudio.WPF.Views.Helpers;

namespace VkAudio.WPF.ViewModels
{
    [INotifyPropertyChanged]
    internal partial class FfmpegChoiceViewModel
    {
        [RelayCommand]
        private void Download()
        {
            if (DialogHost.IsDialogOpen(DialogIdentifiers.MainWindowName))
                DialogHost.Close(DialogIdentifiers.MainWindowName, FfmpegChoiceEnum.Download);
        }

        [RelayCommand]
        private void SetPaths()
        {
            if (DialogHost.IsDialogOpen(DialogIdentifiers.MainWindowName))
                DialogHost.Close(DialogIdentifiers.MainWindowName, FfmpegChoiceEnum.SetPath);
        }
    }
}
