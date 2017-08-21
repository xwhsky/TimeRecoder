using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;

namespace MainView.Utility
{
    /// <summary>
    /// 软件版本
    /// </summary>
    public class TheVersion
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public float VersionNum { get; set; }

        /// <summary>
        /// 版本新特征
        /// </summary>
        public List<string> NewFeatures { get; set; }

        public string NewFeatureStr()
        {
            var res = new StringBuilder();
            foreach (var feature in NewFeatures)
                res.AppendLine(feature);
            return res.ToString();
        }

        public TheVersion()
        {
            NewFeatures = new List<string>();
        }



    }

    /// <summary>
    /// 自动更新类
    /// </summary>
    public class AutoUpdate
    {
        private readonly string _updateFolder;

        public AutoUpdate(string updateFolder)
        {
            _updateFolder = updateFolder;
        }

        public bool NeedUpdate(out TheVersion newVersion)
        {
            return TheCommunity.CheckForNewVersion(out newVersion);
        }


        /// <summary>
        /// 解压安装包
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newVersion"></param>
        public void UnZip(string path, TheVersion newVersion)
        {
            TheCommunity.CheckForNewVersion(out newVersion);
            var latestzip =
                Directory.GetFiles(_updateFolder)
                    .SingleOrDefault(_ => _.EndsWith(string.Format("Time_Version{0}.zip", newVersion.VersionNum)));

            Directory.SetCurrentDirectory(path);

            if (latestzip != null)
            {
                using (var fs = File.OpenRead(latestzip))
                {
                    using (var s = new ZipInputStream(fs))
                    {
                        ZipEntry theEntry;
                        while ((theEntry = s.GetNextEntry()) != null)
                        {
                            string directoryName = Path.GetDirectoryName(theEntry.Name);
                            string fileName = Path.GetFileName(theEntry.Name);

                            if (directoryName.Length > 0)
                                Directory.CreateDirectory(directoryName);

                            if (fileName != string.Empty)
                            {
                                using (var streamWriter = File.Create(theEntry.Name))
                                {
                                    var size = 2048;
                                    var data = new byte[2048];
                                    while (true)
                                    {
                                        size = s.Read(data, 0, data.Length);
                                        if (size > 0)
                                        {
                                            streamWriter.Write(data, 0, size);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="path"></param>
        private void UpdateMe(string path)
        {
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = string.Format("{0}Update.exe", AppDomain.CurrentDomain.BaseDirectory),
                    Arguments = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture),
                    //WindowStyle = ProcessWindowStyle.Hidden,
                }
            };

            try
            {
                TheUniversal.TheCurrentTimeDb.Dispose();
            }
            catch
            {
            }
            finally
            {
                p.Start();
                Application.Current.Shutdown();
            }
            
        }

        public void Update(TheVersion newVersion)
        {
            //设置解压路径
            var extractPath = string.Format("{0}TMPDATA", AppDomain.CurrentDomain.BaseDirectory);
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath);
            Directory.CreateDirectory(extractPath);

            //解压缩
            UnZip(extractPath, newVersion);
            //升级
            UpdateMe(extractPath);
        }
    }
}
