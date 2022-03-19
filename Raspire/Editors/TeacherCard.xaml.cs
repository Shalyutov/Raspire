using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Raspire
{
    internal sealed partial class TeacherCard : ContentDialog
    {
        public Teacher Teacher { get; set; }
        public ObservableCollection<Subject> Subjects { get; set; }
        public ObservableCollection<Form> Forms { get; set; }
        Subject SelectedSubject;
        Host SelectedHost;
        bool edit = true;
        /// <summary>
        /// Конструктор диалогового окна для редактирования информации об учителе
        /// </summary>
        /// <param name="teacher">Созданный на странице настроек экзепляр класса, который хранит информацию об учителе</param>
        /// <param name="subjects">Список изучаемых предметов</param>
        /// <param name="forms">Список классов</param>
        public TeacherCard(Teacher teacher, ObservableCollection<Subject> subjects, ObservableCollection<Form> forms)
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
                int prev = Teacher.Classroom;
                Teacher.Classroom = int.Parse((sender as TextBox).Text);
                CabValidator.Symbol = Symbol.Accept;
                foreach (var i in Teacher.HostedSubjects)
                {
                    foreach (var f in i.Forms)
                    {
                        if (f.Classroom == prev)
                        {
                            f.Classroom = Teacher.Classroom;
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
            SelectedSubject = e.ClickedItem as Subject;
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
                var unit = new Host(SelectedSubject, new ObservableCollection<FormClassroom>());
                SelectedHost = unit;
                Teacher.HostedSubjects.Add(SelectedHost);
                Subjects.Remove(SelectedSubject);
            }
            foreach (var item in e.AddedItems)
            {
                int c = Teacher.Classroom;
                foreach (var i in SelectedHost.Forms)
                {
                    if (i.Form.ToString() == (item as Form).ToString())
                    {
                        SelectedHost.Forms.Remove(i);
                        c = i.Classroom;
                        return;
                    }
                }
                SelectedHost.Forms.Add(new FormClassroom((Form)item, c, 0));
            }
            foreach (var item in e.RemovedItems)
            {
                List<FormClassroom> list = new List<FormClassroom>();
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
                SelectedHost = e.AddedItems[0] as Host;
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
                SelectedSubject = e.AddedItems[0] as Subject;
                SelectedHost = null;

                ClassesList.SelectedItems.Clear();
            }
        }
    }
}
