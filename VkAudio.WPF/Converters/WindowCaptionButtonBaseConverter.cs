﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace VkAudio.WPF.Converters
{
    /// <summary>
    /// Base class for the converters controlling the visibility and enabled state of window caption buttons.
    /// </summary>
    public abstract class WindowCaptionButtonBaseConverter : IMultiValueConverter
    {
        /// <summary>
        /// Identifier for the minimize caption button.
        /// </summary>
        public string MinimizeButtonName => "MinimizeButton";

        /// <summary>
        /// Identifier for the maximize/restore caption button.
        /// </summary>
        public string MaximizeRestoreButtonName => "MaximizeRestoreButton";

        /// <summary>
        /// Identifier for the close caption button.
        /// </summary>
        public string CloseButtonName => "CloseButton";

        /// <summary>
        /// Creates a new <see cref="WindowCaptionButtonBaseConverter" />.
        /// </summary>
        public WindowCaptionButtonBaseConverter() { }

        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
