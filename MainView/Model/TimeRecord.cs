using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using MainView.Annotations;

namespace MainView.Model
{
    [Serializable]
    public class TimeRecordDaily : INotifyPropertyChanged
    {
        [field: NonSerializedAttribute]
        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime _startTimeRecord;
        private TimeSpan[] _timeRecord;
        private string _leaveStatement;

        /// <summary>
        /// 数据录入开始时间
        /// </summary>
        public DateTime StartTimeRecord
        {
            get { return _startTimeRecord; }
            set
            {
                if (value == _startTimeRecord)
                    return;
                _startTimeRecord = value;
                FirePropertyChanged("StartTimeRecord");
            }
        }



        /// <summary>
        /// 打进打出时间
        /// </summary>
        public TimeSpan[] TimeRecord
        {
            get { return _timeRecord; }
            set
            {
                if (value == _timeRecord)
                    return;
                _timeRecord = value;
                FirePropertyChanged("TimeRecord");
            }
        }

        /// <summary>
        /// 出差请假事由
        /// </summary>
        public string LeaveStatement
        {
            get { return _leaveStatement; }
            set
            {
                if (value == _leaveStatement)
                    return;
                _leaveStatement = value;
                FirePropertyChanged("LeaveStatement");
            }
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    
    [Serializable]
    public class TimeSumDaily : INotifyPropertyChanged
    {
        private TimeSpan _sumTime;
        private double _sumTimeInDouble;

        public TimeSpan SumTime
        {
            get { return _sumTime; }
            set
            {
                if (value == _sumTime)
                    return;
                _sumTime = value;
                SumTimeInDouble = value.TotalHours;
                FirePropertyChanged("SumTime");
            }
        }

        public double SumTimeInDouble
        {
            get { return _sumTimeInDouble; }
            set
            {
                if (value == _sumTimeInDouble)
                    return;
                _sumTimeInDouble = value;
                FirePropertyChanged("SumTimeInDouble");
            }
        }
       
        [field: NonSerializedAttribute]
        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


}
