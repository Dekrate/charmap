using Microsoft.UI.Xaml;
using System;

namespace charmap
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                this.InitializeComponent();
                this.UnhandledException += OnUnhandledException;
                AppLogger.Info("Application initialized");
            }
            catch (Exception ex)
            {
                AppLogger.Fatal("Failed to initialize application", ex);
                throw;
            }
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            AppLogger.Fatal("Unhandled exception", e.Exception);
            e.Handled = true;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                AppLogger.Info("Application launching...");
                var window = new MainWindow();
                window.Activate();
                AppLogger.Info("MainWindow activated successfully");
            }
            catch (Exception ex)
            {
                AppLogger.Fatal("Failed to launch MainWindow", ex);
                throw;
            }
        }
    }
}
