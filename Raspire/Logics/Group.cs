using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raspire.Logics
{
    /// <summary>
    /// Класс представляющий экземпляр учебной группы
    /// </summary>
    internal class Group : IEquatable<Group>
    {
        public string Name { get; set; }
        public int SubgroupCount { get; set; }
        public Group(string name, int subgroupCount)
        {
            Name = name;
            SubgroupCount = subgroupCount;
        }
        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Group);
        }

        public bool Equals(Group other)
        {
            return other != null &&
                   Name == other.Name &&
                   SubgroupCount == other.SubgroupCount;
        }

        public override int GetHashCode()
        {
            int hashCode = 1874024985;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + SubgroupCount.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Group left, Group right)
        {
            return EqualityComparer<Group>.Default.Equals(left, right);
        }

        public static bool operator !=(Group left, Group right)
        {
            return !(left == right);
        }
    }
    /// <summary>
    /// Класс представляющий экземпляр учебного класса
    /// </summary>
    internal class Form : Group
    {
        public int Shift { get; set; }

        [JsonConstructor]
        public Form(string name, int subgroupCount, int shift = 1) : base(name, subgroupCount)
        {
            Shift = shift;
        }
    }
}
