using Environment;
using System;
using Windows.UI.Xaml.Data;

namespace SimEarth2020App
{
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
