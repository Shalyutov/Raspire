using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Raspire.Editors
{
    public sealed partial class PrintWizard : ContentDialog
    {
        private ObservableCollection<WorkdayForms> Units { get; set; }
        private Canvas Canvas { get; set; }
        public PrintWizard(ObservableCollection<WorkdayForms> units, Canvas canvas)
        {
            this.InitializeComponent();
            Units = units;
            Canvas = canvas;
        }

        private async void PrintA3Land(object sender, RoutedEventArgs e)
        {
            var options = new PrintHelperOptions();

            options.Orientation = PrintOrientation.Landscape;
            options.MediaSize = PrintMediaSize.IsoA3;

            var printHelper = new PrintHelper(Canvas);

            printHelper.AddFrameworkElementToPrint(new LayoutA3Landscape(Units));

            await printHelper.ShowPrintUIAsync("Печать расписания", options);

            Hide();
        }

        private async void PrintA4Book(object sender, RoutedEventArgs e)
        {
            var options = new PrintHelperOptions();

            options.Orientation = PrintOrientation.Portrait;
            options.MediaSize = PrintMediaSize.IsoA4;

            var printHelper = new PrintHelper(Canvas);

            ObservableCollection<WorkdayForms> workdayForms = new ObservableCollection<WorkdayForms>();
            int i = 0;
            for (int w = 0; w < Units.Count;)
            {
                if (Units[w].FormLessons.Count < 7*i)
                {
                    if(w == Units.Count - 1)
                    {
                        break;
                    }
                    w++;
                    continue;
                };
                WorkdayForms item = new WorkdayForms(Units[w].Workday, new ObservableCollection<FormLessons>());
                for (int l = 7*i; l < Units[w].FormLessons.Count;)
                {
                    item.FormLessons.Add(Units[w].FormLessons[l]);
                    l++;
                    if (l % 7 == 0)
                    {
                        break;
                    }
                }
                workdayForms.Add(item);
                
                if (w == Units.Count - 1)
                {
                    printHelper.AddFrameworkElementToPrint(new LayoutA4Landscape(workdayForms));
                    workdayForms = new ObservableCollection<WorkdayForms>();
                    w = 0;
                    i++;
                }
                else
                {
                    w++;
                }
            }
            if (workdayForms.Count > 0) printHelper.AddFrameworkElementToPrint(new LayoutA4Landscape(workdayForms));

            await printHelper.ShowPrintUIAsync("Печать расписания", options);

            Hide();
        }
    }
}
