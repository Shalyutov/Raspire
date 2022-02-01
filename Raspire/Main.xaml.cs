using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Raspire
{
    /// <summary>
    /// Главная навигационная страница. Хранит Frame и осуществляет переходы на другие страницы приложения.
    /// </summary>
    public sealed partial class Main : Page
    {
        public Main()
        {
            this.InitializeComponent();
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            MainFrame.Navigate(typeof(SchedulePage));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = e.Parameter as List<object>;
            if (parameter != null)
            {
                MainFrame.Navigate(typeof(EditorV3), parameter, new DrillInNavigationTransitionInfo());
            }
        }
        private void NavSchedule(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content.GetType() != typeof(SchedulePage))
            {
                _ = MainFrame.Navigate(typeof(SchedulePage));
            }
        }

        private void NavEditor(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content.GetType() != typeof(EditorV2))
            {
                _ = MainFrame.Navigate(typeof(EditorV2));
            }
        }

        private void NavHelp(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content.GetType() != typeof(HelpPage))
            {
                _ = MainFrame.Navigate(typeof(HelpPage));
            }
        }

        private void NavSettings(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content.GetType() != typeof(SettingsPage))
            {
                _ = MainFrame.Navigate(typeof(SettingsPage));
            }
        }

        private void MainNav(object sender, NavigationEventArgs e)
        {
            FileNameParent.Visibility = Visibility.Collapsed;
            Type page = (sender as Frame).Content.GetType();
            
            string p;
            if (page == typeof(SchedulePage))
            {
                p = "Расписание";
                
            }
            else if (page == typeof(EditorV2))
            {
                p = "Редактор";
                FileNameParent.Visibility = Visibility.Visible;
                if (e.Parameter!=null) FileName.Text = ((e.Parameter as List<object>)[0] as Schedule).Name;
                
            }
            else if (page == typeof(EditorV3))
            {
                p = "Редактор";
                FileNameParent.Visibility = Visibility.Visible;
                if (e.Parameter != null) FileName.Text = ((e.Parameter as List<object>)[0] as Schedule).Name;
                
            }
            else if (page == typeof(HelpPage))
            {
                p = "Поддержка";
                
            }
            else if (page == typeof(SettingsPage))
            {
                p = "Настройки";
                
            }
            else
            {
                return;
            }
            FrameState.Text = p;
            
        }

    }
}
