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
    public class CompatExtensions
    {
        public static bool GetAllowFocusOnInteraction(DependencyObject obj)
        {
            return (bool)obj.GetValue(AllowFocusOnInteractionProperty);
        }
        public static void SetAllowFocusOnInteraction(DependencyObject obj, bool value)
        {
            obj.SetValue(AllowFocusOnInteractionProperty, value);
        }
        // Using a DependencyProperty as the backing store for AllowFocusOnInteraction.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowFocusOnInteractionProperty =
            DependencyProperty.RegisterAttached("AllowFocusOnInteraction",
                                                typeof(bool),
                                                typeof(CompatExtensions),
                                                new PropertyMetadata(0, AllowFocusOnInteractionChanged));

        private static bool allowFocusOnInteractionAvailable =
            Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent(
                "Windows.UI.Xaml.FrameworkElement",
                "AllowFocusOnInteraction");
        private static void AllowFocusOnInteractionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (allowFocusOnInteractionAvailable)
            {
                var element = d as FrameworkElement;
                if (element != null)
                {
                    element.AllowFocusOnInteraction = (bool)e.NewValue;
                }
            }
        }
    }
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
            CabinetBox.Text = Teacher.Cabinet.ToString();
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

        private void CabinetChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            Teacher.Cabinet = (int)sender.Value;
        }

        private void SubjectClicked(object sender, ItemClickEventArgs e)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions
            {
                ShowMode = FlyoutShowMode.Standard
            };
            SubjectCommandBar.ShowAt((sender as ListView).ContainerFromItem(e.ClickedItem), myOption);
            SelectedSubject = e.ClickedItem as SubjectUnit;
        }

        private void SubjectSelect(object sender, RoutedEventArgs e)
        {
            if(SelectedSubject != null)
            {
                Teacher.HostedSubjects.Add(new HostUnit(SelectedSubject, new ObservableCollection<FormCabinetPair>()));
                SubjectCommandBar.Hide();
            }
        }

        private void HostClicked(object sender, ItemClickEventArgs e)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions
            {
                ShowMode = FlyoutShowMode.Standard
            };
            HostCommandBar.ShowAt((sender as ListView).ContainerFromItem(e.ClickedItem), myOption);
            SelectedHost = e.ClickedItem as HostUnit;
        }
        private void HostSelect(object sender, RoutedEventArgs e)
        {
            if (SelectedHost != null)
            {
                Teacher.HostedSubjects.Remove(SelectedHost);
                HostCommandBar.Hide();
            }
        }

        private void FormsSelected(object sender, SelectionChangedEventArgs e)
        {
            if (edit)
            {
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
            }
            
        }

        private void LoadForms(object sender, RoutedEventArgs e)
        {
            edit = false;
            (sender as ListView).SelectedItems.Clear();
            foreach (FormCabinetPair item in SelectedHost.Forms)
            { 
                foreach (FormUnit unit in Forms)
                {
                    if(unit.ToString() == item.Form.ToString()) (sender as ListView).SelectedItems.Add(unit);
                }
            }
            edit = true;
        }

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
            SubgroupCommandBar.ShowAt((sender as ListView).ContainerFromItem(e.ClickedItem), myOption);

            CurrentSelectForm = e.ClickedItem as FormUnit;
            foreach(var i in SelectedHost.Forms)
            {
                if (i.Form.ToString() == CurrentSelectForm.ToString())
                {
                    CabinetBox.Text = i.Cabinet.ToString();
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
                    i.Cabinet = int.Parse(CabinetBox.Text);
                }
            }
            
            SubgroupCommandBar.Hide();
        }

        private void SetCabinet(object sender, RoutedEventArgs e)
        {

        }
    }
}
