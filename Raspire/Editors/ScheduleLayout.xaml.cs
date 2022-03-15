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
    public sealed partial class ScheduleLayout : Page
    {
        public ObservableCollection<WorkdayForms> UnitsWorkdays { get; set; }
        public ScheduleLayout(ObservableCollection<WorkdayForms> units)
        {
            this.InitializeComponent();
            UnitsWorkdays = units;
        }
    }
}
