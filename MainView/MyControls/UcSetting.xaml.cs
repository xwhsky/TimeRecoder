using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro;
using MahApps.Metro.Controls;
using EngineUtil;
using System.ComponentModel;

namespace MainView.MyControls
{
    /// <summary>
    /// UcSetting.xaml 的交互逻辑
    /// </summary>
    public partial class UcSetting
    {
        public UcSetting()
        {
            InitializeComponent();
            TheCommunity.BindCommandsAndTools(UcSettingGrid);
            TheCommunity.BindCommandsAndTools(BtnLogin);
            TheCommunity.BindCommandsAndTools(BtnRegister);

            InitAppearanceTiles();

            UpdateListFeeds();
        }

        private void InitAppearanceTiles()
        {
            foreach (var accent in ThemeManager.Accents)
            {
                var tile = new Tile()
                {
                    Title = accent.Name,
                    Width = 100,
                    Height = 75,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(accent.Resources["AccentColor"].ToString()))
                };
                var accent1 = accent;
                tile.Click +=
                    (s, e) =>
                    {
                        TheUniversal.TheCurrentConfig.TheCurrentAccent = accent1;
                        ThemeManager.ChangeAppStyle(Application.Current, TheUniversal.TheCurrentConfig.TheCurrentAccent,
                            TheUniversal.TheCurrentConfig.TheCurrentTheme);
                    };

                WpColor.Children.Add(tile);
            }

            foreach (var theme in ThemeManager.AppThemes)
            {
                
                var tile = new Tile()
                {
                    Title = theme.Name,
                    Width = 100,
                    Height = 75,
                };
                var theme1 = theme;
                tile.Click +=
                    (s, e) =>
                    {
                        TheUniversal.TheCurrentConfig.TheCurrentTheme = theme1;
                        ThemeManager.ChangeAppStyle(Application.Current, TheUniversal.TheCurrentConfig.TheCurrentAccent,
                            TheUniversal.TheCurrentConfig.TheCurrentTheme);
                    };

                WpTheme.Children.Add(tile);
            }
        }

        private void UpdateListFeeds()
        {
            
            var feeds = new System.Collections.Generic.List<string>();

            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                while (TheUniversal.TheCurrentTimeDb == null || !TheUniversal.TheCurrentTimeDb.IsOpen)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                foreach (var ss in TheCommunity.GetLatestFeedback(20))
                {
                    feeds.Add(string.Format("[{0}] {1}:   {2}", ss.Item3, ss.Item1, ss.Item2));
                }
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                LvFeedbacks.ItemsSource = feeds;
            };

            worker.RunWorkerAsync();
        }


        //回车发送信息
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var tb = ((System.Windows.Controls.TextBox)sender);
                if (TheUniversal.TheCurrentUser.UserType == UserType.UnRegister)
                {
                }
                else
                {
                    TheCommunity.SendFeedback(TheUniversal.TheCurrentUser.UserName,tb.Text);
                    UpdateListFeeds();
                    tb.Text = string.Empty;
                }
            } 
        }

    }
}
