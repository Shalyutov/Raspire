using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Popups;

namespace Raspire
{
    /// <summary>
    /// Главный класс-структура для храненния расписания.
    /// </summary>
    public class Schedule
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public int Number { get; set; }
        public DateTimeOffset? Date { get; set; }
        public List<LessonItem> LessonItems { get; set; }
        public Settings Settings { get; set; }
        [JsonConstructor]
        public Schedule(string name, List<LessonItem> lessons, int type, int number, Settings settings, DateTimeOffset? date = null)
        {
            Name = name;
            LessonItems = lessons;
            Type = type;
            Number = number;
            Date = date;
            Settings = settings;
        }
        public Schedule(string name, int type, Settings settings, DateTimeOffset? date = null)
        {
            Name = name;
            LessonItems = new List<LessonItem>();
            Type = type;
            Number = 1;
            Date = date;
            Settings = settings;
        }
        public Schedule(string name, int type, int number = 1, DateTimeOffset? date = null)
        {
            Name = name;
            LessonItems = new List<LessonItem>();
            Type = type;
            Number = number;
            Date = date;
            Settings = new Settings();
        }
        public void Init(ObservableCollection<LessonsClassUnit> LessonsClassUnits)
        {
            /*if (LessonsClassUnits.Count == 0) return;
            UnitsWorkdays = new ObservableCollection<WorkdayClassesUnit>();
            int i = 0;
            while(i < LessonsClassUnits[0].LessonUnits.Count)
            {
                WorkdayClassesUnit unit = new WorkdayClassesUnit("", new ObservableCollection<ClassesLessonUnit>());
                string h = "";
                foreach (LessonsClassUnit u in LessonsClassUnits)
                {
                    h = u.LessonUnits[i].Workday;
                    unit.ClassesLessonUnits.Add(new ClassesLessonUnit(u.LessonUnits[i].LessonUnits, u.FormUnit));
                }
                i++;
                unit.Workday = h;
                UnitsWorkdays.Add(unit);
            } */
        }
        public ObservableCollection<LessonsClassUnit> GetStructLesssonClass()
        {
            ObservableCollection<LessonsClassUnit> LessonsClassUnits = new ObservableCollection<LessonsClassUnit>();
            foreach (FormUnit form in Settings.FormUnits)
            {
                LessonsClassUnits.Add(new LessonsClassUnit(form, new ObservableCollection<CollectionWorkdaysLessons>()));
                if (Type == 0 || Type == 1)
                {
                    foreach (int day in Settings.Workdays)
                    {
                        LessonsClassUnits.Last().WorkdaysUnits.Add(new CollectionWorkdaysLessons(new ObservableCollection<LessonUnit>(), GetDay(day)));
                    }
                }
                else
                {
                    LessonsClassUnits.Last().WorkdaysUnits.Add(new CollectionWorkdaysLessons(new ObservableCollection<LessonUnit>(), ""));
                }
            }
            foreach (LessonItem item in LessonItems)
            {
                int f = GetIndexForm(item.Form);
                int w = GetIndexWorkday(item.Workday);
                if (f == -1)
                {
                    continue;
                }
                if (w == -1)
                {
                    continue;
                }
                LessonsClassUnits[f].WorkdaysUnits[w].LessonUnits.Add(item.Lesson);
            }
            return LessonsClassUnits;
        }
        private int GetIndexForm(FormUnit form)
        {
            int f = 0;
            foreach(FormUnit unit in Settings.FormUnits)
            {
                if (unit.ToString() == form.ToString())
                {
                    return f;
                }
                f++;
            }
            return -1;
        }
        private int GetIndexWorkday(string workday)
        {
            int w = 0;
            foreach (int unit in Settings.Workdays)
            {
                if (GetDay(unit) == workday)
                {
                    return w;
                }
                w++;
            }
            return -1;
        }
        public ObservableCollection<WorkdayClassesUnit> GetStructWorkday()
        {
            ObservableCollection<WorkdayClassesUnit> WorkdayClassesUnits = new ObservableCollection<WorkdayClassesUnit>();
            foreach (int workday in Settings.Workdays)
            {
                var units = new ObservableCollection<ClassesLessonUnit>();
                foreach (FormUnit form in Settings.FormUnits)
                {
                    units.Add(new ClassesLessonUnit(new ObservableCollection<LessonUnit>(), form));
                }
                WorkdayClassesUnits.Add(new WorkdayClassesUnit(GetDay(workday), units));
            }
            foreach (LessonItem item in LessonItems)
            {
                int f = GetIndexForm(item.Form);
                int w = GetIndexWorkday(item.Workday);
                WorkdayClassesUnits[w].ClassesUnits[f].Units.Add(item.Lesson);
            }
            return WorkdayClassesUnits;
        }
        public void Init(Settings SettingsInstance)
        {
            /*if (LessonsClassUnits.Count == 0)
            {
                foreach (FormUnit form in SettingsInstance.FormUnits)
                {
                    LessonsClassUnits.Add(new LessonsClassUnit(form, new ObservableCollection<CollectionWorkdaysLessons>()));
                    if (Type == 0 || Type == 1)
                    {
                        foreach (int day in SettingsInstance.Workdays)
                        {
                            LessonsClassUnits.Last().LessonUnits.Add(new CollectionWorkdaysLessons(new ObservableCollection<LessonUnit>(), GetDay(day)));
                        }
                    }
                    else
                    {
                        LessonsClassUnits.Last().LessonUnits.Add(new CollectionWorkdaysLessons(new ObservableCollection<LessonUnit>(), ""));
                    }
                }
            }

            UnitsWorkdays = new ObservableCollection<WorkdayClassesUnit>();
            int i = 0;
            while (i < LessonsClassUnits[0].LessonUnits.Count)
            {
                WorkdayClassesUnit unit = new WorkdayClassesUnit("", new ObservableCollection<ClassesLessonUnit>());
                string h = "";
                foreach (LessonsClassUnit u in LessonsClassUnits)
                {
                    h = u.LessonUnits[i].Workday;
                    unit.ClassesLessonUnits.Add(new ClassesLessonUnit(u.LessonUnits[i].LessonUnits, u.FormUnit));
                }
                i++;
                unit.Workday = h;
                UnitsWorkdays.Add(unit);
            }*/
        }
        public async Task<StorageFile> Save(StorageFile file)
        {
            if (file != null)
            {
                try
                {
                    SettingsHelper.SaveObjectLocal("Recent", file.Path);
                    await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(this));
                    _ = StorageApplicationPermissions.FutureAccessList.Add(file);
                    return file;
                }
                catch(Exception)
                {
                    MessageDialog dialog = new MessageDialog($"Произошла ошибка при сохранении файла");
                    _ = await dialog.ShowAsync();
                    return null;
                }
            }
            else
            {
                return await Export();
            }
        }
        public async Task<StorageFile> Export()
        {
            try
            {
                Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker
                {
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = Name
                };
                savePicker.FileTypeChoices.Add("Файл разметки расписания", new List<string>() { ".rsl" });
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);
                    SettingsHelper.SaveObjectLocal("Recent", file.Path);
                    _ = StorageApplicationPermissions.FutureAccessList.Add(file);
                    await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(this));

                    Windows.Storage.Provider.FileUpdateStatus status =
                        await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        MessageDialog dialog = new MessageDialog($"Файл {file.Name} невозможно сохранить");
                        _ = await dialog.ShowAsync();
                        return null;
                    }
                    return file;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                MessageDialog dialog = new MessageDialog($"Произошла ошибка при сохранении файла");
                _ = await dialog.ShowAsync();
                return null;
            }
            
        }
        public static string GetDay(int day)
        {
            switch (day)
            {
                case 0: return "ПН";
                case 1: return "ВТ";
                case 2: return "СР";
                case 3: return "ЧТ";
                case 4: return "ПТ";
                case 5: return "СБ";
                case 6: return "ВС";
                default: return "?";
            }
        }
        public static int GetDayInt(string day)
        {
            switch (day)
            {
                case "ПН": return 0;
                case "ВТ": return 1;
                case "СР": return 2;
                case "ЧТ": return 3;
                case "ПТ": return 4;
                case "СБ": return 5;
                case "ВС": return 6;
                default: return -1;
            }
        }
    }
    public class LessonItem
    {
        public LessonUnit Lesson { get; set; }
        public FormUnit Form { get; set; }
        public string Workday { get; set; }

        public LessonItem(LessonUnit lesson, FormUnit form, string workday)
        {
            Lesson = lesson;
            Form = form;
            Workday = workday;
        }
    }
    public class LessonsClassUnit
    {
        public FormUnit FormUnit { get; set; }
        public ObservableCollection<CollectionWorkdaysLessons> WorkdaysUnits { get; set; }
        public LessonsClassUnit(FormUnit formUnit, ObservableCollection<CollectionWorkdaysLessons> lessonUnits)
        {
            FormUnit = formUnit;
            WorkdaysUnits = lessonUnits;
        }
    }
    public class WorkdayClassesUnit
    {
        public string Workday { get; set; }
        public ObservableCollection<ClassesLessonUnit> ClassesUnits { get; set; }

        public WorkdayClassesUnit(string workday, ObservableCollection<ClassesLessonUnit> classesLessonUnits)
        {
            Workday = workday;
            ClassesUnits = classesLessonUnits;
        }
        
    }
    public class ClassesLessonUnit
    {
        public ObservableCollection<LessonUnit> Units { get; set; }
        public FormUnit Form { get; set; }

        public ClassesLessonUnit(ObservableCollection<LessonUnit> units, FormUnit form)
        {
            Units = units;
            Form = form;
        }
    }
    public class CollectionWorkdaysLessons
    {
        public string Workday { get; set; }
        public ObservableCollection<LessonUnit> LessonUnits { get; set; }
        public CollectionWorkdaysLessons(ObservableCollection<LessonUnit> lessonUnits, string workday)
        {
            LessonUnits = lessonUnits;
            Workday = workday;
        }
    }
    public class LessonUnit
    {
        public ObservableCollection<SubjectUnit> Subject { get; set; } = new ObservableCollection<SubjectUnit>();
        public ObservableCollection<int> Cabinet { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<string> Teacher { get; set; } = new ObservableCollection<string>();
        public LessonUnit(SubjectUnit subject, int cabinet, TeacherUnit teacher)
        {
            Subject.Add(subject);
            Cabinet.Add(cabinet);
            Teacher.Add(teacher.Name);
        }
        [JsonConstructor]
        public LessonUnit(ObservableCollection<SubjectUnit> subject, ObservableCollection<int> cabinet, ObservableCollection<string> teacher)
        {
            Subject = subject;
            Cabinet = cabinet;
            Teacher = teacher;
        }
        public LessonUnit(SubjectUnit subject, TeacherUnit teacher)
        {
            Subject.Add(subject);
            Cabinet.Add(teacher.Cabinet);
            Teacher.Add(teacher.Name);
        }
        public LessonUnit()
        {

        }
        public override string ToString()
        {
            string result = "";
            if (Subject.Count == 1)
            {
                if(Cabinet[0] == -1)
                {
                    result = Subject[0].CallName;
                }
                else
                {
                    result = Subject[0].CallName + " | " + Cabinet[0].ToString();
                }
                
            }
            else if (Subject.Count() == 2)
            {
                if (Subject[0].CallName == Subject[1].CallName)
                {
                    result = Subject[0].CallName + " | " + Cabinet[0].ToString() + "/" + Cabinet[1].ToString();
                }
                else
                {
                    result = Subject[0].CallName.Substring(0, 5) + "/" + Subject[1].CallName.Substring(0, 5) + " | " + Cabinet[0].ToString() + "/" + Cabinet[1].ToString();
                }
            }
            if (result == "")
                result = "|  пустой  |";
            return result;
        }
        public string Represent()
        {
            string result = "";
            if (Subject.Count == 1)
            {
                if (Cabinet[0] == -1)
                {
                    result = Subject[0].CallName;
                }
                else
                {
                    result = Subject[0].CallName;
                }

            }
            else if (Subject.Count() == 2)
            {
                if (Subject[0].CallName == Subject[1].CallName)
                {
                    result = Subject[0].CallName;
                }
                else
                {
                    result = Subject[0].CallName.Substring(0, 5) + "/" + Subject[1].CallName.Substring(0, 5);
                }
            }
            if (result == "")
                result = "|  пустой  |";
            return result;
        }
        public string RepresentCab()
        {
            string result = "";
            if (Subject.Count == 1)
            {
                result = Cabinet[0].ToString();
            }
            else if (Subject.Count() == 2)
            {
                result = Cabinet[0].ToString() + "/" + Cabinet[1].ToString();
            }
            return result;
        }
    }
}
