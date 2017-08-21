using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MahApps.Metro.Controls.Dialogs;
using MainView.MyControls;

namespace MainView.Commands
{
    class CmdRegister:BaseCommand
    {
        private static bool flag = true;
        public override void OnClick(object sender, EventArgs e)
        {
            if (flag)
            {
                TheUniversal.TheMainWindow.ShowMetroDialogAsync(new UcRegister() { Title = "注册" });
            }
            flag = !flag;
        }
    }
}
