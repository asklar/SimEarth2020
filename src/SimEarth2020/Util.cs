using Windows.UI.Xaml;

namespace SimEarth2020
{
    public static class Util
    {
        public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            var p = element.Parent as FrameworkElement;
            while (p != null && !(p is T))
            {
                p = p.Parent as FrameworkElement;
            }
            return (T)p;
        }
    }

}
