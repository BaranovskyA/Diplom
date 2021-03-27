using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Diplom.Entities
{
    class FtpClient
    {
        //поле для хранения имени фтп-сервера
        private string _Host;

        //поле для хранения логина
        private string _UserName;

        //поле для хранения пароля
        private string _Password;

        //объект для запроса данных
        FtpWebRequest ftpRequest;

        //объект для получения данных
        FtpWebResponse ftpResponse;

        //флаг использования SSL
        private bool _UseSSL = false;

        public string Host
        {
            get
            {
                return _Host;
            }
            set
            {
                _Host = value;
            }
        }

        public string UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                _UserName = value;
            }
        }

        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
            }
        }

        public bool UseSSL
        {
            get
            {
                return _UseSSL;
            }
            set
            {
                _UseSSL = value;
            }
        }

        public void DownloadFile(string fileName)
        {
            ftpRequest = (FtpWebRequest)WebRequest.Create($"{Host}/{fileName}");

            ftpRequest.Credentials = new NetworkCredential(_UserName, _Password);
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            try
            {
                ftpRequest.EnableSsl = _UseSSL;
                FileStream downloadedFile = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);

                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                Stream responseStream = ftpResponse.GetResponseStream();

                byte[] buffer = new byte[1024];
                int size = 0;

                while ((size = responseStream.Read(buffer, 0, 1024)) > 0)
                {
                    downloadedFile.Write(buffer, 0, size);

                }
                ftpResponse.Close();
                downloadedFile.Close();
                responseStream.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void UploadFile(string fileName)
        {
            string shortName = fileName.Remove(0, fileName.LastIndexOf(@"\") + 1);

            FileStream uploadedFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            ftpRequest = (FtpWebRequest)WebRequest.Create($"{_Host}/{shortName}");
            ftpRequest.Credentials = new NetworkCredential(_UserName, _Password);
            ftpRequest.EnableSsl = _UseSSL;
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

            byte[] file_to_bytes = new byte[uploadedFile.Length];
            uploadedFile.Read(file_to_bytes, 0, file_to_bytes.Length);

            uploadedFile.Close();

            Stream writer = ftpRequest.GetRequestStream();

            writer.Write(file_to_bytes, 0, file_to_bytes.Length);
            writer.Close();
        }

        public void DeleteFile(string path)
        {
            ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + _Host + path);
            ftpRequest.Credentials = new NetworkCredential(_UserName, _Password);
            ftpRequest.EnableSsl = _UseSSL;
            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

            FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            ftpResponse.Close();
        }
    }
}
