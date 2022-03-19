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
        public Lesson Unit { get; set; }
        public ObservableCollection<Subject> Subjects { get; set; }
        public ObservableCollection<Teacher> Teachers { get; set; }
        public ObservableCollection<Form> Forms { get; set; }
        
        public LessonEditor(Lesson unit)
        {
            Unit = unit == null ? new Lesson(new Subject(""), new Form("0", 0), "", 0) : unit;
            Settings settings = Settings.GetSavedSettings();
            Subjects = settings.Subjects;
            Teachers = settings.Teachers;
            Forms = settings.Forms;
            this.InitializeComponent();
        }
        private void DialogLoaded(object sender, RoutedEventArgs e)
        {
            Lesson.Text = Unit.Subject.ToString();
            Classroom.Text = Unit.Classroom.ToString();
        }

        private void ClearLesson(object sender, RoutedEventArgs e)
        {
            Unit.Subject = new Subject("");
            Unit.Classroom = 0;
            Unit.TeacherName = "";
        }

        private void LessonSubmit(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            PrimaryButtonCommandParameter = Unit;
        }

        private void ClassroomChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Unit.Classroom = int.Parse(Classroom.Text);
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
                Teacher teacher = GetTeacher(new Subject(item), Forms.IndexOf(Unit.Group));
                if (teacher != null)
                {
                    Unit.TeacherName = teacher.Name;
                    Unit.Classroom = teacher.Classroom;
                    Classroom.Focus(FocusState.Programmatic);
                }
                else
                {
                    Lesson.Text = "";
                }
            }
        }
    }
}
