using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Raspire
{
    public sealed partial class TeacherCard : ContentDialog
    {
        public TeacherUnit Teacher { get; set; }
        public ObservableCollection<SubjectUnit> Subjects { get; set; }
        public ObservableCollection<FormUnit> Forms { get; set; }
        SubjectUnit SelectedSubject;
        HostUnit SelectedHost;
        bool edit = true;
        /// <summary>
        /// Конструктор диалогового окна для редактирования информации об учителе
        /// </summary>
        /// <param name="teacher">Созданный на странице настроек экзепляр класса, который хранит информацию об учителе</param>
        /// <param name="subjects">Список изучаемых предметов</param>
        /// <param name="forms">Список классов</param>
        public TeacherCard(TeacherUnit teacher, ObservableCollection<SubjectUnit> subjects, ObservableCollection<FormUnit> forms)
        {
            Teacher = teacher;
            Subjects = subjects;
            Forms = forms;
            this.InitializeComponent();
            Update();
        }
        private void Update()
        {
            SubjectsList.ItemsSource = Subjects;
            ClassesList.ItemsSource = Forms;
            CabValidator.Symbol = Symbol.Accept;
            NameValidator.Symbol = Symbol.Accept;
            foreach (var host in Teacher.HostedSubjects)
            {
                int j = 0;
                foreach (var f in Subjects)
                {
                    if (f.ToString() == host.Subject.ToString())
                    {
                        break;
                    }
                    j++;
                }
                if (j < Subjects.Count) Subjects.RemoveAt(j);
            }
        }
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (CabValidator.Symbol == Symbol.Clear)
            {
                args.Cancel = true;
                MessageDialog dialog = new MessageDialog("Неверно задан параметр кабинета");
                _ = await dialog.ShowAsync();
            }
            PrimaryButtonCommandParameter = Teacher;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            PrimaryButtonCommandParameter = null;
        }

        private void NameChanged(object sender, TextChangedEventArgs e)
        {
            Teacher.Name = (sender as TextBox).Text;
        }

        private void CabinetChanged(object sender, TextChangedEventArgs args)
        {
            try
            {
                int prev = Teacher.Cabinet;
                Teacher.Cabinet = int.Parse((sender as TextBox).Text);
                CabValidator.Symbol = Symbol.Accept;
                foreach (var i in Teacher.HostedSubjects)
                {
                    foreach (var f in i.Forms)
                    {
                        if (f.Cabinet == prev)
                        {
                            f.Cabinet = Teacher.Cabinet;
                        }
                    }
                }
            }
            catch (Exception)
            {
                CabValidator.Symbol = Symbol.Clear;
            }
        }

        private void SubjectClicked(object sender, ItemClickEventArgs e)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions
            {
                ShowMode = FlyoutShowMode.Standard
            };
            SelectedSubject = e.ClickedItem as SubjectUnit;
        }
        private void FormsSelected(object sender, SelectionChangedEventArgs e)
        {
            if (!edit) return; //предохранитель защищающий от выполнения кода во время выбора следующего предмета (во избежание коллизий)
            if (SelectedHost != null)
            {
                if (ClassesList.SelectedItems.Count == 0)
                {
                    Subjects.Add(SelectedHost.Subject);
                    Teacher.HostedSubjects.Remove(SelectedHost);
                    SelectedHost = null;
                    return;
                }
            }
            else if (SelectedSubject != null)
            {
                if (ClassesList.SelectedItems.Count == 0)
                {
                    SelectedHost = null;
                    return;
                }
                var unit = new HostUnit(SelectedSubject, new ObservableCollection<FormCabinetPair>());
                SelectedHost = unit;
                Teacher.HostedSubjects.Add(SelectedHost);
                Subjects.Remove(SelectedSubject);
            }
            foreach (var item in e.AddedItems)
            {
                int c = Teacher.Cabinet;
                foreach (var i in SelectedHost.Forms)
                {
                    if (i.Form.ToString() == (item as FormUnit).ToString())
                    {
                        SelectedHost.Forms.Remove(i);
                        c = i.Cabinet;
                        return;
                    }
                }
                SelectedHost.Forms.Add(new FormCabinetPair((FormUnit)item, c, 0));
            }
            foreach (var item in e.RemovedItems)
            {
                List<FormCabinetPair> list = new List<FormCabinetPair>();
                foreach (var pair in SelectedHost.Forms)
                {
                    if (pair.Form.ToString() == item.ToString()) list.Add(pair);
                }
                foreach (var i in list) SelectedHost.Forms.Remove(i);
            }
            HostedSubjectsList.SelectedItem = SelectedHost;
        }
        private void SelectAllClasses(object sender, RoutedEventArgs e)
        {
            ClassesList.SelectAll();
        }
        private void UnSelectAllClasses(object sender, RoutedEventArgs e)
        {
            ClassesList.SelectedItems.Clear();
        }
        private void HostSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SubjectsList.SelectedItem = null;
                SelectedHost = e.AddedItems[0] as HostUnit;
                SelectedSubject = null;

                edit = false;
                ClassesList.SelectedItems.Clear();
                foreach (var i in SelectedHost.Forms)
                {
                    int j = 0;
                    foreach (var f in Forms)
                    {
                        if (f.ToString() == i.Form.ToString())
                        {
                            break;
                        }
                        j++;
                    }
                    ClassesList.SelectedItems.Add(Forms.ElementAt(j));
                }
                edit = true;
            }

        }

        private void SubjectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                HostedSubjectsList.SelectedItem = null;
                SelectedSubject = e.AddedItems[0] as SubjectUnit;
                SelectedHost = null;

                ClassesList.SelectedItems.Clear();
            }
        }
    }
}
