using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raspire.Logics
{
    internal class Lesson : IEquatable<Lesson>
    {
        public Subject Subject { get; set; }
        public Group Group { get; set; }
        public string TeacherName { get; set; }
        public int Classroom { get; set; }
        public Lesson(Subject subject, Group group, string teacherName, int classroom)
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
                   EqualityComparer<Group>.Default.Equals(Group, other.Group) &&
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
            return $"{Subject} • {Classroom}";
        }
    }
    internal class LessonItem
    {
        public Lesson Lesson { get; set; }
        public int Workday { get; set; }
        public LessonItem(Lesson lesson, int workday)
        {
            Lesson = lesson;
            Workday = workday;
        }
    }

}
