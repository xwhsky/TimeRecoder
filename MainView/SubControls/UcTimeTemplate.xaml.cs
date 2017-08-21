using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

namespace MainView.SubControls
{
    /// <summary>
    /// 相关联的对象
    /// </summary>
    [Serializable]
    public class TimeRecordWeeklyData : INotifyPropertyChanged
    {
        private string _recordedUser;
        private int _recordedNumber;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 主卡时间
        /// </summary>
        public ObservableCollection<TimeRecordDaily> TimeRecordWeek { get; set; }

        /// <summary>
        /// 副卡时间
        /// </summary>
        public ObservableCollection<TimeRecordDaily> TimeOutRecordWeek { get; set; }

        /// <summary>
        /// 总计时间
        /// </summary>
        public ObservableCollection<TimeSumDaily> TimeSumWeek { get; private set; }

        /// <summary>
        /// 被记录人员的姓名
        /// </summary>
        public string RecordedUser
        {
            get { return _recordedUser; }
            set
            {
                if (value != _recordedUser)
                {
                    _recordedUser = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("RecordedUser"));
                }
            }
        }

        /// <summary>
        /// 被记录人员的学号
        /// </summary>
        public int RecordedNumber
        {
            get { return _recordedNumber; }
            set
            {
                if (value != _recordedNumber)
                {
                    _recordedNumber = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("RecordedNumber"));
                }
            }
        }
    }

    /// <summary>
    /// 用户输入类
    /// UcTimeTemplate.xaml 的交互逻辑
    /// </summary>
    public partial class UcTimeTemplate : UserControl,INotifyPropertyChanged
    {
        private string _recordedUser;
        private int _recordedNumber;
        private TimeRecordWeeklyData _timeRecordWeeklyBindingData;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 主卡时间
        /// </summary>
        public ObservableCollection<TimeRecordDaily> TimeRecordWeek { get; set; }

        /// <summary>
        /// 副卡时间
        /// </summary>
        public ObservableCollection<TimeRecordDaily> TimeOutRecordWeek { get; set; }

        /// <summary>
        /// 总计时间
        /// </summary>
        public ObservableCollection<TimeSumDaily> TimeSumWeek { get; private set; }

        /// <summary>
        /// 被记录人员的姓名
        /// </summary>
        public string RecordedUser
        {
            get { return _recordedUser; }
            set
            {
                if (value != _recordedUser)
                {
                    _recordedUser = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("RecordedUser"));
                }
            }
        } 

        /// <summary>
        /// 被记录人员的学号
        /// </summary>
        public int RecordedNumber
        {
            get { return _recordedNumber; }
            set
            {
                if (value != _recordedNumber)
                {
                    _recordedNumber = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("RecordedNumber"));
                }
            }
        }

        public TimeRecordWeeklyData TimeRecordWeeklyBindingData()
        {
            _timeRecordWeeklyBindingData = new TimeRecordWeeklyData()
            {
                RecordedNumber = RecordedNumber,
                RecordedUser = RecordedUser,
                TimeOutRecordWeek = TimeOutRecordWeek,
                TimeRecordWeek = TimeRecordWeek
            };
            return _timeRecordWeeklyBindingData;
        }

        public void SetTimeRecordWeekly(TimeRecordWeeklyData data)
        {
            RecordedNumber = data.RecordedNumber;
            RecordedUser = data.RecordedUser;
            TimeOutRecordWeek = data.TimeOutRecordWeek;
            TimeRecordWeek = data.TimeRecordWeek;
            DpPickTime.SelectedDate = TimeRecordWeek.First().StartTimeRecord;
            DgTimeInputMain.ItemsSource = TimeRecordWeek;
            DgTimeInputVice.ItemsSource = TimeOutRecordWeek;
        }

        private const int DaysPerWeekly = 7;
        private const int RecordTimesDaily = 3;

        public UcTimeTemplate()
        {
            InitializeComponent();

            CleanTimeTable();
            SpRecordConfig.DataContext = this;

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

            DgTimeInputMain.SelectionChanged += (s, e) => UpdateSumTable();
            DgTimeInputVice.SelectionChanged += (s, e) => UpdateSumTable();

            DpPickTime.SelectedDateChanged += (s, e) =>
            {
                for (var i = 0; i < DaysPerWeekly; i++)
                {
                    if (DpPickTime.SelectedDate != null)
                    {
                        TimeRecordWeek[i].StartTimeRecord = ((DateTime) DpPickTime.SelectedDate).AddDays(i);
                        TimeOutRecordWeek[i].StartTimeRecord = ((DateTime) DpPickTime.SelectedDate).AddDays(i);
                    }
                }
                DgTimeInputMain.ItemsSource = TimeRecordWeek;
                DgTimeInputVice.ItemsSource = TimeOutRecordWeek;
            };
        }

        /// <summary>
        /// 控制打字输入样式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="textChangedEventArgs"></param>
        private void EventSetter_OnHandler(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var txt = sender as TextBox;
            const string regularDigital = @"^\d+$";

            if (txt.Text.Length == 2 && System.Text.RegularExpressions.Regex.IsMatch(txt.Text, regularDigital))
            {
                var firstOrDefault = textChangedEventArgs.Changes.FirstOrDefault();
                //在删除阶段
                if (firstOrDefault != null && firstOrDefault.RemovedLength > 0)
                { }
                else
                {
                    if (!txt.Text.Contains(":"))
                    {
                        txt.Text = txt.Text + ":";
                    }
                    txt.Focus();
                    txt.SelectionStart = txt.Text.Length;
                    txt.SelectionLength = 0;
                }
            }
            else if (txt.Text.Length == 1 && System.Text.RegularExpressions.Regex.IsMatch(txt.Text, regularDigital))
            {
            }
            else if (txt.Text.Length == 3)
            {
            }
            else if (txt.Text.Length == 4 &&
                     System.Text.RegularExpressions.Regex.IsMatch(txt.Text.Substring(3), regularDigital))
            {

            }
            else if (txt.Text.Length == 5 &&
                     System.Text.RegularExpressions.Regex.IsMatch(txt.Text.Substring(3), regularDigital))
            {
                //切换之下一个

                if (txt.Text == "00:00")
                {

                }
                else
                {
                    DateTime res;
                    if (DateTime.TryParse(txt.Text, out res))
                    {

                        const FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;

                        // MoveFocus takes a TraveralReqest as its argument.
                        var request = new TraversalRequest(focusDirection);

                        // Gets the element with keyboard focus.
                        var elementWithFocus = Keyboard.FocusedElement as UIElement;

                        // Change keyboard focus.
                        if (elementWithFocus != null)
                        {
                            elementWithFocus.MoveFocus(request);
                            // UpdateSumTable();
                        }
                    }

                }
            }
            else
            {
                txt.Text = "";
            }

        }

        /// <summary>
        /// 重新统计合计时间
        /// </summary>
        private void UpdateSumTable()
        {
            var res = new Dictionary<int, TimeSpan>();

            var weekTimeSum = new TimeSpan(0, 0, 0, 0);
            for (var i = 0; i < TimeRecordWeek.Count; i++)
            {
                var timeSum = new TimeSpan();
                for (var j = 0; j < TimeRecordWeek[i].TimeRecord.Length; j += 2)
                {
                    var startTime = TimeRecordWeek[i].TimeRecord[j];
                    var endTime = TimeRecordWeek[i].TimeRecord[j + 1];

                    if (j == TimeRecordWeek[i].TimeRecord.Length - 2 && startTime > endTime)
                        endTime = endTime + new TimeSpan(0, 24, 0, 0);

                    timeSum += (endTime - startTime);
                }
                for (var j = 0; j < TimeOutRecordWeek[i].TimeRecord.Length; j += 2)
                {
                    var startTime = TimeOutRecordWeek[i].TimeRecord[j];
                    var endTime = TimeOutRecordWeek[i].TimeRecord[j + 1];

                    if (j == TimeOutRecordWeek[i].TimeRecord.Length / 2 - 1 && startTime > endTime)
                        endTime = endTime.Add(new TimeSpan(0, 24, 0, 0));

                    timeSum -= (endTime - startTime);
                }

                weekTimeSum += timeSum;
                TimeSumWeek[i].SumTime = timeSum;
                res.Add(i, timeSum);
            }

            TimeSumWeek[DaysPerWeekly].SumTime = weekTimeSum;
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void CleanTimeTable()
        {
            TimeRecordWeek = new ObservableCollection<TimeRecordDaily>();
            for (var i = 0; i < DaysPerWeekly; i++)
                TimeRecordWeek.Add(new TimeRecordDaily());

            foreach (var timeRecordDaily in TimeRecordWeek)
                timeRecordDaily.TimeRecord = new TimeSpan[RecordTimesDaily * 2];

            TimeOutRecordWeek = new ObservableCollection<TimeRecordDaily>();
            for (var i = 0; i < DaysPerWeekly; i++)
                TimeOutRecordWeek.Add(new TimeRecordDaily());

            foreach (var timeRecordDaily in TimeOutRecordWeek)
                timeRecordDaily.TimeRecord = new TimeSpan[RecordTimesDaily * 2];

            TimeSumWeek = new ObservableCollection<TimeSumDaily>();
            for (var i = 0; i < DaysPerWeekly + 1; i++)
                TimeSumWeek.Add(new TimeSumDaily());

            //恢复上一次的记录时间点
            for (var i = 0; i < DaysPerWeekly; i++)
            {
                if (DpPickTime.SelectedDate != null)
                {
                    TimeRecordWeek[i].StartTimeRecord = ((DateTime)DpPickTime.SelectedDate).AddDays(i);
                    TimeOutRecordWeek[i].StartTimeRecord = ((DateTime)DpPickTime.SelectedDate).AddDays(i);
                }
            }

            DgTimeInputMain.ItemsSource = TimeRecordWeek;
            DgTimeInputVice.ItemsSource = TimeOutRecordWeek;
            DgTimeSum.ItemsSource = TimeSumWeek;
        }

        /// <summary>
        /// 根据时间间隔显示打卡时间(尚未做)
        /// </summary>
        /// <param name="startDate">查询开始时间</param>
        public void ShowRecordByTime(DateTime startDate)
        {
            var requiredCard = TheCommunity.GetIdCardByUserAndNum(RecordedUser, RecordedNumber);
            var recordedTimeTable = TheCommunity.GetAllTimeRecordsByIdcard(startDate, 7, requiredCard);
            var recordedLeaveEventTable = TheCommunity.GetAllLeaveEventByIdcard(startDate, 7, requiredCard);

        }
    }
}
