using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace FileBackup
{
    public class Mail
    {
        private string _SMTP_Server;
        private int _SMTP_Port;
        private bool _SMTP_SSL;
        private string _SMTP_User;
        private string _SMTP_Password;
        private string _SMTP_Email;
        private string[] _SMTP_To;

        private SmtpClient _smtp;

        public Mail()
        {

        }

        public void ReadConfig(string filename)
        {
            if (!File.Exists(filename))
            {
                File.AppendAllText(filename, @"# Sample:
# SMTP      = smtp.xxx.com
# PORT      = 25
# SSL       = False
# USER_NAME = xxx@xxx.com
# PASSWORD  = xxx
# FROM      = xxx@xxx.com
# TO        = xxx@xxx.com, xxx2@gmail.com, xxx3@xxx.com
");
                return;
            }
            using (StreamReader sr = new StreamReader(filename))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;
                    int i = line.IndexOf('=');
                    if (i > 0)
                    {
                        string key = line.Substring(0, i).Trim();
                        string value = line.Substring(i + 1).Trim();

                        #region
                        switch (key)
                        {
                            case "SMTP":
                                _SMTP_Server = value;
                                break;
                            case "PORT":
                                _SMTP_Port = Convert.ToInt32(value);
                                break;
                            case "SSL":
                                _SMTP_SSL = Convert.ToBoolean(value);
                                break;
                            case "USER_NAME":
                                _SMTP_User = value;
                                break;
                            case "PASSWORD":
                                _SMTP_Password = value;
                                break;
                            case "FROM":
                                _SMTP_Email = value;
                                break;
                            case "TO":
                                {
                                    string[] arr = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    List<string> tolist = new List<string>();
                                    foreach(string s in arr)
                                    {
                                        if (s.Trim() != "")
                                            tolist.Add(s.Trim());
                                    }
                                    _SMTP_To = tolist.ToArray();
                                }
                                break;
                        }
                        #endregion

                    }
                }
            }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(_SMTP_Server)
                && _SMTP_Port > 0
                && !string.IsNullOrEmpty(_SMTP_User)
                && !string.IsNullOrEmpty(_SMTP_Password)
                && !string.IsNullOrEmpty(_SMTP_Email)
                && _SMTP_To != null
                && _SMTP_To.Length > 0;
        }

        public void Init()
        {
            if (!IsValid())
                return;
            _smtp = new SmtpClient(_SMTP_Server);
            _smtp.Port = _SMTP_Port;
            _smtp.EnableSsl = _SMTP_SSL;
            _smtp.UseDefaultCredentials = false;
            _smtp.Credentials = new NetworkCredential(_SMTP_User, _SMTP_Password);
        }

        public void SendLogFile(string subject, string body, string logfile)
        {
            MailMessage mailMessage = new MailMessage();
            foreach(string s in _SMTP_To)
            {
                mailMessage.To.Add(s);
            }
            mailMessage.From = new MailAddress(_SMTP_Email);
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.IsBodyHtml = true;
            mailMessage.SubjectEncoding = Encoding.UTF8;
            mailMessage.Attachments.Add(new Attachment(logfile, "text/plain"));

            _smtp.Send(mailMessage);
        }

    }
}
