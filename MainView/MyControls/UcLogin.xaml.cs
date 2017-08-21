using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using MainView.Model;

namespace MainView.MyControls
{
    /// <summary>
    /// UcLogin.xaml 的交互逻辑
    /// </summary>
    public partial class UcLogin
    {
        private UserModel _newUserLogin;

        public UcLogin()
        {
            InitializeComponent();
            _newUserLogin = new UserModel();
            grid.DataContext = _newUserLogin;

           
            Loaded += (s, e) => TbLoginUser.Focus();

            KeyDown += (s, e) =>
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        Login();
                        break;
                    case Key.Escape:
                        Cancel();
                        break;
                }
            };
        }

        private void BtLoginClick(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void BtCancelClick(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Login()
        {
            string msg;
            if (TheCommunity.ChangeUser(_newUserLogin.UserName, _newUserLogin.PassWord, out msg))
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

    public static class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.RegisterAttached("Password",
        typeof(string), typeof(PasswordHelper),
        new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));
        public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached("Attach",
        typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, Attach));
        private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
        typeof(PasswordHelper));
        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }
        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }
        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }
        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value);
        }
        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }
        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }
        private static void OnPasswordPropertyChanged(DependencyObject sender,
        DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            passwordBox.PasswordChanged -= PasswordChanged;
            if (!(bool)GetIsUpdating(passwordBox))
            {
                passwordBox.Password = (string)e.NewValue;
            }
            passwordBox.PasswordChanged += PasswordChanged;
        }
        private static void Attach(DependencyObject sender,
        DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox == null)
                return;
            if ((bool)e.OldValue)
            {
                passwordBox.PasswordChanged -= PasswordChanged;
            }
            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }
        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }
    }
}
