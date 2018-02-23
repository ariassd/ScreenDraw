using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ScreenDraw
{
	internal enum AccentState
	{
		ACCENT_DISABLED = 0,
		ACCENT_ENABLE_GRADIENT = 1,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_INVALID_STATE = 4
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AccentPolicy
	{
		public AccentState AccentState;
		public int AccentFlags;
		public int GradientColor;
		public int AnimationId;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WindowCompositionAttributeData
	{
		public WindowCompositionAttribute Attribute;
		public IntPtr Data;
		public int SizeOfData;
	}

	internal enum WindowCompositionAttribute
    { 
		WCA_ACCENT_POLICY = 3
	}

	public partial class MainWindow : Window
	{
        System.Windows.Point currentPoint = new System.Windows.Point();
        System.Windows.Media.Color colorLine = Colors.Black;

        [DllImport("user32.dll")]
		internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

		public MainWindow()
		{
			InitializeComponent();

        }
		
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			EnableBlur();
		}
		
		internal void EnableBlur()
		{
			var windowHelper = new WindowInteropHelper(this);
			
			var accent = new AccentPolicy();
			accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

			var accentStructSize = Marshal.SizeOf(accent);

			var accentPtr = Marshal.AllocHGlobal(accentStructSize);
			Marshal.StructureToPtr(accent, accentPtr, false);

			var data = new WindowCompositionAttributeData();
			data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
			data.SizeOfData = accentStructSize;
			data.Data = accentPtr;
			
			SetWindowCompositionAttribute(windowHelper.Handle, ref data);

			Marshal.FreeHGlobal(accentPtr);
		}

        private void canv_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                currentPoint = e.GetPosition(this);
        }

        private void canv_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();
                SolidColorBrush colorBrush = new SolidColorBrush();
                colorBrush.Color = colorLine;
                line.Stroke = colorBrush;
                line.StrokeThickness = 2;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(this).X;
                line.Y2 = e.GetPosition(this).Y;
                currentPoint = e.GetPosition(this);
                canv.Children.Add(line);
            }
        }

        private Bitmap CaptureScreen()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                   Screen.PrimaryScreen.Bounds.Height,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                    Screen.PrimaryScreen.Bounds.Y,
                    0,
                    0,
                    Screen.PrimaryScreen.Bounds.Size,
                    CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void cp_ChangeColor(object sender, RoutedEventArgs e)
        {
            //Colors
            //https://www.timvandevall.com/info/rgb-color-wheel-hex-values-printable-blank-color-wheel-templates/
            System.Windows.Media.Brush brush = ((System.Windows.Controls.Button)e.OriginalSource).Background;
            colorLine = ((SolidColorBrush)brush).Color;
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            stakPControls.Visibility = Visibility.Hidden;
            btnClose.Visibility = Visibility.Hidden;
            btnSave.Visibility = Visibility.Hidden;

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            path += @"\ScreenDraw_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".bmp";
            DispatcherTimer mdtTakeSS = new DispatcherTimer();
            mdtTakeSS.Interval = new TimeSpan(0, 0, 0, 0, 50);
            mdtTakeSS.Tick += delegate (object s, EventArgs args) {
                CaptureScreen().Save(path);
                this.txtMessages.Text = string.Format("Screenshot saved on {0}", path);
                txtMessages.Visibility = Visibility.Visible;
                wpShoot.Visibility = Visibility.Visible;
                mdtTakeSS.Stop();
            };

            DispatcherTimer mdtShowControls = new DispatcherTimer();
            mdtShowControls.Interval = new TimeSpan(0, 0, 0, 0, 100);
            mdtShowControls.Tick += delegate (object s, EventArgs args) {
                wpShoot.Visibility = Visibility.Hidden;
                stakPControls.Visibility = Visibility.Visible;
                btnClose.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                mdtShowControls.Stop();
            };

            DispatcherTimer mdtHideMessge = new DispatcherTimer();
            mdtHideMessge.Interval = new TimeSpan(0, 0, 0, 5, 0);
            mdtHideMessge.Tick += delegate (object s, EventArgs args) {
                this.txtMessages.Text = string.Format("Screenshot saved on {0}", path);
                txtMessages.Visibility = Visibility.Hidden;
                mdtHideMessge.Stop();
            };

            mdtTakeSS.Start();
            mdtShowControls.Start();
            mdtHideMessge.Start();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                canv.Children.Clear();
            }
        }
    }
}
