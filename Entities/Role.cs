using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }

        public Role() { }

        public Role(string name, int priority)
        {
            Name = name;
            Priority = priority;
        }

        public Role(int id, string name, int priority)
        {
            Id = id;
            Name = name;
            Priority = priority;
        }
    }
}
