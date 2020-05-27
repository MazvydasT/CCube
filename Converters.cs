using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

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

    public class ImportStatusToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ImportManager.ImportStatusOptions)value)
            {
                case ImportManager.ImportStatusOptions.Idle: return "Start";
                case ImportManager.ImportStatusOptions.Running: return "Stop";
                case ImportManager.ImportStatusOptions.Stopping: return "Stopping";
                default: return null;
            }
        }

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
