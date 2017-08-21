//Summary
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace EngineUtil
{
    /// <summary>
    /// 账户类型
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// 未注册（仅限游览）
        /// </summary>
        UnRegister = -1,
        /// <summary>
        /// 管理员用户（所有操作）
        /// </summary>
        Administrator = 0,
        
        /// <summary>
        /// 记录用户 （仅限添加）
        /// </summary>
        Recoder = 1,
        
        /// <summary>
        /// 游客用户（仅限浏览）
        /// </summary>
        Guider = 2
    }

    /// <summary>
    /// 数据库集成模块
    /// </summary>
    public class TimeDbUtil:IDisposable
    {
        
        private readonly OleDbConnection _con;

        private string _connectionString;
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectString
        {
            get { return _connectionString; }
        }


        private bool _isOpen;
        /// <summary>
        /// 数据库是否打开
        /// </summary>
        public bool IsOpen {get { return _isOpen; }}


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="urlPath">ACCESS文件路径</param>
        /// <param name="psd">文件密码</param>
        public TimeDbUtil(string urlPath, string psd)
        {
            InitializeConnection(urlPath, psd);
            _con = new OleDbConnection(_connectionString) ;
            
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            try
            {
                _con.Open();
            }
            catch (Exception ex)
            {
                _isOpen = false;
                return false;
            }

            _isOpen = true;
            return true;

        }

        /// <summary>
        /// 查询表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable GetTable(string sql)
        {
            using (var cmd = new OleDbCommand { Connection = _con, CommandText = sql })
            {
                using (var da = new OleDbDataAdapter { SelectCommand = cmd })
                {
                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds.Tables[0];
                }
            }
        }

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool ExecuteSql(params string[] sql)
        {
            using (var tran = _con.BeginTransaction())
            {
                try
                {
                    using (var cmd = new OleDbCommand {Connection = _con, Transaction = tran})
                    {
                        foreach (var state in sql)
                        {
                            cmd.CommandText = state;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return false;
                }

                tran.Commit();
                return true;
            }


        }

        void InitializeConnection(string urlPath, string psd)
        {
            if (string.IsNullOrEmpty(psd))
            {
                _connectionString =
                    string.Format(@"Provider = Microsoft.ACE.OLEDB.12.0;Data Source={0};Persist Security Info=False", urlPath);
            }
            else
            {
                _connectionString =
                string.Format(@"Provider = Microsoft.ACE.OLEDB.12.0;Data Source={0};Jet OLEDB:Database Password='{1}'",
                    urlPath, psd);
            }
            
        }

        public void Dispose()
        {
            _con.Dispose();
            _isOpen = false;
        }
    }
}
