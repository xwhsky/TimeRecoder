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
using MainView.Model;

namespace MainView.MyControls
{
    /// <summary>
    /// UcTimeQuery.xaml 的交互逻辑
    /// </summary>
    public partial class UcTimeQuery : UserControl
    {
        /// <summary>
        /// 被查询用户
        /// </summary>
        public UserModel RequiredUser;

        /// <summary>
        /// 查询开始时间
        /// </summary>
        public DateTime StartQueryTime;

        /// <summary>
        /// 查询结束时间
        /// </summary>
        public DateTime EndQueryTime;

        public UcTimeQuery()
        {
            InitializeComponent();

            StartQueryTime = DateTime.Now;
            EndQueryTime = DateTime.Now;

            TbRecordedStu.TextChanged += (s, e) =>
            {
                var sql = string.Format("select NUMBER from TB_STUDENT_INFO where NAME = '{0}'", TbRecordedStu.Text);
                var dt = TheUniversal.TheCurrentTimeDb.GetTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    CbNumId.ItemsSource = dt.Rows[0].ItemArray;
                    CbNumId.SelectedIndex = 0;
                }
                else
                {
                    CbNumId.ItemsSource = null;
                }

            };

            DpStartTime.SelectedDateChanged += (s, e) =>
            { if (DpStartTime.SelectedDate != null) StartQueryTime = (DateTime) DpStartTime.SelectedDate; };

            DpEndTime.SelectedDateChanged += (s, e) =>
            { if (DpEndTime.SelectedDate != null) EndQueryTime = (DateTime) DpEndTime.SelectedDate; };

        }

        public void SetStudent(UserModel user)
        {
            var sql = string.Format("select NAME from TB_STUDENT_INFO where IDCARD = '{0}'", user.IdCard);
            var dt = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                TbRecordedStu.Text = dt.Rows[0].ItemArray[0].ToString();
                RequiredUser = user;
            }
            else
            {
                
            }

        }

        /// <summary>
        /// 控制查询用户是否可更换
        /// </summary>
        /// <param name="flag"></param>
        public void SetStudentChangeable(bool flag)
        {
            if (flag)
            {
                TbRecordedStu.IsReadOnly = false;
                CbNumId.IsReadOnly = false;
            }
            
            else
            {
                TbRecordedStu.IsReadOnly = true;
                CbNumId.IsReadOnly = true;
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnQuery(object sender, RoutedEventArgs e)
        {

            var table = TheCommunity.GetAllTimeRecordsByUserAndDays(StartQueryTime,
                (int) (EndQueryTime - StartQueryTime).TotalDays,
                RequiredUser);
            Dg.ItemsSource = table.DefaultView;
          
        }

        /// <summary>
        /// 执行导出excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport(object sender, RoutedEventArgs e)
        {

        }
    }
}
