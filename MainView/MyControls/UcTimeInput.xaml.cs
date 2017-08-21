using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using EngineUtil;
using MahApps.Metro.Controls.Dialogs;
using MainView.Model;
using MainView.SubControls;
using Microsoft.Win32;

namespace MainView.MyControls
{
    /// <summary>
    /// UcTimeInput.xaml 的交互逻辑
    /// </summary>
    public partial class UcTimeInput
    {
        private ObservableCollection<TimeRecordDaily> TimeRecordWeek { get; set; }
        private ObservableCollection<TimeRecordDaily> TimeOutRecordWeek { get; set; }
        public string RecordedUser { get; set; }
        public int RecordedNumber { get; set; }

        public UcTimeInput()
        {
            InitializeComponent();
            BtnCleanRecord.Click += (s, e) => UcTimeInputControl.CleanTimeTable();

            BtnOpenRecord.Click += (s, e) =>
            {
                var dialog = new OpenFileDialog() {Title = "打开Time文件", Filter = "*.time|*.time", Multiselect = false};
                
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var deserializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        using (var localStream = new FileStream(dialog.FileName, FileMode.Open))
                        {
                            var data = deserializer.Deserialize(localStream) as TimeRecordWeeklyData;
                            UcTimeInputControl.SetTimeRecordWeekly(data);
                        }
                    }
                    catch (Exception)
                    {
                        TheUniversal.TheMainWindow.ShowMessageAsync("打开失败", "");
                    }
                }
            };

            BtnSaveRecord.Click += (s, e) =>
            {
                var dialog = new SaveFileDialog() { Title = "保存Time文件", Filter = "*.time|*.time"};
                if (dialog.ShowDialog() == true)
                {
                    var deserializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    using (var localStream = new FileStream(dialog.FileName, FileMode.OpenOrCreate))
                    {
                        deserializer.Serialize(localStream, UcTimeInputControl.TimeRecordWeeklyBindingData());
                        TheUniversal.TheMainWindow.ShowMessageAsync("保存成功！", "");
                    }
                }
            };
        }

        /// <summary>
        /// 入库操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStorageTimeRecord_Click(object sender, RoutedEventArgs e)
        {
            TimeRecordWeek = UcTimeInputControl.TimeRecordWeek;
            TimeOutRecordWeek = UcTimeInputControl.TimeOutRecordWeek;
            RecordedUser = UcTimeInputControl.RecordedUser;
            RecordedNumber = UcTimeInputControl.RecordedNumber;

            switch (TheUniversal.TheCurrentUser.UserType)
            {
                case UserType.Administrator:
                case UserType.Recoder:
                {
                    if (string.IsNullOrEmpty(RecordedUser) || RecordedNumber <= 0)
                        TheUniversal.TheMainWindow.ShowMessageAsync("被记录人员有误", "");
                    else if (TimeRecordWeek[0].StartTimeRecord.DayOfWeek != DayOfWeek.Monday ||
                             TimeRecordWeek[0].StartTimeRecord == DateTime.MinValue)
                        TheUniversal.TheMainWindow.ShowMessageAsync("开始记录时间有误", "");
                    else if (
                        TheUniversal.TheCurrentTimeDb.GetTable(
                            string.Format("select * from TB_STUDENT_INFO where NUMBER = {0} and NAME = '{1}'",
                                RecordedNumber, RecordedUser)).Rows.Count == 0)
                        TheUniversal.TheMainWindow.ShowMessageAsync("用户名与学号不匹配", "");
                    else
                    {
                        string errorMsg;
                        int[] errorPos;
                        if (!TheCommunity.CheckIfTimeRecordedByUser(TimeRecordWeek, TimeOutRecordWeek, RecordedUser,
                            RecordedNumber, DateTime.Now, out errorMsg, out errorPos))
                            TheUniversal.TheMainWindow.ShowMessageAsync(errorMsg, "");
                        else
                        {
                            if (TheCommunity.StoreTimeRecordWeekly(TimeRecordWeek, TimeOutRecordWeek, RecordedUser,
                                RecordedNumber, DateTime.Now))
                            {
                                TheUniversal.TheMainWindow.ShowMessageAsync("入库成功！", "");
                                UcTimeInputControl.CleanTimeTable();
                            }
                            else
                            {
                                TheUniversal.TheMainWindow.ShowMessageAsync("入库失败！", "");
                            }

                        }
                    }
                }
                    break;
                case UserType.Guider:
                    TheUniversal.TheMainWindow.ShowMessageAsync("您不具有数据入库权限，请联系管理员修改", "");
                    break;
                default:
                    TheUniversal.TheMainWindow.ShowMessageAsync("您尚未登录，无法入库", "");
                    break;
            }
        }


    }

    

}
