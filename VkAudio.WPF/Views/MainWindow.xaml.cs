using VkAudio.WPF.Controls;
using VkAudio.WPF.ViewModels;

namespace VkAudio.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : MaterialWindow
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            ViewModel = viewModel;
            ViewModel.IsActive = true;
        }

        private void MaterialWindow_Closed(object sender, System.EventArgs e)
        {
            ViewModel.IsActive = false;
        }
    }
}
