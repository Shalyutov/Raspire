using Microsoft.UI.Xaml.Controls;
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
    public sealed partial class TeacherCard : ContentDialog
    {
        public TeacherUnit Teacher { get; set; }
        public ObservableCollection<SubjectUnit> Subjects { get; set; }
        public ObservableCollection<FormUnit> Forms { get; set; }
        SubjectUnit SelectedSubject;
        HostUnit SelectedHost;
        FormUnit CurrentSelectForm;
        bool edit = false;
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
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
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
                Teacher.Cabinet = int.Parse((sender as TextBox).Text);
                CabValidator.Symbol = Symbol.Accept;
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
            //SubjectCommandBar.ShowAt((sender as ListView).ContainerFromItem(e.ClickedItem), myOption);
            SelectedSubject = e.ClickedItem as SubjectUnit;
        }

        private void SubjectSelect(object sender, RoutedEventArgs e)
        {
            if(SelectedSubject != null)
            {
                Teacher.HostedSubjects.Add(new HostUnit(SelectedSubject, new ObservableCollection<FormCabinetPair>()));
                //SubjectCommandBar.Hide();
            }
        }

        private void HostClicked(object sender, ItemClickEventArgs e)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions
            {
                ShowMode = FlyoutShowMode.Standard
            };
            //HostCommandBar.ShowAt((sender as ListView).ContainerFromItem(e.ClickedItem), myOption);
            SelectedHost = e.ClickedItem as HostUnit;
        }
        private void HostSelect(object sender, RoutedEventArgs e)
        {
            if (SelectedHost != null)
            {
                Teacher.HostedSubjects.Remove(SelectedHost);
                //HostCommandBar.Hide();
            }
        }

        private void FormsSelected(object sender, SelectionChangedEventArgs e)
        {
            if (!edit) return;
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

        /*private void LoadForms(object sender, RoutedEventArgs e)
        {
            edit = false;
            (sender as ListView).SelectedItems.Clear();
            foreach (FormCabinetPair item in SelectedHost.Forms)
            { 
                foreach (FormUnit unit in Forms)
                {
                    if (unit.ToString() == item.Form.ToString()) (sender as ListView).SelectedItems.Add(unit);
                }
            }
            edit = true;
        }*/

        private void SelectAllClasses(object sender, RoutedEventArgs e)
        {
            ClassesList.SelectAll();
        }
        private void UnSelectAllClasses(object sender, RoutedEventArgs e)
        {
            ClassesList.SelectedItems.Clear();
        }

        private void ClassesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions
            {
                ShowMode = FlyoutShowMode.Standard
            };
            //SubgroupCommandBar.ShowAt((sender as ListView).ContainerFromItem(e.ClickedItem), myOption);

            CurrentSelectForm = e.ClickedItem as FormUnit;
            foreach(var i in SelectedHost.Forms)
            {
                if (i.Form.ToString() == CurrentSelectForm.ToString())
                {
                    //CabinetBox.Text = i.Cabinet.ToString();
                }
            }
        }

        private void SelectAllClassForSubject(object sender, RoutedEventArgs e)
        {
            SetSubgroup(0);
        }

        private void SelectFirstSubgroup(object sender, RoutedEventArgs e)
        {
            SetSubgroup(1);
        }

        private void SelectSecondSubgroup(object sender, RoutedEventArgs e)
        {
            SetSubgroup(2);
        }

        private void SetSubgroup(int subgroup)
        {
            ClassesList.SelectedItems.Add(ClassesList.Items.ElementAt(ClassesList.Items.IndexOf(CurrentSelectForm)));
            foreach (var i in SelectedHost.Forms)
            {
                if (i.Form.ToString() == CurrentSelectForm.ToString())
                {
                    i.Subgroup = subgroup;
                    //i.Cabinet = int.Parse(CabinetBox.Text);
                }
            }
            
            //SubgroupCommandBar.Hide();
        }

        private void SetCabinet(object sender, RoutedEventArgs e)
        {

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
                    foreach(var f in Forms)
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
