using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Xml;
using System.Xml.Linq;
using MahApps.Metro;
using MainView.Model;
using System.IO;

namespace MainView
{
    /// <summary>
    /// 
    /// </summary>
    public class TheConfig
    {
        public readonly string DataPassword = "123";

        private readonly string _configPath;
        private readonly XElement _xmlRoot;
        private static TheConfig _theConfig;
        /// <summary>
        /// 数据库路径
        /// </summary>
        public string DatabasePath { get; set; }

        /// <summary>
        /// 更新文件目录
        /// </summary>
        public string UpdateFolder { get; set; }

        /// <summary>
        /// 所需环境文件
        /// </summary>
        public List<string> EnvironmentFiles { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public UserModel TheCurrentUser { get; set; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        public Accent TheCurrentAccent { get; set; }

        /// <summary>
        /// 背景主题
        /// </summary>
        public AppTheme TheCurrentTheme { get; set; }

        private TheConfig()
        {
            _configPath = string.Format("{0}CONFIG", AppDomain.CurrentDomain.BaseDirectory);

            _xmlRoot = XElement.Load(_configPath);
            DatabasePath = _xmlRoot.Element("setting").Element("database").Value;
            if (!File.Exists(DatabasePath))
                DatabasePath = String.Format("{0}DBTIMERECORD.accdb", AppDomain.CurrentDomain.BaseDirectory);

            UpdateFolder = _xmlRoot.Element("setting").Element("updatefolder").Value;
            EnvironmentFiles =
                _xmlRoot.Element("setting")
                    .Element("environmentfile")
                    .Elements("file")
                    .Select(_ => _.Value)
                    .ToList();

            var userflag = _xmlRoot.Element("appearance").Element("saveuser").Element("flag").Value;

            if (userflag.ToUpper().Equals("TRUE"))
            {
                var username = _xmlRoot.Element("appearance").Element("saveuser").Element("user").Value;
                var psd = _xmlRoot.Element("appearance").Element("saveuser").Element("password").Value;
                var connectFlag = TheCommunity.DbConnect(DatabasePath, DataPassword);
                if (connectFlag)
                    TheCurrentUser = TheCommunity.GetUser(username, psd);
                else
                    TheCurrentUser = null;
            }

            TheCurrentAccent =
                ThemeManager.Accents.First(
                    x => x.Name == _xmlRoot.Element("appearance").Element("border").Element("accent").Value);
            TheCurrentTheme =
                ThemeManager.AppThemes.First(
                    x => x.Name == _xmlRoot.Element("appearance").Element("border").Element("theme").Value);
            ThemeManager.ChangeAppStyle(Application.Current, TheCurrentAccent, TheCurrentTheme);
        }

        public static TheConfig InitilizeConfig()
        {
            try
            {
                if (_theConfig != null)
                {
                    return _theConfig;
                }
                else
                {
                    return new TheConfig();
                }
            }
            catch (Exception)
            {
                return null;
            } 
        }

        public void SaveChanges()
        {
            if (_xmlRoot == null)
                return;
            _xmlRoot.Element("setting").Element("database").Value = DatabasePath;
            _xmlRoot.Element("setting").Element("updatefolder").Value = UpdateFolder;
            _xmlRoot.Element("setting").Element("environmentfile").RemoveAll();
            foreach (var environmentFile in EnvironmentFiles)
            {
                _xmlRoot.Element("setting").Element("environmentfile").Add(new XElement("file", environmentFile));
            }

            if (TheCurrentUser != null)
            {
                _xmlRoot.Element("appearance").Element("saveuser").Element("user").Value = TheCurrentUser.UserName;
                _xmlRoot.Element("appearance").Element("saveuser").Element("password").Value = TheCurrentUser.PassWord;
                _xmlRoot.Element("appearance").Element("saveuser").Element("flag").Value = "TRUE";
            }
            else
            {
                _xmlRoot.Element("appearance").Element("saveuser").Element("user").Value = string.Empty;
                _xmlRoot.Element("appearance").Element("saveuser").Element("password").Value = string.Empty;
                _xmlRoot.Element("appearance").Element("saveuser").Element("flag").Value = "FALSE";
            }

            _xmlRoot.Element("appearance").Element("border").Element("accent").Value = TheCurrentAccent.Name;
            _xmlRoot.Element("appearance").Element("border").Element("theme").Value = TheCurrentTheme.Name;

            _xmlRoot.Save(_configPath);

        }

    }
}
