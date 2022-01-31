using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace Raspire
{
    public class Settings
    {
        public List<int> Workdays { get; set; }
        public ObservableCollection<FormUnit> FormUnits { get; set; }
        public ObservableCollection<SubjectUnit> SubjectUnits { get; set; }
        public ObservableCollection<TeacherUnit> TeacherUnits { get; set; }
        public string SchoolName { get; set; }
        public string HeadSchool { get; set; }
        public string ScheduleHolder { get; set; }
        /// <summary>
        /// Create new instance of settings by providing information about school
        /// </summary>
        /// <param name="workdays">Days when lessons are on</param>
        /// <param name="schoolName">CallName of the school</param>
        /// <param name="headSchool">CallName of Head of the school</param>
        /// <param name="scheduleHolder">CallName of Schedule Responder</param>
        
        public Settings(List<int> workdays, string schoolName, string headSchool, string scheduleHolder)
        {
            Workdays = workdays;
            FormUnits = new ObservableCollection<FormUnit>();
            SubjectUnits = new ObservableCollection<SubjectUnit>();
            TeacherUnits = new ObservableCollection<TeacherUnit>();
            SchoolName = schoolName;
            HeadSchool = headSchool;
            ScheduleHolder = scheduleHolder;
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
        }
        /// <summary>
        /// Fetch data from previously saved settings
        /// </summary>
        public Settings()//Fetch data from saved settings or create new blank instance
        {
            if (SettingsHelper.GetStringLocal("Settings") == "Saved")
            {
                Workdays = SettingsHelper.GetListLocal<int>("workdays");
                FormUnits = SettingsHelper.GetObservableCollectionLocal<FormUnit>("formUnits");
                SubjectUnits = SettingsHelper.GetObservableCollectionLocal<SubjectUnit>("subjectUnits");
                TeacherUnits = SettingsHelper.GetObservableCollectionLocal<TeacherUnit>("teacherUnits");
                SchoolName = SettingsHelper.GetStringLocal("schoolName");
                HeadSchool = SettingsHelper.GetStringLocal("headSchool");
                ScheduleHolder = SettingsHelper.GetStringLocal("scheduleHolder");
            }
            else
            {
                Workdays = new List<int>();
                FormUnits = new ObservableCollection<FormUnit>();
                SubjectUnits = new ObservableCollection<SubjectUnit>();
                TeacherUnits = new ObservableCollection<TeacherUnit>();
                SchoolName = "";
                HeadSchool = "";
                ScheduleHolder = "";
                SaveSettings();
            }
        }
        [JsonConstructor]
        public Settings(List<int> workdays, ObservableCollection<FormUnit> formUnits, ObservableCollection<SubjectUnit> subjectUnits, ObservableCollection<TeacherUnit> teacherUnits, string schoolName, string headSchool, string scheduleHolder)
        {
            Workdays = workdays;
            FormUnits = formUnits;
            SubjectUnits = subjectUnits;
            TeacherUnits = teacherUnits;
            SchoolName = schoolName;
            HeadSchool = headSchool;
            ScheduleHolder = scheduleHolder;
        }

        public void DefaultSettings()//default data template
        {
            Workdays = new List<int>() { 0, 1, 2, 3, 4 };
            FormUnits = new ObservableCollection<FormUnit>();
            foreach (string form in "1 А;2 А;3 А;4 А;5 А;6 А;7 А;8 А;9 А;10 А;11 А".Split(";"))
            {
                FormUnits.Add(new FormUnit(form));
            }

            SubjectUnits = new ObservableCollection<SubjectUnit>();
            foreach (string subject in "Математика;Русский язык;Литература;История;Обществозание;География;Биология;Информатика;Английский язык;Физика;Химия;ИЗО;Музыка;МХК;Чтение;Окружающий мир;ОБЖ".Split(";"))
            {
                SubjectUnits.Add(new SubjectUnit(subject));
            }

            TeacherUnits = new ObservableCollection<TeacherUnit>();
            SchoolName = "";
            HeadSchool = "";
            ScheduleHolder = "";
            SaveSettings();
        }
        public void SaveSettings()
        {
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
            SettingsHelper.SaveObjectLocal("workdays", Workdays);
            SettingsHelper.SaveObjectLocal("formUnits", FormUnits);
            SettingsHelper.SaveObjectLocal("subjectUnits", SubjectUnits);
            SettingsHelper.SaveObjectLocal("teacherUnits", TeacherUnits);
            SettingsHelper.SaveObjectLocal("SchoolName", SchoolName);
            SettingsHelper.SaveObjectLocal("HeadSchool", HeadSchool);
            SettingsHelper.SaveObjectLocal("ScheduleHolder", ScheduleHolder);
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
        }
        public void SaveWorkdays()
        {
            SettingsHelper.SaveObjectLocal("workdays", Workdays);
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
        }
        public void SaveFormUnits()
        {
            SettingsHelper.SaveObjectLocal("formUnits", FormUnits);
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
        }
        public void SaveSubjectUnits()
        {
            SettingsHelper.SaveObjectLocal("subjectUnits", SubjectUnits);
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
        }
        public void SaveTeacherUnits()
        {
            SettingsHelper.SaveObjectLocal("teacherUnits", TeacherUnits);
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
        }
        public void SaveSchoolName()
        {
            SettingsHelper.SaveObjectLocal("schoolName", SchoolName);
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
        }
        public void SaveHeadSchool()
        {
            SettingsHelper.SaveObjectLocal("headSchool", HeadSchool);
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
        }
        public void SaveScheduleHolder()
        {
            SettingsHelper.SaveObjectLocal("scheduleHolder", ScheduleHolder);
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
        }
        public bool AreSettingsCorreсt()
        {
            if (Workdays.Count < 1)
            {
                return false;
            }

            if (FormUnits.Count < 1)
            {
                return false;
            }

            if (SubjectUnits.Count < 1)
            {
                return false;
            }

            if (TeacherUnits.Count < 1)
            {
                return false;
            }

            SettingsHelper.SaveObjectLocal("Settings", "Saved");
            return true;
        }
    }
    /// <summary>
    /// Класс-помощник для работы с настройками приложения. Предоставляет методы-обёртки для доступа к сохранённым элементам и позволяет производить запись элементом. 
    /// </summary>
    public class SettingsHelper
    {
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private static readonly ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        public static void SaveObjectLocal(string key, object obj)
        {
            localSettings.Values[key] = JsonConvert.SerializeObject(obj);
        }

        public static void SaveObjectRoaming(string key, object obj)
        {
            roamingSettings.Values[key] = JsonConvert.SerializeObject(obj);
        }

        public static void SaveObjectLocal(string key, string obj)
        {
            localSettings.Values[key] = obj;
        }

        public static void SaveObjectRoaming(string key, string obj)
        {
            roamingSettings.Values[key] = obj;
        }
        public static object GetObjectLocal(string key)
        {
            return JsonConvert.DeserializeObject(localSettings.Values[key] as string);
        }

        public static object GetObjectRawLocal(string key)
        {
            return localSettings.Values[key];
        }

        public static object GetObjectRoaming(string key)
        {
            return JsonConvert.DeserializeObject(roamingSettings.Values[key] as string);
        }
        public static string GetStringLocal(string key)
        {
            return localSettings.Values[key] as string;
        }

        public static string GetStringRoaming(string key)
        {
            return roamingSettings.Values[key] as string;
        }

        public static ObservableCollection<T> GetObservableCollectionLocal<T>(string key)
        {
            return JsonConvert.DeserializeObject<ObservableCollection<T>>(localSettings.Values[key] as string);
        }

        public static List<T> GetListLocal<T>(string key)
        {
            return JsonConvert.DeserializeObject<List<T>>(localSettings.Values[key] as string);
        }
    }
    /// <summary>
    /// Класс представляющий экземпляр учебного класса и его параллель
    /// </summary>
    public class FormUnit
    {
        public int Number { get; set; }
        public string Letter { get; set; }
        public int Shift { get; set; } = 1;
        [JsonConstructor]
        public FormUnit(int Number, string Letter, int shift)//Main Constructor
        {
            this.Number = Number;
            this.Letter = Letter;
            Shift = shift;
        }
        public FormUnit(string concat)//Additional constructor
        {
            string[] args = concat.Split(" ");
            if (args.Length == 2)
            {
                Number = int.Parse(args[0]);
                Letter = args[1];
            }
        }
        public FormUnit(string concat, bool secondShift)//Additional constructor
        {
            string[] args = concat.Split(" ");
            if (args.Length == 2)
            {
                Number = int.Parse(args[0]);
                Letter = args[1];
                if (secondShift) Shift = 2;
            }
        }
        public override string ToString()
        {
            return Number.ToString() + Letter;
        }
    }
    public class SubjectUnit
    {
        public string CallName { get; set; }
        [JsonConstructor]
        public SubjectUnit(string name)
        {
            CallName = name;
        }
        public override string ToString()
        {
            return CallName;
        }
    }
    public class TeacherUnit
    {
        /// <summary>
        /// Класс представляющий информацию об учителе, его кабинете и проводимых им предметов
        /// </summary>
        public string Name { get; set; }
        public int Cabinet { get; set; }
        public ObservableCollection<HostUnit> HostedSubjects { get; set; } = new ObservableCollection<HostUnit>();
        public TeacherUnit(string name, int cabinet)
        {
            Name = name;
            Cabinet = cabinet;
            HostedSubjects = new ObservableCollection<HostUnit>();
        }

        [JsonConstructor]
        public TeacherUnit(string name, int cabinet, ObservableCollection<HostUnit> subjects)
        {
            Name = name;
            Cabinet = cabinet;
            if (subjects != null)
            {
                HostedSubjects = subjects;
            }
        }
    }
    /// <summary>
    /// Запись о проводимом предмете и у каких классов (подрупп)
    /// </summary>
    public class HostUnit
    {
        public SubjectUnit Subject { get; set; }
        public ObservableCollection<FormCabinetPair> Forms { get; set; }
        public HostUnit(SubjectUnit subject, ObservableCollection<FormCabinetPair> forms)
        {
            Subject = subject;
            Forms = forms;
        }
    }
    /// <summary>
    /// Запись о проведении предмета у конкретно представленного класса (подгруппы класса)
    /// </summary>
    public class FormCabinetPair
    {
        public FormUnit Form { get; set; }
        public int Subgroup { get; set; }
        public int Cabinet { get; set; }

        public FormCabinetPair(FormUnit form, int cabinet, int subgroup)
        {
            Form = form;
            Subgroup = subgroup;
            Cabinet = cabinet;
        }
    }
}
