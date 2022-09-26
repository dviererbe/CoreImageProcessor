using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CoreImageProcessor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string ApplicationName = "Core Image Processor";

        private const string EnvironmentVariableKey_UseCUDA = "CoreImageProcessor_UseCUDA";
        private const string EnvironmentVariableKey_ThreadLimit = "CoreImageProcessor_ThreadLimit";

        private static AppSettings? _settings;

        public static event EventHandler? SettingsChanged;

        internal static AppSettings Settings
        {
            get => _settings ?? AppSettings.Default;
            set
            {
                _settings = value;
                SettingsChanged?.Invoke(_settings, EventArgs.Empty);
            }
        }

        private static void LoadSettingsFromEnvironmentvariables()
        {
            bool useCUDA = AppSettings.Default.UseCUDA;
            int threadLimit = AppSettings.Default.ThreadLimit;

            try
            {
                if (bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKey_UseCUDA), out bool value))
                    useCUDA = value;
            }
            catch
            {
            }

            try
            {
                if (int.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKey_UseCUDA), out int value))
                    threadLimit = value;
            }
            catch
            {
            }

            _settings = new AppSettings(useCUDA, threadLimit);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            LoadSettingsFromEnvironmentvariables();
            SettingsChanged += OnSettingsChanged;

            base.OnStartup(e);
        }

        //Save Settings
        private void OnSettingsChanged(object? sender, EventArgs e)
        {
            try
            {
                Environment.SetEnvironmentVariable(EnvironmentVariableKey_UseCUDA, Settings.UseCUDA.ToString());
            }
            catch { }

            try
            {
                Environment.SetEnvironmentVariable(EnvironmentVariableKey_ThreadLimit, Settings.ThreadLimit.ToString());
            }
            catch { }
        }
    }
}
