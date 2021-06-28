using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace CCube
{
    public class ProgressMaxValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value == 0 ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class VisibilityToBlurRadiusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (values ?? new object[0]).Where(value => (Visibility)value == Visibility.Visible).Count() > 0 ? 5d : 0d;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class ImportStatusToEnabledConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            (ImportManager.ImportStatusOptions)value == ImportManager.ImportStatusOptions.Idle;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class ImportStatusToContentConverter : MarkupExtension, IValueConverter, IMultiValueConverter
    {
        public object StartContent { get; set; }
        public object StopContent { get; set; }
        public object StoppingContent { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            Convert(new[] { value, false, false }, targetType, parameter, culture);

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ImportManager.ImportStatusOptions)values[0])
            {
                case ImportManager.ImportStatusOptions.Running: return StopContent ?? ApplicationData.Service.MainWindow?.Resources["Stop"];
                case ImportManager.ImportStatusOptions.Stopping: return StoppingContent ?? ApplicationData.Service.MainWindow?.Resources[(bool)values[1] && (bool)values[2] ? "Skull" : "Hourglass"];
                default: return StartContent ?? ApplicationData.Service.MainWindow?.Resources["Start"];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotificationCountToMinHeightConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => ((int)value) == 0 ? 0 : 100;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class StringToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class BoolInverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => !((bool)value);

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => ((bool)value) ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class StartStopButtonEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            (((ImportManager.ImportStatusOptions)values[0]) != ImportManager.ImportStatusOptions.Stopping || ((int)values[1]) == 0) && (bool)values[2];

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class TitleValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var titles = (string[])parameter ?? new string[0];
            var titlesLength = titles.Length;

            var outputList = new List<object>(titlesLength);

            for (var i = 0; i < titlesLength; ++i)
            {
                var value = values?[i];

                if (value != null)
                {
                    var valueType = value.GetType();

                    switch (valueType.Name)
                    {
                        case "TimeSpan":
                            value = ((TimeSpan)value).ToString("hh':'mm':'ss");
                            break;
                    }
                }

                outputList.Add(new
                {
                    Title = titles[i],
                    Value = value
                });
            }

            return outputList;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
