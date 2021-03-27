using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Entities
{
    public class Employer
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public Employer() { }

        public Employer(int id, string fIO, int age, string gender, string email, int userId, User user)
        {
            Id = id;
            FIO = fIO;
            Age = age;
            Gender = gender;
            Email = email;
            UserId = userId;
            User = user;
        }

        public Employer(string fIO, int age, string gender, string email, int userId, User user)
        {
            FIO = fIO;
            Age = age;
            Gender = gender;
            Email = email;
            UserId = userId;
            User = user;
        }

        public Employer(string fIO, int age, string gender, string email, int userId)
        {
            FIO = fIO;
            Age = age;
            Gender = gender;
            Email = email;
            UserId = userId;
            User = DBHelper.FindUserById(userId);
        }
    }
}
