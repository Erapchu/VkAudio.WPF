using System.Windows.Controls;
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

        private void ListView_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (e.OriginalSource is not ScrollViewer scrollViewer)
                return;

            if (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight)
            {
                ViewModel.GetNextAudiosCommand.ExecuteAsync(null);
            }
        }
    }
}
