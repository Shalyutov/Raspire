using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
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
using Windows.UI.Xaml.Printing;

namespace Raspire
{
    /// <summary>
    /// Страница логики главного редактора
    /// </summary>
    public sealed partial class EditorV2 : Page
    {
        public Schedule Schedule { get; set; }
        public ObservableCollection<LessonsClassUnit> LessonsClassUnits { get; set; }
        public StorageFile File { get; set; }
        private Settings SettingsInstance { get; set; }
        private ListView CurrentList;
        private readonly PrintHelper printHelper = null;
        private readonly List<UIElement> pages = new List<UIElement>();
        private bool shield = false;
        private int Mode { get; set; } = 0;
        public EditorV2()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsInstance = new Settings();
            if (e.Parameter != null)
            {
                if (Schedule == (Schedule)(e.Parameter as List<object>)[0] && Schedule != null) return;
                if (File == (StorageFile)(e.Parameter as List<object>)[1] && File != null) return;
                Schedule = (Schedule)(e.Parameter as List<object>)[0];
                File = (StorageFile)(e.Parameter as List<object>)[1];
                LessonsClassUnits = Schedule.GetStructLesssonClass();
                if (SettingsInstance.AreSettingsCorreсt())
                {
                    Schedule.Settings = SettingsInstance;
                }
                else if (Schedule.Settings.AreSettingsCorreсt())
                {
                    SettingsInstance = Schedule.Settings;
                }
                SavedModeUpdate();
                UpdateScheduleType();
                Bindings.Update();
                CurrentList = null;
                if (File != null)
                {
                    SettingsHelper.SaveObjectLocal("Recent", File.Path);
                }
                else
                {
                    SettingsHelper.SaveObjectLocal("Recent", "");
                }
                switch (Schedule.Type)
                {
                    case 0:
                        SemiButtonD.Visibility = Visibility.Visible;
                        QuartButtonD.Visibility = Visibility.Collapsed;
                        break;
                    case 1:
                        SemiButtonD.Visibility = Visibility.Collapsed;
                        QuartButtonD.Visibility = Visibility.Visible;
                        break;
                }
            }
            else if (Schedule == null)
            {
                _ = Frame.Navigate(typeof(SchedulePage));
            }
            else
            {
                Bindings.Update();
            }
        }
        private void UpdateScheduleType()
        {
            int c = 0;
            foreach(UIElement i in ScheduleTypeSwitcher.Children)
            {
                i.Visibility = c == Schedule.Type ? Visibility.Visible : Visibility.Collapsed;
                if (Schedule.Type == 2)
                {
                    DateSchedule.Date = DateTime.Now;
                    if (Schedule.Date != null)
                    {
                        DateSchedule.Date = Schedule.Date;
                    }
                }
                c++;
            }
        }
        private void SavedModeUpdate()
        {
            string SavedMode = SettingsHelper.GetStringLocal("SavedMode");
            if (SavedMode != null)
            {
                Mode = int.Parse(SavedMode);
                switch (Mode)
                {
                    case 0:
                        OpenMainHolder(null, null);
                        break;
                    case 1:
                        OpenClassHolder(null, null);
                        break;
                    default:
                        OpenMainHolder(null, null);
                        break;
                }
            }
            else
            {
                Mode = 0;
                OpenMainHolder(null, null);
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
        private async void OpenDocumentProperties(object sender, RoutedEventArgs e)
        {
            DocumentDialog document = new DocumentDialog(Schedule);
            _ = await document.ShowAsync();
            Schedule = document.PrimaryButtonCommandParameter as Schedule;
            if (Schedule == null)
            {
                _ = Frame.Navigate(typeof(SchedulePage), null, new DrillInNavigationTransitionInfo());
                SettingsHelper.SaveObjectLocal("Recent", "");
            }
            Bindings.Update();
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            PrintHelpInit();
        }
        private void AddEmptyLesson(object sender, RoutedEventArgs e)
        {
            (CurrentList.ItemsSource as ObservableCollection<LessonUnit>).Add(new LessonUnit());
            int f = GetCurrentListForm();
            int w = GetCurrentListWorkday(f);
            Schedule.LessonItems.Add(
                    new LessonItem(
                        new LessonUnit(),
                        Schedule.Settings.Forms[f],
                        Schedule.GetDay(Schedule.Settings.Workdays[w])
                    )
                );
        }
        private async void LessonEditOpen(object sender, RoutedEventArgs e)
        {
            shield = true;
            int f = GetCurrentListForm();
            int w = GetCurrentListWorkday(f);
            LessonEditor lessonEditor = new LessonEditor(CurrentList.SelectedItem as LessonUnit, SettingsInstance.SubjectUnits, SettingsInstance.TeacherUnits, SettingsInstance.Forms, f);
            _ = await lessonEditor.ShowAsync();
            if (lessonEditor.PrimaryButtonCommandParameter != null)
            {
                ObservableCollection<LessonUnit> src = CurrentList.ItemsSource as ObservableCollection<LessonUnit>;
                foreach (object o in CurrentList.SelectedItems)
                {
                    int index = 0;
                    foreach(LessonItem item in Schedule.LessonItems)
                    {
                        if ((o as LessonUnit).ToString() == item.Lesson.ToString() && item.Form.ToString() == Schedule.Settings.Forms[f].ToString() && item.Workday == Schedule.GetDay(w))
                        {
                            index = Schedule.LessonItems.IndexOf(item);
                            Schedule.LessonItems.Remove(item);
                            break;
                        }
                    }
                    src.Insert(src.IndexOf(o as LessonUnit), lessonEditor.PrimaryButtonCommandParameter as LessonUnit);

                    FormUnit form = Schedule.Settings.Forms[f];
                    string workday = Schedule.GetDay(Schedule.Settings.Workdays[GetCurrentListWorkday(f)]);
                    Schedule.LessonItems.Insert(index, new LessonItem(lessonEditor.PrimaryButtonCommandParameter as LessonUnit, form, workday));
                }
                List<object> unitsToDelete = new List<object>();
                unitsToDelete.AddRange(CurrentList.SelectedItems);
                foreach (object u in unitsToDelete)
                {
                    _ = src.Remove(u as LessonUnit);
                }
                CurrentList.ItemsSource = src;//what a heck? double lesson don't update label returning
                                              //i don't know why that thing sometimes do not work ^
                if (CurrentList.ContainerFromItem(lessonEditor.PrimaryButtonCommandParameter) is ListViewItem ihj)//fix patch
                {
                    (ihj.ContentTemplateRoot as TextBlock).Text = (ihj.Content as LessonUnit).ToString();//don't touch this
                }
            }
            else
            {
                DeleteLessons(null, null);
            }
            Bindings.Update();
            shield = false;
        }
        private void AddLesson(object sender, RoutedEventArgs e)//function finds best cadidate to assign lesson
        {
            Teacher teacher = null;
            int c = -1;
            foreach (Teacher t in SettingsInstance.TeacherUnits)
            {
                foreach (Host s in t.HostedSubjects)
                {
                    bool subjectMatch = s.Subject.ToString() == SubjectSelector.SelectedItem.ToString();
                    bool formMatch = false;
                    foreach (FormClassroom f in s.Forms)
                    {
                        int i = GetCurrentListForm();
                        if (f.Form.ToString() == SettingsInstance.Forms[i].ToString())
                        {
                            formMatch = true;
                            c = f.Cabinet;
                            break;
                        }
                    }
                    if (subjectMatch && formMatch)
                    {
                        teacher = t;
                        break;
                    }
                }
                if (teacher != null)
                {
                    break;
                }
            }
            if (teacher != null)
            {
                LessonUnit lesson = new LessonUnit(
                    (Subject)SubjectSelector.SelectedItem,
                    c,
                    teacher
                );
                int f = GetCurrentListForm();
                FormUnit form = Schedule.Settings.Forms[f];
                string workday = Schedule.GetDay(Schedule.Settings.Workdays[GetCurrentListWorkday(f)]);
                (CurrentList.ItemsSource as ObservableCollection<LessonUnit>).Add(lesson);
                Schedule.LessonItems.Add(new LessonItem(lesson, form, workday));
            }
        }
        private int GetCurrentListForm()
        {
            int i = 0;
            foreach (LessonsClassUnit unit in LessonsClassUnits)
            {
                if (unit.WorkdaysUnits.Contains((CurrentList.Parent as Grid).DataContext))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
        private int GetCurrentListWorkday(int form)
        {
            int w = 0;
            foreach (WorkdayLessons i in LessonsClassUnits[form].WorkdaysUnits)
            {
                if (i.LessonUnits == (CurrentList.ItemsSource as ObservableCollection<LessonUnit>))
                {
                    return w;
                }
                w++;
            }
            return -1;
        }
        private void DeleteLessons(object sender, RoutedEventArgs e)
        {
            if (CurrentList.SelectedItems != null)
            {
                int f = GetCurrentListForm();
                int w = GetCurrentListWorkday(f);
                List<object> unitsToDelete = new List<object>();
                unitsToDelete.AddRange(CurrentList.SelectedItems);
                foreach (object u in unitsToDelete)
                {
                    _ = (CurrentList.ItemsSource as ObservableCollection<LessonUnit>).Remove(u as LessonUnit);
                    foreach (LessonItem item in Schedule.LessonItems)
                    {
                        if ((u as LessonUnit).ToString() == item.Lesson.ToString() && item.Form.ToString() == Schedule.Settings.Forms[f].ToString() && item.Workday == Schedule.GetDay(w))
                        {
                            Schedule.LessonItems.Remove(item);
                            break;
                        }
                    }
                }
            }
        }
        private void RemoveAllLessonsList(object sender, RoutedEventArgs e)
        {
            (CurrentList.ItemsSource as ObservableCollection<LessonUnit>).Clear();
            int f = GetCurrentListForm();
            int w = GetCurrentListWorkday(f);
            foreach (LessonItem item in Schedule.LessonItems)
            {
                if (item.Form.ToString() == Schedule.Settings.Forms[f].ToString() && item.Workday == Schedule.GetDay(w))
                {
                    Schedule.LessonItems.Remove(item);
                    break;
                }
            }
        }
        private async void SaveDocument(object sender, RoutedEventArgs e)//save document in local app storage
        {
            SaveProgress.IsIndeterminate = true;
            SaveProgress.Visibility = Visibility.Visible;
            SaveProgressLabel.Text = "Сохранение";

            var f = await Schedule.Save(File);
            if (f != null)
            {
                File = f;
                SaveProgressLabel.Text = "Готово";
            }
            else
            {
                SaveProgressLabel.Text = "Ошибка";
            }

            SaveProgress.IsIndeterminate = false;
            SaveProgress.Visibility = Visibility.Collapsed;
            
            await Task.Delay(600);
            (sender as AppBarButton).Flyout.Hide();
        }
        private void ReassignLesson(object sender, RoutedEventArgs e)
        {
            /*shield = true;
            TeacherUnit teacher = null;
            int c = -1;
            foreach (TeacherUnit t in SettingsInstance.TeacherUnits)
            {
                foreach (HostUnit s in t.HostedSubjects)
                {
                    bool subjectMatch = s.Subject.ToString() == SubjectSelector.SelectedItem.ToString();
                    bool formMatch = false;
                    foreach (FormCabinetPair f in s.Forms)
                    {
                        if (f.Form.ToString() == SettingsInstance.FormUnits[GetCurrentListForm()].ToString())
                        {
                            formMatch = true;
                            c = f.Cabinet;
                            break;
                        }
                    }
                    if (subjectMatch && formMatch)
                    {
                        teacher = t;
                        break;
                    }
                }
                if (teacher != null)
                {
                    break;
                }
            }
            if (teacher != null)
            {
                ObservableCollection<LessonUnit> src = CurrentList.ItemsSource as ObservableCollection<LessonUnit>;
                foreach (object o in CurrentList.SelectedItems)
                {
                    src.Insert(src.IndexOf(o as LessonUnit), new LessonUnit((SubjectUnit)SubjectSelector.SelectedItem, c, teacher));
                }
                List<object> unitsToDelete = new List<object>();
                unitsToDelete.AddRange(CurrentList.SelectedItems);
                foreach (object u in unitsToDelete)
                {
                    _ = (CurrentList.ItemsSource as ObservableCollection<LessonUnit>).Remove(u as LessonUnit);
                }
                CurrentList.ItemsSource = src;
            }
            shield = false;
            Bindings.Update();*/
        }
        private void ListViewPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentList != null)
            {
                if (CurrentList.Parent != null)
                {
                    ((CurrentList.Parent as Grid).Children[0] as StackPanel).Visibility = Visibility.Collapsed;
                }
            }
            if (!shield)
            {
                CurrentList = sender as ListView;
                if (CurrentList.Parent != null)
                {
                    ((CurrentList.Parent as Grid).Children[0] as StackPanel).Visibility = Visibility.Visible;
                }

                _ = CurrentList.Focus(FocusState.Programmatic);
            }
        }
        private void PrintHelpInit()
        {
            /*PrintHelperOptions defaultPrintHelperOptions = new PrintHelperOptions();

            defaultPrintHelperOptions.AddDisplayOption(StandardPrintTaskOptions.Orientation);
            defaultPrintHelperOptions.Orientation = PrintOrientation.Portrait;

            //PrintCanvas.Visibility = Visibility.Visible;
            printHelper = new PrintHelper(PrintCanvas, defaultPrintHelperOptions);

            int c = 0;
            int c2 = 0;
            Schedule page = new Schedule(Schedule.Name, new ObservableCollection<LessonsClassUnit>(), Schedule.Type, Schedule.Date)
            {
                Number = Schedule.Number
            };
            Schedule page2 = new Schedule(Schedule.Name, new ObservableCollection<LessonsClassUnit>(), Schedule.Type, Schedule.Date)
            {
                Number = Schedule.Number
            };*/
            /*foreach (LessonsClassUnit i in LessonsClassUnits)
            {
                if (c > 5)
                {
                    PrintPage p = new PrintPage(page, SettingsInstance.SchoolName, SettingsInstance.HeadSchool);
                    printHelper.AddFrameworkElementToPrint(p);
                    pages.Add(p);
                    page = new Schedule(Schedule.Name, new ObservableCollection<LessonsClassUnit>(), Schedule.Type, Schedule.Date)
                    {
                        Number = Schedule.Number
                    };
                    c = 0;
                }
                if (c2 > 5)
                {
                    PrintPage p = new PrintPage(page, SettingsInstance.SchoolName, SettingsInstance.HeadSchool);
                    printHelper.AddFrameworkElementToPrint(p);
                    pages.Add(p);
                    page2 = new Schedule(Schedule.Name, new ObservableCollection<LessonsClassUnit>(), Schedule.Type, Schedule.Date)
                    {
                        Number = Schedule.Number
                    };
                    c2 = 0;
                }
                if (i.FormUnit.Shift == 1)
                {
                    page.LessonsClassUnits.Add(i);
                    c += 1;
                }
                else
                {
                    page2.LessonsClassUnits.Add(i);
                    c2 += 1;
                }
            }
            if (page.LessonsClassUnits.Count > 0)
            {
                PrintPage p = new PrintPage(page, SettingsInstance.SchoolName, SettingsInstance.HeadSchool);
                printHelper.AddFrameworkElementToPrint(p);
                pages.Add(p);
            }
            if (page2.LessonsClassUnits.Count > 0)
            {
                PrintPage p = new PrintPage(page, SettingsInstance.SchoolName, SettingsInstance.HeadSchool);
                printHelper.AddFrameworkElementToPrint(p);
                pages.Add(p);
            }
            printHelper.OnPrintFailed += PrintHelper_OnPrintFailed;
            printHelper.OnPrintSucceeded += PrintHelper_OnPrintSucceeded;
            printHelper.OnPrintCanceled += CancelPrint;
            await printHelper.ShowPrintUIAsync($"Печать расписания \"{Schedule.Name}\"");*/
        }
        private void CancelPrint()
        {
            printHelper.ClearListOfPrintableFrameworkElements();
            printHelper.Dispose();
        }
        private async void PrintHelper_OnPrintSucceeded()
        {
            printHelper.ClearListOfPrintableFrameworkElements();
            printHelper.Dispose();
            MessageDialog dialog = new MessageDialog("Успешно отправлено на печать");
            _ = await dialog.ShowAsync();
        }
        private async void PrintHelper_OnPrintFailed()
        {
            printHelper.ClearListOfPrintableFrameworkElements();
            printHelper.Dispose();
            MessageDialog dialog = new MessageDialog("Печать завершилась неудачно");
            _ = await dialog.ShowAsync();
        }

        private void SelectedClassChanged(object sender, SelectionChangedEventArgs e)
        {
            /*if (Schedule == null)
            {
                return;
            }
            if (LessonsClassUnits.Count == 0)
            {
                return;
            }
            if ((sender as ComboBox).SelectedIndex == -1)
            {
                return;
            }
            LessonsClassUnit unit = LessonsClassUnits[(sender as ComboBox).SelectedIndex];
            foreach (Grid grid in ClassLists.Children)
            {
                ListView List = grid.Children[1] as ListView;
                List.ItemsSource = null;
                List.Visibility = Visibility.Collapsed;
            }
            foreach (CollectionWorkdaysLessons w in unit.LessonUnits)
            {
                if (w.Workday == "")
                {
                    return;
                }
                ListView list = (ClassLists.Children[Schedule.GetDayInt(w.Workday)] as Grid).Children[1] as ListView;
                list.ItemsSource = w.LessonUnits;
                list.Visibility = Visibility.Visible;
            }*/
        }

        private void OpenMainHolder(object sender, RoutedEventArgs e)
        {
            MainHolder.Visibility = Visibility.Visible;
            //ClassHolder.Visibility = Visibility.Collapsed;
            Mode = 0;
            SettingsHelper.SaveObjectLocal("SavedMode", 0);
            Bindings.Update();
        }
        private void OpenClassHolder(object sender, RoutedEventArgs e)
        {
            MainHolder.Visibility = Visibility.Collapsed;
            //ClassHolder.Visibility = Visibility.Visible;
            Mode = 1;
            SettingsHelper.SaveObjectLocal("SavedMode", 1);
            Bindings.Update();
        }
        private async void AddEvent(object sender, RoutedEventArgs e)
        {
            shield = true;
            int i = GetCurrentListForm();
            LessonEditor lessonEditor = new LessonEditor(CurrentList.SelectedItem as LessonUnit, SettingsInstance.SubjectUnits, SettingsInstance.TeacherUnits, SettingsInstance.Forms, i);
            _ = await lessonEditor.ShowAsync();
            if (lessonEditor.PrimaryButtonCommandParameter != null)
            {
                ObservableCollection<LessonUnit> src = CurrentList.ItemsSource as ObservableCollection<LessonUnit>;
                src.Add(lessonEditor.PrimaryButtonCommandParameter as LessonUnit);
                
                int f = GetCurrentListForm();
                FormUnit form = Schedule.Settings.Forms[f];
                string workday = Schedule.GetDay(Schedule.Settings.Workdays[GetCurrentListWorkday(f)]);
                (CurrentList.ItemsSource as ObservableCollection<LessonUnit>).Add(lessonEditor.PrimaryButtonCommandParameter as LessonUnit);
                Schedule.LessonItems.Add(new LessonItem(lessonEditor.PrimaryButtonCommandParameter as LessonUnit, form, workday));
            }
            else
            {
                DeleteLessons(null, null);
            }
            Bindings.Update();
            shield = false;
        }
        public bool LogicsOn()
        {
            object res = SettingsHelper.GetObjectRawLocal("Logics");
            return res == null || (string)res == "true";
        }
        private void DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate.HasValue)
            {
                Schedule.Date = args.NewDate.Value;
            }
        }
        private void EnterEditor3(object sender, RoutedEventArgs e)
        {
            List<object> param = new List<object>() { Schedule, File };
            Frame.Navigate(typeof(EditorV3), param);
        }
        private void NavHelp(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(HelpPage));
        }

        private void NavSettings(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(SettingsPage));
        }
        private void NavSchedule(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(SchedulePage));
        }
        public string GetTypeLabel()
        {
            if (Schedule == null) return "";
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
        private void ChangeSemiD(object sender, RoutedEventArgs e)
        {
            Schedule.Number = int.Parse((sender as Button).Content.ToString());
            ChangeSemiScheduleFlyout.Hide();
            Bindings.Update();
        }
        public string GetSemiScheduleLabelD()
        {
            return Schedule != null ? $"{Schedule.Number} полугодие" : "-";
        }
        private void ChangeQuarterD(object sender, RoutedEventArgs e)
        {
            Schedule.Number = int.Parse((sender as Button).Content.ToString());
            ChangeQuarterScheduleFlyout.Hide();
            Bindings.Update();
        }
        public string GetQuarterScheduleLabelD()
        {
            return Schedule != null ? $"{Schedule.Number} четверть" : "-";
        }
    }
}
