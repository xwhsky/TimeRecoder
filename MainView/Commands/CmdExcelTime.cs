using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.DataVisualization;
using EngineUtil;
using MahApps.Metro.Controls.Dialogs;
using MainView.MyControls;
using Microsoft.Win32;

namespace MainView.Commands
{
    internal class CmdExcelTime : BaseCommand
    {
        private static bool flag = true;

        public override void OnClick(object sender, EventArgs e)
        {
            if (flag)
            {
                #region ????,???????.Used for Last Statistics;
                //var startTime = TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, -55).Date;
                //TimeReporter.CreateTimeExcelWeekly(startTime, DateTime.Now, @"E:\TimeRecord");
                #endregion

                //TimeReporter.CreateWholeTimeExcel(startTime, DateTime.Now, @"E:\whole.xlsx");
                //TimeReporter.CreateWholeTimeExcelInDetail(startTime, DateTime.Now, @"E:\whole2.xlsx");


                var intervalWeeks = TheUniversal.TheMainWindow.MyUcLastWeekInfo.IntervalWeeks;
                var startTime = TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, intervalWeeks).Date;

                var dialog = new SaveFileDialog()
                {
                    Title = "导出上周统计时间",
                    Filter = "*.xlsx|*.xlsx",
                    FileName =
                        string.Format("时间统计{0}-{1}", startTime.ToString("yyyy.MM.dd"),
                            startTime.AddDays(6).ToString("yyyy.MM.dd"))
                };

                if (dialog.ShowDialog() == true)
                {
                    int dayCounts = 7;

                    var timeRecordsByDays = TheCommunity.GetAllTimeRecordsByDays(startTime, dayCounts);
                    var leaveEventByDays = TheCommunity.GetAllLeaveEventByDays(startTime, dayCounts);

                    var keys = new List<string>();

                    foreach (DataRow dr in timeRecordsByDays.Rows)
                    {
                        if (!keys.Contains(dr[0].ToString()))
                            keys.Add(dr[0].ToString());
                    }

                    var times = new DateTime[keys.Count];
                    var leaveEvents = new StringBuilder[keys.Count];

                    foreach (DataRow dr in timeRecordsByDays.Rows)
                    {
                        var index = keys.IndexOf(dr[0].ToString());
                        if ((bool)dr[3])
                            times[index] += (DateTime)dr[2] - (DateTime)dr[1];
                        else
                            times[index] -= (DateTime)dr[2] - (DateTime)dr[1];
                    }

                    foreach (DataRow dr in leaveEventByDays.Rows)
                    {
                        var index = keys.IndexOf(dr[0].ToString());
                        if (index >= 0)
                        {
                            if (leaveEvents[index] == null)
                                leaveEvents[index] = new StringBuilder();
                            leaveEvents[index].Append(dr[2] + ";");
                        }
                    }

                    var valueList = new List<KeyValuePair<string, double>>();

                    for (var i = 0; i < keys.Count; i++)
                    {
                        valueList.Add(new KeyValuePair<string, double>(string.Format("{0}", keys[i]),
                            (times[i] - DateTime.MinValue).TotalHours));
                    }

                    var startDate = TheCommunity.GetWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, intervalWeeks);
                    var endDate = startDate.AddDays(7);

                    // File.Copy(string.Format("{0}\\时间统计.xlsx", AppDomain.CurrentDomain.BaseDirectory), dialog.FileName,true);

                    var dt = new DataTable();
                    dt.Columns.Add(new DataColumn());
                    dt.Columns.Add(new DataColumn());

                    foreach (var keyValuePair in valueList)
                    {
                        var index = keys.IndexOf(keyValuePair.Key);

                        var newRow = dt.NewRow();
                        if (index >= 0)
                        {
                            newRow[0] = string.Format("{0}({1})", keyValuePair.Key, leaveEvents[index]);
                        }
                        else
                        {
                            newRow[0] = string.Format("{0}", keyValuePair.Key);
                        }

                        newRow[1] = keyValuePair.Value;
                        dt.Rows.Add(newRow);
                    }
                    TimeExcelUtil.CreateTimeExcel(dialog.FileName, dt, ExcelType.XlsxDocument);

                }


            }
            flag = !flag;

        }

        
 

}
}
