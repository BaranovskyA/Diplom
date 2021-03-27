using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Diplom.Entities
{
    public class DBHelper
    {
        private static string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=corporation;Integrated Security=True";

        // Employer
        public static void AddEmployer(Employer employer)
        {
            string sqlExpression = $"INSERT INTO [Employers] VALUES ('{employer.FIO}', {employer.Age}, '{employer.Gender}'," +
                $"'{employer.Email}', {employer.UserId})";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveEmployer(int id)
        {
            var tempEmp = FindEmployerById(id);

            string sqlExpression = $"DELETE FROM Employers WHERE id = {id}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }

            sqlExpression = $"DELETE FROM Users WHERE id = {tempEmp.UserId}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        public static List<Employer> SelectAllEmployers()
        {
            List<Employer> employers = new List<Employer>();
            string sqlExpression = "SELECT * FROM Employers";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        User tempUser = FindUserById(int.Parse(reader["user_id"].ToString()));

                        employers.Add(new Employer(int.Parse(reader["id"].ToString()), reader["FIO"].ToString(),
                            int.Parse(reader["age"].ToString()), reader["gender"].ToString(), reader["email"].ToString(),
                            int.Parse(reader["user_id"].ToString()), tempUser));
                    }
                }

                reader.Close();
            }
            return employers;
        }

        public static Employer FindEmployerByUsername(string username)
        {
            Employer employer = new Employer();
            User tempUser = FindUserByUsername(username);
            tempUser.Role = FindRoleById(tempUser.RoleId);
            string sqlExpression = $"SELECT * FROM Employers WHERE [user_id] = {tempUser.Id}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        employer = new Employer(int.Parse(reader["id"].ToString()), reader["FIO"].ToString(),
                            int.Parse(reader["age"].ToString()), reader["gender"].ToString(), reader["email"].ToString(), 
                            tempUser.Id, tempUser);
                    }
                }

                reader.Close();
            }
            return employer;
        }

        public static Employer FindEmployerById(int id)
        {
            Employer employer = new Employer();
            User tempUser = FindUserById(id);
            tempUser.Role = FindRoleById(tempUser.RoleId);
            string sqlExpression = $"SELECT * FROM Employers WHERE [id] = {tempUser.Id}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        employer = new Employer(int.Parse(reader["id"].ToString()), reader["FIO"].ToString(),
                            int.Parse(reader["age"].ToString()), reader["gender"].ToString(), reader["email"].ToString(),
                            tempUser.Id, tempUser);
                    }
                }

                reader.Close();
            }
            return employer;
        }

        public static bool CheckAuthorization(string username, string password)
        {
            if(username != "" && password != "" && username.Length >= 5 && password.Length >= 5)
            {
                bool isAuthorizated = false;
                User checkUser = FindUserByUsername(username);
                if (checkUser.Password == password)
                    isAuthorizated = true;
                return isAuthorizated;
            }
            return false;
        }

        public static void EditEmployer(Employer oldUser, Employer newUser)
        {
            try
            {
                string sqlExpression = $"UPDATE [Users] SET";
                bool isDataOK = true;
                if (newUser.User.Username.Length < 5 || newUser.User.Password.Length < 5)
                    isDataOK = false;
                if (isDataOK)
                {
                    sqlExpression += $" username='{newUser.User.Username}',";
                    sqlExpression += $" [password]='{newUser.User.Password}',";
                    sqlExpression += $" role_id={newUser.User.RoleId}";
                    sqlExpression += $" WHERE id={oldUser.User.Id};";
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.ExecuteNonQuery();
                }

                isDataOK = true;
                if (newUser.FIO.Length < 5 || newUser.Age < 5 || newUser.Gender == "")
                    isDataOK = false;

                if (isDataOK)
                {
                    sqlExpression = $"UPDATE [Employers] SET";
                    sqlExpression += $" FIO='{newUser.FIO}',";
                    sqlExpression += $" age={newUser.Age},";
                    sqlExpression += $" gender='{newUser.Gender}',";
                    sqlExpression += $" email='{newUser.Email}'";
                    sqlExpression += $" WHERE id={oldUser.Id}";
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        // User
        public static void AddUser(User user, string role)
        {
            Role currRole = FindRoleByName(role);

            string sqlExpression = $"INSERT INTO [Users] VALUES ('{user.Username}', '{user.Password}', {currRole.Id})";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        public static List<User> SelectAllUsers()
        {
            List<User> users = new List<User>();
            string sqlExpression = "SELECT * FROM Users";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        users.Add(new User(int.Parse(reader["id"].ToString()), reader["username"].ToString(),
                            reader["password"].ToString(), int.Parse(reader["role_id"].ToString()), 
                            FindRoleById(int.Parse(reader["role_id"].ToString()))));
                    }
                }

                reader.Close();
            }
            return users;
        }

        public static User FindUserByUsername(string username)
        {
            User user = new User();
            string sqlExpression = $"SELECT * FROM Users WHERE [username] = '{username}'";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        user = new User(int.Parse(reader["id"].ToString()), reader["username"].ToString(),
                            reader["password"].ToString(), int.Parse(reader["role_id"].ToString()),
                            FindRoleById(int.Parse(reader["role_id"].ToString())));
                    }
                }

                reader.Close();
            }
            return user;
        }

        public static User FindUserById(int id)
        {
            User user = new User();
            string sqlExpression = $"SELECT * FROM Users WHERE [id] = {id}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Role tempRole = FindRoleById(int.Parse(reader["role_id"].ToString()));

                        user = new User(int.Parse(reader["id"].ToString()), reader["username"].ToString(),
                            reader["password"].ToString(), tempRole.Id, tempRole);
                    }
                }

                reader.Close();
            }
            return user;
        }

        // Role
        public static List<Role> SelectAllRoles()
        {
            List<Role> roles = new List<Role>();
            string sqlExpression = "SELECT * FROM Roles";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        roles.Add(new Role(int.Parse(reader["id"].ToString()), reader["name"].ToString(), int.Parse(reader["priority"].ToString())));
                    }
                }

                reader.Close();
            }
            return roles;
        }


        public static Role FindRoleByName(string name)
        {
            Role role = new Role();
            string sqlExpression = $"SELECT * FROM Roles WHERE [name] = '{name}'";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        role = new Role(int.Parse(reader["id"].ToString()), 
                            reader["name"].ToString(), int.Parse(reader["priority"].ToString()));
                    }
                }

                reader.Close();
            }
            return role;
        }

        public static Role FindRoleById(int id)
        {
            Role role = new Role();
            string sqlExpression = $"SELECT * FROM Roles WHERE [id] = {id}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        role = new Role(int.Parse(reader["id"].ToString()),
                            reader["name"].ToString(), int.Parse(reader["priority"].ToString()));
                    }
                }

                reader.Close();
            }
            return role;
        }

        // Task

        public static List<Tasks> SelectAllTasks()
        {
            List<Tasks> tasks = new List<Tasks>();
            string sqlExpression = "SELECT * FROM Tasks";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tasks.Add(new Tasks(int.Parse(reader["id"].ToString()), reader["title"].ToString(),
                            reader["description"].ToString(), DateTime.Parse(reader["date_issue"].ToString()),
                            DateTime.Parse(reader["date_delivery"].ToString()), reader["answer"].ToString(),
                            reader["comment"].ToString(), reader["status"].ToString(), 
                            int.Parse(reader["issuer_id"].ToString()), int.Parse(reader["worker_id"].ToString())));
                    }
                }

                reader.Close();
            }
            return tasks;
        }

        public static Tasks FindTaskById(int id)
        {
            Tasks task = new Tasks();
            string sqlExpression = $"SELECT * FROM Tasks WHERE [id] = {id}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        task = new Tasks(int.Parse(reader["id"].ToString()), reader["title"].ToString(),
                            reader["description"].ToString(), DateTime.Parse(reader["date_issue"].ToString()),
                            DateTime.Parse(reader["date_delivery"].ToString()), reader["answer"].ToString(),
                            reader["comment"].ToString(), reader["status"].ToString(),
                            int.Parse(reader["issuer_id"].ToString()), int.Parse(reader["worker_id"].ToString()));
                    }
                }

                reader.Close();
            }
            return task;
        }

        public static List<Tasks> FindTasksForEmployer(int id)
        {
            List<Tasks> tasks = new List<Tasks>();
            string sqlExpression = $"SELECT * FROM Tasks WHERE [worker_id] = {id}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tasks.Add(new Tasks(int.Parse(reader["id"].ToString()), reader["title"].ToString(),
                            reader["description"].ToString(), DateTime.Parse(reader["date_issue"].ToString()),
                            DateTime.Parse(reader["date_delivery"].ToString()), reader["answer"].ToString(),
                            reader["comment"].ToString(), reader["status"].ToString(),
                            int.Parse(reader["issuer_id"].ToString()), int.Parse(reader["worker_id"].ToString())));
                    }
                }

                reader.Close();
            }
            return tasks;
        }

        public static void PushTaskOnCheck(Tasks task)
        {
            try
            {
                string sqlExpression = $"UPDATE [Tasks] SET";
                sqlExpression += $" [status]='ON CHECKING',";
                sqlExpression += $" [answer]='{task.Answer}',";
                sqlExpression += $" [comment]='{task.Comment}'";
                sqlExpression += $" WHERE id={task.Id};";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public static void CheckingTaskResult(int id, string desc, string comment, string status)
        {
            try
            {
                string sqlExpression = $"UPDATE [Tasks] SET";
                sqlExpression += $" [status]='{status}',";
                sqlExpression += $" [description]='{desc}',";
                sqlExpression += $" [comment]='{comment}'";
                sqlExpression += $" WHERE id={id};";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public static void AddTask(Tasks task)
        {
            string sqlExpression = $"INSERT INTO [Tasks] VALUES ('{task.Title}', '{task.Description}', '{task.DateIssue.ToShortDateString()}'," +
                $"'{task.DateDelivery.ToShortDateString()}', '{task.Answer}', '{task.Comment}', '{task.Status}', {task.IssuerId}, {task.WorkerId})";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveTask(int id)
        {
            string sqlExpression = $"DELETE FROM Tasks WHERE id = {id}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "abc123";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "abc123";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
