using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Accord.Math;
using Accord.Video.FFMPEG;
using IronPython.Hosting;
using Microsoft.Expression.Interactivity.Layout;
using Microsoft.Scripting.Hosting;
using Microsoft.Win32;

namespace ManimInteractive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public const string MANIM_DIR = @"C:\Users\jjask\Documents\manim\";

        public MainWindow()
        {
            InitializeComponent();

            /*DrawRectangle(DisplayCanvas, new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Red),
                Width = 100,
                Height = 100
            }, 170, 52);
            DrawRectangle(DisplayCanvas, new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Blue),
                Width = 150,
                Height = 100
            }, 0, 0);*/
        }

        #region Python/Video
        //[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private void run_cmd(string cmd, string args)
        {
            //Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = MANIM_DIR;
            startInfo.FileName = cmd;
            startInfo.Arguments = "/C " + args;

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }

            #region old
            /*ProcessStartInfo start = new ProcessStartInfo();
            try
            {
                start.FileName = GetPython32("3.7");
            }
            catch (FileLoadException ex)
            {
                if (ex.Message == PYTHON_NOT_FOUND_MSG)
                {
                    MessageBox.Show("Python 3.7 is not installed.", "Python 3.7", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }*/
            #endregion
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //MediaObject.Source = new Uri(@"C:\Users\jjask\Videos\Manim Exports\videos\example_scenes\1440p60\SquareToCircle.mp4");
        }

        public static void WriteVideoFromFrames(System.Drawing.Bitmap[] frames, System.Drawing.Size size, string destination, Rational fps)
        {
            using (var writer = new VideoFileWriter())          // Initialize a VideoFileWriter
            {
                writer.Width = size.Width;
                writer.Height = size.Height;
                writer.FrameRate = fps;
                writer.VideoCodec = VideoCodec.Mpeg4;
                writer.Open(destination);              // Start output of video

                foreach (var image in frames)   // Iterate files
                {
                    using (var resized = new System.Drawing.Bitmap(image, size))        // Resize if necessary
                        writer.WriteVideoFrame(resized);                    // Write frame
                }
                writer.Close();                                 // Close VideoFileWriter
            }
        }

        public const string PYTHON_NOT_FOUND_MSG = "Python: Specified version not installed";
        public static string GetPython32(string version)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey($@"Software\Python\PythonCore\{version}\InstallPath"))
            {
                if (key != null)
                {
                    Object o = key.GetValue("");
                    if (o != null)
                    {
                        string pyPath32 = o as string;
                        if (pyPath32 != null)
                        {
                            return pyPath32;
                        }
                        throw new FileLoadException(PYTHON_NOT_FOUND_MSG);
                    }
                    throw new FileLoadException(PYTHON_NOT_FOUND_MSG);
                }
                throw new FileLoadException(PYTHON_NOT_FOUND_MSG);
            }
            
        }
        public static string GetPython64(string version)
        {
            string pyPath64 = Registry.GetValue($@"HKLM\SOFTWARE\Wow6432Node\Python\PythonCore\{version}\InstallPath", "(Default)", null) as string;
            if (pyPath64 != null)
            {
                return pyPath64;
            }
            else
            {
                throw new FileLoadException(PYTHON_NOT_FOUND_MSG);
            }
        }

        bool IsPreviewing = false;
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPreviewing)
            {
                Player.Stop();
                IsPreviewing = false;
                Player.MediaEnded -= Player_LoopMedia;
                Player.Source = null;

                PreviewButton.Icon = new Uri(@"pack://application:,,,/Assets/Icons/Run_16x.png");
                PreviewButton.LargeIcon = new Uri(@"pack://application:,,,/Assets/Icons/Run_32x.png");
                PreviewButton.Header = "Start Preview";

                PlaybackPlayButton.IsEnabled = false;
                PlaybackStopButton.IsEnabled = false;
                Player.Visibility = Visibility.Collapsed;
            }
            else
            {
                run_cmd("cmd.exe", $"py -3 extract_scene.py example_scenes.py SquareToCircle -pl");

                Player.Stretch = Stretch.Uniform;
                Player.MediaEnded += Player_LoopMedia;
                Player.Visibility = Visibility.Visible;
                Player.Source = new Uri(@"C:\Users\jjask\Videos\Manim Exports\videos\example_scenes\480p15\SquareToCircle.mp4");
                IsPreviewing = true;

                PreviewButton.Icon = new Uri(@"pack://application:,,,/Assets/Icons/Stop_16x.png");
                PreviewButton.LargeIcon = new Uri(@"pack://application:,,,/Assets/Icons/Stop_32x.png");
                PreviewButton.Header = "Stop Preview";

                PlaybackPlayButton.IsEnabled = true;
                PlaybackStopButton.IsEnabled = true;
            }
        }

        private void Player_LoopMedia(object sender, RoutedEventArgs e)
        {
            Player.Rewind();
            Player.Play();
        }

        private void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            run_cmd("cmd.exe", $"py -3 extract_scene.py example_scenes.py SquareToCircle");

            Player.Stretch = Stretch.Uniform;
            Player.MediaEnded += (psender, pe) => {
                Player.Rewind(); Player.Play();
            };
            Player.Source = new Uri(@"C:\Users\jjask\Videos\Manim Exports\videos\example_scenes\1440p60\SquareToCircle.mp4");
        }

        private void PlaybackPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.IsPlaying)
            {
                Player.Rewind();
                Player.Play();
            }
            else
            {
                Player.Play();
            }
        }

        private void PlaybackStopButton_Click(object sender, RoutedEventArgs e)
        {
            Player.PausePlayback();
        }
        #endregion

        private void DrawRectangle(Viewport view, Rectangle rect, double xpos, double ypos)
        {
            MouseDragElementBehavior dragBehavior = new MouseDragElementBehavior
            {
                ConstrainToParentBounds = true
            };
            dragBehavior.Attach(rect);
            DisplayCanvas.Children.Add(rect);
            Canvas.SetTop(rect, ypos);
            Canvas.SetLeft(rect, xpos);
        }
    }

    #region Canvas Addons
    public class ViewportItem : Panel
    {
        public static readonly DependencyProperty RelativeRectProperty = DependencyProperty.RegisterAttached(
            "RelativeRect", typeof(Rect), typeof(Viewport),
            new FrameworkPropertyMetadata(new Rect(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public Rect RelativeRect = new Rect(0, 0, 0.1, 0.1);
        private bool Dragging = false;
        private Point mouseOffset = new Point();

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            RelativeRect = (Rect)GetValue(RelativeRectProperty);

            MouseDown += Draggable_MouseDown;
            MouseUp += Draggable_MouseUp;
            MouseMove += Draggable_MouseMove;
        }

        private void Draggable_MouseMove(object sender, MouseEventArgs e)
        {
                if (IsMouseCaptured)
                {
                    Point mouseDelta = Mouse.GetPosition(this);
                    mouseDelta.Offset(-mouseOffset.X, -mouseOffset.Y);

                    Margin = new Thickness(
                        Margin.Left + mouseDelta.X,
                        Margin.Top + mouseDelta.Y,
                        Margin.Right - mouseDelta.X,
                        Margin.Bottom - mouseDelta.Y
                    );
            }
        }

        private void Draggable_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Drop item
            Dragging = false;
            ReleaseMouseCapture();

            // Recalculate if in Viewport
            if (Parent != null)
            {
                if (Parent.GetType() == typeof(Viewport))
                {
                    var view = (Viewport)Parent;

                    Point AbsLocation = TranslatePoint(new Point(0, 0), view);
                    RelativeRect.X = AbsLocation.X / view.ActualWidth;
                    RelativeRect.Y = AbsLocation.Y / view.ActualHeight;
                    Margin = new Thickness(0);
                    view.InvalidateArrange();
                    Console.WriteLine(RelativeRect);
                }
            }
        }

        private void Draggable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Dragging = true;
            mouseOffset = Mouse.GetPosition(this);
            CaptureMouse();
        }
    }

    public class Viewport : Canvas
    {
        public static Rect? GetRelativeRect(UIElement element)
        {
            if (element.GetType() == typeof(ViewportItem))
            {
                return (element as ViewportItem).RelativeRect;
            }
            return null;
        }

        public static void SetRelativeRect(UIElement element, Rect value)
        {
            if (element.GetType() == typeof(ViewportItem))
            {
                var item = element as ViewportItem;
                item.RelativeRect = value;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement element in InternalChildren)
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (ViewportItem element in InternalChildren)
            {
                double newX = element.RelativeRect.X * finalSize.Width;
                double newY = element.RelativeRect.Y * finalSize.Height;

                element.Arrange(new Rect(
                    newX,
                    newY,
                    element.RelativeRect.Width * finalSize.Width,
                    element.RelativeRect.Height * finalSize.Height));
            }

            return finalSize;
        }
    }

    public class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Control item = this.DataContext as Control;

            if (item != null)
            {
                double left = Canvas.GetLeft(item);
                double top = Canvas.GetTop(item);

                Canvas.SetLeft(item, left + e.HorizontalChange);
                Canvas.SetTop(item, top + e.VerticalChange);
            }
        }
    }

    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Control item = this.DataContext as Control;

            if (item != null)
            {
                double deltaVertical, deltaHorizontal;

                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:
                        deltaVertical = Math.Min(-e.VerticalChange,
                            item.ActualHeight - item.MinHeight);
                        item.Height -= deltaVertical;
                        break;
                    case VerticalAlignment.Top:
                        deltaVertical = Math.Min(e.VerticalChange,
                            item.ActualHeight - item.MinHeight);
                        Canvas.SetTop(item, Canvas.GetTop(item) + deltaVertical);
                        item.Height -= deltaVertical;
                        break;
                    default:
                        break;
                }

                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        deltaHorizontal = Math.Min(e.HorizontalChange,
                            item.ActualWidth - item.MinWidth);
                        Canvas.SetLeft(item, Canvas.GetLeft(item) + deltaHorizontal);
                        item.Width -= deltaHorizontal;
                        break;
                    case HorizontalAlignment.Right:
                        deltaHorizontal = Math.Min(-e.HorizontalChange,
                            item.ActualWidth - item.MinWidth);
                        item.Width -= deltaHorizontal;
                        break;
                    default:
                        break;
                }
            }

            e.Handled = true;
        }
    }
    #endregion

    class AccentColorSet
    {
        public static AccentColorSet[] AllSets {
            get {
                if (_allSets == null)
                {
                    UInt32 colorSetCount = UXTheme.GetImmersiveColorSetCount();

                    List<AccentColorSet> colorSets = new List<AccentColorSet>();
                    for (UInt32 i = 0; i < colorSetCount; i++)
                    {
                        colorSets.Add(new AccentColorSet(i, false));
                    }

                    AllSets = colorSets.ToArray();
                }

                return _allSets;
            }
            private set {
                _allSets = value;
            }
        }

        public static AccentColorSet ActiveSet {
            get {
                UInt32 activeSet = UXTheme.GetImmersiveUserColorSetPreference(false, false);
                ActiveSet = AllSets[Math.Min(activeSet, AllSets.Length - 1)];
                return _activeSet;
            }
            private set {
                if (_activeSet != null) _activeSet.Active = false;

                value.Active = true;
                _activeSet = value;
            }
        }

        public Boolean Active { get; private set; }

        public System.Windows.Media.Color this[String colorName] {
            get {
                IntPtr name = IntPtr.Zero;
                UInt32 colorType;

                try
                {
                    name = Marshal.StringToHGlobalUni("Immersive" + colorName);
                    colorType = UXTheme.GetImmersiveColorTypeFromName(name);
                    if (colorType == 0xFFFFFFFF) throw new InvalidOperationException();
                }
                finally
                {
                    if (name != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(name);
                        name = IntPtr.Zero;
                    }
                }

                return this[colorType];
            }
        }

        public System.Windows.Media.Color this[UInt32 colorType] {
            get {
                UInt32 nativeColor = UXTheme.GetImmersiveColorFromColorSetEx(this._colorSet, colorType, false, 0);
                //if (nativeColor == 0)
                //    throw new InvalidOperationException();
                return System.Windows.Media.Color.FromArgb(
                    (Byte)((0xFF000000 & nativeColor) >> 24),
                    (Byte)((0x000000FF & nativeColor) >> 0),
                    (Byte)((0x0000FF00 & nativeColor) >> 8),
                    (Byte)((0x00FF0000 & nativeColor) >> 16)
                    );
            }
        }

        AccentColorSet(UInt32 colorSet, Boolean active)
        {
            this._colorSet = colorSet;
            this.Active = active;
        }

        static AccentColorSet[] _allSets;
        static AccentColorSet _activeSet;

        UInt32 _colorSet;

        // HACK: GetAllColorNames collects the available color names by brute forcing the OS function.
        //   Since there is currently no known way to retrieve all possible color names,
        //   the method below just tries all indices from 0 to 0xFFF ignoring errors.
        public List<String> GetAllColorNames()
        {
            List<String> allColorNames = new List<String>();
            for (UInt32 i = 0; i < 0xFFF; i++)
            {
                IntPtr typeNamePtr = UXTheme.GetImmersiveColorNamedTypeByIndex(i);
                if (typeNamePtr != IntPtr.Zero)
                {
                    IntPtr typeName = (IntPtr)Marshal.PtrToStructure(typeNamePtr, typeof(IntPtr));
                    allColorNames.Add(Marshal.PtrToStringUni(typeName));
                }
            }

            return allColorNames;
        }

        static class UXTheme
        {
            [DllImport("uxtheme.dll", EntryPoint = "#98", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern UInt32 GetImmersiveUserColorSetPreference(Boolean forceCheckRegistry, Boolean skipCheckOnFail);

            [DllImport("uxtheme.dll", EntryPoint = "#94", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern UInt32 GetImmersiveColorSetCount();

            [DllImport("uxtheme.dll", EntryPoint = "#95", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern UInt32 GetImmersiveColorFromColorSetEx(UInt32 immersiveColorSet, UInt32 immersiveColorType,
                Boolean ignoreHighContrast, UInt32 highContrastCacheMode);

            [DllImport("uxtheme.dll", EntryPoint = "#96", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern UInt32 GetImmersiveColorTypeFromName(IntPtr name);

            [DllImport("uxtheme.dll", EntryPoint = "#100", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern IntPtr GetImmersiveColorNamedTypeByIndex(UInt32 index);
        }
    }
}
