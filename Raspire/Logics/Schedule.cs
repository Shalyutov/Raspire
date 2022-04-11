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
        public List<LessonItem> LessonItems { get; set; }
        public Settings Settings { get; set; }
        public Schedule(List<LessonItem> lessons, Settings settings)
        {
            LessonItems = lessons;
            Settings = settings;
        }
        public ObservableCollection<FormWorkdays> GetStructForms()
        {
            ObservableCollection<FormWorkdays> units = new ObservableCollection<FormWorkdays>();
            foreach (Form form in Settings.Forms)
            {
                units.Add(new FormWorkdays(form, new ObservableCollection<WorkdayLessons>()));
                foreach (int day in Settings.Workdays)
                {
                    units.Last().WorkdayLessons.Add(new WorkdayLessons(new ObservableCollection<MultipleLesson>(), day));
                }
            }
            foreach (LessonItem item in LessonItems)
            {
                int f = Settings.Forms.IndexOf(item.Lesson.Lessons[0].Form);
                units[f].WorkdayLessons[item.Workday].Lessons.Add(item.Lesson);
            }
            return units;
        }
        public ObservableCollection<WorkdayForms> GetStructWorkday()
        {
            ObservableCollection<WorkdayForms> units = new ObservableCollection<WorkdayForms>();
            foreach (int workday in Settings.Workdays)
            {
                var unitsl = new ObservableCollection<FormLessons>();
                foreach (Form form in Settings.Forms)
                {
                    unitsl.Add(new FormLessons(new ObservableCollection<MultipleLesson>(), form));
                }
                units.Add(new WorkdayForms(workday, unitsl));
            }
            foreach (LessonItem item in LessonItems)
            {
                int f = Settings.Forms.IndexOf(item.Lesson.Lessons[0].Form);
                units[item.Workday].FormLessons[f].Lessons.Add(item.Lesson);
            }
            return units;
        }
        public void RestoreStruct(ObservableCollection<WorkdayForms> units)
        {
            LessonItems.Clear();
            foreach(WorkdayForms unit in units)
            {
                foreach(FormLessons form in unit.FormLessons)
                {
                    foreach(MultipleLesson lesson in form.Lessons)
                    {
                        LessonItems.Add(new LessonItem(lesson, unit.Workday));
                    }
                }
            }
        }
        public async Task<StorageFile> Save(StorageFile file)
        {
            if (file != null)
            {
                try
                {
                    SettingsHelper.SaveObjectLocal("Recent", file.Path);
                    await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    }));
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
                    SuggestedFileName = "Расписание"
                };
                savePicker.FileTypeChoices.Add("Файл разметки расписания", new List<string>() { ".rsl" });
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);
                    SettingsHelper.SaveObjectLocal("Recent", file.Path);
                    _ = StorageApplicationPermissions.FutureAccessList.Add(file);
                    await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    }));

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
    }
    public class FormLessons
    {
        public Form Form { get; set; }
        public ObservableCollection<MultipleLesson> Lessons { get; set; }
        public FormLessons(ObservableCollection<MultipleLesson> units, Form form)
        {
            Lessons = units;
            Form = form;
        }
    }
    public class WorkdayLessons
    {
        public int Workday { get; set; }
        public ObservableCollection<MultipleLesson> Lessons { get; set; }
        public WorkdayLessons(ObservableCollection<MultipleLesson> lessonUnits, int workday)
        {
            Lessons = lessonUnits;
            Workday = workday;
        }
    }
    public class FormWorkdays
    {
        public Form Form { get; set; }
        public ObservableCollection<WorkdayLessons> WorkdayLessons { get; set; }
        public FormWorkdays(Form formUnit, ObservableCollection<WorkdayLessons> lessonUnits)
        {
            Form = formUnit;
            WorkdayLessons = lessonUnits;
        }
    }
    public class WorkdayForms
    {
        public int Workday { get; set; }
        public ObservableCollection<FormLessons> FormLessons { get; set; }
        public WorkdayForms(int workday, ObservableCollection<FormLessons> formLessonUnits)
        {
            Workday = workday;
            FormLessons = formLessonUnits;
        }
        public string GetDay(int day)
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
    }
}
