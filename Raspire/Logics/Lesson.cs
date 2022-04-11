using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raspire
{
    public class Lesson : IEquatable<Lesson>
    {
        public Subject Subject { get; set; }
        public Form Form { get; set; }
        public int Subgroup { get; set; }
        public string TeacherName { get; set; }
        public int Classroom { get; set; }
        public Lesson(Subject subject, Form form, string teacherName, int classroom, int subgroup)
        {
            Subject = subject;
            Form = form;
            TeacherName = teacherName;
            Classroom = classroom;
            Subgroup = subgroup;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Lesson);
        }
        public bool Equals(Lesson other)
        {
            return other != null &&
                   EqualityComparer<Subject>.Default.Equals(Subject, other.Subject) &&
                   EqualityComparer<Form>.Default.Equals(Form, other.Form) &&
                   TeacherName == other.TeacherName &&
                   Classroom == other.Classroom &&
                   Subgroup == other.Subgroup;
        }
        public override int GetHashCode()
        {
            int hashCode = -1645466367;
            hashCode = hashCode * -1521134295 + EqualityComparer<Subject>.Default.GetHashCode(Subject);
            hashCode = hashCode * -1521134295 + EqualityComparer<Group>.Default.GetHashCode(Form);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TeacherName);
            hashCode = hashCode * -1521134295 + Classroom.GetHashCode();
            hashCode = hashCode * -1521134295 + Subgroup.GetHashCode();
            return hashCode;
        }
        public static bool operator ==(Lesson left, Lesson right)
        {
            return EqualityComparer<Lesson>.Default.Equals(left, right);
        }
        public static bool operator !=(Lesson left, Lesson right)
        {
            return !(left == right);
        }
        public override string ToString()
        {
            string ClassroomFormat = Classroom == 0 ? "" : Classroom.ToString();
            string subgroupformat = Subgroup > 0 ? $"({Subgroup} подгруппа)" : "";
            return $"{Subject} • {ClassroomFormat}{subgroupformat}";
        }
    }
    public class LessonItem : IEquatable<LessonItem>
    {
        public MultipleLesson Lesson { get; set; }
        public int Workday { get; set; }
        public LessonItem(MultipleLesson lesson, int workday)
        {
            Lesson = lesson;
            Workday = workday;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LessonItem);
        }

        public bool Equals(LessonItem other)
        {
            if (other != null)
            {
                return EqualityComparer<MultipleLesson>.Default.Equals(Lesson as MultipleLesson, other.Lesson as MultipleLesson) &&
                           Workday == other.Workday;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 1009865043;
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Lesson);
            hashCode = hashCode * -1521134295 + Workday.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(LessonItem left, LessonItem right)
        {
            return EqualityComparer<LessonItem>.Default.Equals(left, right);
        }

        public static bool operator !=(LessonItem left, LessonItem right)
        {
            return !(left == right);
        }
    }
    public class MultipleLesson : IEquatable<MultipleLesson>
    {
        public List<Lesson> Lessons { get; set; }
        [JsonConstructor]
        public MultipleLesson(List<Lesson> lessons)
        {
            Lessons = lessons;
        }
        public MultipleLesson(Lesson lesson)
        {
            Lessons = new List<Lesson>
            {
                lesson
            };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MultipleLesson);
        }

        public bool Equals(MultipleLesson other)
        {
            return other != null &&
                   EqualityComparer<List<Lesson>>.Default.Equals(Lessons, other.Lessons);
        }

        public override int GetHashCode()
        {
            return 2007667718 + EqualityComparer<List<Lesson>>.Default.GetHashCode(Lessons);
        }

        public static bool operator ==(MultipleLesson left, MultipleLesson right)
        {
            return EqualityComparer<MultipleLesson>.Default.Equals(left, right);
        }

        public static bool operator !=(MultipleLesson left, MultipleLesson right)
        {
            return !(left == right);
        }
        public override string ToString()
        {
            switch (Lessons.Count)
            {
                case 0:
                    return "";
                case 1:
                    return Lessons[0].ToString();
                case 2:
                    return $"{Lessons[0].Subject.Name.Substring(0, 5)}/{Lessons[1].Subject.Name.Substring(0, 5)} • {Lessons[0].Classroom}/{Lessons[1].Classroom}";
                default:
                    string multi = "";
                    for (int i = 0; i < Lessons.Count; i++) multi += " •";
                    return multi;
            }
        }
        public string Subject()
        {
            switch (Lessons.Count)
            {
                case 0:
                    return "";
                case 1:
                    return Lessons[0].Subject.ToString();
                case 2:
                    return $"{Lessons[0].Subject.Name.Take(6)}/{Lessons[1].Subject.Name.Take(6)}";
                default:
                    string multi = "";
                    for (int i = 0; i < Lessons.Count; i++) multi += " •";
                    return multi;
            }
        }
        public string Classroom()
        {
            switch (Lessons.Count)
            {
                case 0:
                    return "";
                case 1:
                    return Lessons[0].Classroom.ToString();
                case 2:
                    return $"{Lessons[0].Classroom}/{Lessons[1].Classroom}";
                default:
                    string multi = "";
                    for (int i = 0; i < Lessons.Count; i++) multi += " •";
                    return multi;
            }
        }
    }
}
