using System;
using System.Collections.Generic;
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

    public class ImportStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (ImportManager.ImportStatusOptions)value == ImportManager.ImportStatusOptions.Idle ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class ImportStatusToContentConverter : MarkupExtension, IValueConverter
    {
        public object StartContent { get; set; }
        public object StopContent { get; set; }
        public object StoppingContent { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ImportManager.ImportStatusOptions)value)
            {
                case ImportManager.ImportStatusOptions.Idle: return StartContent ?? ApplicationData.Service.MainWindow?.Resources["Start"];
                case ImportManager.ImportStatusOptions.Running: return StopContent ?? ApplicationData.Service.MainWindow?.Resources["Stop"];
                case ImportManager.ImportStatusOptions.Stopping: return StopContent ?? ApplicationData.Service.MainWindow?.Resources["Hourglass"];
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
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

    public class ParamsOutVisibleConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => !((bool)value);

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class StartStopButtonEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (((ImportManager.ImportStatusOptions)values[0]) == ImportManager.ImportStatusOptions.Stopping) return false;

            return true;
        }

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
