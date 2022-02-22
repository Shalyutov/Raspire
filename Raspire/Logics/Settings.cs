using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace Raspire
{
    public class Settings
    {
        public List<int> Workdays { get; set; }
        public ObservableCollection<Form> Forms { get; set; }
        public ObservableCollection<Subject> SubjectUnits { get; set; }
        public ObservableCollection<Teacher> TeacherUnits { get; set; }
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
            Forms = new ObservableCollection<Form>();
            SubjectUnits = new ObservableCollection<Subject>();
            TeacherUnits = new ObservableCollection<Teacher>();
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
                Forms = SettingsHelper.GetObservableCollectionLocal<Form>("formUnits");
                SubjectUnits = SettingsHelper.GetObservableCollectionLocal<Subject>("subjectUnits");
                TeacherUnits = SettingsHelper.GetObservableCollectionLocal<Teacher>("teacherUnits");
                SchoolName = SettingsHelper.GetStringLocal("schoolName");
                HeadSchool = SettingsHelper.GetStringLocal("headSchool");
                ScheduleHolder = SettingsHelper.GetStringLocal("scheduleHolder");
            }
            else
            {
                Workdays = new List<int>();
                Forms = new ObservableCollection<Form>();
                SubjectUnits = new ObservableCollection<Subject>();
                TeacherUnits = new ObservableCollection<Teacher>();
                SchoolName = "";
                HeadSchool = "";
                ScheduleHolder = "";
                SaveSettings();
            }
        }
        [JsonConstructor]
        public Settings(List<int> workdays, ObservableCollection<Form> formUnits, ObservableCollection<Subject> subjectUnits, ObservableCollection<Teacher> teacherUnits, string schoolName, string headSchool, string scheduleHolder)
        {
            Workdays = workdays;
            Forms = formUnits;
            SubjectUnits = subjectUnits;
            TeacherUnits = teacherUnits;
            SchoolName = schoolName;
            HeadSchool = headSchool;
            ScheduleHolder = scheduleHolder;
        }

        public void DefaultSettings()//default data template
        {
            Workdays = new List<int>() { 0, 1, 2, 3, 4 };
            Forms = new ObservableCollection<Form>();
            foreach (string form in "1А;2А;3А;4А;5А;6А;7А;8А;9А;10А;11А".Split(";"))
            {
                Forms.Add(new Form(form, 0));
            }

            SubjectUnits = new ObservableCollection<Subject>();
            foreach (string subject in "Математика;Русский язык;Литература;История;Обществозание;География;Биология;Информатика;Английский язык;Физика;Химия;ИЗО;Музыка;МХК;Чтение;Окружающий мир;ОБЖ".Split(";"))
            {
                SubjectUnits.Add(new Subject(subject));
            }

            TeacherUnits = new ObservableCollection<Teacher>();
            SchoolName = "";
            HeadSchool = "";
            ScheduleHolder = "";
            SaveSettings();
        }
        public void SaveSettings()
        {
            SettingsHelper.SaveObjectLocal("Settings", "Saved");
            SettingsHelper.SaveObjectLocal("workdays", Workdays);
            SettingsHelper.SaveObjectLocal("formUnits", Forms);
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
            SettingsHelper.SaveObjectLocal("formUnits", Forms);
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

            if (Forms.Count < 1)
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
    
    public class Subject
    {
        public string Name { get; set; }
        [JsonConstructor]
        public Subject(string name)
        {
            Name = name;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class Teacher
    {
        /// <summary>
        /// Класс представляющий информацию об учителе, его кабинете и проводимых им предметов
        /// </summary>
        public string Name { get; set; }
        public int Classroom { get; set; }
        public ObservableCollection<Host> HostedSubjects { get; set; } = new ObservableCollection<Host>();
        public Teacher(string name, int classroom)
        {
            Name = name;
            Classroom = classroom;
            HostedSubjects = new ObservableCollection<Host>();
        }

        [JsonConstructor]
        public Teacher(string name, int classroom, ObservableCollection<Host> subjects)
        {
            Name = name;
            Classroom = classroom;
            if (subjects != null)
            {
                HostedSubjects = subjects;
            }
        }
    }
    /// <summary>
    /// Запись о проводимом предмете и у каких классов (подрупп)
    /// </summary>
    public class Host
    {
        public Subject Subject { get; set; }
        public ObservableCollection<FormClassroom> Forms { get; set; }
        public Host(Subject subject, ObservableCollection<FormClassroom> forms)
        {
            Subject = subject;
            Forms = forms;
        }
        public override string ToString()
        {
            return $"{Subject.Name} • {Forms.Count}";
        }
    }
    /// <summary>
    /// Запись о проведении предмета у конкретно представленного класса (подгруппы класса)
    /// </summary>
    public class FormClassroom
    {
        public Form Form { get; set; }
        public int Subgroup { get; set; }
        public int Cabinet { get; set; }

        public FormClassroom(Form form, int cabinet, int subgroup)
        {
            Form = form;
            Subgroup = subgroup;
            Cabinet = cabinet;
        }
    }
}
