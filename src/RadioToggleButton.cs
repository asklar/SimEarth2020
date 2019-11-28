using System.Windows.Controls;

namespace SimEarth2020
{
    public class RadioToggleButton : RadioButton
    {
        protected override void OnToggle()
        {
            if (IsChecked == true) IsChecked = IsThreeState ? (bool?)null : (bool?)false;
            else IsChecked = IsChecked.HasValue;

            if (!IsChecked.HasValue)
            {
                var main = Util.FindParent<MainWindow>(this);
                main.SetCurrentTool(Tool.None, null);
            }
        }
    }
}
