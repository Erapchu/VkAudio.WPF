using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using VkAudio.WPF.Models;

namespace VkAudio.WPF.Views.Helpers
{
    internal static class MaterialInputBox
    {
        /// <summary>
        /// Shows materialized analog of input MessageBox. To use this method you need to set <see cref="DialogHost"/> instance in your window
        /// (typically this may be specified towards the root of a Window's XAML).
        /// </summary>
        /// <param name="header">Header of the dialog.</param>
        /// <param name="hint">Hint text.</param>
        /// <param name="dialogIdentifier"><see cref="DialogHost"/> identifier where need to show materialized message box. It's analog of window's HWND.</param>
        /// <param name="isPassword"><see langword="true"/> for password input.</param>
        /// <returns>Inputed text by user or <see langword="null"/> in case if output is not string.</returns>
        public static async Task<LoginResult> ShowAsync(
            string header,
            string textBoxHint,
            string passwordBoxHint,
            string dialogIdentifier)
        {
            var instance = (System.Windows.Application.Current as App).Services.GetService<MaterialInputBoxContent>();
            instance.ViewModel.Header = header;
            instance.ViewModel.TextBoxHint = textBoxHint;
            instance.ViewModel.PasswordBoxHint = passwordBoxHint;
            instance.ViewModel.DialogIdentifier = dialogIdentifier;
            var result = await DialogHost.Show(instance, dialogIdentifier);
            return result is LoginResult lr ? lr : null;
        }
    }
}
