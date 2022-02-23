using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;

namespace Raspire
{
    /// <summary>
    /// Page for the Settings
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private Settings SettingsInstance = new Settings();
        private readonly List<int> Workdays = new List<int>();
        private ObservableCollection<Form> Shift1 = new ObservableCollection<Form>();
        private ObservableCollection<Form> Shift2 = new ObservableCollection<Form>();
        public SettingsPage()
        {
            this.InitializeComponent();
            Update();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!Frame.CanGoBack)
            {
                //BackButton.Visibility = Visibility.Collapsed;
            }
        }
        public void Update()
        {
            foreach (int workday in SettingsInstance.Workdays)
            {
                Workdays.Add(workday);
            }

            if (!SecondShiftEnabled())
            {
                FormUnitsList.ItemsSource = SettingsInstance.Forms;
                FormUnitsList2.Visibility = Visibility.Collapsed;
                ShiftSeparator.Visibility = Visibility.Collapsed;
            }
            else
            {
                FormUnitsList2.Visibility = Visibility.Visible;
                ShiftSeparator.Visibility = Visibility.Visible;
                foreach (Form unit in SettingsInstance.Forms)
                {
                    if (unit.Shift == 1)
                    {
                        Shift1.Add(unit);
                    }
                    else
                    {
                        Shift2.Add(unit);
                    }
                }
                FormUnitsList.ItemsSource = Shift1;
            }

            SubjectUnitsList.ItemsSource = SettingsInstance.SubjectUnits;
            TeacherUnitsList.ItemsSource = SettingsInstance.TeacherUnits;
            Organization.Text = SettingsInstance.SchoolName;
            Head.Text = SettingsInstance.HeadSchool;
            Holder.Text = SettingsInstance.ScheduleHolder;
            CheckSettings();
        }
        public void CheckSettings()
        {
            WorkdaysValidator.Symbol = Workdays.Count > 0 ? Symbol.Accept : Symbol.Clear;
            ClassesValidator.Symbol = SettingsInstance.Forms.Count > 0 ? Symbol.Accept : Symbol.Clear;
            SubjectsValidator.Symbol = SettingsInstance.SubjectUnits.Count > 0 ? Symbol.Accept : Symbol.Clear;
            TeachersValidator.Symbol = SettingsInstance.TeacherUnits.Count > 0 ? Symbol.Accept : Symbol.Clear;
        }
        public bool ShowContinueButton()
        {
            object res = SettingsHelper.GetObjectRawLocal("ContinueButton");
            return res == null || (string)res == "true";
        }
        public bool ScheduleStartupButton()
        {
            object res = SettingsHelper.GetObjectRawLocal("ScheduleStartup");
            return res != null && (string)res == "true";
        }
        public bool SemanticSelectionButton()
        {
            object res = SettingsHelper.GetObjectRawLocal("SemanticSelection");
            return res == null || (string)res == "true";
        }
        public bool LogicsOn()
        {
            object res = SettingsHelper.GetObjectRawLocal("Logics");
            return res == null || (string)res == "true";
        }
        public bool SecondShiftEnabled()
        {
            object res = SettingsHelper.GetObjectRawLocal("SecondShift");
            return res != null && (string)res == "true";
        }
        private void AddFormUnit()
        {
            if (FormUnitBox.Text != "")//could use regex
            {
                string n = "";
                string l = "";
                foreach (char i in FormUnitBox.Text)
                {
                    if ("0123456789".Contains(i))
                    {
                        n += i;
                    }
                    else if (i.ToString() != " ")
                    {
                        l += i;
                    }
                }
                if (SecondShiftEnabled())
                {
                    var value = SecondShiftCheckBox.IsChecked;
                    if (value.HasValue)
                    {
                        SettingsInstance.Forms.Add(new Form(FormUnitBox.Text, 0, value.Value ? 2 : 1));
                        if (value.Value)
                        {
                            Shift2.Add(SettingsInstance.Forms.Last());
                        }
                        else
                        {
                            Shift1.Add(SettingsInstance.Forms.Last());
                        }
                    }
                    else
                    {
                        SettingsInstance.Forms.Add(new Form(FormUnitBox.Text, 0));
                    }
                }
                else
                {
                    SettingsInstance.Forms.Add(new Form(FormUnitBox.Text, 0));
                }

                SettingsInstance.SaveFormUnits();
            }
            CheckSettings();
        }
        private void DeleteFormUnitMenuFlyout(object sender, RoutedEventArgs e)
        {
            if (FormUnitsList.SelectedItem != null)
            {
                _ = SettingsInstance.Forms.Remove(FormUnitsList.SelectedItem as Form);
                if (SecondShiftEnabled())
                {
                    _ = Shift1.Remove(FormUnitsList.SelectedItem as Form);
                }
                SettingsInstance.SaveFormUnits();
                CheckSettings();
                return;
            }
            if (FormUnitsList2.SelectedItem != null)
            {
                _ = SettingsInstance.Forms.Remove(FormUnitsList2.SelectedItem as Form);
                if (SecondShiftEnabled())
                {
                    _ = Shift2.Remove(FormUnitsList2.SelectedItem as Form);
                }
                SettingsInstance.SaveFormUnits();
                CheckSettings();
                return;
            }
        }
        private void KeyHandler(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.ToString() == "Enter")
            {
                AddFormUnit();
                FormUnitBox.Text = "";
                _ = FormUnitBox.Focus(FocusState.Programmatic);
                CheckSettings();
            }
        }
        private void KeyHandlerSubject(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.ToString() == "Enter")
            {
                TextBox SubjectBox = sender as TextBox;
                if (SubjectBox.Text != "")
                {
                    AddSubjectUnit(SubjectBox.Text);
                    SubjectBox.Text = "";
                    _ = SubjectBox.Focus(FocusState.Programmatic);
                    CheckSettings();
                }
            }
        }
        private void SaveOrderFormUnits(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            SettingsInstance.SaveFormUnits();
            CheckSettings();
        }

        private void AddSubjectUnit(string subject)
        {
            SettingsInstance.SubjectUnits.Add(new Subject(subject));
            SettingsInstance.SaveSubjectUnits();
            CheckSettings();
        }
        private void KeyDeleteSubjectHandler(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.ToString() == "Delete")
            {
                if ((sender as ListView).SelectedItem != null)
                {
                    _ = SettingsInstance.SubjectUnits.Remove((sender as ListView).SelectedItem as Subject);
                    SettingsInstance.SaveSubjectUnits();
                    CheckSettings();
                }
            }
        }
        private void DeleteSubjectUnit(object sender, RoutedEventArgs e)
        {
            if (SubjectUnitsList.SelectedItem != null)
            {
                _ = SettingsInstance.SubjectUnits.Remove(SubjectUnitsList.SelectedItem as Subject);
                SettingsInstance.SaveSubjectUnits();
                CheckSettings();
            }
        }

        private void EnterTeacherHandler(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.ToString() == "Enter")
            {
                AddTeacherUnit((sender as TextBox).Text);
                (sender as TextBox).Text = "";
                _ = (sender as TextBox).Focus(FocusState.Programmatic);
                CheckSettings();
            }
        }
        private void AddTeacherUnit(string name)
        {
            if (name == "") return;
            SettingsInstance.TeacherUnits.Add(new Teacher(name, 0));
            SettingsInstance.SaveTeacherUnits();
            CheckSettings();
        }
        private async void TeacherSelected(object sender, SelectionChangedEventArgs e)
        {
            ListView list = sender as ListView;
            if (list.SelectedIndex != -1)
            {
                TeacherCard card = new TeacherCard(SettingsInstance.TeacherUnits.ElementAt(list.SelectedIndex), SettingsInstance.SubjectUnits, SettingsInstance.Forms);
                _ = await card.ShowAsync();
                if (card.PrimaryButtonCommandParameter != null)
                {
                    SettingsInstance.TeacherUnits[list.SelectedIndex] = card.PrimaryButtonCommandParameter as Teacher;
                }
                else
                {
                    SettingsInstance.TeacherUnits.RemoveAt(list.SelectedIndex);
                }
                SettingsInstance.SaveTeacherUnits();
                CheckSettings();
            }
        }
        private void SchoolNameChanged(object sender, TextChangedEventArgs e)
        {
            SettingsInstance.SchoolName = (sender as TextBox).Text;
            SettingsInstance.SaveSchoolName();
        }
        private void HeadSchoolChanged(object sender, TextChangedEventArgs e)
        {
            SettingsInstance.HeadSchool = (sender as TextBox).Text;
            SettingsInstance.SaveHeadSchool();
        }
        private void ScheduleHolderChanged(object sender, TextChangedEventArgs e)
        {
            SettingsInstance.ScheduleHolder = (sender as TextBox).Text;
            SettingsInstance.SaveScheduleHolder();
        }
        private void LoadDefaults(object sender, RoutedEventArgs e)
        {
            DefaultsPopup.Hide();
            SettingsInstance.DefaultSettings();
            foreach (var b in WorkdaysPanel.Children)
            {
                (b as ToggleButton).IsChecked = false;
            }
            foreach (int i in SettingsInstance.Workdays)
            {
                (WorkdaysPanel.Children[i] as ToggleButton).IsChecked = true;
            }
            //Bindings.Update();
            CheckSettings();
            Update();
        }
        
        private void FormUnitsKeyHandler(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.ToString() == "Delete")
            {
                if ((sender as ListView).SelectedItem != null)
                {
                    _ = SettingsInstance.Forms.Remove((sender as ListView).SelectedItem as Form);
                    SettingsInstance.SaveFormUnits();
                    CheckSettings();
                }
            }
        }

        private void ShowContinueButtonToogled(object sender, RoutedEventArgs e)
        {
            SettingsHelper.SaveObjectLocal("ContinueButton", (sender as ToggleSwitch).IsOn);
        }

        private void StartupToggled(object sender, RoutedEventArgs e)
        {
            SettingsHelper.SaveObjectLocal("ScheduleStartup", (sender as ToggleSwitch).IsOn);
        }
        private void SemanticSelectionToggled(object sender, RoutedEventArgs e)
        {
            SettingsHelper.SaveObjectLocal("SemanticSelection", (sender as ToggleSwitch).IsOn);
        }

        private void WorkdayClick(object sender, RoutedEventArgs e)
        {
            int workday;
            switch ((sender as ToggleButton).Content)
            {
                case "ПН":
                    workday = 0;
                    break;
                case "ВТ":
                    workday = 1;
                    break;
                case "СР":
                    workday = 2;
                    break;
                case "ЧТ":
                    workday = 3;
                    break;
                case "ПТ":
                    workday = 4;
                    break;
                case "СБ":
                    workday = 5;
                    break;
                case "ВС":
                    workday = 6;
                    break;
                default:
                    return;
            }
            if (Workdays.Contains(workday))
            {
                _ = Workdays.Remove(workday);
            }
            else
            {
                Workdays.Add(workday);
            }
            SettingsInstance.Workdays = Workdays;
            SettingsInstance.SaveWorkdays();
            CheckSettings();
        }

        private void CleanSettings(object sender, RoutedEventArgs e)
        {
            SettingsHelper.SaveObjectLocal("Settings", "");
            SettingsInstance = new Settings();
            ClearSettingsPopup.Hide();
            foreach (var b in WorkdaysPanel.Children)
            {
                (b as ToggleButton).IsChecked = false;
            }
            Workdays.Clear();
            FormUnitsList.ItemsSource = null;
            FormUnitsList2.ItemsSource = null;
            SubjectUnitsList.ItemsSource = null;
            TeacherUnitsList.ItemsSource = null;
            CheckSettings();
            Update();
        }

        private void SecondShiftToggled(object sender, RoutedEventArgs e)
        {
            SettingsHelper.SaveObjectLocal("SecondShift", (sender as ToggleSwitch).IsOn);
            if ((sender as ToggleSwitch).IsOn)
            {
                Shift1 = new ObservableCollection<Form>();
                Shift2 = new ObservableCollection<Form>();
                foreach (Form unit in SettingsInstance.Forms)
                {
                    if (unit.Shift == 1)
                    {
                        Shift1.Add(unit);
                    }
                    else
                    {
                        Shift2.Add(unit);
                    }
                }
                FormUnitsList2.Visibility = Visibility.Visible;
                SecondShiftCheckBox.IsEnabled = true;
                FormUnitsList.ItemsSource = Shift1;
                //Bindings.Update();
            }
            else
            {
                FormUnitsList2.Visibility = Visibility.Collapsed;
                SecondShiftCheckBox.IsEnabled = false;
                FormUnitsList.ItemsSource = SettingsInstance.Forms;
                //Bindings.Update();
            }
        }

        private void GetShiftChecked(object sender, RoutedEventArgs e)
        {
            _ = FormUnitBox.Focus(FocusState.Programmatic);
        }

        private void LogicsToogled(object sender, RoutedEventArgs e)
        {
            SettingsHelper.SaveObjectLocal("Logics", (sender as ToggleSwitch).IsOn);
        }
        public void BackWork(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
