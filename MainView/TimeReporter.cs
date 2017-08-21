using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using EngineUtil;

namespace MainView
{
    /// <summary>
    /// 数据统计制表类库
    /// </summary>
    public class TimeReporter
    {
        /// <summary>
        /// 按照周进行统计输出表格
        /// </summary>
        /// <param name="startTime">搜索开始时间</param>
        /// <param name="endTime">搜索结束时间</param>
        /// <param name="saveFolder">存储文件夹路径</param>
        public static void CreateTimeExcelWeekly(DateTime startTime, DateTime endTime, string saveFolder)
        {
            const int dayCounts = 7;
            var calcuTime = startTime;
            while (calcuTime < endTime)
            {
                var timeRecordsByDays = TheCommunity.GetAllTimeRecordsByDays(calcuTime, dayCounts);
                var leaveEventByDays = TheCommunity.GetAllLeaveEventByDays(calcuTime, dayCounts);

                var saveFile =
                    String.Format("时间统计{0}-{1}", calcuTime.ToString("yyyy.MM.dd"),
                        calcuTime.AddDays(6).ToString("yyyy.MM.dd"));
                saveFile = String.Format("{0}//{1}.xlsx", saveFolder, saveFile);
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
                    valueList.Add(new KeyValuePair<string, double>(String.Format("{0}", keys[i]),
                        (times[i] - DateTime.MinValue).TotalHours));
                }

                var dt = new DataTable();
                dt.Columns.Add(new DataColumn());
                dt.Columns.Add(new DataColumn());

                foreach (var keyValuePair in valueList)
                {
                    var index = keys.IndexOf(keyValuePair.Key);

                    var newRow = dt.NewRow();
                    if (index >= 0)
                    {
                        newRow[0] = String.Format("{0}({1})", keyValuePair.Key, leaveEvents[index]);
                    }
                    else
                    {
                        newRow[0] = String.Format("{0}", keyValuePair.Key);
                    }

                    newRow[1] = keyValuePair.Value;
                    dt.Rows.Add(newRow);
                }
                TimeExcelUtil.CreateTimeExcel(saveFile, dt, ExcelType.XlsxDocument);
                calcuTime = calcuTime.AddDays(dayCounts);
            }
        }

        /// <summary>
        /// 生成详细的时间统计结果
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="filePath"></param>
        public static void CreateWholeTimeExcelInDetail(DateTime startTime, DateTime endTime, string filePath)
        {
            var keys = new List<string>();
            var wholeTimeRecords = TheCommunity.GetAllTimeRecordsByDays(startTime, (endTime - startTime).Days);
            foreach (DataRow dr in wholeTimeRecords.Rows)
            {
                if (!keys.Contains(dr[0].ToString()))
                    keys.Add(dr[0].ToString());
            }

            var dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "姓名" });
            foreach (var key in keys)
            {
                var newRow = dt.NewRow();
                newRow[0] = key;
                dt.Rows.Add(newRow);
            }

            const int dayCounts = 1;
            var calcuTime = startTime;
            while (calcuTime < endTime)
            {
                var timeRecordsByDays = TheCommunity.GetAllTimeRecordsByDays(calcuTime, dayCounts);
                var leaveEventByDays = TheCommunity.GetAllLeaveEventByDays(calcuTime, dayCounts);

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
                    valueList.Add(new KeyValuePair<string, double>(String.Format("{0}", keys[i]),
                        (times[i] - DateTime.MinValue).TotalHours));
                }


                dt.Columns.Add(new DataColumn()
                {
                    ColumnName = string.Format("{0}-{1}", calcuTime.ToString("MM.dd"), calcuTime.AddDays(dayCounts).ToString("MM.dd"))
                });

                foreach (var keyValuePair in valueList)
                {
                    var index = keys.IndexOf(keyValuePair.Key);
                    var row = dt.Rows[index];
                    row[dt.Columns.Count - 1] = Math.Round(keyValuePair.Value, 1);
                    //var newRow = dt.NewRow();
                    //if (index >= 0)
                    //{
                    //    //newRow[0] = String.Format("{0}({1})", keyValuePair.Key, leaveEvents[index]);
                    //    newRow[0] = String.Format("{0}", keyValuePair.Key);
                    //}
                    //else
                    //{
                    //    newRow[0] = String.Format("{0}", keyValuePair.Key);
                    //}

                    //newRow[1] = keyValuePair.Value;
                    //dt.Rows.Add(newRow);
                }

                calcuTime = calcuTime.AddDays(dayCounts);
            }

            //ToDO;过滤出差情况
            //foreach (DataRow row in dt.Rows)
            //{
            //    foreach (DataColumn column in dt.Columns)
            //    {
            //        double value;
            //        if (double.TryParse(row[column].ToString(), out value))
            //        {
            //            if (value <= 40)
            //                row[column] = 0;
            //        }
            //    }
            //}

            //Dictionary<string, Tuple<int, double, double>> dic = new Dictionary<string, Tuple<int, double, double>>();
            //foreach (DataRow row in dt.Rows)
            //{
            //    var sum = row.ItemArray.Skip(1).Sum(a => (double.Parse(a.ToString())));
            //    var count = row.ItemArray.Skip(1).Count(a => (double.Parse(a.ToString()) > 0));
            //    var average = sum / count;
            //    dic.Add(row[0].ToString(), new Tuple<int, double, double>(count, average, sum));
            //}

            //using (var fs = new System.IO.StreamWriter(filePath))
            //{
            //    foreach (var item in dic)
            //    {
            //        fs.WriteLine(string.Format("{0} {1} {2} {3}", item.Key, item.Value.Item1, item.Value.Item2, item.Value.Item3));
            //    }
            //}

            TimeExcelUtil.CreateTimeExcel(filePath, dt, ExcelType.XlsxDocument);
        }

        /// <summary>
        /// 按照时间输出总的统计时间表
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="filePath">输出Excel目录</param>
        public static void CreateWholeTimeExcel(DateTime startDate, DateTime endDate, string filePath)
        {
            var dayCounts = (endDate - startDate).Days;
            var timeRecordsByDays = TheCommunity.GetAllTimeRecordsByDays(startDate, dayCounts);
            var leaveEventByDays = TheCommunity.GetAllLeaveEventByDays(startDate, dayCounts);

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
                valueList.Add(new KeyValuePair<string, double>(String.Format("{0}", keys[i]),
                    (times[i] - DateTime.MinValue).TotalHours));
            }

            var dt = new DataTable();
            dt.Columns.Add(new DataColumn());
            dt.Columns.Add(new DataColumn());

            foreach (var keyValuePair in valueList)
            {
                var index = keys.IndexOf(keyValuePair.Key);

                var newRow = dt.NewRow();
                if (index >= 0)
                {
                   // newRow[0] = String.Format("{0}({1})", keyValuePair.Key, leaveEvents[index]);
                    newRow[0] = String.Format("{0}", keyValuePair.Key);
                }
                else
                {
                    newRow[0] = String.Format("{0}", keyValuePair.Key);
                }

                newRow[1] = keyValuePair.Value;
                dt.Rows.Add(newRow);
            }
            TimeExcelUtil.CreateTimeExcel(filePath, dt, ExcelType.XlsxDocument);

        }

        /// <summary>
        /// 导出所有的人员记录，精确至每个人的打进打出记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="filePath"></param>
        public static void CreateWholeExcelInDetail(DateTime startDate, DateTime endDate, string filePath)
        {
            var dayCounts = (endDate - startDate).Days;
            var timeRecordsByDays = TheCommunity.GetAllTimeRecordsByDays(startDate, dayCounts);
            var leaveEventByDays = TheCommunity.GetAllLeaveEventByDays(startDate, dayCounts);

            DataSet dataset = new DataSet();

            IEnumerable<IGrouping<string, DataRow>> groups =
                 timeRecordsByDays.Rows.Cast<DataRow>().GroupBy(_ => _[0].ToString());  //以人名为键值

            foreach (var group in groups)
            {
                DataTable table = new DataTable() { TableName = group.Key };
                table.Columns.Add("IN");
                table.Columns.Add("OUT");

                for (int i = 0; i < group.Count(); i++)
                {
                    var unitRecord = group.ToList()[i];
                    DataRow row = table.Rows.Add();
                    var flag = (bool)unitRecord[3];
                    if (flag)
                    {
                        row[0] = unitRecord[1];
                        row[ 1] = unitRecord[2];
                    }
                    else
                    {
                        row[0] = -(double)unitRecord[1];
                        row[ 1] = -(double)unitRecord[2];
                    }
                }

                dataset.Tables.Add(table);
            }

            TimeExcelUtil.CreateCommonExcel(filePath, dataset, ExcelType.XlsxDocument);
        }

        
    }
}
