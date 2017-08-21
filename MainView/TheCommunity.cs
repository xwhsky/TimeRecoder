using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EngineUtil;
using MainView.Commands;
using MainView.Model;
using System.Data;
using MainView.Utility;

namespace MainView
{
    /// <summary>
    /// 公共类库-实现基元功能
    /// </summary>
    public class TheCommunity
    {
        /// <summary>
        /// 检测新版本
        /// </summary>
        public static bool CheckForNewVersion(out TheVersion newVersion)
        {
            try
            {
                var sql =
                    string.Format("select top 1 version,newfeature,datetime from TB_VERSION_INFO order by version desc");
                var res = TheUniversal.TheCurrentTimeDb.GetTable(sql);


                newVersion = new TheVersion()
                {
                    VersionNum = (float) res.Rows[0].ItemArray[0],
                    NewFeatures = res.Rows[0].ItemArray[1].ToString().Split('#').ToList()
                };
                //newVersion = new TheVersion();
                //using (
                //    var sr =
                //        new StreamReader(File.OpenRead(string.Format("{0}README", AppDomain.CurrentDomain.BaseDirectory)),Encoding.UTF8)
                //    )
                //{
                //    string line;
                //    while ((line = sr.ReadLine()) != null)
                //    {
                //        //版本
                //        if (line.Contains("Time Recorder version"))
                //        {
                //            newVersion.VersionNum = double.Parse(line.Substring(22, 5).Split(' ')[0]);
                //        }
                //        //新特征读取
                //        if (line.Equals("New Features"))
                //        {
                //            for (var i = 0; i < 4; i++)
                //                sr.ReadLine();

                //            while (!string.IsNullOrEmpty(line = sr.ReadLine().Trim()))
                //                newVersion.NewFeatures.Add(line);
                //        }
                //    }
                //}
                return newVersion.VersionNum > TheUniversal.TheCurrentVersion.VersionNum;
            }
            catch (Exception)
            {
                newVersion = new TheVersion();
                return false;
            }
        }


        /// <summary>
        /// 连接远程数据库
        /// </summary>
        /// <returns></returns>
        public static bool DbConnect(string dbPath,string psd)
        {
            bool flag;
            try
            {
                TheUniversal.TheCurrentTimeDb = new TimeDbUtil(dbPath,psd);
                flag = TheUniversal.TheCurrentTimeDb.Open();
            }
            catch (Exception ex)
            {
                return false;
            }

            return flag;
        }

        /// <summary>
        /// 根据用户名密码获取用户实例
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static UserModel GetUser(string username, string password)
        {
            var sql = string.Format("select username,password,authority,idcard from TB_USER_INFO where username = '{0}' and password = '{1}'",
                username, password);
            var res = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            var userModel = new UserModel();
            if (res != null && res.Rows.Count > 0)
            {
                var userTb = res.Rows[0].ItemArray;
                userModel.UserName = userTb[0].ToString();
                userModel.PassWord = userTb[1].ToString();
                userModel.UserType = (UserType)(userTb[2]);
                userModel.IdCard = userTb[3].ToString();
            }
            return userModel;
        }

        /// <summary>
        /// 切换用户
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="msg"></param>
        public static bool ChangeUser(string username,string password,out string msg)
        {
            msg = string.Empty;
            var sql = string.Format("select username,password,authority,idcard from TB_USER_INFO where username = '{0}' and password = '{1}'",
                username, password);
            var res = TheUniversal.TheCurrentTimeDb.GetTable(sql);
       
            if (res !=null && res.Rows.Count>0)
            {
                var userTb = res.Rows[0].ItemArray;
                TheUniversal.TheCurrentUser.UserName = userTb[0].ToString();
                TheUniversal.TheCurrentUser.PassWord = userTb[1].ToString();
                TheUniversal.TheCurrentUser.UserType = (UserType)(userTb[2]);
                TheUniversal.TheCurrentUser.IdCard = userTb[3].ToString();

                switch (TheUniversal.TheCurrentUser.UserType)
                {
                    case UserType.Administrator:
                        TheUniversal.TheMainWindow.TiBackAdmin.Visibility = Visibility.Visible;
                        TheUniversal.TheMainWindow.TiMyTime.Visibility = Visibility.Visible;
                        TheUniversal.TheMainWindow.MyUcSetting.TbInfo.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.TiExport.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.BtnLogin.Content = "切换账户";
                        break;
                    case UserType.Recoder:
                        TheUniversal.TheMainWindow.TiBackAdmin.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.TiMyTime.Visibility = Visibility.Visible;
                        TheUniversal.TheMainWindow.MyUcSetting.TbInfo.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.TiExport.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.BtnLogin.Content = "切换账户";
                        break;
                    case UserType.Guider:
                        TheUniversal.TheMainWindow.TiBackAdmin.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.TiMyTime.Visibility = Visibility.Visible;
                        TheUniversal.TheMainWindow.MyUcSetting.TbInfo.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.TiExport.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.BtnLogin.Content = "切换账户";
                        break;
                    case UserType.UnRegister:
                        TheUniversal.TheMainWindow.TiBackAdmin.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.TiMyTime.Visibility = Visibility.Collapsed;
                        TheUniversal.TheMainWindow.MyUcSetting.TbInfo.Visibility = Visibility.Visible;
                        TheUniversal.TheMainWindow.BtnLogin.Content = "登录";
                        break;

                }

                msg = "登录成功！";
                return true;
            }

            msg = "登录失败！";
            return false;

        }

        /// <summary>
        /// 注册新用户
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool RegisterNewUser(string username, string password,string idcard,UserType authority, out string msg)
        {
             msg = string.Empty;
            if (string.IsNullOrEmpty(username))
            {
                msg = "用户名不能为空";
                return false;
            }
            if (string.IsNullOrEmpty(idcard))
            {
                msg = "身份证不能为空";
                return false;
            }
            if (string.IsNullOrEmpty(password))
            {
                msg = "密码不能为空";
                return false;
            }

            var sql = string.Format("select name from TB_STUDENT_INFO where IDCARD = '{0}'", idcard);
            var res = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            if (res == null || res.Rows.Count == 0)
            {
                msg = "该身份证号码不存在，请联系管理员添加";
                return false;
            }

             sql = string.Format("select username from TB_USER_INFO where username = '{0}'", username);
             res = TheUniversal.TheCurrentTimeDb.GetTable(sql);
             if (res != null && res.Rows.Count > 0)
             {
                 msg = "该用户名已被注册";
                 return false;
             }



            sql =
                string.Format(
                    "insert into TB_USER_INFO([username],[password],[authority],[idcard]) values('{0}','{1}',{2},'{3}')",
                    username, password, (int) authority, idcard);

            if (TheUniversal.TheCurrentTimeDb.ExecuteSql(sql))
            {
                return ChangeUser(username, password, out msg);
            }
            msg = "注册失败";
            return false;
        }

        /// <summary>
        /// 事件绑定
        /// </summary>
        /// <param name="myVisual"></param>
        public static void BindCommandsAndTools(Visual myVisual)
        {
            if(myVisual is Button)
                ((Button)myVisual).Click += (s, e) =>
                {
                    var btnName = ((Button)e.Source).Name;
                    var cmdName = btnName.Replace("Btn", "Cmd");
                    BaseCommand cmd;
                    TheUniversal.DicCommands.TryGetValue(cmdName, out cmd);

                    if (cmd != null)
                        cmd.OnClick(s, e);
                };

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(myVisual); i++)
            {
                // Retrieve child visual at specified index value.
                var childVisual = (Visual)VisualTreeHelper.GetChild(myVisual, i);

                // Do processing of the child visual object.
                if (childVisual is Button)
                    ((Button) childVisual).Click += (s, e) =>
                    {
                        var btnName = ((Button)e.Source).Name;
                        var cmdName = btnName.Replace("Btn", "Cmd");
                        BaseCommand cmd;
                        TheUniversal.DicCommands.TryGetValue(cmdName, out cmd);

                        if (cmd != null)
                            cmd.OnClick(s, e);
                    };

                // Enumerate children of the child visual object.
                BindCommandsAndTools(childVisual);
            }
        }

        /// <summary>
        /// 存储一周记录
        /// </summary>
        /// <param name="timeMain">主卡</param>
        /// <param name="timeVice">副卡</param>
        /// <param name="user">被记录用户名称</param>
        /// <param name="number">被记录人学号</param>
        /// <param name="recordTime">记录的时间</param>
        /// <returns></returns>
        public static bool StoreTimeRecordWeekly(ObservableCollection<TimeRecordDaily> timeMain,
            ObservableCollection<TimeRecordDaily> timeVice, string user, int number,DateTime recordTime)
        {
            var recordedId = GetIdCardByUserAndNum(user, number);

            var sql = new List<string>();

            //主卡
            foreach (var timeRecordDaily in timeMain)
            {
                for (var i = 0; i < timeRecordDaily.TimeRecord.Length/2; i ++)
                {
                    var startTime = DateTime.Parse(timeRecordDaily.StartTimeRecord.ToString("yyyy-MM-dd")) + timeRecordDaily.TimeRecord[2 * i];
                    var endTime = DateTime.Parse(timeRecordDaily.StartTimeRecord.ToString("yyyy-MM-dd")) +
                                  timeRecordDaily.TimeRecord[2*i + 1];

                    //最后一条记录添加24小时，作为第二天时间
                    if (i == timeRecordDaily.TimeRecord.Length / 2 - 1 && startTime > endTime)
                    {
                        endTime = endTime.AddDays(1);
                    }

                    if (startTime >= endTime)
                        continue;
                    sql.Add(
                        string.Format(
                            "insert into TB_TIME_RECORD_ONCE ([IDCARD],[DATETIME],[INTIME],[OUTTIME],[RecordUser],[RecordType]) values('{0}',\'{1}\',\'{2}\',\'{3}\','{4}',{5})",
                            recordedId, recordTime, startTime, endTime,
                            TheUniversal.TheCurrentUser.UserName, true));
                }
            }

            //副卡
            foreach (var timeRecordDaily in timeVice)
            {
                for (var i = 0; i < timeRecordDaily.TimeRecord.Length/2; i ++)
                {
                    var startTime = DateTime.Parse(timeRecordDaily.StartTimeRecord.ToString("yyyy-MM-dd")) + timeRecordDaily.TimeRecord[2 * i];
                    var endTime = DateTime.Parse(timeRecordDaily.StartTimeRecord.ToString("yyyy-MM-dd")) +
                                  timeRecordDaily.TimeRecord[2 * i + 1];

                    //最后一条记录添加24小时，作为第二天时间
                    if (i == timeRecordDaily.TimeRecord.Length / 2 - 1 && startTime > endTime)
                    {
                        endTime = endTime.AddDays(1);
                    }

                    if (startTime >= endTime)
                        continue;
                    sql.Add(
                        string.Format(
                            "insert into TB_TIME_RECORD_ONCE ([IDCARD],[DATETIME],[INTIME],[OUTTIME],[RecordUser],[RecordType]) values('{0}',\'{1}\',\'{2}\',\'{3}\','{4}',{5})",
                            recordedId,recordTime, startTime, endTime,
                            TheUniversal.TheCurrentUser.UserName, false));
                }
            }

            //出差请假事由
            foreach (var timeRecordDaily in timeMain)
            {
                var state = timeRecordDaily.LeaveStatement;
                if (state == null || string.IsNullOrEmpty(state.Trim()) || string.IsNullOrWhiteSpace(state.Trim()))
                    continue;

                sql.Add(string.Format(
                    "insert into TB_LEAVE_RECORD_DAILY ([IDCARD],[EVENT],[DATETIME]) values('{0}',\'{1}\',\'{2}\')",
                    recordedId, state, timeRecordDaily.StartTimeRecord));
            }

            return TheUniversal.TheCurrentTimeDb.ExecuteSql(sql.ToArray());
        }

        /// <summary>
        /// 获取一段时间全部的打卡记录(包括主卡、副卡)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="dayCounts"></param>
        /// <returns></returns>
        public static DataTable GetAllTimeRecordsByDays(DateTime startDate, int dayCounts)
        {
            var endDate = startDate.AddDays(dayCounts);
            var sql =
                string.Format(
                    "select b.name,a.intime,a.outtime,a.recordtype from TB_TIME_RECORD_ONCE as a  left join TB_STUDENT_INFO as b on a.idcard = b.idcard where [intime] >= #{0}# and [intime] < #{1}#",
                    startDate, endDate);
            var dt = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            return dt;
        }

        /// <summary>
        /// 获取一段时间全部的出差请假记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="dayCounts"></param>
        /// <returns></returns>
        public static DataTable GetAllLeaveEventByDays(DateTime startDate, int dayCounts)
        {
            var endDate = startDate.AddDays(dayCounts);
            var sql =
                string.Format(
                    "select b.name,a.datetime,a.event from TB_LEAVE_RECORD_DAILY as a  left join TB_STUDENT_INFO as b on a.idcard = b.idcard where [datetime] >= #{0}# and [datetime] < #{1}#",
                    startDate, endDate);
            var dt = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            return dt;
        }

        /// <summary>
        /// 获取用户一段时间全部的出差请假记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="dayCounts"></param>
        /// <param name="idcard"></param>
        /// <returns></returns>
        public static DataTable GetAllLeaveEventByIdcard(DateTime startDate, int dayCounts, string idcard)
        {
            var endDate = startDate.AddDays(dayCounts);
            var sql =
                string.Format(
                    "select a.datetime,a.event from TB_LEAVE_RECORD_DAILY  where [datetime] >= #{0}# and [datetime] < #{1}# and idcard='{2}'",
                    startDate, endDate, idcard);
            var dt = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            return dt;
        }

        /// <summary>
        /// 获取用户一段时间全部的打卡记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="dayCounts"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static DataTable GetAllTimeRecordsByUserAndDays(DateTime startDate, int dayCounts, UserModel user)
        {
            var endDate = startDate.AddDays(dayCounts);
            var sql =
                string.Format(
                    "select a.intime,a.outtime,a.recordtype from TB_TIME_RECORD_ONCE as a   where a.idcard = '{2}' and [intime] >= #{0}# and [intime] < #{1}# order by a.intime asc",
                    startDate, endDate,user.IdCard);
            var dt = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            return dt;
        }

        /// <summary>
        /// 获取用户一段时间全部的打卡记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="dayCounts"></param>
        /// <param name="idcard"></param>
        /// <returns></returns>
        public static DataTable GetAllTimeRecordsByIdcard(DateTime startDate, int dayCounts, string idcard)
        {
            var endDate = startDate.AddDays(dayCounts);
            var sql =
                string.Format(
                    "select a.intime,a.outtime,a.recordtype from TB_TIME_RECORD_ONCE as a   where a.idcard = '{2}' and [intime] >= #{0}# and [intime] < #{1}# order by a.intime asc",
                    startDate, endDate, idcard);
            var dt = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            return dt;
        }

        /// <summary>
        /// 获取最近的反馈信息
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<Tuple<string, string, DateTime>> GetLatestFeedback(int count)
        {
            var sql = string.Format("select top {0} username,text,datetime from TB_FEEDBACK order by datetime desc", count);
            var res = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            var ss = new List<Tuple<string, string, DateTime>>();
            foreach (System.Data.DataRow row in res.Rows)
            {
                ss.Add(new Tuple<string, string, DateTime>(row.ItemArray[0].ToString(), row.ItemArray[1].ToString(), (DateTime)row.ItemArray[2]));
            }
            return ss;
        }

        /// <summary>
        /// 发送反馈信息
        /// </summary>
        /// <param name="username"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool SendFeedback(string username, string msg)
        {
            var sql = string.Format("insert into TB_FEEDBACK ([username],[text],[datetime]) values('{0}','{1}','{2}')", username, msg, DateTime.Now);
            return TheUniversal.TheCurrentTimeDb.ExecuteSql(sql);
        }

        /// <summary>
        /// 检查输入数据是否有误
        /// </summary>
        /// <param name="timeMain"></param>
        /// <param name="timeVice"></param>
        /// <param name="user"></param>
        /// <param name="number"></param>
        /// <param name="recordTime"></param>
        /// <param name="msg"></param>
        /// <param name="errorPos"></param>
        /// <returns></returns>
        public static bool CheckIfTimeRecordedByUser(ObservableCollection<TimeRecordDaily> timeMain,
            ObservableCollection<TimeRecordDaily> timeVice, string user, int number, DateTime recordTime, out string msg,
            out int[] errorPos)
        {
            msg = string.Empty;
            errorPos = new int[2];

            var recordedId = GetIdCardByUserAndNum(user, number);
            var startTime = DateTime.Parse(timeMain.First().StartTimeRecord.ToString("yyyy-MM-dd"));
            var endTime = DateTime.Parse(timeMain.Last().StartTimeRecord.ToString("yyyy-MM-dd"));

            //检测是否已记录过
            var sql =
                string.Format(
                    "select recorduser from TB_TIME_RECORD_ONCE where [intime] >= #{0}# and [intime] <= #{1}# and idcard = '{2}'",
                    startTime, endTime, recordedId);
            var table1 = TheUniversal.TheCurrentTimeDb.GetTable(sql);
            if (table1.Rows.Count > 0)
            {
                msg = string.Format("记录重复：已由{0}统计过", table1.Rows[0].ItemArray[0]);
                errorPos = new[] {0, 0};
                return false;
            }

            //主卡检测
            for (var i = 0; i < timeMain.Count; i++)
            {
                var timeMainRecordDaily = timeMain[i];
                var timeViceRecordDaily = timeVice[i];

                //遍历主卡每一条时间对
                for (var j = 0; j < timeMainRecordDaily.TimeRecord.Length/2 - 1; j++)
                {
                    var timeInRecordOnceFormer = timeMainRecordDaily.TimeRecord[2*j];
                    var timeOutRecordOnceFormer = timeMainRecordDaily.TimeRecord[2*j + 1];
                    var timeInRecordOnceLater = timeMainRecordDaily.TimeRecord[2*j + 2];
                    var timeOutRecordOnceLater = timeMainRecordDaily.TimeRecord[2*j + 3];

                    if (timeInRecordOnceFormer > timeOutRecordOnceFormer)
                    {
                        msg = "主卡记录问题：打出时间早于打进时间";
                        errorPos = new[] {j*2, i};
                        return false;
                    }
                    //if (timeInRecordOnceLater > timeOutRecordOnceLater)
                    //{
                    //    msg = "主卡记录问题：打出时间早于打进时间";
                    //    errorPos = new[] {(j + 1)*2, i};
                    //    return false;
                    //}
                    if (timeOutRecordOnceFormer > timeInRecordOnceLater &&
                        timeInRecordOnceLater != new TimeSpan(0, 0, 0, 0))
                    {
                        msg = "主卡记录问题：时间有重叠";
                        errorPos = new[] {j*2, i};
                        return false;
                    }
                }

                //遍历副卡每一条时间对
                for (var j = 0; j < timeViceRecordDaily.TimeRecord.Length / 2 - 1; j++)
                {
                    var timeInRecordOnceFormer = timeViceRecordDaily.TimeRecord[j];
                    var timeOutRecordOnceFormer = timeViceRecordDaily.TimeRecord[j + 1];
                    var timeInRecordOnceLater = timeViceRecordDaily.TimeRecord[j + 2];
                    var timeOutRecordOnceLater = timeViceRecordDaily.TimeRecord[j + 3];

                    if (timeInRecordOnceFormer > timeOutRecordOnceFormer)
                    {
                        msg = "主卡记录问题：打出时间早于打进时间";
                        errorPos = new[] { j * 2, i };
                        return false;
                    }
                    //if (timeInRecordOnceLater > timeOutRecordOnceLater)
                    //{
                    //    msg = "主卡记录问题：打出时间早于打进时间";
                    //    errorPos = new[] { (j + 1) * 2, i };
                    //    return false;
                    //}
                    if (timeOutRecordOnceFormer > timeInRecordOnceLater)
                    {
                        msg = "主卡记录问题：时间有重叠";
                        errorPos = new[] { j * 2, i };
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 获取距离参考时间的日子
        /// </summary>
        /// <param name="dt">参考时间</param>
        /// <param name="weekday">星期几</param>
        /// <param name="Number">间隔周数，正数为下周，负数为上周</param>
        /// <returns></returns>
        public static DateTime GetWeekUpOfDate(DateTime dt, DayOfWeek weekday, int Number)
        {
            var wd1 = (int)weekday;
            var wd2 = (int)dt.DayOfWeek;
            if (wd2 == 0)
                wd2 = 7;
            return wd2 == wd1 ? dt.AddDays(7 * Number) : dt.AddDays(7 * Number - wd2 + wd1);
        }

        /// <summary>
        /// 根据姓名和学号获取身份证号码
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetIdCardByUserAndNum(string userName, int number)
        {
            return TheUniversal.TheCurrentTimeDb.GetTable(
                string.Format("select IDCARD from TB_STUDENT_INFO where NUMBER = {0} and NAME = '{1}'", number, userName))
                .Rows[0].ItemArray[0].ToString();
        }


        public static void SumAllTimesForEach(string timeExcelFolder, string savePath)
        {
            Dictionary<DataTable, string> tableNameDic = new Dictionary<DataTable, string>();
            List<TimeExcelUtil> timeExcels = Directory.GetFiles(timeExcelFolder,"*.xlsx").Select(_ => new TimeExcelUtil(_)).ToList();
            List<DataTable> tables = timeExcels.Select(_ => _.ReadTimeDataAsDataTable()).ToList();
            DataTable tableResult = new DataTable();  //final sum result table

            List<string> names = new List<string>();
            List<string> times = new List<string>();
            List<string> reasons = new List<string>();


            Dictionary<string, int> nameRowDic = new Dictionary<string, int>();

            var nameColumn =  tableResult.Columns.Add();
            nameColumn.ColumnName = "Name";

            foreach (var timeExcel in timeExcels)
            {
                var table = timeExcel.ReadTimeDataAsDataTable();
                var newColumn = tableResult.Columns.Add();
                string excelName =  timeExcel.GetName();

                newColumn.ColumnName = excelName.Substring(excelName.IndexOf('计') + 1, excelName.LastIndexOf('.') - excelName.IndexOf('计')-1);
                foreach (DataRow row in table.Rows)
                {
                    string row0Str =  row[0].ToString();
                    if (string.IsNullOrEmpty(row0Str.Trim())) continue;

                    int nameLength = row0Str.Length;
                    if(row0Str.Contains('('))
                        nameLength = row0Str.IndexOf('(');
                    else if (row0Str.Contains('（'))
                        nameLength = row0Str.IndexOf('（');
                    string name = row0Str.Substring(0, nameLength);
                    string time = row[1].ToString();
                    DataRow resultRow = null;
                    if (!nameRowDic.ContainsKey(name))
                    {
                        resultRow = tableResult.Rows.Add();
                        resultRow[0] = name;
                        nameRowDic.Add(name, tableResult.Rows.IndexOf(resultRow));
                    }
                    else
                        resultRow = tableResult.Rows[nameRowDic[name]];
                    resultRow[newColumn.ColumnName] = time;
                }    
            }

            TimeExcelUtil.CreateTimeExcel(savePath, tableResult, ExcelType.XlsxDocument);

        }
    }
}
