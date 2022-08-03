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
        private PrintHelper PrintHelper { get; set; }
        public PrintWizard(ObservableCollection<WorkdayForms> units, Canvas canvas)
        {
            this.InitializeComponent();
            Units = units;
            PrintHelper = new PrintHelper(canvas);
            if (DateTime.Now.Month > 6)
            {
                Number.Text = "1";
                Year.Text = $"{DateTime.Now.Year%100}/{(DateTime.Now.Year + 1)%100}";
            }
            else
            {
                Number.Text = "2";
                Year.Text = $"{(DateTime.Now.Year - 1)%100}/{DateTime.Now.Year%100}";
            }
            Law.Text = "";
            Date.SelectedDate = DateTime.Now;
        }

        private async void PrintA3Land(object sender, RoutedEventArgs e)
        {
            var options = new PrintHelperOptions
            {
                Orientation = PrintOrientation.Landscape,
                MediaSize = PrintMediaSize.IsoA3
            };

            ObservableCollection<WorkdayForms> workdayForms = new ObservableCollection<WorkdayForms>();
            int i = 0;
            for (int w = 0; w < Units.Count;)
            {
                if (Units[w].FormLessons.Count < 15 * i)
                {
                    if (w == Units.Count - 1)
                    {
                        break;
                    }
                    w++;
                    continue;
                };
                WorkdayForms item = new WorkdayForms(Units[w].Workday, new ObservableCollection<FormLessons>());
                for (int l = 15 * i; l < Units[w].FormLessons.Count;)
                {
                    item.FormLessons.Add(Units[w].FormLessons[l]);
                    l++;
                    if (l % 15 == 0)
                    {
                        break;
                    }
                }
                workdayForms.Add(item);

                if (workdayForms.Count == 5)
                {
                    PrintHelper.AddFrameworkElementToPrint(new LayoutA3Landscape(workdayForms, Number.Text, Year.Text, Law.Text, Date.Date.Date));
                    workdayForms = new ObservableCollection<WorkdayForms>();

                    w++;
                    continue;
                }
                if (w == Units.Count - 1)
                {
                    PrintHelper.AddFrameworkElementToPrint(new LayoutA3Landscape(workdayForms, Number.Text, Year.Text, Law.Text, Date.Date.Date));
                    workdayForms = new ObservableCollection<WorkdayForms>();
                    w = 0;
                    i++;
                }
                else
                {
                    w++;
                }
            }
            if (workdayForms.Count > 0) PrintHelper.AddFrameworkElementToPrint(new LayoutA3Landscape(workdayForms, Number.Text, Year.Text, Law.Text, Date.Date.Date));

            await PrintHelper.ShowPrintUIAsync("Печать расписания", options);

            //Hide();
        }

        private async void PrintA4Book(object sender, RoutedEventArgs e)
        {
            var options = new PrintHelperOptions
            {
                Orientation = PrintOrientation.Portrait,
                MediaSize = PrintMediaSize.IsoA4
            };

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
                    PrintHelper.AddFrameworkElementToPrint(new LayoutA4Book(workdayForms, Number.Text, Year.Text, Law.Text, Date.Date.Date));
                    workdayForms = new ObservableCollection<WorkdayForms>();
                    w = 0;
                    i++;
                }
                else
                {
                    w++;
                }
            }
            if (workdayForms.Count > 0) PrintHelper.AddFrameworkElementToPrint(new LayoutA4Book(workdayForms, Number.Text, Year.Text, Law.Text, Date.Date.Date));

            await PrintHelper.ShowPrintUIAsync("Печать расписания", options);

            //Hide();
        }
        private async void PrintA4Land(object sender, RoutedEventArgs e)
        {
            var options = new PrintHelperOptions
            {
                Orientation = PrintOrientation.Landscape,
                MediaSize = PrintMediaSize.IsoA4
            };

            ObservableCollection<WorkdayForms> workdayForms = new ObservableCollection<WorkdayForms>();
            int i = 0;
            for (int w = 0; w < Units.Count;)
            {
                if (Units[w].FormLessons.Count < 15 * i)
                {
                    if (w == Units.Count - 1)
                    {
                        break;
                    }
                    w++;
                    continue;
                };
                WorkdayForms item = new WorkdayForms(Units[w].Workday, new ObservableCollection<FormLessons>());
                for (int l = 15 * i; l < Units[w].FormLessons.Count;)
                {
                    item.FormLessons.Add(Units[w].FormLessons[l]);
                    l++;
                    if (l % 15 == 0)
                    {
                        break;
                    }
                }
                workdayForms.Add(item);

                if (workdayForms.Count == 5)
                {
                    PrintHelper.AddFrameworkElementToPrint(new LayoutA4Land(workdayForms, Number.Text, Year.Text, Law.Text, Date.Date.Date));
                    workdayForms = new ObservableCollection<WorkdayForms>();

                    w++;
                    continue;
                }
                if (w == Units.Count - 1)
                {
                    PrintHelper.AddFrameworkElementToPrint(new LayoutA4Land(workdayForms, Number.Text, Year.Text, Law.Text, Date.Date.Date));
                    workdayForms = new ObservableCollection<WorkdayForms>();
                    w = 0;
                    i++;
                }
                else
                {
                    w++;
                }
            }
            if (workdayForms.Count > 0) PrintHelper.AddFrameworkElementToPrint(new LayoutA4Land(workdayForms, Number.Text, Year.Text, Law.Text, Date.Date.Date));

            await PrintHelper.ShowPrintUIAsync("Печать расписания", options);

            //Hide();
        }
        private void LawUpdate(object sender, RoutedEventArgs e)
        {
            Law.IsEnabled = (bool)(sender as CheckBox).IsChecked;
            Date.IsEnabled = (bool)(sender as CheckBox).IsChecked;
            if (Law.IsEnabled)
            {
                if (Law.Text == "") Law.Text = "1";
                Law.Focus(FocusState.Programmatic);
            }
            else
            {
                Law.Text = "";
            }
        }
        private void HideDialog(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
