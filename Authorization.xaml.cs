using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Diplom.Entities;
using System.Security.Cryptography;
using System.Threading;

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        private string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}/auth.txt";

        public Authorization()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void bLogin_Click(object sender, RoutedEventArgs e)
        {
            lError.Content = "";
            if (!CheckTextBoxes())
                lError.Content += "Ошибка: неверно введен логин\r\n или пароль";
            else
            {
                if(DBHelper.CheckAuthorization(tbLogin.Text, Encrypt(tbPassword.Password)))
                {
                    using (StreamWriter streamWriter = new StreamWriter(path))
                    {
                        if (chbRemember.IsChecked == true)
                            streamWriter.WriteLine(1);
                        else
                            streamWriter.WriteLine(0);
                        streamWriter.WriteLine(Encrypt(tbLogin.Text));
                        streamWriter.WriteLine(Encrypt(tbPassword.Password));

                        streamWriter.Close();
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();

                        this.Owner.Close();
                    }
                    Close();
                }
                else
                {
                    lError.Content += "Ошибка: неверно введен логин\r\n или пароль";
                }
               
            }
        }

        private bool CheckTextBoxes()
        {
            if (tbLogin.Text.Length <= 4 || tbPassword.Password.Length <= 4)
                return false;
            else
                return true;
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
