using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Accord.Collections;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<string, string> pair in ManimHelper.Colors)
            {
                FillColorSelectBox.Items.Add(new ComboBoxItem()
                {
                    Background = Common.BrushFromHex(pair.Value),
                    Content = pair.Key
                });
            }

            DisplayCanvas.Width = ManimHelper.PixelWidth;
            DisplayCanvas.Height = ManimHelper.PixelHeight;
        }

        #region Python
        private string GenerateScene()
        {
            string pythonScene = ManimHelper.PythonSceneHeader + $"class Default(Scene):\r\n";
            pythonScene += $"{ManimHelper.PY_TAB}def construct(self):\r\n";

            int itemindex = 0;
            foreach (FrameworkElement item in DisplayCanvas.Children)
            {
                var shape = item as ManimHelper.IMobject_Shape;
                if (shape != null)
                {
                    pythonScene += $"{ManimHelper.PY_TAB}{ManimHelper.PY_TAB}{shape.GetPyInitializer(shape.MobjType + itemindex)}\r\n";
                    itemindex++;
                }
            }
            pythonScene += $"\r\n";
            itemindex = 0;
            foreach (FrameworkElement item in DisplayCanvas.Children)
            {
                var shape = item as ManimHelper.IMobject_Shape;
                if (shape != null)
                {
                    pythonScene += $"{ManimHelper.PY_TAB}{ManimHelper.PY_TAB}self.play(ShowCreation({shape.Name}))\r\n";
                    itemindex++;
                }
            }

            Console.Write(pythonScene);
            return pythonScene;
        }

        //[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private void run_cmd(string cmd, string args)
        {
            //Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal; // Change to 'Hidden' for release
            startInfo.WorkingDirectory = ManimHelper.MANIM_DIR;
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
        #endregion

        #region Media Playback
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
                File.WriteAllText(System.IO.Path.Combine(ManimHelper.MANIM_DIR, "testing\\exported_scenes.py"), GenerateScene());
                run_cmd("cmd.exe", @"py -3 extract_scene.py testing\exported_scenes.py Default -pl");

                Player.Stretch = Stretch.Uniform;
                Player.MediaEnded += Player_LoopMedia;
                Player.Visibility = Visibility.Visible;
                Player.Source = new Uri(@"C:\Users\jjask\Videos\Manim Exports\videos\testing\exported_scenes\480p15\Default.mp4");
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

        private void NewRectButton_Click(object sender, RoutedEventArgs e)
        {
            ManimHelper.Mobject_Rectangle.Draw("TestRectangle", DisplayCanvas, new Rect(0.5, 0.5, 0.2, 0.2), "WHITE","YELLOW_E");
        }
        private void NewEllipseButton_Click(object sender, RoutedEventArgs e)
        {
            ManimHelper.Mobject_Ellipse.Draw("TestEllipse", DisplayCanvas, new Rect(0.5, 0.5, 0.2, 0.2), "WHITE", "YELLOW_E");
        }

        public ManimHelper.IMobject_Shape SelectedVisual;
        public bool IsLoadingSelected = false;
        private void DisplayCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ManimHelper.IMobject_Shape)
            {
                SelectMobject(e.OriginalSource as ManimHelper.IMobject_Shape);
            }
            else
            {
                Deselect();
            }
        }
        private void SelectMobject(ManimHelper.IMobject_Shape shape)
        {
            SelectedVisual = shape;
            IsLoadingSelected = true;
            //SelectedVisual.DrawBorder(DisplayCanvas);
            drawingGroup.Visibility = Visibility.Visible;
            SelectedVisual.DragStateChanged += SelectedVisual_DragStateChanged;
            UpdateDrawingToolsUI(SelectedVisual);
            IsLoadingSelected = false;
        }
        private void Deselect()
        {
            //SelectedVisual.DragStateChanged -= SelectedVisual_DragStateChanged;
            SelectedVisual = null;
            drawingGroup.Visibility = Visibility.Collapsed;
        }

        private void SelectedVisual_DragStateChanged(object sender, ViewportItemDragStateChanged e)
        {
            UpdateDrawingToolsUI(SelectedVisual);
        }

        private void DisplayCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Draggable))
            {
                if (SelectedVisual == e.OriginalSource as Draggable)
                {
                    IsLoadingSelected = true;
                    UpdateDrawingToolsUI(SelectedVisual);
                    IsLoadingSelected = false;
                }
            }
        }

        private void UpdateDrawingToolsUI(ManimHelper.IMobject_Shape item)
        {
            if (item != null)
            {
                ItemHeightBox.Text = Math.Round(Draggable.GetRelativeRect(item).Height * DisplayCanvas.ActualHeight).ToString();
                ItemWidthBox.Text = Math.Round(Draggable.GetRelativeRect(item).Width * DisplayCanvas.ActualWidth).ToString();
                ItemXBox.Text = Math.Round(Draggable.GetRelativeRect(item).X * DisplayCanvas.ActualWidth).ToString();
                ItemYBox.Text = Math.Round(Draggable.GetRelativeRect(item).Y * DisplayCanvas.ActualHeight).ToString();

                FillColorSelectBox.SelectedIndex = ManimHelper.Colors.Keys.ToList().IndexOf(item.Fill);
            }
        }

        private void FillColorSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null)
            {
                var newColor = (FillColorSelectBox.SelectedItem as ComboBoxItem).Content.ToString();
                SelectedVisual.Fill = newColor;
                FillColorDisplay.Background = Common.BrushFromHex(ManimHelper.Colors[newColor]);
            }
        }
        private void ItemXBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null)
            {
                if (String.IsNullOrWhiteSpace(ItemXBox.Text))
                {
                    ItemXBox.Text = "0";
                }
                double NewRelative = Convert.ToDouble(ItemXBox.Text) / DisplayCanvas.Width;
                SelectedVisual.SetX(NewRelative, DisplayCanvas);
            }
        }
        private void ItemYBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null)
            {
                if (String.IsNullOrWhiteSpace(ItemYBox.Text))
                {
                    ItemYBox.Text = "0";
                }
                double NewRelative = Convert.ToDouble(ItemYBox.Text) / DisplayCanvas.Height;
                SelectedVisual.SetY(NewRelative, DisplayCanvas);
            }
        }
        private void ItemWidthBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null)
            {
                if (String.IsNullOrWhiteSpace(ItemWidthBox.Text))
                {
                    ItemWidthBox.Text = "0";
                }
                double NewRelative = Convert.ToDouble(ItemWidthBox.Text) / DisplayCanvas.Width;
                SelectedVisual.SetWidth(NewRelative, DisplayCanvas);
            }
        }
        private void ItemHeightBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null)
            {
                if (String.IsNullOrWhiteSpace(ItemHeightBox.Text))
                {
                    ItemHeightBox.Text = "0";
                }
                double NewRelative = Convert.ToDouble(ItemHeightBox.Text) / DisplayCanvas.Height;
                SelectedVisual.SetHeight(NewRelative, DisplayCanvas);
            }
        }
    }

    #region Canvas Addons
    /// <summary>
    /// A panel designed for use in a Viewport. Positioning and dimentions are relative to its containing Viewport. Draggable by default.
    /// </summary>
    public class ViewportItem : Panel
    {
        #region Properties
        public static readonly DependencyProperty RelativeRectProperty = DependencyProperty.RegisterAttached(
            "RelativeRect", typeof(Rect), typeof(Viewport),
            new FrameworkPropertyMetadata(new Rect(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public Rect RelativeRect = new Rect();
        // TODO: public bool IsOriginCenterOfMass;

        public bool IsDraggable;
        private bool _dragging = false;
        public bool IsDragging
        {
            get {
                return _dragging;
            }
            internal set {
                bool oldValue = _dragging;
                _dragging = value;

                DragStateChanged?.Invoke(null, new ViewportItemDragStateChanged { OldState = oldValue, NewState = value });
            }
         }
        public event EventHandler<ViewportItemDragStateChanged> DragStateChanged;
        private Point mouseOffset = new Point();
        #endregion

        public ViewportItem() { }
        public ViewportItem(Rect rect, bool isDraggable = true)
        {
            RelativeRect = rect;
            IsDraggable = isDraggable;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (RelativeRect == new Rect())
                RelativeRect = (Rect)GetValue(RelativeRectProperty);

            MouseDown += Draggable_MouseDown;
            MouseUp += Draggable_MouseUp;
            MouseMove += Draggable_MouseMove;
        }

        private void Draggable_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDraggable && IsMouseCaptured)
            {
                IsDragging = true;
                Point mouseDelta = Mouse.GetPosition(this);
                mouseDelta.Offset(-mouseOffset.X, -mouseOffset.Y);

                // Recalculate if in Viewport
                if (Parent != null)
                {
                    var view = Parent as Viewport;
                    if (view != null)
                    {
                        RelativeRect.X += (mouseDelta.X / view.ActualWidth);
                        RelativeRect.Y += (mouseDelta.Y / view.ActualHeight);
                    }
                }

                /*Margin = new Thickness(
                    Margin.Left + mouseDelta.X,
                    Margin.Top + mouseDelta.Y,
                    Margin.Right - mouseDelta.X,
                    Margin.Bottom - mouseDelta.Y
                );*/
            }
        }

        private void Draggable_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDraggable)
            {
                // Drop item
                IsDragging = false;
                ReleaseMouseCapture();

                // Recalculate if in Viewport
                if (Parent != null)
                {
                    if (Parent is RelativeLayoutPanel)
                    {
                        RecalculateRelative(Parent as RelativeLayoutPanel);
                        Console.WriteLine(RelativeRect);
                    }
                }
            }
        }

        private void Draggable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsDraggable)
            {
                mouseOffset = Mouse.GetPosition(this);
                CaptureMouse();
            }
        }

        /// <summary>
        /// Draws a red border with thickness 10 around the object
        /// </summary>
        /// <param name="canvas">The containing Viewport</param>
        public void DrawBorder(RelativeLayoutPanel canvas)
        {
            Rect bounds = TransformToVisual(canvas).TransformBounds(new Rect(RenderSize));
            Children.Add(new Border
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(10)
            });
        }

        public void AddUIChild(Rect relativeRect, FrameworkElement element)
        {
            element.Width = relativeRect.Width * ActualWidth;
            element.Height = relativeRect.Height * ActualHeight;
            Children.Add(element);
        }

        /// <summary>
        /// Moves the item to the specified position
        /// </summary>
        /// <param name="vector">Relative change</param>
        /// <param name="view">The containing view</param>
        public void MoveToLocation(Point vector, RelativeLayoutPanel view)
        {
            RelativeRect.X = vector.X;
            RelativeRect.Y = vector.Y;
            view.InvalidateArrange();
        }

        public void SetX(Double NewX, RelativeLayoutPanel view)
        {
            MoveToLocation(new Point(NewX, RelativeRect.Y), view);
        }
        public void SetY(Double NewY, RelativeLayoutPanel view)
        {
            MoveToLocation(new Point(RelativeRect.X, NewY), view);
        }
        public void SetWidth(Double NewWidth, RelativeLayoutPanel view)
        {
            RelativeRect.Width = NewWidth;
            view.InvalidateArrange();
        }
        public void SetHeight(Double NewHeight, RelativeLayoutPanel view)
        {
            RelativeRect.Height = NewHeight;
            view.InvalidateArrange();
        }

        /// <summary>
        /// Recalculates the relative location according to the specified Viewport. Useful for resetting Margin offsets but keeping translation.
        /// </summary>
        /// <param name="view">The containing Viewport</param>
        private void RecalculateRelative(RelativeLayoutPanel view, bool ResetMargin = true)
        {
            Point AbsLocation = TranslatePoint(new Point(0, 0), view);
            RelativeRect.X = (AbsLocation.X / view.ActualWidth) + (RelativeRect.Width / 2);
            RelativeRect.Y = (AbsLocation.Y / view.ActualHeight) + (RelativeRect.Height / 2);
            if(ResetMargin)
                Margin = new Thickness(0);

            // RelativeRect updated with new calculations, now force the containing view to rearrange
            view.InvalidateArrange();
        }
    }
    public class ViewportItemDragStateChanged : EventArgs
    {
        /// <summary>
        /// True if was previously dragging
        /// </summary>
        public bool OldState { get; set; }

        /// <summary>
        /// True if started dragging
        /// </summary>
        public bool NewState { get; set; }
    }

    public class Viewport : Grid
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
            //availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement element in InternalChildren)
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (FrameworkElement element in InternalChildren)
            {
                try
                {
                    var item = element as ViewportItem;
                    double newX = (item.RelativeRect.X * finalSize.Width) - (item.ActualWidth / 2);
                    double newY = (item.RelativeRect.Y * finalSize.Height) - (item.ActualHeight / 2);
                    double newW = (item.RelativeRect.Width * finalSize.Width);
                    double newH = (item.RelativeRect.Height * finalSize.Height);

                    item.Arrange(new Rect(
                        newX,
                        newY,
                        newW,
                        newH));
                }
                catch
                {
                    // TODO: Handle objects that aren't ViewportItems
                }
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

    public static class ManimHelper
    {
        public const string MANIM_DIR = @"C:\Users\jjask\Documents\manim\";
        public const string PY_TAB = @"    ";
        public const string PythonSceneHeader = "#!/usr/bin/env python\r\n\r\nfrom big_ol_pile_of_manim_imports import *\r\n\r\n";

        public const int PixelHeight = 1440;
        public const int PixelWidth = 2560;
        public const double FrameHeight = 8;
        public const double FrameWidth = (8 * PixelWidth / PixelHeight);
        public static readonly Point FrameOrigin = new Point(FrameWidth / 2, FrameHeight / 2);

        public static readonly Dictionary<string, string> Colors = new Dictionary<string, string>() {
            { "DARK_BLUE", "#236B8E" },
            { "DARK_BROWN", "#8B4513" },
            { "LIGHT_BROWN", "#CD853F" },
            { "BLUE_E", "#1C758A" },
            { "BLUE_D", "#29ABCA" },
            { "BLUE_C", "#58C4DD" },
            { "BLUE_B", "#9CDCEB" },
            { "BLUE_A", "#C7E9F1" },
            { "TEAL_E", "#49A88F" },
            { "TEAL_D", "#55C1A7" },
            { "TEAL_C", "#5CD0B3" },
            { "TEAL_B", "#76DDC0" },
            { "TEAL_A", "#ACEAD7" },
            { "GREEN_E", "#699C52" },
            { "GREEN_D", "#77B05D" },
            { "GREEN_C", "#83C167" },
            { "GREEN_B", "#A6CF8C" },
            { "GREEN_A", "#C9E2AE" },
            { "YELLOW_E", "#E8C11C" },
            { "YELLOW_D", "#F4D345" },
            { "YELLOW_C", "#FFFF00" },
            { "YELLOW_B", "#FFEA94" },
            { "YELLOW_A", "#FFF1B6" },
            { "GOLD_E", "#C78D46" },
            { "GOLD_D", "#E1A158" },
            { "GOLD_C", "#F0AC5F" },
            { "GOLD_B", "#F9B775" },
            { "GOLD_A", "#F7C797" },
            { "RED_E", "#CF5044" },
            { "RED_D", "#E65A4C" },
            { "RED_C", "#FC6255" },
            { "RED_B", "#FF8080" },
            { "RED_A", "#F7A1A3" },
            { "MAROON_E", "#94424F" },
            { "MAROON_D", "#A24D61" },
            { "MAROON_C", "#C55F73" },
            { "MAROON_B", "#EC92AB" },
            { "MAROON_A", "#ECABC1" },
            { "PURPLE_E", "#644172" },
            { "PURPLE_D", "#715582" },
            { "PURPLE_C", "#9A72AC" },
            { "PURPLE_B", "#B189C6" },
            { "PURPLE_A", "#CAA3E8" },
            { "WHITE", "#FFFFFF" },
            { "BLACK", "#000000" },
            { "LIGHT_GRAY", "#BBBBBB" },
            { "LIGHT_GREY", "#BBBBBB" },
            { "GRAY", "#888888" },
            { "GREY", "#888888" },
            { "DARK_GREY", "#444444" },
            { "DARK_GRAY", "#444444" },
            { "GREY_BROWN", "#736357" },
            { "PINK", "#D147BD" },
            { "GREEN_SCREEN", "#00FF00" },
            { "ORANGE", "#FF862F" },
        };

        #region Shapes
        public abstract class IMobject_Shape : Draggable
        {
            public abstract string Fill {
                get; set;
            }
            public string Outline;
            public double OutlineThickness = 0;
            public abstract string MobjType { get; }

            public abstract string GetPyInitializer(string defaultName);

            public static string CalculateXPosition(double X)
            {
                double DistanceFromCenter = X - .5;
                double Shift = Math.Abs(DistanceFromCenter) * (FrameWidth / 1);
                string result = Shift.ToString();
                if (DistanceFromCenter < 0)
                {
                    result += "*LEFT";
                }
                else
                {
                    result += "*RIGHT";
                }

                return result;
            }
            public static string CalculateYPosition(double Y)
            {
                double DistanceFromCenter = Y - .5;
                double Shift = Math.Abs(DistanceFromCenter) * (FrameHeight / 1);
                string result = Shift.ToString();
                if (DistanceFromCenter < 0)
                {
                    result += "*UP";
                }
                else
                {
                    result += "*DOWN";
                }

                return result;
            }
        }

        public class Mobject_Rectangle : IMobject_Shape
        {
            private string _fill;
            public override string Fill {
                get {
                    return _fill;
                }
                set {
                    _fill = value;
                    Background = Common.BrushFromHex(Colors[_fill]);
                }
            }
            public override string MobjType {
                get;
            } = "Rectangle";

            public static void Draw(string name, Panel view, Rect rect, string outline, string fill)
            {
                var item = new Mobject_Rectangle()
                {
                    Name = name,
                    Fill = fill,
                    Outline = outline,
                    IsDraggable = true,
                };
                SetRelativeRect(item, rect);
                view.Children.Add(item);
            }
            public override string GetPyInitializer(string defaultName)
            {
                string name = Name;
                if (String.IsNullOrWhiteSpace(Name))
                    name = defaultName;

                string init = $"{name} = Rectangle()\r\n";

                init += $"{PY_TAB}{PY_TAB}{name}.set_fill({Fill}, opacity=1.0)\r\n";
                //init += $"{PY_TAB}{PY_TAB}{name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{PY_TAB}{PY_TAB}{name}.set_height({GetRelativeRect(this).Height * FrameHeight})\r\n";
                init += $"{PY_TAB}{PY_TAB}{name}.stretch_to_fit_width({GetRelativeRect(this).Width * FrameWidth})\r\n";

                // TODO: Calculate vectors for positioning
                init += $"{PY_TAB}{PY_TAB}{name}.shift({CalculateXPosition(GetRelativeRect(this).X)} + {CalculateYPosition(GetRelativeRect(this).Y)})";
                return init;
            }
        }

        public class Mobject_Ellipse : IMobject_Shape
        {
            private string _fill;
            public override string Fill
            {
                get {
                    return _fill;
                }
                set {
                    _fill = value;
                    InternalEllipse.Fill = Common.BrushFromHex(Colors[_fill]);
                }
            }
            public override string MobjType {
                get;
            } = "Ellipse";
            private Ellipse InternalEllipse;

            public static void Draw(string name, Panel view, Rect rect, string outline, string fill)
            {
                var ellipse = new Ellipse()
                {
                    Fill = Common.BrushFromHex(Colors[fill]),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false
                };
                var item = new Mobject_Ellipse()
                {
                    Name = name,
                    Children =
                    {
                        ellipse
                    },
                    IsDraggable = true,
                    IsHitTestVisible = true,
                    Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    InternalEllipse = ellipse,
                };
                SetRelativeRect(item, rect);
                view.Children.Add(item);
            }
            public override string GetPyInitializer(string defaultName)
            {
                string name = Name;
                if (String.IsNullOrWhiteSpace(Name))
                    name = defaultName;

                string init = $"{name} = Ellipse()\r\n";

                init += $"{PY_TAB}{PY_TAB}{name}.set_fill({Fill}, opacity=1.0)\r\n";
                //init += $"{PY_TAB}{PY_TAB}{name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{PY_TAB}{PY_TAB}{name}.set_height({GetRelativeRect(this).Height * FrameHeight})\r\n";
                init += $"{PY_TAB}{PY_TAB}{name}.stretch_to_fit_width({GetRelativeRect(this).Width * FrameWidth})\r\n";

                // TODO: Calculate vectors for positioning
                init += $"{PY_TAB}{PY_TAB}{name}.shift({CalculateXPosition(GetRelativeRect(this).X)} + {CalculateYPosition(GetRelativeRect(this).Y)})";
                return init;
            }
        }
        #endregion
    }

    public class RelativeLayoutPanel : Panel
    {
        public static readonly DependencyProperty RelativeRectProperty = DependencyProperty.RegisterAttached(
            "RelativeRect", typeof(Rect), typeof(RelativeLayoutPanel),
            new FrameworkPropertyMetadata(new Rect(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static Rect GetRelativeRect(UIElement element)
        {
            return (Rect)element.GetValue(RelativeRectProperty);
        }

        public static void SetRelativeRect(UIElement element, Rect value)
        {
            element.SetValue(RelativeRectProperty, value);
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
            foreach (UIElement element in InternalChildren)
            {
                var rect = GetRelativeRect(element);

                double newW = (rect.Width * finalSize.Width);
                double newH = (rect.Height * finalSize.Height);
                double newX = (rect.X * finalSize.Width) - (newW / 2);
                double newY = (rect.Y * finalSize.Height) - (newH / 2);

                element.Arrange(new Rect(
                    newX,
                    newY,
                    newW,
                    newH));
            }

            return finalSize;
        }
    }

    public class Draggable : Panel
    {
        #region Properties
        // TODO: public bool IsOriginCenterOfMass;

        public bool IsDraggable;
        private bool _dragging = false;
        public bool IsDragging {
            get {
                return _dragging;
            }
            internal set {
                bool oldValue = _dragging;
                _dragging = value;

                DragStateChanged?.Invoke(null, new ViewportItemDragStateChanged { OldState = oldValue, NewState = value });
            }
        }
        public event EventHandler<ViewportItemDragStateChanged> DragStateChanged;
        private Point mouseOffset = new Point();
        #endregion

        public Draggable() { }
        public Draggable(Rect rect, bool isDraggable = true)
        {
            SetRelativeRect(this, rect);
            IsDraggable = isDraggable;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            MouseDown += Draggable_MouseDown;
            MouseUp += Draggable_MouseUp;
            MouseMove += Draggable_MouseMove;
        }

        private void Draggable_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDraggable && IsMouseCaptured)
            {
                IsDragging = true;
                Point mouseDelta = Mouse.GetPosition(this);
                mouseDelta.Offset(-mouseOffset.X, -mouseOffset.Y);

                // Recalculate if in Viewport
                if (Parent != null)
                {
                    var view = Parent as Panel;
                    if (view != null)
                    {
                        var RelativeRect = GetRelativeRect(this);
                        RelativeRect.X += (mouseDelta.X / view.ActualWidth);
                        RelativeRect.Y += (mouseDelta.Y / view.ActualHeight);
                        SetRelativeRect(this, RelativeRect);
                    }
                }

                /*Margin = new Thickness(
                    Margin.Left + mouseDelta.X,
                    Margin.Top + mouseDelta.Y,
                    Margin.Right - mouseDelta.X,
                    Margin.Bottom - mouseDelta.Y
                );*/
            }
        }

        private void Draggable_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDraggable)
            {
                // Drop item
                IsDragging = false;
                ReleaseMouseCapture();

                // Recalculate if in Viewport
                if (Parent != null)
                {
                    if (Parent is Panel)
                    {
                        RecalculateRelative(Parent as Panel);
                        Console.WriteLine(GetRelativeRect(this));
                    }
                }
            }
        }

        private void Draggable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsDraggable)
            {
                mouseOffset = Mouse.GetPosition(this);
                CaptureMouse();
            }
        }

        /// <summary>
        /// Draws a red border with thickness 10 around the object
        /// </summary>
        /// <param name="canvas">The containing Viewport</param>
        public void DrawBorder()//RelativeLayoutPanel canvas)
        {
            //Rect bounds = TransformToVisual(canvas).TransformBounds(new Rect(RenderSize));
            Children.Add(new Border
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(10)
            });
        }

        /// <summary>
        /// Moves the item to the specified position
        /// </summary>
        /// <param name="vector">Relative change</param>
        /// <param name="view">The containing view</param>
        public void MoveToLocation(Point vector, Panel view)
        {
            var rect = GetRelativeRect(this);
            rect.X = vector.X;
            rect.Y = vector.Y;
            SetRelativeRect(this, rect);
            view.InvalidateArrange();
        }

        public void SetX(Double NewX, Panel view)
        {
            MoveToLocation(new Point(NewX, GetRelativeRect(this).Y), view);
        }
        public void SetY(Double NewY, Panel view)
        {
            MoveToLocation(new Point(GetRelativeRect(this).X, NewY), view);
        }
        public void SetWidth(Double NewWidth, Panel view)
        {
            var rect = GetRelativeRect(this);
            rect.Width = NewWidth;
            SetRelativeRect(this, rect);
            view.InvalidateArrange();
        }
        public void SetHeight(Double NewHeight, Panel view)
        {
            var rect = GetRelativeRect(this);
            rect.Height = NewHeight;
            SetRelativeRect(this, rect);
            view.InvalidateArrange();
        }

        /// <summary>
        /// Recalculates the relative location according to the specified Viewport. Useful for resetting Margin offsets but keeping translation.
        /// </summary>
        /// <param name="view">The containing Viewport</param>
        private void RecalculateRelative(Panel view, bool ResetMargin = true)
        {
            Rect RelativeRect = GetRelativeRect(this);
            Point AbsLocation = TranslatePoint(new Point(0, 0), view);
            RelativeRect.X = (AbsLocation.X / view.ActualWidth) + (RelativeRect.Width / 2);
            RelativeRect.Y = (AbsLocation.Y / view.ActualHeight) + (RelativeRect.Height / 2);
            SetRelativeRect(this, RelativeRect);
            if (ResetMargin)
                Margin = new Thickness(0);

            // RelativeRect updated with new calculations, now force the containing view to rearrange
            view.InvalidateArrange();
        }

        public static void SetRelativeRect(UIElement element, Rect rect)
        {
            RelativeLayoutPanel.SetRelativeRect(element, rect);
        }
        public static Rect GetRelativeRect(UIElement element)
        {
            return RelativeLayoutPanel.GetRelativeRect(element);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 1)
            {
                var Child = Children[0];
                var rect = new Rect(0, 0, finalSize.Width, finalSize.Height);

                Child.Arrange(rect);
            }

            return finalSize;
        }
    }

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
