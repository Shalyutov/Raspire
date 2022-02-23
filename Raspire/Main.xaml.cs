using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
                MainFrame.Navigate(typeof(Editor), parameter, new DrillInNavigationTransitionInfo());
            }
        }
        private void MainNav(object sender, NavigationEventArgs e)
        {
            FileNameParent.Visibility = Visibility.Collapsed;
            Type page = (sender as Frame).Content.GetType();
            
            string p;
            if (page == typeof(SchedulePage))
            {
                p = "Raspire";
                NavView.SelectedItem = NavDocItem;
            }
            else if (page == typeof(Editor))
            {
                p = "Редактор";
                FileNameParent.Visibility = Visibility.Visible;
                if (e.Parameter != null) FileName.Text = ((e.Parameter as List<object>)[1] as StorageFile).DisplayName;
                NavView.SelectedItem = NavEditItem;
            }
            else if (page == typeof(HelpPage))
            {
                p = "Поддержка";
                NavView.SelectedItem = NavHelpItem;
            }
            else if (page == typeof(SettingsPage))
            {
                p = "Настройки";
                NavView.SelectedItem = NavView.SettingsItem;
            }
            else
            {
                NavView.SelectedItem = null;
                return;
            }
            FrameState.Text = p;
        }

        private void NavItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            Type pageType;
            if (args.InvokedItem.ToString() == NavDocItem.Content.ToString())
            {
                pageType = typeof(SchedulePage);
            }
            else if (args.InvokedItem.ToString() == NavEditItem.Content.ToString())
            {
                pageType = typeof(Editor);
            }
            else if (args.InvokedItem.ToString() == NavHelpItem.Content.ToString())
            {
                pageType = typeof(HelpPage);
            }
            else if (args.IsSettingsInvoked)
            {
                pageType = typeof(SettingsPage);
            }
            else
            {
                return;
            }
            MainFrame.Navigate(pageType, null, new DrillInNavigationTransitionInfo());

        }
    }
}
