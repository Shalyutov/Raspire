using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Printing;

namespace Raspire
{
    public class PrintHelper
    {
        /// <summary>
        /// PrintDocument is used to prepare the pages for printing.
        /// Prepare the pages to print in the handlers for the Paginate, GetPreviewPage, and AddPages events.
        /// </summary>
        protected PrintDocument printDocument;

        /// <summary>
        /// Marker interface for document source
        /// </summary>
        protected IPrintDocumentSource printDocumentSource;

        /// <summary>
        /// A list of UIElements used to store the print preview pages.  This gives easy access
        /// to any desired preview page.
        /// </summary>
        internal List<FrameworkElement> printPreviewPages;

        /// <summary>
        ///  A reference back to the scenario page used to access XAML elements on the scenario page
        /// </summary>
        protected Page scenarioPage;

        internal List<FrameworkElement> Pages;

        private ObservableCollection<WorkdayForms> Units;
        private int A4Book = 0;
        private int A4Land = 0;
        //private int A3Book = 0;
        private int A3Land = 1;

        /// <summary>
        ///  A hidden canvas used to hold pages we wish to print
        /// </summary>
        protected Canvas PrintCanvas
        {
            get
            {
                return scenarioPage.FindName("PrintCanvas") as Canvas;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scenarioPage">The scenario page constructing us</param>
        public PrintHelper(Page scenarioPage, ObservableCollection<WorkdayForms> units)
        {
            this.scenarioPage = scenarioPage;
            printPreviewPages = new List<FrameworkElement>();
            Pages = new List<FrameworkElement>();

            Units = units;
        }

        public void StructLandA4()
        {
            ObservableCollection<WorkdayForms> workdayForms = new ObservableCollection<WorkdayForms>();
            int count = 0;
            foreach (WorkdayForms unit in Units)
            {
                workdayForms.Add(unit);
                count++;
                if (count % 3 == 0)
                {
                    PreparePrintContent(new ScheduleLayout(workdayForms));
                    workdayForms = new ObservableCollection<WorkdayForms>();
                    count = 0;
                }
            }
            if (workdayForms.Count > 0) PreparePrintContent(new ScheduleLayout(workdayForms));
            A4Land = Pages.Count - A4Book;
        }
        public void StructBookA4()
        {
            ObservableCollection<WorkdayForms> workdayForms = new ObservableCollection<WorkdayForms>();
            for (int w = 0; w < Units.Count; w++)
            {
                if (Units[w].FormLessons.Count < 1) continue;
                WorkdayForms item = new WorkdayForms(Units[w].Workday, new ObservableCollection<FormLessons>());
                for(int l = 0; l< Units[w].FormLessons.Count; l++)
                {
                    item.FormLessons.Add(Units[w].FormLessons[l]);
                    if (l % 6 == 0)
                    {
                        break;
                    }
                }
                workdayForms.Add(item);
                if(w == Units.Count - 1)
                {
                    PreparePrintContent(new ScheduleLayout(workdayForms));
                    workdayForms = new ObservableCollection<WorkdayForms>();
                    w = 0;
                }
            }
            if (workdayForms.Count > 0) PreparePrintContent(new ScheduleLayout(workdayForms));
            A4Book = Pages.Count;
        }
        /// <summary>
        /// This function registers the app for printing with Windows and sets up the necessary event handlers for the print process.
        /// </summary>
        public virtual void RegisterForPrinting()
        {
            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;
            printDocument.Paginate += CreatePrintPreviewPages;
            printDocument.GetPreviewPage += GetPrintPreviewPage;
            printDocument.AddPages += AddPrintPages;

            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested += PrintTaskRequested;
        }

        /// <summary>
        /// This function unregisters the app for printing with Windows.
        /// </summary>
        public virtual void UnregisterForPrinting()
        {
            if (printDocument == null)
            {
                return;
            }

            printDocument.Paginate -= CreatePrintPreviewPages;
            printDocument.GetPreviewPage -= GetPrintPreviewPage;
            printDocument.AddPages -= AddPrintPages;
            printDocument = null;

            // Remove the handler for printing initialization.
            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested -= PrintTaskRequested;

            PrintCanvas.Children.Clear();
        }

        public async Task ShowPrintUIAsync()
        {
            try
            {
                await PrintManager.ShowPrintUIAsync();
            }
            catch (Exception e)
            {
                MessageDialog dialog = new MessageDialog("Ошибка вывода на печать: " + e.Message + ", hr=" + e.HResult);
                _ = await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Method that will generate print content for the scenario
        /// For scenarios 1-4: it will create the first page from which content will flow
        /// Scenario 5 uses a different approach
        /// </summary>
        /// <param name="page">The page to print</param>
        public virtual void PreparePrintContent(FrameworkElement page)
        {
            PrintCanvas.Children.Add(page);
            PrintCanvas.InvalidateMeasure();
            PrintCanvas.UpdateLayout();

            Pages.Add(page);
        }

        /// <summary>
        /// This is the event handler for PrintManager.PrintTaskRequested.
        /// </summary>
        /// <param name="sender">PrintManager</param>
        /// <param name="e">PrintTaskRequestedEventArgs </param>
        protected virtual void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)
        {
            PrintTask printTask = null;
            printTask = e.Request.CreatePrintTask("Печать текущего расписания", sourceRequested =>
            {
                //var deferral = sourceRequested.GetDeferral();
                PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(printTask.Options);
                IList<string> displayedOptions = printDetailedOptions.DisplayedOptions;

                // Choose the printer options to be shown.
                // The order in which the options are appended determines the order in which they appear in the UI
                displayedOptions.Clear();

                displayedOptions.Add(StandardPrintTaskOptions.Orientation);
                displayedOptions.Add(StandardPrintTaskOptions.MediaSize);

                printTask.Options.MediaSize = PrintMediaSize.IsoA3;
                printTask.Options.Orientation = PrintOrientation.Landscape;

                printDetailedOptions.OptionChanged += printDetailedOptions_OptionChanged;
                // Print Task event handler is invoked when the print job is completed.
                printTask.Completed += async (s, args) =>
                {
                    // Notify the user when the print operation fails.
                    if (args.Completion == PrintTaskCompletion.Failed)
                    {
                        await scenarioPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async() =>
                        {
                            MessageDialog dialog = new MessageDialog("Не удалось вывести на печать");
                            _ = await dialog.ShowAsync();
                        });
                    }
                };
                sourceRequested.SetSource(printDocumentSource);

                //deferral.Complete();
            });
        }
        /// <summary>
        /// This is the event handler for whenever the user makes changes to the options. 
        /// In this case, the options of interest are PageContent, Margins and Header.
        /// </summary>
        /// <param name="sender">PrintTaskOptionDetails</param>
        /// <param name="args">PrintTaskOptionChangedEventArgs</param>
        async void printDetailedOptions_OptionChanged(PrintTaskOptionDetails sender, PrintTaskOptionChangedEventArgs args)
        {
            string optionId = args.OptionId as string;
            if (string.IsNullOrEmpty(optionId))
            {
                return;
            }
            var value_m = sender.Options["PageMediaSize"] as PrintMediaSizeOptionDetails;
            var value_o = sender.Options["PageOrientation"] as PrintOrientationOptionDetails;

            /*printPreviewPages.Clear();
            if ((PrintMediaSize)value_m.Value == PrintMediaSize.IsoA4)
            {
                if ((PrintOrientation)value_o.Value == PrintOrientation.Landscape)
                {
                    for(int i = A4Book; i < A4Book + A4Land; i++)
                    {
                        printPreviewPages.Add(Pages[i]);
                    }
                    //StructLandA4();
                }
                else if ((PrintOrientation)value_o.Value == PrintOrientation.Portrait)
                {
                    for (int i = 0; i < A4Book; i++)
                    {
                        printPreviewPages.Add(Pages[i]);
                    }
                }
            }
            else if ((PrintMediaSize)value_m.Value == PrintMediaSize.IsoA3)
            {
                printPreviewPages.Add(Pages[A4Book + A4Land]);
                //PreparePrintContent(new ScheduleLayout(Units));
            }*/
            await scenarioPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PrintCanvas.InvalidateMeasure();
                PrintCanvas.UpdateLayout();
                
                printDocument.InvalidatePreview();
            });
        }

        /// <summary>
        /// This is the event handler for PrintDocument.Paginate. It creates print preview pages for the app.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Paginate Event Arguments</param>
        protected virtual void CreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
            PrintCanvas.Children.Clear();
            printDocument.SetPreviewPageCount(Pages.Count, PreviewPageCountType.Final);
        }

        /// <summary>
        /// This is the event handler for PrintDocument.GetPrintPreviewPage. It provides a specific print preview page,
        /// in the form of an UIElement, to an instance of PrintDocument. PrintDocument subsequently converts the UIElement
        /// into a page that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Arguments containing the preview requested page</param>
        protected virtual void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            printDocument.SetPreviewPage(e.PageNumber, Pages[e.PageNumber - 1]);
        }

        /// <summary>
        /// This is the event handler for PrintDocument.AddPages. It provides all pages to be printed, in the form of
        /// UIElements, to an instance of PrintDocument. PrintDocument subsequently converts the UIElements
        /// into a pages that the Windows print system can deal with.
        /// </summary>
        /// <param name="sender">PrintDocument</param>
        /// <param name="e">Add page event arguments containing a print task options reference</param>
        protected virtual void AddPrintPages(object sender, AddPagesEventArgs e)
        {
            foreach (FrameworkElement page in Pages)
            {
                printDocument.AddPage(page);
            }
            printDocument.AddPagesComplete();
        }
    }
}
