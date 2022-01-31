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

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Raspire
{
    public sealed partial class LessonEditor : ContentDialog
    {
        public LessonUnit Unit { get; set; }
        public ObservableCollection<SubjectUnit> SubjectUnits { get; set; }
        public ObservableCollection<TeacherUnit> TeacherUnits { get; set; }
        public ObservableCollection<FormUnit> FormUnits { get; set; }
        int iForm;
        bool loaded = false;
        bool shield = false;
        public LessonEditor(LessonUnit unit, ObservableCollection<SubjectUnit> subjectUnits, ObservableCollection<TeacherUnit> teacherUnits, ObservableCollection<FormUnit> formUnits, int indexForm)
        {
            Unit = unit == null ? new LessonUnit() : unit;
            SubjectUnits = subjectUnits;
            TeacherUnits = teacherUnits;
            FormUnits = formUnits;
            iForm = indexForm;
            loaded = false;
            this.InitializeComponent();
        }
        private void DialogLoaded(object sender, RoutedEventArgs e)
        {
            shield = true;
            if (Unit.Subject.Count == 1)
            {
                EventSelector.Text = Unit.Subject[0].CallName;
                if (Unit.Cabinet[0] == -1)
                {
                    CabSelector.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CabSelector.Text = Unit.Cabinet[0].ToString();
                }
            }
            else if (Unit.Subject.Count == 2)
            {
                SecondUnit.Visibility = Visibility.Visible;

                EventSelector.Text = Unit.Subject[0].CallName;
                if (Unit.Cabinet[0] == -1)
                {
                    CabSelector.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CabSelector.Text = Unit.Cabinet[0].ToString();
                }

                EventSelector2.Text = Unit.Subject[1].CallName;
                if (Unit.Cabinet[1] == -1)
                {
                    CabSelector2.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CabSelector2.Text = Unit.Cabinet[1].ToString();
                }

            }
            loaded = true;
            
        }

        private void EmptyLesson(object sender, RoutedEventArgs e)
        {
            Unit.Subject.Clear();
            Unit.Cabinet.Clear();
            Unit.Teacher.Clear();

            PrimaryButtonCommandParameter = Unit;
            Hide();
        }

        private void DeleteLesson(object sender, RoutedEventArgs e)
        {
            PrimaryButtonCommandParameter = null;
            Hide();
        }

        private void StandardLesson(object sender, RoutedEventArgs e)
        {
            if (Unit.Subject.Count > 1)
            {
                Unit.Subject.RemoveAt(1);
                Unit.Cabinet.RemoveAt(1);
                Unit.Teacher.RemoveAt(1);
            }
            PrimaryButtonCommandParameter = Unit;

            Hide();
        }

        private void DoubleLesson(object sender, RoutedEventArgs e)
        {
            SecondUnit.Visibility = Visibility.Visible;
            if (Unit.Subject.Count != 2 || Unit.Subject[0] == null || Unit.Subject[1] == null) return;
            PrimaryButtonCommandParameter = Unit;
            Hide();
        }

        private void CabSelectorChanged(object sender, TextChangedEventArgs e)
        {
            if (!loaded) return;
            if (Unit == null) return;
            if ((sender as TextBox).Text == "") return;
            if (Unit.Cabinet.Count >= 1)
            {
                try
                {
                    Unit.Cabinet[0] = int.Parse((sender as TextBox).Text);
                }
                catch (Exception)
                {
                    return;
                }
            }
            else return;
        }
        private void CabSelector2Changed(object sender, TextChangedEventArgs e)
        {
            if (!loaded) return;
            if (Unit == null) return;
            if ((sender as TextBox).Text == "") return;
            if (Unit.Cabinet.Count == 2)
            {
                try
                {
                    Unit.Cabinet[1] = int.Parse((sender as TextBox).Text);
                }
                catch (Exception)
                {
                    return;
                }
            }
            else return;
        }

        private void StandardEvent(object sender, RoutedEventArgs e)
        {
            if (Unit.Subject.Count > 1)
            {
                if (Unit.Subject[1] == null)
                {
                    Unit.Subject.RemoveAt(1);
                    Unit.Cabinet.RemoveAt(1);
                    Unit.Teacher.RemoveAt(1);
                }
            }
            else if (Unit.Subject.Count < 1)
            {
                Unit.Subject.Add(null);
                Unit.Teacher.Add(null);
                Unit.Cabinet.Add(-1);
            }

            PrimaryButtonCommandParameter = Unit;

            Hide();
        }

        private void TextChangedSuggest(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
       {
            if (shield)
            {
                shield = false;
                return;
            }
            var text = sender.Text.ToLower();
            var suitableItems = new List<SubjectUnit>();
            
            if (text == "")
            {
                sender.ItemsSource = SubjectUnits;
                return;
            }
            foreach (var unit in SubjectUnits)
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
            suitableItems.Add(new SubjectUnit(sender.Text + "~ мероприятие"));
            
            sender.ItemsSource = suitableItems;
        }

        private void SuggestChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            shield = true;
            if (args.SelectedItem.ToString().Contains("~"))
            {
                (args.SelectedItem as SubjectUnit).CallName = args.SelectedItem.ToString().Split("~")[0];
                EventSelector.Text = (args.SelectedItem as SubjectUnit).CallName;
                if (Unit.Subject.Count > 0)
                {
                    Unit.Subject[0] = (SubjectUnit)args.SelectedItem;
                    Unit.Cabinet[0] = -1;
                    Unit.Teacher[0] = null;
                }
                CabSelector.Visibility = Visibility.Collapsed;
                
                return;
            }
            else
            {
                sender.Text = args.SelectedItem.ToString();
                CabSelector.Visibility = Visibility.Visible;
            }

            TeacherUnit teacher = GetTeacher(args.SelectedItem.ToString(), out int c);
            if (teacher != null)
            {
                Unit = new LessonUnit((SubjectUnit)args.SelectedItem, c, teacher);
                CabSelector.Text = c.ToString();
            }
            else
            {
                sender.Text = "";
            }
        }
        private void SuggestChosen2(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            shield = true;
            if (args.SelectedItem.ToString().Contains("~"))
            {
                (args.SelectedItem as SubjectUnit).CallName = args.SelectedItem.ToString().Split("~")[0];
                EventSelector2.Text = (args.SelectedItem as SubjectUnit).CallName;
                if (Unit.Subject.Count == 1)
                {
                    Unit.Subject.Add((SubjectUnit)args.SelectedItem);
                    Unit.Cabinet.Add(-1);
                    Unit.Teacher.Add(null);
                }
                CabSelector2.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                sender.Text = args.SelectedItem.ToString();
                CabSelector2.Visibility = Visibility.Visible;
            }

            TeacherUnit teacher = GetTeacher(args.SelectedItem.ToString(), out int c);
            if (teacher != null)
            {
                if (Unit.Subject.Count == 1)
                {
                    Unit.Subject.Add((SubjectUnit)args.SelectedItem);
                    Unit.Cabinet.Add(c);
                    Unit.Teacher.Add(teacher.Name);
                }
                CabSelector2.Text = c.ToString();
            }
            else
            {
                sender.Text = "";
            }
        }
        public TeacherUnit GetTeacher(string args, out int cab)
        {
            cab = -1;
            foreach (TeacherUnit t in TeacherUnits)
            {
                foreach (HostUnit s in t.HostedSubjects)
                {
                    bool subjectMatch = s.Subject.ToString() == args;
                    bool formMatch = false;
                    foreach (FormCabinetPair f in s.Forms)
                    {
                        int i = iForm;
                        if (f.Form.ToString() == FormUnits[i].ToString())
                        {
                            formMatch = true;
                            cab = f.Cabinet;
                            break;
                        }
                    }
                    if (subjectMatch && formMatch)
                    {
                        return t;
                    }
                }
            }
            return null;
        }
    }
}
