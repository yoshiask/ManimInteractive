using Fluent;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ManimInteractive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.IsAutomaticWindowsAppModeSettingSyncEnabled = true;
            ThemeManager.SyncThemeWithWindowsAppModeSetting();

            ThemeManager.ChangeTheme(Application.Current, "Dark.Steel");

            

            // now change theme to the custom theme
            //ThemeManager.ChangeTheme(Application.Current, ThemeManager.GetTheme("Office2016"));

            base.OnStartup(e);
        }
    }
}
