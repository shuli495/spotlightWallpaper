using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace spotlightWallpaper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;

        private String configStartWith = "True";    //开机启动
        private String configSavePath = "";         //壁纸保存路径
        private String configChangeTime = "24";     //壁纸更换时间
        private String configWallName = "";         //当前壁纸名

        private String showImageHead = "showImage_";//图片预览子控件名称前缀
        private int showImageWidth = 108;           //图片预览子控件宽度
        private int showImageHeight = 61;           //图片预览子控件高度

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        private static extern int SystemParametersInfo(
            int uAction,
            int uParam,
            string lpvParam,
            int fuWinIni
        );

        public MainWindow()
        {
            InitializeComponent();

            this.initConfig();

            // 设置值
            this.savePathBox.Text = configSavePath;
            this.changeTimeBox.Text = configChangeTime;
            this.runChk.IsChecked = Boolean.Parse(configStartWith);

            // 已经选择了壁纸路径，设置托盘运行
            if (!"".Equals(this.savePathBox.Text))
            {
                this.HidenWindow();
            }

            // 复制聚焦壁纸
            this.copyWallpaper(true);

            // 检测开机启动
            this.runChk_Click();

            // 6小时同步一次壁纸
            DispatcherTimer synTimer = new DispatcherTimer();
            synTimer.Tick += new EventHandler(syn);
            synTimer.Interval = new TimeSpan(6, 0, 0);
            synTimer.Start();

            // this.changeTimeBox.Text小时切换一次壁纸
            DispatcherTimer changeTimer = new DispatcherTimer();
            changeTimer.Tick += new EventHandler(changeWallpaper);
            changeTimer.Interval = new TimeSpan(int.Parse(this.changeTimeBox.Text), 0, 0);
            changeTimer.Start();

            GC.Collect();
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        private void initConfig()
        {
            string exePath = this.GetType().Assembly.Location;
            string configPath = exePath.Replace(".exe", ".config");

            // 配置文件不存在重新生成
            if (!File.Exists(configPath))
            {
                XDocument document = new XDocument();
                XElement root = new XElement("configuration");
                root.SetElementValue("startWith", configStartWith);
                root.SetElementValue("savePath", configSavePath);
                root.SetElementValue("changeTime", configChangeTime);
                root.SetElementValue("wallName", configWallName);
                root.Save(configPath);
            }
            else
            {
                XDocument document = XDocument.Load(configPath);
                XElement root = document.Root;
                XElement startWith = root.Element("startWith");
                configStartWith = startWith.Value;
                XElement savePath = root.Element("savePath");
                configSavePath = savePath.Value;
                XElement changeTime = root.Element("changeTime");
                configChangeTime = changeTime.Value;
                XElement wallName = root.Element("wallName");
                configWallName = wallName.Value;
            }
        }

        /// <summary>
        /// 更新配置文件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void setConfig(string key, string value)
        {
            string exePath = this.GetType().Assembly.Location;
            string configPath = exePath.Replace(".exe", ".config");

            XDocument document = XDocument.Load(configPath);
            XElement root = document.Root;
            XElement config = root.Element(key);
            config.SetValue(value);
            document.Save(configPath);
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowWindow(object sender, EventArgs e)
        {
            // 显示图片预览
            this.showImages();

            this.notifyIcon.Visible = false;    //托盘
            this.ShowInTaskbar = true;          //任务栏
            this.Activate();                    //窗口活动
            this.Show();                        //显示窗口
            this.WindowState = System.Windows.WindowState.Normal;   //窗口状态
        }

        /// <summary>
        /// 最小化隐藏至托盘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
            {
                this.HidenWindow();
            }
        }

        /// <summary>
        /// 隐藏至托盘
        /// </summary>
        private void HidenWindow()
        {
            this.WindowState = System.Windows.WindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();    //隐藏窗口

            if(null == this.notifyIcon)
            {
                this.notifyIcon = new NotifyIcon();
                this.notifyIcon.Text = "聚焦壁纸";  //鼠标悬停提示
                this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);  //程序默认图标
            }

            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.ShowWindow(o, e);
            });
        }

        /// <summary>
        /// 显示图片预览
        /// </summary>
        private void showImages()
        {
            new Thread(new ThreadStart(() => {
                GC.Collect();

                string savePath = "";
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    savePath = this.savePathBox.Text;
                }));

                if (!"".Equals(savePath) && Directory.Exists(savePath))
                {
                    DirectoryInfo TheFolder = new DirectoryInfo(savePath);

                    // 只显示jpg格式壁纸，即横版壁纸
                    FileInfo[] jpgs = TheFolder.GetFiles("*.jpg");

                    // 展示已复制壁纸的数据源
                    List<System.Windows.Controls.Image> showImageLists = new List<System.Windows.Controls.Image>();
                    foreach (FileInfo NextFile in jpgs)
                    {
                        try
                        {
                            // 释放图像资源，节省系统RAM
                            BitmapImage image = new BitmapImage();
                            using (var stream = new FileStream(NextFile.FullName, FileMode.Open))
                            {
                                image.BeginInit();
                                image.StreamSource = stream;

                                image.DecodePixelWidth = this.showImageWidth;
                                image.DecodePixelHeight = this.showImageHeight;

                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.EndInit();
                                image.Freeze();
                            }

                            // 生成图像控件
                            App.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                System.Windows.Controls.Image showImage = new System.Windows.Controls.Image();
                                showImage.Width = this.showImageWidth;
                                showImage.Height = this.showImageHeight;
                                showImage.Source = image;
                                showImage.Name = this.showImageHead + NextFile.Name.Replace(".jpg","");
                                showImageLists.Add(showImage);
                            }));
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    // 绑定数据源
                    App.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        this.showLBox.ItemsSource = showImageLists;
                    }));
                }

                GC.Collect();
            })).Start();
        }

        /// <summary>
        /// 选择壁纸自动换壁纸
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showLBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                string wallName = ((System.Windows.Controls.Image)e.AddedItems[0]).Name.Replace(this.showImageHead, "") + ".jpg";
                this.changeWallpaperByName(wallName);
            }
            catch
            {
            }

        }

        /// <summary>
        /// 复制聚焦壁纸
        /// </summary>
        /// <returns></returns>
        private void copyWallpaper(Boolean isChangeWall)
        {
            new Thread(new ThreadStart(() => {
                string savePath = "";
                Dispatcher.Invoke(new Action(delegate
                {
                    savePath = this.savePathBox.Text;
                }));
                if (savePath.Equals(""))
                {
                    return;
                }

                Dispatcher.Invoke(new Action(delegate
                {
                    this.synBut.Content = "同步中";
                    this.synBut.IsEnabled = false;
                }));

                // 目录不存在则创建
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                //聚焦壁纸保存目录
                string path = Environment.GetEnvironmentVariable("LOCALAPPDATA") + "\\Packages\\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\\LocalState\\Assets";

                //要更换的壁纸名称
                string wallName = "";

                DirectoryInfo TheFolder = new DirectoryInfo(path);
                foreach (FileInfo NextFile in TheFolder.GetFiles())
                {
                    string newName = NextFile.Name;

                    // 设置壁纸扩展名 横版为jpg 竖版为jpeg
                    Image image = Image.FromFile(path + "\\" + newName);
                    if (image.Width > image.Height)
                    {
                        newName += ".jpg";
                    }
                    else
                    {
                        newName += ".jpeg";
                    }

                    // 大于200KB的保存
                    string newFile = savePath + "\\" + newName;
                    if (NextFile.Length / 1024 > 200)
                    {
                        if (!File.Exists(newFile))
                        {
                            NextFile.CopyTo(newFile);

                            if (image.Width > image.Height)
                            {
                                wallName = newName;
                            }
                        }
                    }
                }

                Dispatcher.Invoke(new Action(delegate
                {
                    this.synBut.Content = "立即同步";
                    this.synBut.IsEnabled = true;
                }));

                // 同步后切换最新壁纸
                if ((Boolean)isChangeWall)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        this.changeWallpaperByName(wallName);
                    }));
                }

                // 显示图片预览
                Dispatcher.Invoke(new Action(delegate
                {
                    if (this.WindowState != System.Windows.WindowState.Minimized)
                    {
                        this.showImages();
                    }
                }));

                GC.Collect();
            })).Start();
        }

        /// <summary>
        /// 选择壁纸保存路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePathBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string nowSavePath = this.savePathBox.Text;

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择壁纸存放路径";
            object isOk = dialog.ShowDialog();

            if (isOk.ToString().Equals("OK"))
            {
                string newPath = dialog.SelectedPath;
                this.savePathBox.Text = newPath;

                // 目录改变，移动已保存的壁纸
                if (!nowSavePath.Equals("") && !nowSavePath.Equals(newPath))
                {
                    DirectoryInfo TheFolder = new DirectoryInfo(nowSavePath);
                    foreach (FileInfo NextFile in TheFolder.GetFiles())
                    {
                        NextFile.MoveTo(newPath + NextFile.FullName);
                    }

                    Directory.Delete(nowSavePath, true);
                }

                this.setConfig("savePath", newPath);
            }
            // 复制聚焦壁纸
            this.copyWallpaper(true);
        }

        /// <summary>
        /// 壁纸切换频率只能数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeTimeBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                (e.Key >= Key.D0 && e.Key <= Key.D9) ||
                e.Key == Key.Back ||
                e.Key == Key.Left || e.Key == Key.Right)
            {
                if (e.KeyboardDevice.Modifiers != ModifierKeys.None)
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 保存壁纸切换频率设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeTimeBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            this.setConfig("changeTime", this.changeTimeBox.Text);
        }

        /// <summary>
        /// 后台定时同步壁纸
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void syn(object sender, EventArgs e)
        {
            this.copyWallpaper(false);
        }

        /// <summary>
        /// 切换壁纸
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeWallpaper(object sender, EventArgs e)
        {
            string savePath = this.savePathBox.Text;
            if (!"".Equals(savePath) && Directory.Exists(savePath))
            {
                DirectoryInfo TheFolder = new DirectoryInfo(savePath);

                // 是否找到当前壁纸，找到切换下一张
                Boolean isFind = false;
                // 只切换jpg格式壁纸，即横版壁纸
                FileInfo[] jpgs = TheFolder.GetFiles("*.jpg");

                foreach (FileInfo NextFile in jpgs)
                {
                    if(isFind)
                    {
                        this.changeWallpaperByName(NextFile.FullName);
                        return;
                    }

                    if(NextFile.FullName.Equals(this.configWallName))
                    {
                        isFind = true;
                    }
                }

                // 没找到壁纸 或 最好一张壁纸是当前壁纸，切换第一张壁纸
                this.changeWallpaperByName(jpgs[0].Name);
            }
        }

        /// <summary>
        /// 按名称换壁纸
        /// </summary>
        /// <param name="wallName"></param>
        private void changeWallpaperByName(string wallName)
        {
            string savePath = this.savePathBox.Text;

            if (!"".Equals(savePath) && !"".Equals(wallName))
            {
                // 壁纸只能是bmp格式，保存临时bmp
                string tempFile = savePath + "\\wallpaper.bmp";
                Image image = Image.FromFile(savePath + "\\" + wallName);
                image.Save(tempFile, System.Drawing.Imaging.ImageFormat.Bmp);

                // 设置壁纸
                SystemParametersInfo(20, 1, tempFile, 1);

                // 保存当前壁纸名称
                this.setConfig("wallName", wallName);
            }
        }

        /// <summary>
        /// 立即同步壁纸按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void synBut_Click(object sender, RoutedEventArgs e)
        {
            this.copyWallpaper(true);
        }

        /// <summary>
        /// 开机运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runChk_Click(object sender, RoutedEventArgs e)
        {
            this.runChk_Click();
        }

        /// <summary>
        /// 开机运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runChk_Click()
        {
            string productName = Process.GetCurrentProcess().ProcessName;   //程序名称
            string StartupPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonStartup);  //系统启动路径
            string lnkName = StartupPath + "\\" + productName + ".lnk";

            if (this.runChk.IsChecked == true)
            {
                // 生成快捷方式
                if (!File.Exists(lnkName))
                {
                    string exePath = this.GetType().Assembly.Location; // 程序路径

                    IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                    IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(lnkName);
                    shortcut.TargetPath = exePath;          //关联程序
                    shortcut.IconLocation = exePath + ", 0";//快捷方式图表
                    shortcut.Save();
                }
            }
            else
            {
                // 取消开机启动
                System.IO.File.Delete(lnkName);
            }

            // 保存设置
            this.setConfig("startWith", (Boolean)this.runChk.IsChecked?"True":"False");
        }

        /// <summary>
        /// 主页超链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void indexLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.pighand.com/spotlightWallpaper/");  
        }

        /// <summary>
        /// 关闭窗口提示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ("OK".Equals(System.Windows.Forms.MessageBox.Show("确定退出吗?", "spotlightWallpaper", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2).ToString()))
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
