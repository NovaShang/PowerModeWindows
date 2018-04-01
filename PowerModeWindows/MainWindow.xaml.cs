using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Interop;

namespace PowerModeWindows
{
    /// <summary>
    /// MainWindow.xaml 得逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region 导入需要使用的WindowsAPI

        /// <summary>
        /// 用来适配windowsAPI的点数据类型
        /// </summary>
        public struct IntPoint
        {
            public int x;
            public int y;
            public override string ToString()
            {
                return $"{x},{y}";
            }
            public Point ToPoint()
            {
                return new Point(x, y);
            }
        }

        /// <summary>
        /// 该委托用来接受键盘Hook
        /// </summary>
        /// <param name="code"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate int KeyboardHookProc(int code, int wParam, ref KeyboardHookStruct lParam);

        /// <summary>
        /// 键盘Hook的参数
        /// </summary>
        public struct KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        public enum GWL : int
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }

        /// <summary>
        /// 窗口是ToolWindow
        /// </summary>
        const long WS_EX_TOOLWINDOW = 0x00000080L;

        /// <summary>
        /// 窗口是透明窗口
        /// </summary>
        const long WS_EX_TRANSPARENT = 0x00000020L;

        /// <summary>
        /// 窗口永远在最前
        /// </summary>
        const long WS_EX_TOPMOST = 0x00000008L;

        /// <summary>
        /// 一种键盘HOOK模式
        /// </summary>
        const int WH_KEYBOARD_LL = 13;


        /// <summary>
        /// 表示按键“按下”
        /// </summary>
        const int WM_KEYDOWN = 0x100;

        /// <summary>
        /// 将一个线程的输入处理机制附加或分离为另一个线程的输入处理机制。
        /// </summary>
        /// <param name="idAttach"></param>
        /// <param name="idAttachTo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool value);

        /// <summary>
        /// 获取当前的前台窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// 获取当前的线程的ID
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThreadId();

        /// <summary>
        /// 获取指定窗口的进程ID和线程ID
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lpdwProcessId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindowThreadProcessId(IntPtr hwnd, ref IntPtr lpdwProcessId);

        /// <summary>
        /// 获取当前具有键盘焦点的窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetFocus();

        /// <summary>
        /// 获取光标位置
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern int GetCaretPos(ref IntPoint pt);

        /// <summary>
        /// 将窗口坐标变换为屏幕坐标
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hwnd, ref IntPoint pt);

        /// <summary>
        /// 创建HOOK
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="callback"></param>
        /// <param name="hInstance"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookProc callback, IntPtr hInstance, uint threadId);

        /// <summary>
        /// 取消HOOK
        /// </summary>
        /// <param name="hInstance"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /// <summary>
        /// 调用下一个HOOK
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookStruct lParam);

        /// <summary>
        /// 将指定的模块加载到调用进程的地址空间中
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);


        /// <summary>
        /// 获取窗口属性
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex);

        /// <summary>
        /// 设置窗口属性
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong);

        /// <summary>
        /// 设置窗口分层参数
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="crKey"></param>
        /// <param name="bAlpha"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [DllImport("user32")]
        private static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, int bAlpha, int dwFlags);

        #endregion


        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                Init();
            };
        }

        /// <summary>
        /// 连击次数
        /// </summary>
        public int Count
        {
            get => _count; set
            {
                _count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            }
        }

        /// <summary>
        /// 连击文字颜色
        /// </summary>
        public SolidColorBrush ComboColor
        {
            get => _comboColor; set
            {
                _comboColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ComboColor"));
            }
        }

        DispatcherTimer _timer;

        /// <summary>
        /// 用来储存回调的委托，防止被GC
        /// </summary>
        KeyboardHookProc _hookCallback;

        /// <summary>
        /// 绘图的容器
        /// </summary>
        Canvas _canvas;

        /// <summary>
        /// 连击文字颜色
        /// </summary>
        SolidColorBrush _comboColor;
        /// <summary>
        /// 色相
        /// </summary>
        double _hue = 120;
        double _saturation = 0.65;
        double _value = 0.82;
        double _hueStart = 120;
        double _hueStep = -0.5;
        double _hueEnd = 0;

        /// <summary>
        /// 可用的粒子
        /// </summary>
        ConcurrentStack<Ellipse> _freeObjects = new ConcurrentStack<Ellipse>();

        /// <summary>
        /// Hook的指针，退出时使用该指针取消hook
        /// </summary>
        IntPtr _hook = IntPtr.Zero;

        /// <summary>
        /// Combo计数
        /// </summary>
        int _count = 0;

        /// <summary>
        /// 属性改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            _canvas = canvas as Canvas;

            //兼容Win10多桌面,并且鼠标穿透
            var hwnd = new WindowInteropHelper(this).Handle;
            var style = GetWindowLongPtr(hwnd, GWL.GWL_EXSTYLE);
            style = new IntPtr(style.ToInt64() | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT | WS_EX_TOPMOST);
            SetWindowLongPtr(hwnd, GWL.GWL_EXSTYLE, style);

            //兼容多显示器
            System.Drawing.Rectangle totalSize = System.Drawing.Rectangle.Empty;
            foreach (Screen s in Screen.AllScreens)
                totalSize = System.Drawing.Rectangle.Union(totalSize, s.Bounds);
            Width = totalSize.Width;
            Height = totalSize.Height;
            Left = totalSize.Left;
            Top = totalSize.Top;

            //创建粒子池
            for (int i = 0; i < Properties.Settings.Default.MaxNum; i++)
            {
                var particle = new Ellipse()
                {
                    Visibility = Visibility.Visible,
                    Height = 2,
                    Width = 2,
                    IsHitTestVisible = false,
                };
                _freeObjects.Push(particle);
                _canvas.Children.Add(particle);
            }

            ComboColor = new SolidColorBrush(ColorFromHSV(_hue, _saturation, _value));

            //设置计时器，用以重置COMBO
            _timer = new DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.Background, ResetCombo, Dispatcher);

            //使用类的成员变量储存该委托，防止被GC
            _hookCallback = HookProc;
            IntPtr hInstance = LoadLibrary("User32");

            //创建键盘HOOK
            _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallback, hInstance, 0);
        }

        /// <summary>
        /// 重置Combo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetCombo(object sender, EventArgs e)
        {
            Count = 0;
            _hue = _hueStart;
            ComboColor = new SolidColorBrush(ColorFromHSV(_hue, _saturation, _value));
        }

        /// <summary>
        /// Hook的响应函数
        /// </summary>
        /// <param name="code"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public int HookProc(int code, int wParam, ref KeyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                if (wParam == WM_KEYDOWN)
                {
                    KeyPressed(lParam.vkCode);
                }
            }
            return CallNextHookEx(_hook, code, wParam, ref lParam);
        }


        /// <summary>
        /// 每次按键后执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyPressed(int keyCode)
        {
            var windowHwnd = GetForegroundWindow();
            var threadId1 = GetCurrentThreadId();
            var processId = IntPtr.Zero;
            var threadId2 = GetWindowThreadProcessId(windowHwnd, ref processId);
            if (threadId1 != threadId2)
            {
                AttachThreadInput(threadId1, threadId2, true);
            }
            var hwdtxt = GetFocus();
            var postion = new IntPoint();
            GetCaretPos(ref postion);
            ClientToScreen(hwdtxt, ref postion);
            var localPos = postion.ToPoint() - new Vector(Left, Top);
            SpawnParticleGroup(localPos);
            if (threadId1 != threadId2)
            {
                AttachThreadInput(threadId1, threadId2, false);
            }
            Count++;


            if (!(Math.Abs(_hue - _hueEnd) <=Math.Abs(_hueStep)))
            {
                _hue += _hueStep;

                if (_hue > 360)
                {
                    _hue -= 360;
                }
                if (_hue < 0)
                {
                    _hue += 360;
                }
                ComboColor = new SolidColorBrush(ColorFromHSV(_hue, _saturation, _value));
            }

            _timer.Stop();
            _timer.Start();
            var storyBoard = Resources["KeyStrockAniation"] as Storyboard;
            storyBoard.Stop();
            storyBoard.Begin();
        }

        /// <summary>
        /// 析构函数，确保钩子被取消
        /// </summary>
        ~MainWindow()
        {
            UnhookWindowsHookEx(_hook);
        }

        /// <summary>
        /// 生成一组粒子特效
        /// </summary>
        /// <param name="position"></param>
        private void SpawnParticleGroup(Point position)
        {
            //该storyboard包含本组特效的所有动画
            var storyboard = new Storyboard();
            var rand = new Random();

            //暂存本组特效中用到的粒子，使用闭包特性保留到动画播放完成
            var particles = new Stack<Ellipse>();

            //为每个粒子创建特效
            for (int i = 0; i < Properties.Settings.Default.NumPerGroup; i++)
            {

                //试图从池里拿闲置的粒子
                if (!_freeObjects.TryPop(out var particle))
                    break;

                //暂存到本组粒子的列表中
                particles.Push(particle);

                //设置粒子的各种属性
                particle.Opacity = 1;
                particle.Visibility = Visibility.Visible;
                Canvas.SetLeft(particle, position.X);
                Canvas.SetTop(particle, position.Y);
                particle.Fill = new SolidColorBrush() { Color = Color.FromRgb((byte)(rand.Next() * 255), (byte)(rand.Next() * 255), (byte)(rand.Next() * 255)) };

                //创建变换组，包括缩放和移动变换
                var transGroup = new TransformGroup()
                {
                    Children = new TransformCollection(new Transform[] {
                        new ScaleTransform(1,1),
                        new TranslateTransform(0,0)
                    })
                };
                particle.RenderTransform = transGroup;

                var duration = rand.NextDouble() * 500 + 500;

                //垂直运动动画
                var animationY = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(duration),
                    From = 0,
                    To = GetNormalRandom(rand) * 10 - 30,
                    EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTargetProperty(animationY, new PropertyPath("RenderTransform.Children[1].Y"));
                Storyboard.SetTarget(animationY, particle);
                storyboard.Children.Add(animationY);

                //水平运动动画
                var animationX = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(duration),
                    From = 0,
                    To = 40 * GetNormalRandom(rand),
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTargetProperty(animationX, new PropertyPath("RenderTransform.Children[1].X"));
                Storyboard.SetTarget(animationX, particle);
                storyboard.Children.Add(animationX);

                //缩放动画
                var scale = (Math.Pow(rand.NextDouble(), 2) + 1) * 2;
                var animationScaleX = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(duration),
                    From = 1,
                    To = scale,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }

                };
                var animationScaleY = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(duration),
                    From = 1,
                    To = scale,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }

                };
                Storyboard.SetTargetProperty(animationScaleX, new PropertyPath("RenderTransform.Children[0].ScaleX"));
                Storyboard.SetTargetProperty(animationScaleY, new PropertyPath("RenderTransform.Children[0].ScaleY"));
                Storyboard.SetTarget(animationScaleX, particle);
                Storyboard.SetTarget(animationScaleY, particle);
                storyboard.Children.Add(animationScaleX);
                storyboard.Children.Add(animationScaleY);

                //透明度动画
                var animationOpacity = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(duration),
                    From = 1,
                    To = 0
                };
                Storyboard.SetTargetProperty(animationOpacity, new PropertyPath("Opacity"));
                Storyboard.SetTarget(animationOpacity, particle);
                storyboard.Children.Add(animationOpacity);
            }

            //当动画播放完成后将粒子放回池中
            storyboard.Completed += (s, e) => _freeObjects.PushRange(particles.ToArray());

            //开始播放动画
            storyboard.Begin();
        }

        /// <summary>
        /// 生成正太分布的随机数
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        private double GetNormalRandom(Random rand)
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randNormal = 1 * randStdNormal;
            return randNormal;
        }

        /// <summary>
        /// 从HSV转为RGB
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }
}
