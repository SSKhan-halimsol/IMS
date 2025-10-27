using IMS.Data;
using IMS.Views;
using System;
using System.Data.Entity;
using System.Windows;

namespace IMS
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                Database.SetInitializer(new DbInitializer());

                using (var context = new IMSDbContext())
                {
                    context.Database.Initialize(force: false);
                }

                var splash = new SplashScreen("Photos/splash.png");
                splash.Show(autoClose: true);


                var mainWindow = new MainWindow();
                mainWindow.Show();

                Console.WriteLine("✅ Database initialized successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }
    }
}