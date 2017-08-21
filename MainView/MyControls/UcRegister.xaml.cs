using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls.Dialogs;
using MainView.Model;

namespace MainView.MyControls
{
    /// <summary>
    /// UcRegister.xaml 的交互逻辑
    /// </summary>
    public partial class UcRegister
    {
        private UserModel _newUserRegister;

        public UcRegister()
        {
            InitializeComponent();
            _newUserRegister = new UserModel();
            grid.DataContext = _newUserRegister;

            Loaded += (s, e) => TbIdCard.Focus();

            KeyDown += (s, e) =>
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        Register();
                        break;
                    case Key.Escape:
                        Cancel();
                        break;
                }
            };

        }

        private void BtRegisterClick(object sender, RoutedEventArgs e)
        {
            Register();
        }

        private void BtCancelClick(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Register()
        {
            string msg;
            if (TheCommunity.RegisterNewUser(_newUserRegister.UserName, _newUserRegister.PassWord, _newUserRegister.IdCard, EngineUtil.UserType.Guider, out msg))
            {
                TbLoginMsg.Text = msg;
                TbLoginMsg.Visibility = Visibility.Hidden;
                TheUniversal.TheMainWindow.HideMetroDialogAsync(this);
            }
            else
            {
                TbLoginMsg.Text = msg;
                TbLoginMsg.Visibility = Visibility.Visible;
            }
        }

        private void Cancel()
        {
            TheUniversal.TheMainWindow.HideMetroDialogAsync(this);
        }
    }
}
