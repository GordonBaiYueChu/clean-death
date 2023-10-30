using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TuShan.BountyHunterDream.Logger;
using TuShan.CleanDeath.Helps;

namespace TuShan.CleanDeath.Views
{
    /// <summary>
    /// MyMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class MyMessageBox : Window
    {
        private MessageBoxResult messageBoxResult;

        public MyMessageBox()
        {
            InitializeComponent();
        }

        private string Message
        {
            get { return lblMsg.Text; }
            set { lblMsg.Text = value; }
        }
        private const int _maxNum = 160;
        public static MyMessageBox _lastMsg;
        public static MyMessageBox _montageMsg;
        private static bool _useEnglish;
        public static string _montageName = string.Empty;
        private static List<string> _montageNameList = new List<string>();
        static ButtonType _lastButtonType;
        static MessageType _lastMessageType;
        public static MessageBoxResult Show(string message, ButtonType buttontype, MessageType messatype, bool closeLast = true, int waitTimes = 0, List<string> montageNameList = null, bool useEnglish = false, Exception ex = null)
        {
            TLog.Info($"{message} {buttontype}");
            if (closeLast)
                CloseLastWithNegativeResult();
            var window = new MyMessageBox();
            _lastMsg = window;
            _lastButtonType = buttontype;
            _lastMessageType = messatype;
            var property = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
            if (property != null)
            {
                property.AddValueChanged(window.lblMsg, TextChangedHandler);
            }
            ConvertSizeToContent(window, (!string.IsNullOrEmpty(message)) && message.Length > _maxNum);
            window.lblMsg.Text = message;
            switch (buttontype)
            {
                case ButtonType.OKClose:
                    window.OKButton.Visibility = Visibility.Visible;
                    window.CancelButton.Visibility = Visibility.Collapsed;
                    window.YesButton.Visibility = Visibility.Collapsed;
                    window.NoButton.Visibility = Visibility.Collapsed;
                    window.CloseButton.Visibility = Visibility.Visible;
                    break;
                case ButtonType.OKCancel:
                    window.OKButton.Visibility = Visibility.Visible;
                    window.CancelButton.Visibility = Visibility.Visible;
                    window.YesButton.Visibility = Visibility.Collapsed;
                    window.NoButton.Visibility = Visibility.Collapsed;
                    window.CloseButton.Visibility = Visibility.Collapsed;
                    break;
                case ButtonType.YesNo:
                    window.OKButton.Visibility = Visibility.Collapsed;
                    window.CancelButton.Visibility = Visibility.Collapsed;
                    window.YesButton.Visibility = Visibility.Visible;
                    window.NoButton.Visibility = Visibility.Visible;
                    window.CloseButton.Visibility = Visibility.Collapsed;
                    break;
                case ButtonType.YesNoCancel:
                    window.OKButton.Visibility = Visibility.Collapsed;
                    window.CancelButton.Visibility = Visibility.Visible;
                    window.YesButton.Visibility = Visibility.Visible;
                    window.NoButton.Visibility = Visibility.Visible;
                    window.CloseButton.Visibility = Visibility.Collapsed;
                    break;
                case ButtonType.Close:
                    window.OKButton.Visibility = Visibility.Collapsed;
                    window.CancelButton.Visibility = Visibility.Collapsed;
                    window.YesButton.Visibility = Visibility.Collapsed;
                    window.NoButton.Visibility = Visibility.Collapsed;
                    window.CloseButton.Visibility = Visibility.Visible;
                    break;
                case ButtonType.OK:
                    window.OKButton.Visibility = Visibility.Visible;
                    window.CancelButton.Visibility = Visibility.Collapsed;
                    window.YesButton.Visibility = Visibility.Collapsed;
                    window.NoButton.Visibility = Visibility.Collapsed;
                    window.CloseButton.Visibility = Visibility.Collapsed;
                    break;
                case ButtonType.SaveAsSaveCancel:
                    window.SaveAsButton.Visibility = Visibility.Visible;
                    window.SaveButton.Visibility = Visibility.Visible;
                    window.CancelButton.Visibility = Visibility.Visible;
                    window.OKButton.Visibility = Visibility.Collapsed;
                    window.YesButton.Visibility = Visibility.Collapsed;
                    window.NoButton.Visibility = Visibility.Collapsed;
                    window.CloseButton.Visibility = Visibility.Collapsed;
                    break;
                case ButtonType.SaveAsCancel:
                    window.SaveAsButton.Visibility = Visibility.Visible;
                    window.CancelButton.Visibility = Visibility.Visible;
                    window.SaveAsButton.Margin = new Thickness(20, 0, 20, 0);
                    window.SaveButton.Visibility = Visibility.Collapsed;
                    window.OKButton.Visibility = Visibility.Collapsed;
                    window.YesButton.Visibility = Visibility.Collapsed;
                    window.NoButton.Visibility = Visibility.Collapsed;
                    window.CloseButton.Visibility = Visibility.Collapsed;
                    break;
            }

            switch (messatype)
            {
                case MessageType.Error:
                    window.errorPanel.Visibility = Visibility.Visible;
                    window.questionPanel.Visibility = Visibility.Collapsed;
                    window.warnPanel.Visibility = Visibility.Collapsed;
                    window.infoPanel.Visibility = Visibility.Collapsed;
                    window.successPanel.Visibility = Visibility.Collapsed;
                    break;
                case MessageType.Warn:
                    window.errorPanel.Visibility = Visibility.Collapsed;
                    window.questionPanel.Visibility = Visibility.Collapsed;
                    window.warnPanel.Visibility = Visibility.Visible;
                    window.infoPanel.Visibility = Visibility.Collapsed;
                    window.successPanel.Visibility = Visibility.Collapsed;
                    break;
                case MessageType.Question:
                    window.errorPanel.Visibility = Visibility.Collapsed;
                    window.questionPanel.Visibility = Visibility.Visible;
                    window.warnPanel.Visibility = Visibility.Collapsed;
                    window.infoPanel.Visibility = Visibility.Collapsed;
                    window.successPanel.Visibility = Visibility.Collapsed;
                    break;
                case MessageType.Info:
                    window.errorPanel.Visibility = Visibility.Collapsed;
                    window.questionPanel.Visibility = Visibility.Collapsed;
                    window.warnPanel.Visibility = Visibility.Collapsed;
                    window.infoPanel.Visibility = Visibility.Visible;
                    window.successPanel.Visibility = Visibility.Collapsed;
                    break;
                case MessageType.Success:
                    window.errorPanel.Visibility = Visibility.Collapsed;
                    window.questionPanel.Visibility = Visibility.Collapsed;
                    window.warnPanel.Visibility = Visibility.Collapsed;
                    window.infoPanel.Visibility = Visibility.Collapsed;
                    window.successPanel.Visibility = Visibility.Visible;
                    break;
            }
            if (waitTimes != 0)
            {
                //启动一个Timer
                MyTimer _timer = WaitRuningTimer;
                _timer.allTime = waitTimes;
                _timer.timerWindow = window;
                _timer.Start();
                window.ShowDialog();
                TLog.Info("User Checked");
                return window.messageBoxResult;
            }
            else
            {
                window.ShowDialog();
                TLog.Info("User Checked");
                return window.messageBoxResult;
            }
        }

        /// <summary>
        /// 每一秒执行一次的Timer
        /// </summary>
        public static MyTimer WaitRuningTimer
        {
            get
            {
                MyTimer t = new MyTimer() { Interval = 1000 };
                t.Elapsed += new ElapsedEventHandler(Tick);
                t.Enabled = true;
                t.AutoReset = true;
                return t;
            }
        }

        /// <summary>
        /// 带有自定义参数的System.Timers.Timer
        /// </summary>
        public class MyTimer : System.Timers.Timer
        {
            /// <summary>
            /// 所在窗体
            /// </summary>
            public MyMessageBox timerWindow { get; set; }

            /// <summary>
            /// 以等待时间
            /// </summary>
            public int realTime { get; set; }

            /// <summary>
            /// 总时间
            /// </summary>
            public int allTime { get; set; }
        }

        /// <summary>
        /// timer循环方法 (倒计时结束返回的是 ok)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Tick(object sender, ElapsedEventArgs e)
        {
            MyTimer _timer = (MyTimer)sender;
            _timer.timerWindow.lblMsg.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                _timer.realTime++;
                string oldMessage = _timer.timerWindow.lblMsg.Text;
                //更改展示的倒计时文本
                string oldTime = System.Text.RegularExpressions.Regex.Replace(oldMessage, @"[^0-9]+", "");
                string nowTime = (_timer.allTime - _timer.realTime).ToString();

                _timer.timerWindow.lblMsg.Text = oldMessage.Replace(oldTime, nowTime);
                if (_timer.realTime == _timer.allTime)
                {
                    _timer.realTime = 0;
                    _timer.timerWindow.messageBoxResult = MessageBoxResult.Yes;
                    _timer.Enabled = false;
                    _timer.timerWindow.Close();
                    _timer.Stop();
                    _timer.Close();
                    _timer.Dispose();
                }
            }));

        }

        public static (MyMessageBox msg, ButtonType buttonType) GetLastMsg()
        {
            if (_lastMsg == null)
                return (null, ButtonType.YesNo);
            else
                return (_lastMsg, _lastButtonType);
        }
        public static void CloseLastWithResult(MessageBoxResult result)
        {
            var last = GetLastMsg();
            if (last.msg == null)
                return;
            CloseMsgWithResult(last.msg, result);
        }
        private static void CloseMsgWithResult(MyMessageBox msg, MessageBoxResult result)
        {
            msg.Dispatcher.Invoke(() =>
            {
                msg.messageBoxResult = result;
                msg.Close();
            });
        }
        public static void CloseLastWithNegativeResult()
        {
            var last = GetLastMsg();
            if (last.msg == null)
                return;
            var result = MessageBoxResult.Cancel;
            switch (_lastButtonType)
            {
                case ButtonType.OKClose:
                    result = MessageBoxResult.None;
                    break;
                case ButtonType.OKCancel:
                    result = MessageBoxResult.Cancel;
                    break;
                case ButtonType.YesNoCancel:
                    result = MessageBoxResult.Cancel;
                    break;
                case ButtonType.YesNo:
                    result = MessageBoxResult.No;
                    break;
                case ButtonType.Close:
                    result = MessageBoxResult.None;
                    break;
                case ButtonType.OK:
                    result = MessageBoxResult.OK;
                    break;
                default:
                    break;
            }
            CloseMsgWithResult(last.msg, result);
        }

        private static void TextChangedHandler(object sender, EventArgs e)
        {
            TextBlock tbl = sender as TextBlock;
            MyMessageBox win = tbl.FindVisualParent<MyMessageBox>();
            if (win != null)
            {
                ConvertSizeToContent(win, (!string.IsNullOrEmpty(tbl.Text)) && tbl.Text.Length > _maxNum);
            }
        }
        private static void ConvertSizeToContent(MyMessageBox win, bool careWidth)
        {
            if (careWidth)
            {
                win.SizeToContent = SizeToContent.WidthAndHeight;
                win.MinWidth = 380;
                win.MaxWidth = 500;

            }
            else
            {
                win.SizeToContent = SizeToContent.Height;
                win.Width = 380;
            }
        }
        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            messageBoxResult = MessageBoxResult.Yes;
            Close();
        }

        private void NoButton_OnClick(object sender, RoutedEventArgs e)
        {
            messageBoxResult = MessageBoxResult.No;
            Close();
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            messageBoxResult = MessageBoxResult.OK;
            //ShutdownMode="OnMainWindowClose"时，当wpf只有一个窗体时，弹窗（任何窗口）的关闭会触发程序的关闭
            Close();
        }

        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            messageBoxResult = MessageBoxResult.Yes;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            messageBoxResult = MessageBoxResult.Cancel;
            Close();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            messageBoxResult = MessageBoxResult.None;
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (OKButton.Visibility == Visibility.Visible)
                    OKButton_OnClick(this, new RoutedEventArgs());
                if (YesButton.Visibility == Visibility.Visible)
                    YesButton_OnClick(this, new RoutedEventArgs());

            }
            else if (e.Key == Key.Escape)
            {
                if (CancelButton.Visibility == Visibility.Visible)
                    CancelButton_OnClick(this, new RoutedEventArgs());
                if (NoButton.Visibility == Visibility.Visible)
                    NoButton_OnClick(this, new RoutedEventArgs());
                if (CloseButton.Visibility == Visibility.Visible)
                    CloseButton_OnClick(this, new RoutedEventArgs());
            }

        }




    }
    public enum MessageType
    {
        Error,
        Question,
        Warn,
        Info,
        Success
    }

    public enum ButtonType
    {
        OKClose = 0,
        OKCancel = 1,
        YesNoCancel = 3,
        YesNo = 4,
        Close,
        OK,
        SaveAsSaveCancel,
        SaveAsCancel
    }
}
