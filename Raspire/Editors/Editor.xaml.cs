using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
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
using Microsoft.Toolkit.Uwp.Helpers;

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

        #region Startup Setup Layout
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
                if (SettingsInstance == null) SettingsInstance = Schedule.Settings;
            }
            if (Schedule == null)
            {
                MessageDialog dialog = new MessageDialog("Создайте раписание или откройте существующее");
                _ = await dialog.ShowAsync();
                Frame.Navigate(typeof(SchedulePage), null, new DrillInNavigationTransitionInfo());
                return;
            }
            if (SettingsInstance.Subjects.Count > 0)
            {
                MainSelector.SelectedIndex = 0;
                Selector.SelectedIndex = 0;
            }
            UnitsRepeater.ItemsSource = UnitsWorkdays;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
        #endregion
        
        #region Actions
        private async void SaveSchedule(object sender, RoutedEventArgs e)
        {
            Schedule.RestoreStruct(UnitsWorkdays);
            var f = await Schedule.Save(File);
            if (f != null)
            {
                File = f;
            }
        }
        private async void OnPrintButtonClick(object sender, RoutedEventArgs e)
        {
            if (!PrintManager.IsSupported())
            {
                MessageDialog dialog = new MessageDialog("Печать не поддерживается");
                _ = await dialog.ShowAsync();
                return;
            }
            Print();
        }
        private async void Print()
        {
            SaveSchedule(null, null);
            Editors.PrintWizard wizard = new Editors.PrintWizard(UnitsWorkdays, PrintCanvas);
            await wizard.ShowAsync();
        }
        private async void OpenLessonEditor(object sender, RoutedEventArgs e)
        {
            LessonEditor lessonEditor = new LessonEditor(list.SelectedItems[0] as Lesson);
            _ = await lessonEditor.ShowAsync();

            Lesson lesson = lessonEditor.PrimaryButtonCommandParameter as Lesson;
            if (lesson != null)
            {
                foreach (var item in list.SelectedItems)
                {
                    (item as Lesson).Subject = lesson.Subject;
                    (item as Lesson).Classroom = lesson.Classroom;
                    (item as Lesson).TeacherName = lesson.TeacherName;
                }
            }
            var s = list.ItemsSource;
            list.ItemsSource = null;
            list.ItemsSource = s;
        }
        #endregion

        #region Menu Entries
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
                list = (sender as Grid).Children[1] as ListView;
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
                list = (sender as Grid).Children[1] as ListView;
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
        #endregion

        #region Position Logic
        int GetListIndex()
        {
            var i = list.Parent as Grid;
            return (i.Parent as ItemsRepeater).GetElementIndex(i);
        }
        int GetListWorkday()
        {
            var i = ((list.Parent as Grid).Parent as ItemsRepeater).Parent as StackPanel;
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
        #endregion

        #region Data Manipulation Methods
        private void AddLesson(object sender, RoutedEventArgs e)
        {
            Subject subject = Selector.SelectedItem as Subject;
            int form = GetListIndex();
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
                }
                list.SelectedItems.Clear();
            }
        }
        #endregion

        #region Update Layout Methods
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

        #endregion

        #region Drag and Drop Logic
        private void List_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            list.SelectedItems.Clear();
            sender.SelectedItems.Clear();
            if (list != sender)
            {
                foreach (var i in args.Items)
                {
                    (sender.ItemsSource as ObservableCollection<Lesson>).Remove((Lesson)i);
                }
            }
        }
        private void List_DragStarting(object sender, DragItemsStartingEventArgs e)
        {
            var items = new StringBuilder();
            foreach (var item in e.Items)
            {
                if (items.Length > 0) items.AppendLine();
                items.Append(item.ToString());
            }
            e.Data.SetText(items.ToString());
            e.Data.RequestedOperation = DataPackageOperation.Move;
        }
        private void List_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = (e.DataView.Contains(StandardDataFormats.Text)) ? DataPackageOperation.Move : DataPackageOperation.None;
        }
        private async void List_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                list = sender as ListView;
                var def = e.GetDeferral();
                var s = await e.DataView.GetTextAsync();
                var items = s.Split('\n');
                foreach (var item in items)
                {
                    Subject subject = new Subject(item.Split(" • ")[0]);
                    int form = GetListIndex();
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
                }
                e.AcceptedOperation = DataPackageOperation.Move;
                def.Complete();
            }
        }
        #endregion

        
    }
}
