using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DaysCounter2.Utils;

namespace DaysCounter2
{
    public partial class App : Application
    {
        public static string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DaysCounter2");
        public static string appFilePath = "", appSettingsPath = "";
        public static SettingsManager settings = new SettingsManager();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            appFilePath = Path.Combine(appDataPath, "days_counter_database.json");
            appSettingsPath = Path.Combine(appDataPath, "settings.json");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            if (File.Exists(appSettingsPath))
            {
                FileStream stream = File.OpenRead(appSettingsPath);
                try
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SettingsManager));
                    var result = ser.ReadObject(stream);
                    if (result != null)
                    {
                        settings = (SettingsManager)result;
                    }
                }
                catch { }
                stream.Close();
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Lang.Resources.Culture = new System.Globalization.CultureInfo(settings.languageId);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}