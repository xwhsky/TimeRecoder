using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using EngineUtil;

namespace MainView.Model
{
    public class UserModel : INotifyPropertyChanged
    {
        private string _userName;
        private string _passWord;
        private UserType _userType;
        private string _idCard;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName

        {
            get { return _userName; }
            set {
                if (value == _userName)
                return;
                _userName = value;
                FirePropertyChanged("UserName");
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord
        {
            get { return _passWord; }
            set
            {
                if (value == _passWord)
                    return;
                _passWord = value;
                FirePropertyChanged("PassWord");
            }
        }

        /// <summary>
        /// 用户类型
        /// </summary>
        public UserType UserType
        {
            get { return _userType; }
            set
            {
                if (value == _userType)
                    return;
                _userType = value;
                FirePropertyChanged("UserType");
            }
        }

        /// <summary>
        /// 身份证号码
        /// </summary>
        public string IdCard
        {
            get { return _idCard; }
            set
            {
                if (value == _idCard)
                    return;
                _idCard = value;
                FirePropertyChanged("IdCard");
            }
        }

        /// <summary>
        /// 创建非注册用户
        /// </summary>
        /// <returns></returns>
        public static UserModel InitUnRegisterUser()
        {
            return new UserModel()
            {
                UserType = UserType.UnRegister,
                UserName = "游客"
            };
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
