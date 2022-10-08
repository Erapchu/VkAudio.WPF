using System.ComponentModel;
using System.Windows;

namespace VkAudio.WPF.Views.Helpers
{
    internal static class DialogIdentifiers
    {
        public static string MainWindowName { get; } = "MainWindowDialogHost";

        public static bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());
    }
}
