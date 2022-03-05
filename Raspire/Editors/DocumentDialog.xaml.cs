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
        private void PrimaryClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
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
