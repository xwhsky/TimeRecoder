using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MahApps.Metro.Controls;

namespace MainView.Commands
{
    class CmdSetting:BaseCommand
    {
        public override void OnClick(object sender, EventArgs e)
        {
            TheUniversal.TheMainWindow.TbMainPanel.Visibility = Visibility.Collapsed;
            TheUniversal.TheMainWindow.MyUcSetting.Visibility = Visibility.Visible;
        }
    }
}
