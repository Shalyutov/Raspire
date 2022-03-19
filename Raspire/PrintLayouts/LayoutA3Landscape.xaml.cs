﻿using System;
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
    /// <summary>
    /// Страница макета печати общего расписания
    /// </summary>
    public sealed partial class LayoutA3Landscape : Page
    {
        public ObservableCollection<WorkdayForms> UnitsWorkdays { get; set; }
        private string Number;
        private string Year;
        private string Law;
        private DateTime Date;
        public LayoutA3Landscape(ObservableCollection<WorkdayForms> units, string number, string year, string law, DateTime date)
        {
            this.InitializeComponent();
            UnitsWorkdays = units;
            this.Number = number;
            this.Year = year;
            this.Law = law;
            this.Date = date;
        }
        private string GetHeader()
        {
            return $"Расписание уроков на {Number} полугодие {Year} учебного года";
        }
        private string GetFooter()
        {
            string footer = "Утверждено";
            var SettingsInstance = Settings.GetSavedSettings();
            if (Law != "1" | Law != "")
            {

                footer += $" приказом № {Law} от {Date.ToShortDateString()}";
            }
            footer += $"\nДиректор {SettingsInstance.SchoolName} {SettingsInstance.HeadSchool}";
            return footer;
        }
    }
}
