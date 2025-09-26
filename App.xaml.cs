using System.Windows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        SplashScreen splash = new SplashScreen("Resources/splash.png");
        splash.Show(true);
    }
}