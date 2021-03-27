using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public User() { }

        public User(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public User(int id, string username, string password, int roleId)
        {
            Id = id;
            Username = username;
            Password = password;
            RoleId = roleId;
        }

        public User(string username, string password, int roleId)
        {
            Username = username;
            Password = password;
            RoleId = roleId;
        }

        public User(int id, string username, string password, int roleId, Role role) : this(id, username, password, roleId)
        {
            Role = role;
        }
    }
}
