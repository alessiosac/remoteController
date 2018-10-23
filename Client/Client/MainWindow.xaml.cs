using Client.Pages;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Switcher.pageSwitcher = this;
            //Switcher.Switch(new SendKey("ciao", 1, new TaskManager(), new Server() ));
            Switcher.Switch(new Home());
        }

        public void Navigate(UserControl nextPage)
        {
            this.Content = nextPage;
        }
    }
}
