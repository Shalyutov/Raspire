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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Raspire
{
    /// <summary>
    /// Логика главного редактора расписаний
    /// </summary>
    public sealed partial class Editor : Page
    {
        public Schedule Schedule { get; set; }
        public ObservableCollection<WorkdayForms> UnitsWorkdays { get; set; }
        public StorageFile File { get; set; }
        private Settings SettingsInstance { get; set; }
        private ListView list;
        public Editor()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsInstance = Settings.GetSavedSettings();
            if (e.Parameter != null)
            {
                if (Schedule == (Schedule)(e.Parameter as List<object>)[0] & Schedule != null) return;
                if (File == (StorageFile)(e.Parameter as List<object>)[1] & File != null) return;
                Schedule = (Schedule)(e.Parameter as List<object>)[0];
                File = (StorageFile)(e.Parameter as List<object>)[1];
                UnitsWorkdays = Schedule.GetStructWorkday();
            }
            if (Schedule == null)
            {
                MessageDialog dialog = new MessageDialog("Создайте раписание или откройте существующее");
                _ = await dialog.ShowAsync();
                Frame.Navigate(typeof(SchedulePage), null, new DrillInNavigationTransitionInfo());
            }
            if (SettingsInstance.Subjects.Count > 0)
            {
                MainSelector.SelectedIndex = 0;
                Selector.SelectedIndex = 0;
            }
        }
        /*private List<string> GetWorkdayStringList()
        {
            List<string> result = new List<string>();
            foreach (int day in SettingsInstance.Workdays)
            {
                result.Add(Schedule.GetDay(day));
            }
            return result;
        }*/
        /*private void LessonUnitFocused(object sender, PointerRoutedEventArgs e)
        {
            if (shield) return;
            TextBlock t = sender as TextBlock;
            //CheckBox t = tt.Parent as CheckBox;
            CurrentS = (t.Parent as ItemsRepeater).GetElementIndex(t);
            CurrentC = (((t.Parent as ItemsRepeater).Parent as StackPanel).Parent as ItemsRepeater).GetElementIndex((t.Parent as ItemsRepeater).Parent as StackPanel);
            CurrentW = (((((t.Parent as ItemsRepeater).Parent as StackPanel).Parent as ItemsRepeater).Parent as StackPanel).Parent as ItemsRepeater).GetElementIndex((((t.Parent as ItemsRepeater).Parent as StackPanel).Parent as ItemsRepeater).Parent as StackPanel);
            //ClassSelector.SelectedIndex = CurrentC;
            //WorkdaySelector.SelectedIndex = CurrentW;
            //SubjectSelector.Text = UnitsWorkdays[CurrentW].ClassesUnits[CurrentC].Units[CurrentS].Represent();
            //CabinetSelector.Text = UnitsWorkdays[CurrentW].ClassesUnits[CurrentC].Units[CurrentS].RepresentCab();
            //LessonIndex.Text = (CurrentS + 1).ToString();
            t.Foreground = ActualTheme == ElementTheme.Dark ? new SolidColorBrush(Windows.UI.Colors.White) : new SolidColorBrush(Windows.UI.Colors.Black);
        }*/
        private void OpenLessonEditor(object sender, DoubleTappedRoutedEventArgs e)
        {
            //shield = true;

            //int f = GetCurrentListForm();
            //int w = GetCurrentListWorkday(f);

            /*Lesson unit = UnitsWorkdays[CurrentW].ClassesUnits[CurrentC].Units[CurrentS];
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
            }*/
            //shield = false;
        }

        private async void SaveSchedule(object sender, RoutedEventArgs e)
        {
            var f = await Schedule.Save(File);
            if (f != null)
            {
                File = f;
            }
        }
        private async void OpenDocumentProperties(object sender, RoutedEventArgs e)
        {
            DocumentDialog document = new DocumentDialog(Schedule);
            _ = await document.ShowAsync();
            Schedule = document.PrimaryButtonCommandParameter as Schedule;
        }
        private void OpenCommander(object sender, PointerRoutedEventArgs e)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions
            {
                ShowMode = FlyoutShowMode.Transient
            };
            var previous = list;
            if (sender as ListView != null)
                list = sender as ListView;
            else
                list = (sender as StackPanel).Children[1] as ListView;
            Selector.ItemsSource = SettingsInstance.Subjects;
            if (previous != list)
            {
                Commander.Hide();
                Commander.ShowAt(sender as UIElement, myOption);
                Selector.Focus(FocusState.Programmatic);
            }
        }

        private void OpenCommander(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions
            {
                ShowMode = FlyoutShowMode.Standard
            };
            var previous = list;
            if (sender as ListView != null)
                list = sender as ListView;
            else
                list = (sender as StackPanel).Children[1] as ListView;
            Selector.ItemsSource = SettingsInstance.Subjects;
            if (previous != list)
            {
                Commander.Hide();
                LessonCommander.Hide();
                if (list.SelectedItems.Count > 0)
                {
                    LessonCommander.ShowAt(sender as UIElement, myOption);
                }
                else
                {
                    Commander.ShowAt(sender as UIElement, myOption);
                    Selector.Focus(FocusState.Programmatic);
                }
            }
            else
            {
                if(list.SelectedItems.Count > 0)
                {
                    if (!LessonCommander.IsOpen)
                    {
                        LessonCommander.ShowAt(sender as UIElement, myOption);
                    }
                }
                else
                {
                    if (!Commander.IsOpen)
                    {
                        Commander.ShowAt(sender as UIElement, myOption);
                    }
                }
                
            }
        }
        int GetListIndex()
        {
            var i = list.Parent as StackPanel;
            return (i.Parent as ItemsRepeater).GetElementIndex(i);
        }
        int GetListWorkday()
        {
            var i = ((list.Parent as StackPanel).Parent as ItemsRepeater).Parent as StackPanel;
            return (i.Parent as ItemsRepeater).GetElementIndex(i);
        }
        int GetHostedClassroom(Teacher teacher, Subject subject, Form form)
        {
            foreach (Host s in teacher.HostedSubjects)
            {
                if (s.Subject != subject) continue;
                foreach (FormClassroom f in s.Forms)
                {
                    if (f.Form == form)
                    {
                        return f.Classroom;
                    }
                }
            }
            return -1;
        }
        Teacher GetTeacher(Subject subject, int form)
        {
            foreach (Teacher t in SettingsInstance.Teachers)
            {
                foreach (Host s in t.HostedSubjects)
                {
                    if (s.Subject != subject) continue;
                    foreach (FormClassroom f in s.Forms)
                    {
                        if (f.Form == SettingsInstance.Forms[form])
                        {
                            return t;
                        }
                    }
                }
            }
            return null;
        }
        private void AddLesson(object sender, RoutedEventArgs e)
        {
            Subject subject = Selector.SelectedItem as Subject;
            int form = GetListIndex();
            int workday = GetListWorkday();
            Teacher teacher = GetTeacher(subject, form);
            if (teacher == null) return;
            int classroom = GetHostedClassroom(teacher, subject, SettingsInstance.Forms[form]);
            Lesson lesson = new Lesson(
                subject,
                SettingsInstance.Forms[form],
                teacher.Name,
                classroom
                );
            (list.ItemsSource as ObservableCollection<Lesson>).Add(lesson);
            Schedule.LessonItems.Add(new LessonItem(lesson, workday));
        }

        private void SelectorTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var text = sender.Text.ToLower();
            var suitableItems = new List<Subject>();

            if (text == "")
            {
                sender.ItemsSource = SettingsInstance.Subjects;
                return;
            }
            foreach (var unit in SettingsInstance.Subjects)
            {
                var found = text.Split(" ").All((key) =>
                {
                    return unit.ToString().ToLower().Contains(key);
                });
                if (found)
                {
                    suitableItems.Add(unit);
                }
            }
            suitableItems.Add(new Subject($"{sender.Text} •"));

            sender.ItemsSource = suitableItems;
        }

        private void MainSelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Selector.SelectedItem = e.AddedItems[0];
            }
            else
            {
                Selector.SelectedIndex = 0;
            }
        }

        private void SelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                MainSelector.SelectedItem = e.AddedItems[0];
            }
            else
            {
                MainSelector.SelectedIndex = 0;
            }
        }

        private void DeleteLessons(object sender, RoutedEventArgs e)
        {
            if (list.SelectedItems != null)
            {
                List<object> deleted = new List<object>();
                deleted.AddRange(list.SelectedItems);
                foreach (object lesson in deleted)
                {
                    _ = (list.ItemsSource as ObservableCollection<Lesson>).Remove(lesson as Lesson);
                    foreach (LessonItem item in Schedule.LessonItems)
                    {
                        if (item.Lesson == lesson as Lesson & item.Workday == GetListWorkday())
                        {
                            Schedule.LessonItems.Remove(item);
                            break;
                        }
                    }
                }
            }
        }
    }
}
