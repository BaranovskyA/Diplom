using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Entities
{
    public class TableData
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }

        public TableData(int id, string fIO, int age, string gender, string email, string role, string username)
        {
            Id = id;
            FIO = fIO;
            Age = age;
            Gender = gender;
            Email = email;
            Role = role;
            Username = username;
        }
    }
}
