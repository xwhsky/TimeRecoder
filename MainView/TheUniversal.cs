using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineUtil;
using MahApps.Metro;
using MainView.Commands;
using MainView.Model;
using MainView.Utility;

namespace MainView
{
    public class TheUniversal
    {
        public static TheVersion TheCurrentVersion = new TheVersion() { VersionNum = 1.20f, NewFeatures = null }; // 当前软件版本
        public static TimeDbUtil TheCurrentTimeDb;
        public static UserModel TheCurrentUser = UserModel.InitUnRegisterUser();
        public static MainWindow TheMainWindow;

        public static TheConfig TheCurrentConfig; 
        public static IDictionary<string, BaseCommand> DicCommands;

    }
}
