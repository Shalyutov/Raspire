using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.AccessCache;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Raspire
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SchedulePage : Page
    {
        string recent = "";
        object continue_button;
        object schedule_startup;
        StorageFile file;
        bool doc_opened = false;
        public SchedulePage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            recent = SettingsHelper.GetStringLocal("Recent");
            continue_button = SettingsHelper.GetObjectRawLocal("ContinueButton");
            RecentUpdateLayout();
            UpdateLayout();
        }
        void ScheduleStartup()
        {
            if (recent != null && recent != "" && schedule_startup != null && !doc_opened)
            {
                if ((string)schedule_startup == "true")
                {
                    LoadRecentSchedule(null, null);
                }
            }
        }
        async void RecentUpdateLayout()
        {
            if (recent != null && recent != "" && continue_button != null)
            {
                if ((string)continue_button == "false")
                {
                    RecentScheduleButton.Visibility = Visibility.Collapsed;
                    return;
                }
                try
                {
                    file = await StorageFile.GetFileFromPathAsync(recent);
                }
                catch (Exception)
                {
                    RecentScheduleButton.Visibility = Visibility.Collapsed;
                    return;
                }
                RecentScheduleButton.Visibility = Visibility.Visible;
            }
        }
        public async void LoadRecentSchedule(object sender, RoutedEventArgs e)
        {
            if (doc_opened)
            {
                Frame.Navigate(typeof(EditorV2), null, new DrillInNavigationTransitionInfo());
                return;
            }
            if (file == null)
            {
                try
                {
                    file = await StorageFile.GetFileFromPathAsync(recent);
                }
                catch (Exception)
                {
                    return;
                }
            }
            if (file != null)
            {
                string text = await Windows.Storage.FileIO.ReadTextAsync(file);
                Schedule schedule = JsonConvert.DeserializeObject<Schedule>(text);
                List<object> param = new List<object>() { schedule, file };
                //schedule.IsInternal = false;
                //schedule.Path = file.Path;
                doc_opened = true;
                _ = Frame.Navigate(typeof(EditorV2), param, new DrillInNavigationTransitionInfo());
            }
            else
            {
                ((Frame.Parent as Grid).Children[1] as StackPanel).Visibility = Visibility.Collapsed;
            }
        }
        private async void CreateNewSchedule(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            if (settings.AreSettingsCorreсt())
            {
                Schedule schedule = new Schedule("Расписание", 0);
                
                List<object> param = new List<object>() { schedule, null};
                Frame.Navigate(typeof(EditorV2), param, new DrillInNavigationTransitionInfo());
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Перед началом работы с расписаниями проверьте данные о школе в Параметрах");
                _ = await dialog.ShowAsync();
                Frame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
            }
        }
        private async void CreateNewQuarterSchedule(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            if (settings.AreSettingsCorreсt())
            {
                //Schedule schedule = new Schedule("Расписание", new System.Collections.ObjectModel.ObservableCollection<LessonsClassUnit>(), 1, null);
                Schedule schedule = new Schedule("Расписание", 1);
                List<object> param = new List<object>() { schedule, null };
                Frame.Navigate(typeof(EditorV2), param, new DrillInNavigationTransitionInfo());
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Перед началом работы с расписаниями проверьте данные о школе в Параметрах");
                _ = await dialog.ShowAsync();
                Frame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
            }
        }
        private async void CreateNewChangesSchedule(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            if (settings.AreSettingsCorreсt())
            {
                Schedule schedule = new Schedule("Расписание", 2);
                List<object> param = new List<object>() { schedule, null };
                Frame.Navigate(typeof(EditorV2), param, new DrillInNavigationTransitionInfo());
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Перед началом работы с расписаниями проверьте данные о школе в Параметрах");
                _ = await dialog.ShowAsync();
                Frame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
            }
        }
        private async void OpenSchedule(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".rsl");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                _ = StorageApplicationPermissions.FutureAccessList.Add(file);
                string text = await FileIO.ReadTextAsync(file);
                Schedule schedule = JsonConvert.DeserializeObject<Schedule>(text);
                
                List<object> param = new List<object>() { schedule, file };
                Frame.Navigate(typeof(EditorV2), param, new DrillInNavigationTransitionInfo());
            }
            /*LocalStorageDialog dialog = new LocalStorageDialog();
            await dialog.ShowAsync();
            if (dialog.PrimaryButtonCommandParameter != null)
            {
                Frame.Navigate(typeof(EditorV2), dialog.PrimaryButtonCommandParameter, new DrillInNavigationTransitionInfo());
            }*/
        }
        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), null, new DrillInNavigationTransitionInfo());
        }
        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HelpPage), null, new DrillInNavigationTransitionInfo());
        }

        /*private async void OpenFileSchedule(object sender, RoutedEventArgs e)
        {
            ((Frame.Parent as Grid).Children[1] as StackPanel).Visibility = Visibility.Visible;
            Windows.Storage.Pickers.FileOpenPicker picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".json");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string text = await Windows.Storage.FileIO.ReadTextAsync(file);
                Schedule schedule = JsonConvert.DeserializeObject<Schedule>(text);
                schedule.IsInternal = false;
                schedule.Path = file.Path;
                Frame.Navigate(typeof(EditorV2), schedule, new DrillInNavigationTransitionInfo());
            }
            else
            {
                ((Frame.Parent as Grid).Children[1] as Microsoft.Toolkit.Uwp.UI.Controls.Loading).IsLoading = false;
            }
        }*/

        private void SchedulePageLoaded(object sender, RoutedEventArgs e)
        {
            schedule_startup = SettingsHelper.GetObjectRawLocal("ScheduleStartup");
            ScheduleStartup();
        }
    }
}
