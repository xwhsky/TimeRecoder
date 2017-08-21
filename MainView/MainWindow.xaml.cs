using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EngineUtil;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using MainView.Commands;
using MainView.Model;
using MainView.MyControls;
using MainView.Utility;

namespace MainView
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow()
        {
            InitializeComponent();
            SetReferenceInstances();
            
            Resources.Add("user", TheUniversal.TheCurrentUser);
            
            Loaded += (s, e) =>
            {
                if (TheUniversal.TheCurrentTimeDb == null||!TheUniversal.TheCurrentTimeDb.IsOpen)
                {
                    this.ShowMessageAsync("无法连接数据库,\r\n请确认是否连接服务器！", "");
                }
                else
                {
                    string msg;
                    try
                    {
                        TheCommunity.ChangeUser(TheUniversal.TheCurrentConfig.TheCurrentUser.UserName,
                            TheUniversal.TheCurrentConfig.TheCurrentUser.PassWord, out msg);
                    }
                    catch (Exception ex)
                    {
                        this.ShowMessageAsync(string.Format("{0}用户登录失败，请检查用户名或密码！", TheUniversal.TheCurrentConfig.TheCurrentUser.UserName), "");
                    }
                    
                }

                BindCommandsAndTools();
            };

           Closing += (s, e) => TheUniversal.TheCurrentTimeDb.Dispose();
        }


        private void SetReferenceInstances()
        {
            TheUniversal.TheMainWindow = this;
            Title = string.Format("Time_{0}", TheUniversal.TheCurrentVersion.VersionNum);
        }

        private void BindCommandsAndTools()
        {
            TheUniversal.DicCommands = new Dictionary<string, BaseCommand>();
            foreach (var t in Assembly.GetAssembly(typeof (BaseCommand)).GetTypes())
            {
                if (t.Namespace == "MainView.Commands")
                {
                    var baseCmd = Activator.CreateInstance(t) as BaseCommand;
                    if (baseCmd != null)
                        TheUniversal.DicCommands.Add(t.Name, baseCmd);
                }
                
            }
            TheCommunity.BindCommandsAndTools(this);
        }


    }
}
