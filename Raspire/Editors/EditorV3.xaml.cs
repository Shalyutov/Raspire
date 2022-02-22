using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Raspire
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EditorV3 : Page
    {
        public Schedule Schedule { get; set; }
        public ObservableCollection<WorkdayForms> UnitsWorkdays { get; set; }
        public StorageFile File;
        private Settings SettingsInstance { get; set; }
        private bool shield = false;
        int CurrentS;
        int CurrentC;
        int CurrentW;
        public EditorV3()
        {
            this.InitializeComponent();
            
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsInstance = new Settings();
            if (e.Parameter != null)
            {
                if (Schedule == (Schedule)(e.Parameter as List<object>)[0]) return;
                if (File == (StorageFile)(e.Parameter as List<object>)[1]) return;
                Schedule = (Schedule)(e.Parameter as List<object>)[0];
                File = (StorageFile)(e.Parameter as List<object>)[1];
                UnitsWorkdays = Schedule.GetStructWorkday();
            }
            Bindings.Update();
        }
        private List<string> GetWorkdayStringList()
        {
            List<string> result = new List<string>();
            foreach(int day in SettingsInstance.Workdays)
            {
                result.Add(Schedule.GetDay(day));
            }
            return result;
        }
        private void LessonUnitFocused(object sender, PointerRoutedEventArgs e)
        {
            if (shield) return;
            TextBlock tt = sender as TextBlock;
            CheckBox t = tt.Parent as CheckBox;
            CurrentS = (t.Parent as ItemsRepeater).GetElementIndex(t);
            CurrentC = (((t.Parent as ItemsRepeater).Parent as StackPanel).Parent as ItemsRepeater).GetElementIndex((t.Parent as ItemsRepeater).Parent as StackPanel);
            CurrentW = (((((t.Parent as ItemsRepeater).Parent as StackPanel).Parent as ItemsRepeater).Parent as StackPanel).Parent as ItemsRepeater).GetElementIndex((((t.Parent as ItemsRepeater).Parent as StackPanel).Parent as ItemsRepeater).Parent as StackPanel);
            ClassSelector.SelectedIndex = CurrentC;
            WorkdaySelector.SelectedIndex = CurrentW;
            SubjectSelector.Text = UnitsWorkdays[CurrentW].ClassesUnits[CurrentC].Units[CurrentS].Represent();
            CabinetSelector.Text = UnitsWorkdays[CurrentW].ClassesUnits[CurrentC].Units[CurrentS].RepresentCab();
            LessonIndex.Text = (CurrentS + 1).ToString();
            tt.Foreground = ActualTheme == ElementTheme.Dark ? new SolidColorBrush(Windows.UI.Colors.White) : new SolidColorBrush(Windows.UI.Colors.Black);
        }

        private void LessonUnitUnhovered(object sender, PointerRoutedEventArgs e)
        {
            TextBlock t = sender as TextBlock;
            t.Foreground = new SolidColorBrush(Windows.UI.Colors.DarkGray);
        }

        private async void OpenLessonEditor(object sender, DoubleTappedRoutedEventArgs e)
        {
            shield = true;

            //int f = GetCurrentListForm();
            //int w = GetCurrentListWorkday(f);

            LessonUnit unit = UnitsWorkdays[CurrentW].ClassesUnits[CurrentC].Units[CurrentS];
            LessonEditor lessonEditor = new LessonEditor(unit, SettingsInstance.SubjectUnits, SettingsInstance.TeacherUnits, SettingsInstance.Forms, CurrentC);
            _ = await lessonEditor.ShowAsync();

            int index = 0;
            foreach (LessonItem item in Schedule.LessonItems)
            {
                if ((lessonEditor.PrimaryButtonCommandParameter as LessonUnit).ToString() == item.Lesson.ToString() && item.Form.ToString() == Schedule.Settings.Forms[CurrentC].ToString() && item.Workday == Schedule.GetDay(CurrentW))
                {
                    index = Schedule.LessonItems.IndexOf(item);
                    Schedule.LessonItems.Remove(item);
                    break;
                }
            }

            if (lessonEditor.PrimaryButtonCommandParameter != null)
            {
                ObservableCollection<LessonUnit> src = UnitsWorkdays[CurrentW].ClassesUnits[CurrentC].Units;

                FormUnit form = Schedule.Settings.Forms[CurrentC];
                string workday = Schedule.GetDay(Schedule.Settings.Workdays[CurrentW]);
                Schedule.LessonItems.Insert(index, new LessonItem(lessonEditor.PrimaryButtonCommandParameter as LessonUnit, form, workday));

                src.Insert(CurrentS, lessonEditor.PrimaryButtonCommandParameter as LessonUnit);
                src.RemoveAt(CurrentS);
                (sender as TextBlock).Text = (lessonEditor.PrimaryButtonCommandParameter as LessonUnit).ToString();
            }
            else
            {
                UnitsWorkdays[CurrentW].ClassesUnits[CurrentC].Units.RemoveAt(CurrentS);
                Schedule.LessonItems.RemoveAt(index);
            }
            Bindings.Update();
            shield = false;
        }

        private async void SaveSchedule(object sender, RoutedEventArgs e)
        {
            var f = await Schedule.Save(File);
            if (f != null)
            {
                File = f;
            }
        }

        private void EnterEditor2(object sender, RoutedEventArgs e)
        {
            List<object> param = new List<object>() { Schedule, File };
            Frame.Navigate(typeof(EditorV2), param);
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

        private void SelectUnit(object sender, TappedRoutedEventArgs e)
        {
            //((sender as TextBlock).Parent as CheckBox).IsChecked = !((sender as TextBlock).Parent as CheckBox).IsChecked;
        }
        private void NavHelp(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(HelpPage));
        }

        private void NavSettings(object sender, RoutedEventArgs e)
        {
            _ = Frame.Navigate(typeof(SettingsPage));
        }
    }
}
