using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VkAudio.WPF.ViewModels;

namespace VkAudio.WPF.Views
{
    /// <summary>
    /// Interaction logic for MaterialInputBoxContent.xaml
    /// </summary>
    public partial class MaterialInputBoxContent : UserControl
    {
        public MaterialInputBoxViewModel ViewModel { get; }

        public MaterialInputBoxContent(MaterialInputBoxViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(200);
            TextInputBox.Focus();
        }

        private void PasswordInputBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.Password = PasswordInputBox.Password;
        }
    }
}
