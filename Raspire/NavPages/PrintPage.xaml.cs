//using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Raspire
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PrintPage : Page
    {
        string schoolName;
        string headSchool;
        //public Schedule Schedule;
        public ObservableCollection<LessonsClassUnit> LessonsClassUnits { get; set; }
        int Type;
        int Number;
        DateTimeOffset? Date;
        public PrintPage(ObservableCollection<LessonsClassUnit> lessonsClassUnits, string school, string head, int type, int number, DateTimeOffset? date = null)
        {
            //Schedule = schedule;
            Type = type;
            Number = number;
            LessonsClassUnits = lessonsClassUnits;
            schoolName = school;
            headSchool = head;
            Date = date;
            this.InitializeComponent();
            Bindings.Update();
        }
        public string GetFooter()
        {
            return $"Утверждено директором {schoolName}: {headSchool}";
        }
        public string GetHeader()
        {
            switch (Type)
            {
                case 0:
                    return $"Расписание на {Number} полугодие";
                case 1:
                    return $"Расписание на {Number} четверть";
                case 2:
                    return $"Изменения в расписании на {Date.Value.Date.ToShortDateString()}";
                default:
                    return "Расписание";
            }
        }
        public string GetShift()
        {
            return SecondShiftEnabled() ? $"{LessonsClassUnits[0].FormUnit.Shift} смена" : "";
        }
        public bool SecondShiftEnabled()
        {
            object res = SettingsHelper.GetObjectRawLocal("SecondShift");
            if (res != null)
            {
                return (string)res == "true";
            }
            else
            {
                return false;
            }
        }
    }
}
