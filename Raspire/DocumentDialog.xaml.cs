using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Raspire
{
    public sealed partial class DocumentDialog : ContentDialog
    {
        Schedule Schedule;
        bool close = false;
        public DocumentDialog(Schedule schedule)
        {
            this.InitializeComponent();
            Schedule = schedule;
            switch (Schedule.Type)
            {
                case 0:
                    SemiButton.Visibility = Visibility.Visible;
                    QuartButton.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    SemiButton.Visibility = Visibility.Collapsed;
                    QuartButton.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void ChangeSemi(object sender, RoutedEventArgs e)
        {
            Schedule.Number = int.Parse((sender as Button).Content.ToString());
            ChangeSemiScheduleFlyout.Hide();
            Bindings.Update();
        }
        public string GetSemiScheduleLabel()
        {
            return Schedule != null ? $"{Schedule.Number} полугодие" : "-";
        }
        private void ChangeQuarter(object sender, RoutedEventArgs e)
        {
            Schedule.Number = int.Parse((sender as Button).Content.ToString());
            ChangeQuarterScheduleFlyout.Hide();
            Bindings.Update();
        }
        public string GetQuarterScheduleLabel()
        {
            return Schedule != null ? $"{Schedule.Number} четверть" : "-";
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Schedule.Name = DocumentName.Text;
            PrimaryButtonCommandParameter = Schedule;
            close = true;
            Hide();
        }
        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MessageDialog dialog = new MessageDialog("Вы уверены, что хотите удалить файл с расписанием?");
            dialog.Commands.Add(new UICommand("Да, удалить", null, 1));
            dialog.Commands.Add(new UICommand("Отмена", null, 0));
            
            var res = await dialog.ShowAsync();
            if ((int)res.Id == 1)
            {
                /*if(Schedule.Path != "")
                {
                    var file= await Windows.Storage.StorageFile.GetFileFromPathAsync(Schedule.Path);
                    await file.DeleteAsync();
                }*/
                Schedule = null;
            }
            PrimaryButtonCommandParameter = Schedule;
            close = true;
            Hide();
        }
        private void ClosingDialog(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if(!close) args.Cancel = true;
        }
        public string GetTypeLabel()
        {
            switch (Schedule.Type)
            {
                case 0:
                    return "Полугодие";
                case 1:
                    return "Четверть";
                case 2:
                    return "Изменения";
                default:
                    return "?";
            }
        }
        private void ExportDocument(object sender, RoutedEventArgs e)//save document in path specifed by user
        {
            _ = Schedule.Export();
        }

    }
}
