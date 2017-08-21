using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using MainView.Utility;

namespace MainView
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            TheUniversal.TheCurrentConfig = TheConfig.InitilizeConfig();
            base.OnStartup(e);
        }
    }
}
