using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using MahApps.Metro.Controls.Dialogs;
using MainView.SubControls;

namespace MainView.MyControls
{
    /// <summary>
    /// UcMyTime.xaml 的交互逻辑
    /// </summary>
    public partial class UcMyTime : UserControl
    {
        private DataTable[] _timeRecordsByDays;
        private DataTable _leaveEventByDays;
        private int _intervalWeeks;
        private List<string> _myStatics;
        private readonly List<KeyValuePair<string, double>>  _valueList;
 
        public UcMyTime()
        {
            InitializeComponent();
            _valueList = new List<KeyValuePair<string, double>>();
            var localSerializer = string.Format("{0}localrecord.time", AppDomain.CurrentDomain.BaseDirectory);

            IsVisibleChanged += (s, e) =>
            {
                if ((bool) e.NewValue)
                {
                    UpdateMyTime();

                    //反序列化
                    if (File.Exists(localSerializer))
                    {
                        try
                        {
                            var deserializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            if (File.Exists(localSerializer))
                            {
                                using (var localStream = new FileStream(localSerializer, FileMode.Open))
                                {
                                    var data = deserializer.Deserialize(localStream) as TimeRecordWeeklyData;
                                    UcTimeLocal.SetTimeRecordWeekly(data);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            };

            BtnStorageRecord.Click += (s, e) =>
            {
                //TimeRecordWeeklyData序列化
                try
                {
                    var deserializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    using (var localStream = new FileStream(localSerializer, FileMode.OpenOrCreate))
                    {
                        deserializer.Serialize(localStream, UcTimeLocal.TimeRecordWeeklyBindingData());
                        TheUniversal.TheMainWindow.ShowMessageAsync("保存成功！", "");
                    }
                }
                catch (Exception)
                {
                    TheUniversal.TheMainWindow.ShowMessageAsync("保存失败！", "");

                }
            };

            BtnCleanRecord.Click += (s, e) => UcTimeLocal.CleanTimeTable();
        }

        private void UpdateMyTime()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                while (TheUniversal.TheCurrentTimeDb == null || !TheUniversal.TheCurrentTimeDb.IsOpen)
                {
                    Thread.Sleep(1000);
                }

                _intervalWeeks = 10;
                _timeRecordsByDays = new DataTable[_intervalWeeks];
                
                for (var i = 1; i <= _intervalWeeks; i++)
                {
                    _timeRecordsByDays[i - 1] = TheCommunity.GetAllTimeRecordsByUserAndDays(
                        TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, -(_intervalWeeks - i + 1)).Date, 7,
                        TheUniversal.TheCurrentUser);
                }

                //_leaveEventByDays =
                //    TheCommunity.GetAllLeaveEventByDays(
                //        TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, -(_intervalWeeks - i + 1)).Date, 7);


                //获取近10周统计
                var startDate = TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, -_intervalWeeks);

                var times = new DateTime[_intervalWeeks];

                for (var i = 0; i < _timeRecordsByDays.Length; i++)
                {
                    var timeRecordsByDay = _timeRecordsByDays[i];
                    foreach (DataRow dr in timeRecordsByDay.Rows)
                    {
                        if ((bool)dr[2])
                            times[i] += (DateTime)dr[1] - (DateTime)dr[0];
                        else
                            times[i] -= (DateTime)dr[1] - (DateTime)dr[0];
                    }
                }

                for (var i = 0; i < _intervalWeeks; i++)
                    _valueList.Add(new KeyValuePair<string, double>(startDate.AddDays(7*i).ToShortDateString(),
                        (times[i] - DateTime.MinValue).TotalHours));

                //显示统计数据
                _myStatics = new List<string>();
                startDate = DateTime.Parse("2014/02/10");
                DateTime endDate = TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Sunday, 0);
                var dayCounts = (int)((endDate - startDate).TotalDays);
                var timeSumTable = TheCommunity.GetAllTimeRecordsByUserAndDays(startDate, dayCounts,
                    TheUniversal.TheCurrentUser);

                var timeSum = new TimeSpan();
                foreach (DataRow dr in timeSumTable.Rows)
                {
                    if ((bool)dr[2])
                        timeSum += (DateTime)dr[1] - (DateTime)dr[0];
                    else
                        timeSum -= (DateTime)dr[1] - (DateTime)dr[0];
                }

                _myStatics.Add(string.Format("1.    从{0}到{1}的{2}天内，您累计打卡时间达到{3}小时。", startDate.ToShortDateString(),
                    endDate.ToShortDateString(), dayCounts, timeSum.TotalHours));
                _myStatics.Add("更多统计，敬请期待~~");
                
            };

            //数据绑定
            worker.RunWorkerCompleted += (s, e) =>
            {
                lineChart.Series.Cast<DataPointSeries>().First().ItemsSource = _valueList;
                lineChart.Title = "近10周打卡时间";
                LvMyStatics.ItemsSource = _myStatics;
            };

            worker.RunWorkerAsync();
        }
    }
}
