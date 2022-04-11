using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Raspire
{
    internal sealed partial class LessonEditor : ContentDialog
    {
        public MultipleLesson Unit { get; set; }
        public ObservableCollection<Subject> Subjects { get; set; }
        public ObservableCollection<Teacher> Teachers { get; set; }
        public ObservableCollection<Form> Forms { get; set; }
        public Form Form { get; set; }
        public LessonEditor(MultipleLesson unit, Form form)
        {
            Unit = unit ?? new MultipleLesson(new Lesson(new Subject(""), new Form("0", 0), "", 0, 0));
            Settings settings = Settings.GetSavedSettings();
            Subjects = settings.Subjects;
            Teachers = settings.Teachers;
            Forms = settings.Forms;
            Form = form;
            
            this.InitializeComponent();
        }
        private void DialogLoaded(object sender, RoutedEventArgs e)
        {
            LessonList.ItemsSource = Unit.Lessons;
            LessonList.SelectedIndex = 0;
        }
        private void ClearLesson(object sender, RoutedEventArgs e)
        {
            if (LessonList.SelectedItem != null)
            {
                Lesson.Text = "";
                Classroom.Text = "";
            }
        }
        private void LessonSubmit(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            PrimaryButtonCommandParameter = Unit;
        }
        private void ClassroomChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (LessonList.SelectedItem != null)
                {
                    Unit.Lessons[LessonList.SelectedIndex].Classroom = int.Parse(Classroom.Text);

                }
            }
            catch (Exception)
            {
            }
        }
        private void LessonChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var text = sender.Text.ToLower();
            var suitableItems = new List<Subject>();

            if (text == "")
            {
                sender.ItemsSource = Subjects;
                return;
            }
            foreach (var unit in Subjects)
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
            suitableItems.Add(new Subject(sender.Text + "•"));

            sender.ItemsSource = suitableItems;
        }
        Teacher GetTeacher(Subject subject, int form)
        {
            foreach (Teacher t in Teachers)
            {
                foreach (Host s in t.HostedSubjects)
                {
                    if (s.Subject != subject) continue;
                    foreach (FormClassroom f in s.Forms)
                    {
                        if (f.Form == Forms[form])
                        {
                            return t;
                        }
                    }
                }
            }
            return null;
        }
        private void LessonChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string item = args.SelectedItem.ToString();
            if (item.Contains("•"))
            {
                Lesson.Text = item.Split("•")[0];
                Classroom.Text = "";
                Classroom.Focus(FocusState.Programmatic);
            }
            else
            {
                Lesson.Text = item;
                Teacher teacher = GetTeacher(new Subject(item), Forms.IndexOf(Form));
                if (teacher != null)
                {
                    if (LessonList.SelectedItem != null)
                    {
                        Unit.Lessons[LessonList.SelectedIndex].TeacherName = teacher.Name;
                        Unit.Lessons[LessonList.SelectedIndex].Classroom = teacher.Classroom;
                        Classroom.Focus(FocusState.Programmatic);

                        LessonList.ItemsSource = null;
                        LessonList.ItemsSource = Unit.Lessons;
                    }
                }
                else if (LessonList.SelectedItem != null)
                {
                    Lesson.Text = "";
                }
            }
        }
        private void LessonSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Lesson.Text = (LessonList.SelectedItem as Lesson).Subject.ToString();
                Classroom.Text = (LessonList.SelectedItem as Lesson).Classroom.ToString();
                ClearButton.Visibility = Visibility.Visible;
                AddButton.Visibility = Visibility.Collapsed;
                return;
            }
            else if (e.RemovedItems.Count > 0)
            {
                Lesson.Text = "";
                Classroom.Text = "";
            }
            ClearButton.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Visible;
        }

        private void AddLesson(object sender, RoutedEventArgs e)
        {
            try
            {
                Lesson lesson = new Lesson(new Subject(Lesson.Text), Form, "", int.Parse(Classroom.Text), 0);
                Teacher teacher = GetTeacher(lesson.Subject, Forms.IndexOf(Form));
                if (teacher != null)
                {
                    lesson.TeacherName = teacher.Name;
                    lesson.Classroom = teacher.Classroom;
                }
                Unit.Lessons.Add(lesson);

                LessonList.ItemsSource = Unit.Lessons;
            }
            catch (Exception)
            {

            }
        }
    }
}
