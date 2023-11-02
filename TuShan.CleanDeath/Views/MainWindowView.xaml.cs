using HandyControl.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TuShan.CleanDeath.Views
{
    /// <summary>
    /// MainWindowView.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowView : System.Windows.Window
    {
        public MainWindowView()
        {
            InitializeComponent();
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContextMenu menu = (sender as StackPanel).ContextMenu;
            MenuItem item = menu.Items[0] as MenuItem;
            item.Header = Resources["Clear"];
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Growl.Clear();
        }
    }
}
