using Diplom.Entities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Employer currentAuthorizatedEmployer = new Employer();
        private List<Employer> employers = new List<Employer>();
        private List<Role> roles = new List<Role>();
        private List<string> genders = new List<string>();
        private ObservableCollection<TableData> tableData = new ObservableCollection<TableData>();
        private Authorization authorizationWindow;
        private Employer selectedEditingEmployer;
        private Employer currentEmployerDetail;
        private Tasks currentTaskDetail;

        private const string CODE_AUTH = "auth";
        private const string CODE_LOG_OUT = "logOut";
        private const string CODE_EMPLOYERS = "emp";
        private const string CODE_ADMIN_PANEL = "adminPanel";
        private const string CODE_EMPLOYER_PANEL = "empPanel";
        private const string CODE_SHOW_EMPLOYERS = "showEmp";
        private const string CODE_ADD_EMPLOYER = "addEmp";
        private const string CODE_EDIT_EMPLOYER = "editEmp";
        private const string CODE_SHOW_TASKS = "showTasks";
        private const string CODE_DETAIL_TASK = "detailTask";
        private const string CODE_DETAIL_TASK_CHECKING = "detailTaskChecking";
        private const string CODE_PROFILE_DETAILS = "profileDetails";
        private const string CODE_CREATE_TASK = "createTask";
        private const string CODE_DELETE_TASK = "deleteTask";

        private string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}/auth.txt";

        UdpClient client;
        const int PORT = 12000;
        const int TTL = 10;
        const string IP = "235.5.5.1";
        IPAddress groupAddress = IPAddress.Parse(IP);

        FtpClient ftpClient = new FtpClient();

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            RefreshEmployersList();
            roles = DBHelper.SelectAllRoles();
            bSend.IsEnabled = false;

            genders.Add("Мужчина");
            genders.Add("Женщина");

            if (File.Exists(path))
            {
                try
                {
                    using (StreamReader streamReader = new StreamReader(path))
                    {
                        bool isRemembering = streamReader.ReadLine() == "1";
                        string login = Decrypt(streamReader.ReadLine());
                        string password = streamReader.ReadLine();

                        if (DBHelper.CheckAuthorization(login, password))
                        {
                            var a = DBHelper.FindEmployerByUsername(login);
                            AuthorizeUser(a);
                        }

                        streamReader.Close();

                        if (!isRemembering)
                            File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    File.Delete(path);
                }
            }

            if(currentAuthorizatedEmployer.Id != 0)
                RefreshAndFillDataGrid();

            InitiateFTPClient();

            cbStatusTaskChecking.Items.Add("Принято");
            cbStatusTaskChecking.Items.Add("Переделать");
            cbStatusTaskChecking.Items.Add("Отклонено");
        }

        private void InitiateFTPClient()
        {
            ftpClient.Host = @"ftp://127.0.0.1/";
            ftpClient.UserName = "";
            ftpClient.Password = "";
        }

        private void RefreshEmployersList()
        {
            employers = DBHelper.SelectAllEmployers();
        }

        private void RefreshAndFillDataGrid()
        {
            tableData.Clear();
            RefreshEmployersList();
            foreach (var emp in employers)
            {
                if(currentAuthorizatedEmployer.User.Role.Name == "Sysadmin" && currentAuthorizatedEmployer.Id != emp.Id)
                    tableData.Add(new TableData(emp.Id, emp.FIO, emp.Age, emp.Gender, emp.Email, emp.User.Role.Name, emp.User.Username));
                else if (currentAuthorizatedEmployer.Id != emp.Id)
                    tableData.Add(new TableData(emp.Id, emp.FIO, emp.Age, emp.Gender, emp.Email, emp.User.Role.Name, "HIDDEN"));
            }
            gEmployers.ItemsSource = tableData;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void bAuthorized_Click(object sender, RoutedEventArgs e)
        {
            if (authorizationWindow != null)
                authorizationWindow.Close();

            authorizationWindow = new Authorization();
            authorizationWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            authorizationWindow.Owner = this;
            authorizationWindow.Show();
        }

        private void ShowEmployerButtons()
        {
            bShowEmp.Visibility = Visibility.Visible;
            bAddEmp.Visibility = Visibility.Visible;
            bEditEmp.Visibility = Visibility.Visible;

            tbChat.Visibility = Visibility.Visible;
            tbSend.Visibility = Visibility.Visible;
            bSend.Visibility = Visibility.Visible;
        }

        private void ShowerHider(string code)
        {
            HideAll();
            switch (code)
            {
                case CODE_AUTH:
                    lGreeting.Content = $"Приветствуем, {currentAuthorizatedEmployer.FIO}!";

                    bLogOut.Visibility = Visibility.Visible;
                    bAuthorized.Visibility = Visibility.Hidden;
                    lGreeting.Visibility = Visibility.Visible;
                    bMyProfile.Visibility = Visibility.Visible;

                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    break;
                case CODE_LOG_OUT:
                    lGreeting.Content = "";
                    bEmployers.Visibility = Visibility.Hidden;
                    imageEmployers.Visibility = Visibility.Hidden;

                    bTasks.Visibility = Visibility.Hidden;
                    imageTasks.Visibility = Visibility.Hidden;

                    lNeedAuthInf.Visibility = Visibility.Visible;
                    tbNeedAuhInf.Visibility = Visibility.Visible;

                    bLogOut.Visibility = Visibility.Hidden;
                    bAuthorized.Visibility = Visibility.Visible;
                    lGreeting.Visibility = Visibility.Hidden;

                    bMyProfile.Visibility = Visibility.Hidden;
                    break;
                case CODE_EMPLOYERS:
                    ShowEmployerButtons();
                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    break;
                case CODE_ADMIN_PANEL:
                    bEmployers.Visibility = Visibility.Visible;
                    imageEmployers.Visibility = Visibility.Visible;

                    bTasks.Visibility = Visibility.Visible;
                    imageTasks.Visibility = Visibility.Visible;

                    lNeedAuthInf.Visibility = Visibility.Hidden;
                    tbNeedAuhInf.Visibility = Visibility.Hidden;

                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    break;
                case CODE_EMPLOYER_PANEL:
                    bTasks.Visibility = Visibility.Visible;
                    imageTasks.Visibility = Visibility.Visible;

                    lNeedAuthInf.Visibility = Visibility.Hidden;
                    tbNeedAuhInf.Visibility = Visibility.Hidden;
                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    break;
                case CODE_SHOW_EMPLOYERS:
                    ShowEmployerButtons();
                    RefreshAndFillDataGrid();
                    gEmployers.Visibility = Visibility.Visible;
                    bShowEmpDetails.Visibility = Visibility.Visible;
                    bShowEmpDelete.Visibility = Visibility.Visible;
                    break;
                case CODE_ADD_EMPLOYER:
                    ShowEmployerButtons();

                    cbAddEmpRole.Items.Clear();
                    foreach(Role role in roles)
                    {
                        if(currentAuthorizatedEmployer.User.Role.Priority >= role.Priority)
                            cbAddEmpRole.Items.Add(role.Name);
                    }

                    cbAddEmpGender.Items.Clear();
                    foreach(var gen in genders)
                    {
                        cbAddEmpGender.Items.Add(gen);
                    }

                    lAddEmpFIO.Visibility = Visibility.Visible;
                    tbAddEmpFIO.Visibility = Visibility.Visible;
                    lAddEmpAge.Visibility = Visibility.Visible;
                    tbAddEmpAge.Visibility = Visibility.Visible;
                    lAddEmpEmail.Visibility = Visibility.Visible;
                    tbAddEmpEmail.Visibility = Visibility.Visible;
                    lAddEmpUsername.Visibility = Visibility.Visible;
                    tbAddEmpUsername.Visibility = Visibility.Visible;
                    lAddEmpPassword.Visibility = Visibility.Visible;
                    tbAddEmpPassword.Visibility = Visibility.Visible;
                    bAddEmpForm.Visibility = Visibility.Visible;
                    lAddEmpRole.Visibility = Visibility.Visible;
                    cbAddEmpRole.Visibility = Visibility.Visible;
                    lAddEmpGender.Visibility = Visibility.Visible;
                    cbAddEmpGender.Visibility = Visibility.Visible;
                    break;
                case CODE_EDIT_EMPLOYER:
                    ShowEmployerButtons();
                    ClearEditEmployerFields();
                    ClearEditEmployerFields();
                    RefreshComboBoxEditEmployer();
                    cbEditEmpRole.Items.Clear();
                    foreach (Role role in roles)
                    {
                        if (currentAuthorizatedEmployer.User.Role.Priority >= role.Priority)
                            cbEditEmpRole.Items.Add(role.Name);
                    }

                    cbEditEmpGender.Items.Clear();
                    foreach (var gen in genders)
                    {
                        cbEditEmpGender.Items.Add(gen);
                    }

                    tbEditEmpAge.Visibility = Visibility.Visible;
                    tbEditEmpEmail.Visibility = Visibility.Visible;
                    tbEditEmpFIO.Visibility = Visibility.Visible;
                    tbEditEmpPassword.Visibility = Visibility.Visible;
                    tbEditEmpUsername.Visibility = Visibility.Visible;
                    cbEditEmp.Visibility = Visibility.Visible;
                    cbEditEmpGender.Visibility = Visibility.Visible;
                    cbEditEmpRole.Visibility = Visibility.Visible;
                    lEditEmpAge.Visibility = Visibility.Visible;
                    lEditEmpEmail.Visibility = Visibility.Visible;
                    lEditEmpFIO.Visibility = Visibility.Visible;
                    lEditEmpGender.Visibility = Visibility.Visible;
                    lEditEmpPassword.Visibility = Visibility.Visible;
                    lEditEmpRole.Visibility = Visibility.Visible;
                    lEditEmpUsername.Visibility = Visibility.Visible;
                    bEditEmpForm.Visibility = Visibility.Visible;
                    lEditEmpSelect.Visibility = Visibility.Visible;
                    break;
                case CODE_SHOW_TASKS:
                    dpTasks.Visibility = Visibility.Visible;
                    if(currentAuthorizatedEmployer.User.Role.Priority >= 2)
                    {
                        bTasksShowIssued.Visibility = Visibility.Visible;
                        bTasksShowCheck.Visibility = Visibility.Visible;
                        bTasksCreate.Visibility = Visibility.Visible;
                        bTasksDelete.Visibility = Visibility.Visible;
                    }

                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    ShowTasksList();
                    break;
                case CODE_DETAIL_TASK:
                    dpTasksDetails.Visibility = Visibility.Visible;
                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    break;
                case CODE_DETAIL_TASK_CHECKING:
                    dpTasksDetailsChecking.Visibility = Visibility.Visible;
                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    break;
                case CODE_PROFILE_DETAILS:
                    if(currentEmployerDetail != null)
                    {
                        dpProfileDetails.Visibility = Visibility.Visible;
                        PushInfoIntoProfileDetails();
                    }
                    else
                    {
                        ShowerHider(CODE_EMPLOYERS);
                        ShowerHider(CODE_SHOW_EMPLOYERS);
                    }
                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    break;
                case CODE_CREATE_TASK:
                    dpCreateTask.Visibility = Visibility.Visible;
                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    break;
                case CODE_DELETE_TASK:
                    dpDeleteTask.Visibility = Visibility.Visible;
                    tbChat.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    bSend.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void HideAll()
        {
            bShowEmp.Visibility = Visibility.Hidden;
            bAddEmp.Visibility = Visibility.Hidden;
            bEditEmp.Visibility = Visibility.Hidden;
            // Добавление работника форма
            lAddEmpFIO.Visibility = Visibility.Hidden;
            tbAddEmpFIO.Visibility = Visibility.Hidden;
            lAddEmpAge.Visibility = Visibility.Hidden;
            tbAddEmpAge.Visibility = Visibility.Hidden;
            lAddEmpEmail.Visibility = Visibility.Hidden;
            tbAddEmpEmail.Visibility = Visibility.Hidden;
            lAddEmpUsername.Visibility = Visibility.Hidden;
            tbAddEmpUsername.Visibility = Visibility.Hidden;
            lAddEmpPassword.Visibility = Visibility.Hidden;
            tbAddEmpPassword.Visibility = Visibility.Hidden;
            lAddEmpError.Visibility = Visibility.Hidden;
            bAddEmpForm.Visibility = Visibility.Hidden;
            bShowEmpDetails.Visibility = Visibility.Hidden;
            bShowEmpDelete.Visibility = Visibility.Hidden;
            gEmployers.Visibility = Visibility.Hidden;
            lAddEmpRole.Visibility = Visibility.Hidden;
            cbAddEmpRole.Visibility = Visibility.Hidden;
            lAddEmpGender.Visibility = Visibility.Hidden;
            cbAddEmpGender.Visibility = Visibility.Hidden;
            // Редактирование работника форма
            tbEditEmpAge.Visibility = Visibility.Hidden;
            tbEditEmpEmail.Visibility = Visibility.Hidden;
            tbEditEmpFIO.Visibility = Visibility.Hidden;
            tbEditEmpPassword.Visibility = Visibility.Hidden;
            tbEditEmpUsername.Visibility = Visibility.Hidden;
            cbEditEmp.Visibility = Visibility.Hidden;
            cbEditEmpGender.Visibility = Visibility.Hidden;
            cbEditEmpRole.Visibility = Visibility.Hidden;
            lEditEmpAge.Visibility = Visibility.Hidden;
            lEditEmpEmail.Visibility = Visibility.Hidden;
            lEditEmpError.Visibility = Visibility.Hidden;
            lEditEmpFIO.Visibility = Visibility.Hidden;
            lEditEmpGender.Visibility = Visibility.Hidden;
            lEditEmpPassword.Visibility = Visibility.Hidden;
            lEditEmpRole.Visibility = Visibility.Hidden;
            lEditEmpUsername.Visibility = Visibility.Hidden;
            bEditEmpForm.Visibility = Visibility.Hidden;
            lEditEmpSelect.Visibility = Visibility.Hidden;
            // Отображение списка заданий
            dpTasks.Visibility = Visibility.Hidden;
            // Детальная информация о задании
            dpTasksDetails.Visibility = Visibility.Hidden;
            // Детальная информация о профиле
            dpProfileDetails.Visibility = Visibility.Hidden;
            // Кнопки "Выдано" и "На проверке"
            bTasksShowIssued.Visibility = Visibility.Hidden;
            bTasksShowCheck.Visibility = Visibility.Hidden;
            // Детальная информация о задании на проверку
            dpTasksDetailsChecking.Visibility = Visibility.Hidden;
            // Кнопки "Добавить" и "Удалить" (задания)
            bTasksCreate.Visibility = Visibility.Hidden;
            bTasksDelete.Visibility = Visibility.Hidden;
            // Форма добавления задания
            dpCreateTask.Visibility = Visibility.Hidden;
            // Форма удаления задания
            dpDeleteTask.Visibility = Visibility.Hidden;
            // Чат
            tbChat.Visibility = Visibility.Hidden;
            tbSend.Visibility = Visibility.Hidden;
            bSend.Visibility = Visibility.Hidden;
        }

        private void RefreshComboBoxEditEmployer()
        {
            RefreshEmployersList();
            cbEditEmp.Items.Clear();
            foreach (Employer emp in employers)
            {
                if (emp.Id != currentAuthorizatedEmployer.Id && emp.User.Role.Priority 
                    < currentAuthorizatedEmployer.User.Role.Priority)
                    cbEditEmp.Items.Add($"[{emp.Id}] {emp.FIO}, {emp.Age} лет");
            }
        }

        private void bEmployers_Click(object sender, RoutedEventArgs e)
        {
            ShowerHider(CODE_EMPLOYERS);
            ShowerHider(CODE_SHOW_EMPLOYERS);
        }

        private void bAddEmp_Click(object sender, RoutedEventArgs e)
        {
            ShowerHider(CODE_ADD_EMPLOYER);
        }

        private void bRemEmp_Click(object sender, RoutedEventArgs e)
        {
            ShowerHider("remEmp");
        }

        private void bEditEmp_Click(object sender, RoutedEventArgs e)
        {
            ShowerHider(CODE_EDIT_EMPLOYER);
        }

        private void bAddEmpForm_Click(object sender, RoutedEventArgs e)
        {
            bool isHasError = false;
            lAddEmpError.Content = "Ошибка: ";
            if(tbAddEmpUsername.Text.Length <= 4 || tbAddEmpPassword.Password.Length <= 4)
                lAddEmpError.Content += "[Длина логина и пароля должна быть более 4 символов]";

            if (lAddEmpError.Content.ToString().Length > 10)
                lAddEmpError.Content += "\r\n";

            if(tbAddEmpAge.Text.Length < 1 || tbAddEmpFIO.Text.Length < 1 || cbAddEmpGender.SelectedItem == null)
                lAddEmpError.Content += "[Обязательные поля не были заполнены]";

            if (lAddEmpError.Content.ToString().Length > 10)
                isHasError = true;

            if (isHasError)
                lAddEmpError.Visibility = Visibility.Visible;
            else
            {
                DBHelper.AddUser(new User(tbAddEmpUsername.Text, Encrypt(tbAddEmpPassword.Password)), cbAddEmpRole.SelectedItem.ToString());
                User newUser = DBHelper.FindUserByUsername(tbAddEmpUsername.Text);

                Employer newEmployer = new Employer(tbAddEmpFIO.Text, int.Parse(tbAddEmpAge.Text), cbAddEmpGender.SelectedItem.ToString(),
                    tbAddEmpEmail.Text, newUser.Id, newUser);

                DBHelper.AddEmployer(newEmployer);

                cbAddEmpGender.SelectedItem = null;
                cbAddEmpRole.SelectedItem = null;
                lAddEmpError.Content = "";
                tbAddEmpFIO.Text = "";
                tbAddEmpAge.Text = "";
                tbAddEmpEmail.Text = "";
                tbAddEmpUsername.Text = "";
                tbAddEmpPassword.Password = "";

                MessageBox.Show("Новый пользователь был успешно добавлен!");
            }
        }

        private void bShowEmp_Click(object sender, RoutedEventArgs e)
        {
            ShowerHider(CODE_SHOW_EMPLOYERS);
        }

        private void gEmployers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(gEmployers.SelectedItem != null)
            {
                TableData td = (TableData)gEmployers.SelectedItem;
                currentEmployerDetail = DBHelper.FindEmployerById(td.Id);
            }
        }
       
        public void AuthorizeUser(Employer employer)
        {
            currentAuthorizatedEmployer = employer;
            ShowerHider(CODE_AUTH);
            if (currentAuthorizatedEmployer.User.Role.Priority > 1)
                ShowerHider(CODE_ADMIN_PANEL);
            else
                ShowerHider(CODE_EMPLOYER_PANEL);

            if (currentAuthorizatedEmployer.User.Username != "")
            {
                bLogOut.IsEnabled = true;
                client = new UdpClient(PORT);
                client.JoinMulticastGroup(groupAddress, TTL);
                Task receiveTask = new Task(ReceiveMessages);
                receiveTask.Start();
                string message = currentAuthorizatedEmployer.User.Username + " logged in.";
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Send(data, data.Length, IP, PORT);
                bSend.IsEnabled = true;
            }
        }

        public void LogOutUser()
        {
            currentAuthorizatedEmployer = null;
            client.Close();
            ShowerHider(CODE_LOG_OUT);
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

        private void bLogOut_Click(object sender, RoutedEventArgs e)
        {
            LogOutUser();
        }

        private void cbEditEmp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbEditEmp.SelectedValue != null)
            {
                string strId = cbEditEmp.SelectedValue.ToString();
                strId = strId.Split(']')[0].Split('[')[1];

                selectedEditingEmployer = DBHelper.FindEmployerById(int.Parse(strId));
                tbEditEmpFIO.Text = selectedEditingEmployer.FIO;
                tbEditEmpAge.Text = selectedEditingEmployer.Age.ToString();
                tbEditEmpEmail.Text = selectedEditingEmployer.Email;
                tbEditEmpUsername.Text = selectedEditingEmployer.User.Username;
                tbEditEmpPassword.Password = Decrypt(selectedEditingEmployer.User.Password);
                cbEditEmpRole.Text = selectedEditingEmployer.User.Role.Name;
                cbEditEmpGender.Text = selectedEditingEmployer.Gender;
            }
        }

        private void bEditEmpForm_Click(object sender, RoutedEventArgs e)
        {
            if (tbEditEmpUsername.Text.Length > 5 && tbEditEmpPassword.Password.Length > 5 &&
                tbEditEmpAge.Text.Length > 1 && tbEditEmpFIO.Text.Length > 1 && cbEditEmpGender.Text.Length > 1)
            {
                var findRole = DBHelper.FindRoleByName(cbEditEmpRole.Text);
                var newUser = new User(tbEditEmpUsername.Text, Encrypt(tbEditEmpPassword.Password), findRole.Id);

                DBHelper.EditEmployer(selectedEditingEmployer, new Employer(selectedEditingEmployer.Id,
                    tbEditEmpFIO.Text, int.Parse(tbEditEmpAge.Text), cbEditEmpGender.Text, tbEditEmpEmail.Text, newUser.Id, newUser));

                ClearEditEmployerFields();
                ClearEditEmployerFields();
            }
            else
            {
                lEditEmpError.Visibility = Visibility.Visible;
                lEditEmpError.Content = "Ошибка: Обязательные поля были заполнены неверно.";
            }
            
        }

        private void ClearEditEmployerFields()
        {
            tbEditEmpAge.Text = "";
            tbEditEmpEmail.Text = "";
            tbEditEmpFIO.Text = "";
            tbEditEmpPassword.Password = "";
            tbEditEmpUsername.Text = "";
            cbEditEmp.SelectedValue = null;
            cbEditEmpGender.SelectedValue = null;
            cbEditEmpRole.SelectedValue = null;
        }

        private void TaskDockPanelItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var currentTaskTextBox = (TextBox)sender;
            currentTaskDetail = DBHelper.FindTaskById(int.Parse(currentTaskTextBox.Name.Replace("task", "")));

            ShowerHider(CODE_DETAIL_TASK);
            tbDetailTaskTitle.Text = currentTaskDetail.Title;
            tbDetailTaskDescription.Text = currentTaskDetail.Description;
            tbDetailTaskDate.Text = currentTaskDetail.DateDelivery.ToString().Substring(0, 10);
            tbDetailTaskIssuer.Text = $"Выдал {currentTaskDetail.Issuer.FIO}";
            tbDetailTaskWorker.Text = $"Кому: {currentTaskDetail.Worker.FIO}";
            tbCommentTask.Text = currentTaskDetail.Comment;
        }

        private void TaskDockPanelItemChecking_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var currentTaskTextBox = (TextBox)sender;
            currentTaskDetail = DBHelper.FindTaskById(int.Parse(currentTaskTextBox.Name.Replace("task", "")));

            ShowerHider(CODE_DETAIL_TASK_CHECKING);
            tbDetailTaskTitleChecking.Text = currentTaskDetail.Title;
            tbDetailTaskDescriptionChecking.Text = currentTaskDetail.Description;
            tbDetailTaskDateChecking.Text = currentTaskDetail.DateDelivery.ToString().Substring(0, 10);
            tbCommentTaskChecking.Text = currentTaskDetail.Comment;
            tbDetailTaskIssuerChecking.Text = $"Выдал {currentTaskDetail.Issuer.FIO}";
            tbDetailTaskWorkerChecking.Text = $"Кому: {currentTaskDetail.Worker.FIO}";
        }

        private void bTasks_Click(object sender, RoutedEventArgs e)
        {
            ShowerHider(CODE_SHOW_TASKS);
        }

        private void bChooseFileTask_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt, *.docx)|*.txt;*.docx";
            openFileDialog.ShowDialog();
            string filename = openFileDialog.FileName.Split('\\')[openFileDialog.FileName.Split('\\').Length - 1];
            string filenameForUpload = openFileDialog.FileName;
            ftpClient.UploadFile(filenameForUpload);
            currentTaskDetail.Answer = ftpClient.Host + filename;
            currentTaskDetail.Comment = tbCommentTask.Text;
            MessageBox.Show("Файл был успешно загружен на FTP сервер.");
        }

        private void bFinishTask_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.PushTaskOnCheck(currentTaskDetail);
            tbCommentTask.Text = "Комментарий";
            MessageBox.Show("Задание было успешно отправлено на проверку.");
            ShowerHider(CODE_SHOW_TASKS);
        }

        private void ShowTasksList()
        {
            List<Tasks> tempTasks;
            if (currentAuthorizatedEmployer.User.Role.Priority <= 2)
                tempTasks = DBHelper.FindTasksForEmployer(currentAuthorizatedEmployer.Id);
            else
                tempTasks = DBHelper.SelectAllTasks();
            spTasks.Children.Clear();

            List<Tasks> tasks = new List<Tasks>();

            foreach(Tasks task in tempTasks)
            {
                if (task.Status == "IN ORDER" || task.Status == "TO REVIEW")
                {
                    tasks.Add(task);
                }
            }

            if(tasks.Count == 0)
            {
                DockPanel dockPanel = new DockPanel();
                SolidColorBrush solid = new SolidColorBrush(Color.FromRgb(9, 25, 142));
                dockPanel.Background = solid;
                dockPanel.Height = 65;
                dockPanel.Margin = new Thickness(0, 10, 0, 0);

                TextBox textBox1 = new TextBox();
                textBox1.Width = 700;
                textBox1.Height = 60;
                textBox1.Foreground = new SolidColorBrush(Colors.White);
                textBox1.Background = new SolidColorBrush(Colors.Transparent);
                textBox1.HorizontalAlignment = HorizontalAlignment.Left;
                textBox1.FontSize = 24;
                textBox1.VerticalContentAlignment = VerticalAlignment.Center;
                textBox1.BorderThickness = new Thickness(0);
                textBox1.Margin = new Thickness(10, 0, 0, 0);
                textBox1.FontWeight = FontWeights.DemiBold;
                textBox1.IsReadOnly = true;
                textBox1.Text = "На текущий момент заданий нет :)";

                dockPanel.Children.Add(textBox1);
                spTasks.Children.Add(dockPanel);
            }
            else
            {
                tasks.Sort((a, b) => a.DateDelivery.CompareTo(b.DateDelivery));

                foreach (Tasks task in tasks)
                {
                    DockPanel dockPanel = new DockPanel();
                    dockPanel.Background = new LinearGradientBrush(Color.FromRgb(9, 25, 142), Colors.Navy, 74);
                    //dockPanel.Background = new SolidColorBrush(Color.FromRgb(9, 25, 142));
                    dockPanel.Height = 65;
                    dockPanel.Margin = new Thickness(0, 10, 0, 0);

                    TextBox textBox1 = new TextBox();
                    textBox1.Name = "task" + task.Id;
                    textBox1.Width = 700;
                    textBox1.Height = 60;
                    textBox1.Foreground = new SolidColorBrush(Colors.White);
                    textBox1.Background = new SolidColorBrush(Colors.Transparent);
                    textBox1.HorizontalAlignment = HorizontalAlignment.Left;
                    textBox1.FontSize = 24;
                    textBox1.VerticalContentAlignment = VerticalAlignment.Center;
                    textBox1.BorderThickness = new Thickness(0);
                    textBox1.Margin = new Thickness(10, 0, 0, 0);
                    textBox1.FontWeight = FontWeights.DemiBold;
                    textBox1.MouseDoubleClick += new MouseButtonEventHandler(TaskDockPanelItem_MouseDoubleClick);
                    textBox1.IsReadOnly = true;
                    textBox1.Text = task.Title;

                    TextBox textBox2 = new TextBox();
                    textBox2.Width = 110;
                    if (DateTime.Now.CompareTo(task.DateDelivery) < 0)
                        textBox2.Foreground = new SolidColorBrush(Colors.White);
                    else textBox2.Foreground = new SolidColorBrush(Colors.Red);
                    textBox2.Background = new SolidColorBrush(Colors.Transparent);
                    textBox2.HorizontalContentAlignment = HorizontalAlignment.Right;
                    textBox2.FontSize = 20;
                    textBox2.VerticalContentAlignment = VerticalAlignment.Center;
                    textBox2.BorderThickness = new Thickness(0);
                    textBox2.Margin = new Thickness(10, 0, 0, 0);
                    textBox2.FontWeight = FontWeights.Bold;
                    textBox2.IsReadOnly = true;
                    textBox2.Text = task.DateDelivery.ToString();

                    dockPanel.Children.Add(textBox1);
                    dockPanel.Children.Add(textBox2);
                    spTasks.Children.Add(dockPanel);
                }
            }
        }

        private void ShowTasksListForChecking()
        {
            List<Tasks> tempTasks;
            tempTasks = DBHelper.SelectAllTasks();
            spTasks.Children.Clear();

            List<Tasks> tasks = new List<Tasks>();

            foreach (Tasks task in tempTasks)
            {
                if (task.Status == "ON CHECKING" && DBHelper.FindEmployerById(task.IssuerId).User.Role.Priority <= currentAuthorizatedEmployer.User.Role.Priority)
                {
                    tasks.Add(task);
                }
            }

            if (tasks.Count == 0)
            {
                DockPanel dockPanel = new DockPanel();
                SolidColorBrush solid = new SolidColorBrush(Color.FromRgb(9, 25, 142));
                dockPanel.Background = solid;
                dockPanel.Height = 65;
                dockPanel.Margin = new Thickness(0, 10, 0, 0);

                TextBox textBox1 = new TextBox();
                textBox1.Width = 700;
                textBox1.Height = 60;
                textBox1.Foreground = new SolidColorBrush(Colors.White);
                textBox1.Background = new SolidColorBrush(Colors.Transparent);
                textBox1.HorizontalAlignment = HorizontalAlignment.Left;
                textBox1.FontSize = 24;
                textBox1.VerticalContentAlignment = VerticalAlignment.Center;
                textBox1.BorderThickness = new Thickness(0);
                textBox1.Margin = new Thickness(10, 0, 0, 0);
                textBox1.FontWeight = FontWeights.DemiBold;
                textBox1.IsReadOnly = true;
                textBox1.Text = "На текущий момент заданий для проверки нет :)";

                dockPanel.Children.Add(textBox1);
                spTasks.Children.Add(dockPanel);
            }
            else
            {
                tasks.Sort((a, b) => a.DateDelivery.CompareTo(b.DateDelivery));

                foreach (Tasks task in tasks)
                {
                    DockPanel dockPanel = new DockPanel();
                    dockPanel.Background = new LinearGradientBrush(Color.FromRgb(9, 25, 142), Colors.Navy, 74);
                    //dockPanel.Background = new SolidColorBrush(Color.FromRgb(9, 25, 142));
                    dockPanel.Height = 65;
                    dockPanel.Margin = new Thickness(0, 10, 0, 0);

                    TextBox textBox1 = new TextBox();
                    textBox1.Name = "task" + task.Id;
                    textBox1.Width = 700;
                    textBox1.Height = 60;
                    textBox1.Foreground = new SolidColorBrush(Colors.White);
                    textBox1.Background = new SolidColorBrush(Colors.Transparent);
                    textBox1.HorizontalAlignment = HorizontalAlignment.Left;
                    textBox1.FontSize = 24;
                    textBox1.VerticalContentAlignment = VerticalAlignment.Center;
                    textBox1.BorderThickness = new Thickness(0);
                    textBox1.Margin = new Thickness(10, 0, 0, 0);
                    textBox1.FontWeight = FontWeights.DemiBold;
                    textBox1.MouseDoubleClick += new MouseButtonEventHandler(TaskDockPanelItemChecking_MouseDoubleClick);
                    textBox1.IsReadOnly = true;
                    textBox1.Text = task.Title;

                    TextBox textBox2 = new TextBox();
                    textBox2.Width = 110;
                    if (DateTime.Now.CompareTo(task.DateDelivery) < 0)
                        textBox2.Foreground = new SolidColorBrush(Colors.White);
                    else textBox2.Foreground = new SolidColorBrush(Colors.Red);
                    textBox2.Background = new SolidColorBrush(Colors.Transparent);
                    textBox2.HorizontalContentAlignment = HorizontalAlignment.Right;
                    textBox2.FontSize = 20;
                    textBox2.VerticalContentAlignment = VerticalAlignment.Center;
                    textBox2.BorderThickness = new Thickness(0);
                    textBox2.Margin = new Thickness(10, 0, 0, 0);
                    textBox2.FontWeight = FontWeights.Bold;
                    textBox2.IsReadOnly = true;
                    textBox2.Text = task.DateDelivery.ToString();

                    dockPanel.Children.Add(textBox1);
                    dockPanel.Children.Add(textBox2);
                    spTasks.Children.Add(dockPanel);
                }
            }
        }

        private void PushInfoIntoProfileDetails()
        {
            if (currentEmployerDetail.Gender == "Женщина")
            {
                var bi = new BitmapImage(new Uri(@"/Images/female_profile.png", UriKind.Relative));
                imageProfileDetails.Source = bi;
            }
            else if (currentEmployerDetail.Gender == "Мужчина")
            {
                var bi = new BitmapImage(new Uri(@"/Images/male_profile.png", UriKind.Relative));
                imageProfileDetails.Source = bi;
            }

            tbProfileDetailFIO.Text = currentEmployerDetail.FIO;
            tbProfileDetailAge.Text = "Возраст: " + currentEmployerDetail.Age.ToString();
            tbProfileDetailEmail.Text = "Email: " + (currentEmployerDetail.Email == "" ? "не указан" : currentEmployerDetail.Email);
            tbProfileDetailRole.Text = "Роль: " + currentEmployerDetail.User.Role.Name;
            tbProfileDetailUsername.Text = "Никнейм: " + currentEmployerDetail.User.Username;

            List<Tasks> tasks = DBHelper.FindTasksForEmployer(currentEmployerDetail.Id);
            List<TasksTableData> ttd = new List<TasksTableData>();

            foreach (var task in tasks)
            {
                if (task.Status == "IN ORDER" | task.Status == "TO REVIEW")
                    ttd.Add(new TasksTableData(task.Id, task.Title, task.Description, task.DateIssue.ToShortDateString(),
                        task.DateDelivery.ToShortDateString(), task.Answer, task.Comment, task.Status, task.Issuer.FIO));
            }

            dgProfileDetailTasks.ItemsSource = ttd;
        }

        private void bShowEmpDetails_Click(object sender, RoutedEventArgs e)
        {
            ShowerHider(CODE_PROFILE_DETAILS);
        }

        private void bMyProfile_Click(object sender, RoutedEventArgs e)
        {
            currentEmployerDetail = currentAuthorizatedEmployer;
            ShowerHider(CODE_PROFILE_DETAILS);
        }

        private void bOpenFileTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = currentTaskDetail.Answer.Split('/')[currentTaskDetail.Answer.Split('/').Length - 1];
                ftpClient.DownloadFile(filename);
                Process openFile = new Process();
                if (filename.Split('.')[filename.Split('.').Length - 1] == "txt")
                {
                    openFile.StartInfo.FileName = "notepad.exe";
                    openFile.StartInfo.Arguments = Directory.GetCurrentDirectory() + @"\" + filename;
                }
                else if (filename.Split('.')[filename.Split('.').Length - 1] == "docx")
                    openFile.StartInfo.FileName = Directory.GetCurrentDirectory() + @"\" + filename;
                openFile.Start();
            }
            catch (Exception ex) { }
        }

        private void bFinishTaskChecking_Click(object sender, RoutedEventArgs e)
        {
            string desc = tbDetailTaskDescriptionChecking.Text;
            string comment = tbCommentTaskChecking.Text;
            string status = cbStatusTaskChecking.SelectedValue.ToString();

            switch (status)
            {
                case "Принято":
                    status = "ACCEPTED";
                    break;
                case "Переделать":
                    status = "TO REVIEW";
                    break;
                case "Отклонено":
                    status = "CANCELED";
                    break;
            }

            DBHelper.CheckingTaskResult(currentTaskDetail.Id, desc, comment, status);
            ShowerHider(CODE_SHOW_TASKS);
        }

        private void bTasksShowIssued_Click(object sender, RoutedEventArgs e)
        {
            ShowTasksList();
        }

        private void bTasksShowCheck_Click(object sender, RoutedEventArgs e)
        {
            ShowTasksListForChecking();
        }

        private void bTasksCreate_Click(object sender, RoutedEventArgs e)
        {
            ShowerHider(CODE_CREATE_TASK);

            cbCreateTaskWorker.Items.Clear();
            var tempList = DBHelper.SelectAllEmployers();
            var empList = new List<Employer>();
            foreach(var emp in tempList)
                if(emp.User.Role.Priority < currentAuthorizatedEmployer.User.Role.Priority
                    && emp.Id != currentAuthorizatedEmployer.Id)
                    empList.Add(emp);

            foreach(var emp in empList)
                cbCreateTaskWorker.Items.Add($"[{emp.Id}] {emp.FIO}, {emp.Age} лет");
        }

        private void bCreateTaskForm_Click(object sender, RoutedEventArgs e)
        {
            bool isDataClear = true;
            DateTime tempDate = new DateTime();
            if (tbAddTaskTitle.Text.Length < 5)
            {
                MessageBox.Show("Длина заголовка должна быть длиной 5 или более символов.");
                isDataClear = false;
            }
            if (tbAddTaskDesc.Text.Length <= 0)
            {
                MessageBox.Show("Описание задания не должно быть пустым.");
                isDataClear = false;
            }
            if (DateTime.TryParse(tbAddTaskDateDelivery.Text, out tempDate) == false)
            {
                MessageBox.Show("Дата задана в неверном формате (DD.MM.YYYY). Например: 16.02.2021");
                isDataClear = false;
            }
            if(cbCreateTaskWorker.SelectedValue == null)
            {
                MessageBox.Show("Исполнитель задания не был выбран.");
                isDataClear = false;
            }
            
            if(isDataClear)
            {
                Tasks newTask = new Tasks(tbAddTaskTitle.Text, tbAddTaskDesc.Text, DateTime.Now,
                DateTime.Parse(tbAddTaskDateDelivery.Text), "", "", "IN ORDER", currentAuthorizatedEmployer.Id,
                int.Parse(cbCreateTaskWorker.SelectedValue.ToString().Split(']')[0].Split('[')[1]));

                DBHelper.AddTask(newTask);
                tbAddTaskTitle.Text = "";
                tbAddTaskDesc.Text = "";
                tbAddTaskDateDelivery.Text = "";
                cbCreateTaskWorker.SelectedItem = null;
            }
        }

        private void bTasksDelete_Click(object sender, RoutedEventArgs e)
        {
            ShowerHider(CODE_DELETE_TASK);

            cbDeleteTask.Items.Clear();
            var list = DBHelper.SelectAllTasks();
            foreach(var task in list)
                cbDeleteTask.Items.Add($"[{task.Id}] {task.Title}");
        }

        private void bDeleteTaskForm_Click(object sender, RoutedEventArgs e)
        {
            var item = cbDeleteTask.SelectedItem;
            int id = int.Parse(item.ToString().Split(']')[0].Split('[')[1]);
            DBHelper.RemoveTask(id);
            cbDeleteTask.SelectedItem = null;
        }

        private void bShowEmpDelete_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.RemoveEmployer(currentEmployerDetail.Id);
            MessageBox.Show($"Пользователь под именем {currentEmployerDetail.User.Username} был успешно удален.");
            currentEmployerDetail = null;
            RefreshAndFillDataGrid();
        }

        private void bSend_Click(object sender, RoutedEventArgs e)
        {
            string message = currentAuthorizatedEmployer.User.Username + ": " + tbSend.Text;
            byte[] data = Encoding.Unicode.GetBytes(message);
            client.Send(data, data.Length, IP, PORT);
            tbSend.Text = "";
        }

        private void ReceiveMessages()
        {
            try
            {
                while (currentAuthorizatedEmployer.User.Username != "")
                {
                    IPEndPoint remoteIp = null;
                    byte[] data = client.Receive(ref remoteIp);
                    string message = Encoding.Unicode.GetString(data);
                    Dispatcher.Invoke(() =>
                    {
                        string time = DateTime.Now.ToShortTimeString();
                        tbChat.Text += $"{Environment.NewLine}{time}: {message}";
                    });
                }
            }
            catch (ObjectDisposedException)
            {
                if (currentAuthorizatedEmployer.User.Username == "")
                    return;
                throw;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }
    }
}
