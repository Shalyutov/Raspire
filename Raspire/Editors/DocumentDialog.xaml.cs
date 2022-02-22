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
        public DocumentDialog(Schedule schedule)
        {
            this.InitializeComponent();
            Schedule = schedule;
        }
        private void ChangeSemi(object sender, RoutedEventArgs e)
        {
            //Schedule.Number = int.Parse((sender as Button).Content.ToString());
            ChangeSemiScheduleFlyout.Hide();
            //Bindings.Update();
        }
        /*public string GetSemiScheduleLabel()
        {
            return Schedule != null ? $"{Schedule.Number} полугодие" : "-";
        }*/
        private void ChangeQuarter(object sender, RoutedEventArgs e)
        {
            //Schedule.Number = int.Parse((sender as Button).Content.ToString());
            ChangeQuarterScheduleFlyout.Hide();
            //Bindings.Update();
        }
        /*public string GetQuarterScheduleLabel()
        {
            return Schedule != null ? $"{Schedule.Number} четверть" : "-";
        }*/
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            PrimaryButtonCommandParameter = Schedule;
            Hide();
        }
        private void ExportDocument(object sender, RoutedEventArgs e)
        {
            _ = Schedule.Export();
        }

    }
}
