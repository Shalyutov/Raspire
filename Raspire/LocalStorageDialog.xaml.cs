using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Raspire
{
    public sealed partial class LocalStorageDialog : ContentDialog
    {
        readonly ObservableCollection<Schedule> Schedules = new ObservableCollection<Schedule>();
        List<StorageFile> files = new List<StorageFile>();
        public LocalStorageDialog()
        {
            this.InitializeComponent();
            GetSchedules();
        }

        private async void GetSchedules()//todo left
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            foreach (StorageFile file in await storageFolder.GetFilesAsync())
            {
                string text = await FileIO.ReadTextAsync(file);
                Schedule schedule = JsonConvert.DeserializeObject<Schedule>(text);
                Schedules.Add(schedule);
                files.Add(file);
            }
            ProgressFiles.Visibility = Visibility.Collapsed;
            if(Schedules.Count == 0)
            {
                SchedulesList.Header = "Нет локальных расписаний";
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private void SelectedSchedule(object sender, SelectionChangedEventArgs e)
        {
            List<object> param = new List<object>() { SchedulesList.SelectedItem, files[(sender as ListView).SelectedIndex] };
            PrimaryButtonCommandParameter = param;
            Hide();
        }
        private async void OpenFileSchedule(object sender, RoutedEventArgs e)
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
                string text = await FileIO.ReadTextAsync(file);
                Schedule schedule = JsonConvert.DeserializeObject<Schedule>(text);
                //schedule.IsInternal = false;
                //schedule.Path = file.Path;
                List<object> param = new List<object>() { schedule, file };
                PrimaryButtonCommandParameter = param;
                Hide();
            }
        }
    }
}
