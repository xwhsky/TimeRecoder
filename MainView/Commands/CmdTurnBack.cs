using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MainView.Commands
{
    class CmdTurnBack:BaseCommand
    {
        public override void OnClick(object sender, EventArgs e)
        {
            TheUniversal.TheMainWindow.TbMainPanel.Visibility = Visibility.Visible;
            TheUniversal.TheMainWindow.MyUcSetting.Visibility = Visibility.Collapsed;
        }
    }
}
