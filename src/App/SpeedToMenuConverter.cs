using Environment;
using System;
using System.ComponentModel;
using Windows.UI.Xaml.Data;

namespace SimEarth2020App
{
    public class ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            Type type = value.GetType();

            var memInfo = type.GetMember(value.ToString());

            if (memInfo != null && memInfo.Length > 0)

            {

                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute),
                                                                false);

                if (attrs != null && attrs.Length > 0)

                    return ((DescriptionAttribute)attrs[0]).Description;

            }

            return parameter?.ToString() + value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class TerrainUpDownModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((TerrainUpDownMode)value)
            {
                case TerrainUpDownMode.Up:
                    return "⬆";
                case TerrainUpDownMode.Down:
                    return "⬇";
                default:
                    return "⬆/⬇";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class ScalingPctConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"{((double)value):N0} %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class IsNonNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class IsNullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class TerrainUpDownToCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var mode = (TerrainUpDownMode)value;
            return mode == TerrainUpDownMode.None ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SpeedToMenuConverter : IValueConverter
    {
        public IController AppController { get; set; }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return AppController.Speed == Enum.Parse<Speed>(parameter as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
