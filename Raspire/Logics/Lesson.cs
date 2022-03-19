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
        public Form Group { get; set; }
        public string TeacherName { get; set; }
        public int Classroom { get; set; }
        public Lesson(Subject subject, Form group, string teacherName, int classroom)
        {
            Subject = subject;
            Group = group;
            TeacherName = teacherName;
            Classroom = classroom;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Lesson);
        }
        public bool Equals(Lesson other)
        {
            return other != null &&
                   EqualityComparer<Subject>.Default.Equals(Subject, other.Subject) &&
                   EqualityComparer<Form>.Default.Equals(Group, other.Group) &&
                   TeacherName == other.TeacherName &&
                   Classroom == other.Classroom;
        }
        public override int GetHashCode()
        {
            int hashCode = -1645466367;
            hashCode = hashCode * -1521134295 + EqualityComparer<Subject>.Default.GetHashCode(Subject);
            hashCode = hashCode * -1521134295 + EqualityComparer<Group>.Default.GetHashCode(Group);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TeacherName);
            hashCode = hashCode * -1521134295 + Classroom.GetHashCode();
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
            return $"{Subject} • {ClassroomFormat}";
        }
    }
    public class LessonItem : IEquatable<LessonItem>
    {
        public Lesson Lesson { get; set; }
        public int Workday { get; set; }
        public LessonItem(Lesson lesson, int workday)
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
            return other != null &&
                   EqualityComparer<Lesson>.Default.Equals(Lesson, other.Lesson) &&
                   Workday == other.Workday;
        }

        public override int GetHashCode()
        {
            int hashCode = 1009865043;
            hashCode = hashCode * -1521134295 + EqualityComparer<Lesson>.Default.GetHashCode(Lesson);
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

}
