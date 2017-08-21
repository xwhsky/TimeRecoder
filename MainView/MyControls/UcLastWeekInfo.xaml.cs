using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;


namespace MainView.MyControls
{
    /// <summary>
    /// UcLastWeekInfo.xaml 的交互逻辑
    /// </summary>
    public partial class UcLastWeekInfo
    {
        private DataTable _timeRecordsByDays;
        private DataTable _leaveEventByDays;

        public int IntervalWeeks { get; private set; }

        public UcLastWeekInfo()
        {
            InitializeComponent();
            TheCommunity.BindCommandsAndTools(UcLastWeekInfoGrid);
            IntervalWeeks = -1;
            UpdateLastWeekInfo();

            BtnRecordFormer.Click += (s, e) =>
            {
                BtnRecordFormer.Visibility = BtnRecordLater.Visibility = BtnRecordReset.Visibility = Visibility.Hidden;
                IntervalWeeks--;
                GetTimeRecords();
                ShowRecordInCharts();

                var worker = new BackgroundWorker();
                worker.DoWork += (ss, es) => Thread.Sleep(300);
                worker.RunWorkerCompleted +=
                    (ss, es) =>
                        BtnRecordFormer.Visibility =
                            BtnRecordLater.Visibility = BtnRecordReset.Visibility = Visibility.Visible;
                worker.RunWorkerAsync();
            };

            BtnRecordLater.Click += (s, e) =>
            {
                BtnRecordFormer.Visibility = BtnRecordLater.Visibility = BtnRecordReset.Visibility = Visibility.Hidden;
                IntervalWeeks++;
                GetTimeRecords();
                ShowRecordInCharts();

                var worker = new BackgroundWorker();
                worker.DoWork += (ss, es) => Thread.Sleep(300);
                worker.RunWorkerCompleted +=
                    (ss, es) =>
                        BtnRecordFormer.Visibility =
                            BtnRecordLater.Visibility = BtnRecordReset.Visibility = Visibility.Visible;
                worker.RunWorkerAsync();
            };

            BtnRecordReset.Click += (s, e) =>
            {
                BtnRecordFormer.Visibility = BtnRecordLater.Visibility = BtnRecordReset.Visibility = Visibility.Hidden;
                IntervalWeeks = -1;
                GetTimeRecords();
                ShowRecordInCharts();

                var worker = new BackgroundWorker();
                worker.DoWork += (ss, es) => Thread.Sleep(300);
                worker.RunWorkerCompleted +=
                    (ss, es) =>
                        BtnRecordFormer.Visibility =
                            BtnRecordLater.Visibility = BtnRecordReset.Visibility = Visibility.Visible;
                worker.RunWorkerAsync();
            };
        }

        public void UpdateLastWeekInfo()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) => GetTimeRecords();

            worker.RunWorkerCompleted += (s, e) => ShowRecordInCharts();

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        private void GetTimeRecords()
        {
            while (TheUniversal.TheCurrentTimeDb == null || !TheUniversal.TheCurrentTimeDb.IsOpen)
            {
                System.Threading.Thread.Sleep(1000);
            }

            _timeRecordsByDays =
                TheCommunity.GetAllTimeRecordsByDays(
                    TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, IntervalWeeks).Date, 7);
            _leaveEventByDays =
                TheCommunity.GetAllLeaveEventByDays(
                    TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, IntervalWeeks).Date, 7);
        }

        /// <summary>
        /// 显示统计数据
        /// </summary>
        private void ShowRecordInCharts()
        {
            var keys = new List<string>();

            foreach (DataRow dr in _timeRecordsByDays.Rows)
            {
                if (!keys.Contains(dr[0].ToString()))
                    keys.Add(dr[0].ToString());
            }

            var times = new TimeSpan[keys.Count];
            var leaveEvents = new StringBuilder[keys.Count];

            foreach (DataRow dr in _timeRecordsByDays.Rows)
            {
                var index = keys.IndexOf(dr[0].ToString());
                if ((bool)dr[3])
                    times[index] += (DateTime)dr[2] - (DateTime)dr[1];
                else
                    times[index] -= (DateTime)dr[2] - (DateTime)dr[1];
            }

            foreach (DataRow dr in _leaveEventByDays.Rows)
            {
                var index = keys.IndexOf(dr[0].ToString());
                if (index >= 0)
                {
                    if (leaveEvents[index] == null)
                        leaveEvents[index] = new StringBuilder(keys[index] + ": ");
                    leaveEvents[index].Append(dr[2] + ";");
                }
            }

            var valueList = new List<KeyValuePair<string, double>>();

            for (var i = 0; i < keys.Count; i++)
            {
                valueList.Add(new KeyValuePair<string, double>(string.Format("{0}", keys[i]),
                    times[i].TotalHours));
            }

            var startDate = TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, IntervalWeeks);
            var endDate = startDate.AddDays(7);

            var seriesTitle = string.Format("时间统计（{0} - {1}）", startDate.ToString("yyyy.MM.dd"), endDate.ToString("yyyy.MM.dd"));
            var leaveEventInShow = leaveEvents.AsQueryable().Where(_ => _ != null && !string.IsNullOrEmpty(_.ToString().Trim()));

            foreach (Chart item in FvCharts.Items)
            {
                item.Title = string.Format("时间统计:{0}-{1}", startDate.ToShortDateString(),
                    endDate.ToShortDateString());
                item.MinWidth = 30 * keys.Count;
                item.MinHeight = 400;
                item.Title = seriesTitle;
                item.Series.Cast<DataPointSeries>().First().Title = string.Empty;
                item.Series.Cast<DataPointSeries>().First().AnimationSequence = AnimationSequence.Simultaneous;
                item.Series.Cast<DataPointSeries>().First().ItemsSource = valueList;
                item.Series.Cast<DataPointSeries>().First().SelectionChanged += (s, e) =>
                {
                    var columnSeries = s as ColumnSeries;
                    if (columnSeries != null)
                    {
                        if (columnSeries.SelectedItem is KeyValuePair<string, double>)
                        {
                            var selectedName = ((KeyValuePair<string, double>)columnSeries.SelectedItem).Key;
                            var ss = from a in leaveEventInShow where a.ToString().Split(':')[0].Equals(selectedName) select a;
                            LvLeaveText.SelectedItem = ss.FirstOrDefault();
                        }
                    }
                };
            }

            LvLeaveText.ItemsSource = leaveEventInShow;

        }

    }
}
