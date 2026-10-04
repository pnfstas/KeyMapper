using H.NotifyIcon;
using Microsoft.UI.Xaml;

namespace KeyMapper
{
    public partial class App : Application
    {
        public MainWindow? MainWindow { get; set; }
        public App()
        {
            InitializeComponent();
        }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
			MainWindow = new MainWindow();
			MainWindow.Activate();
        }
    }
}
