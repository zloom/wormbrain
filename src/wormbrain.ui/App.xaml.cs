using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using wormbrain.ui.Views;

namespace wormbrain.ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ViewModels.Main _mainViewModel;
       
        protected override void OnStartup(StartupEventArgs e)
        {
            var theme = ThemeManager.DetectAppStyle(Current);
            var random = new Random();
            var accent = ThemeManager.Accents.OrderBy(t => random.Next()).First();
            ThemeManager.ChangeAppStyle(Current, accent, theme.Item1);
            _mainViewModel = new ViewModels.Main();
            var main = new Main(_mainViewModel);
            main.Show();
            base.OnStartup(e);
        }


        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _mainViewModel.Dispose();
        }
    }
}
