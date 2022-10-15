using System.Windows.Controls;
using VkAudio.WPF.ViewModels;

namespace VkAudio.WPF.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    internal partial class SettingsView : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsView(SettingsViewModel settingsViewModel)
        {
            InitializeComponent();
            DataContext = settingsViewModel;
            ViewModel = settingsViewModel;
        }
    }
}
