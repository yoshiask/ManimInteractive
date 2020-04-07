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
using Microsoft.WindowsAPICodePack.Dialogs;
using ManimLib;

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

            if (Environment.GetEnvironmentVariable("MANIM_PATH") == null)
                Environment.SetEnvironmentVariable("MANIM_PATH", 
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "manim"),
                    EnvironmentVariableTarget.User
                );
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Check for required Python version
            var versions = GetPythonVersions();
            if (versions.Count < 1)
            {
                throw new Exception("No versions of Python are installed.");
            }

            // Get manim color palette
            FillColorSelectBox.Items.Clear();
            foreach (KeyValuePair<string, string> pair in ManimHelper.PYColors)
            {
                FillColorSelectBox.Items.Add(new ComboBoxItem()
                {
                    Background = Common.BrushFromHex(pair.Value),
                    Content = pair.Key
                });
            }

            // Set Display Canvas size
            DisplayCanvas.Width = ManimHelper.PixelWidth;
            DisplayCanvas.Height = ManimHelper.PixelHeight;

            // Create Manim Interactive directory
            ReloadManimLocationBox();
            Directory.CreateDirectory(System.IO.Path.Combine(ManimHelper.ManimDirectory, @"interactive"));

            // Load manim drawings
            foreach (string drawing in ManimHelper.ManimDrawings)
            {
                Console.WriteLine("     > " + drawing);
            }
        }

        #region Python
        private string SceneName = "DefaultScene";

        private string GenerateScene(string sceneName)
        {
            string pythonScene = ManimHelper.PythonSceneHeader + $"class {SceneName}(GraphScene):\r\n";
            if (SceneGraph != null)
            {
                pythonScene += SceneGraph.GetPyInitializer(ManimHelper.PY_TAB);
            }

            pythonScene += $"{ManimHelper.PY_TAB}def construct(self):\r\n";

            UIElementCollection elements = DisplayCanvas.Children;
            foreach (FrameworkElement item in elements)
            {
                var shape = item as ManimHelper.IMobject_Shape;
                if (shape != null && shape as ManimHelper.Mobject_Graph == null)
                {
                    switch (shape.Visibility)
                    {
                        case Visibility.Visible:
                            pythonScene += shape.GetPyInitializer(ManimHelper.PY_TAB + ManimHelper.PY_TAB);
                            pythonScene += "\r\n\r\n";
                            break;

                        case Visibility.Collapsed:
                            DisplayCanvas.Children.Remove(item);
                            break;
                    }
                }
            }
            pythonScene += $"\r\n";

            foreach (FrameworkElement item in elements)
            {
                var shape = item as ManimHelper.IMobject_Shape;
                if (shape != null)
                {
                    if (shape.Visibility == Visibility.Visible)
                    {
                        if (shape.GetType() == typeof(ManimHelper.Mobject_Ellipse))
                        {
                            var ellipseobj = shape as ManimHelper.Mobject_Ellipse;
                            pythonScene += $"{ManimHelper.PY_TAB}{ManimHelper.PY_TAB}{ellipseobj.GetDrawBorderThenFillAnim()}\r\n";
                        }
                        else if (shape.GetType() == typeof(ManimHelper.Mobject_Rectangle))
                        {
                            var rectobj = shape as ManimHelper.Mobject_Rectangle;
                            pythonScene += $"{ManimHelper.PY_TAB}{ManimHelper.PY_TAB}{rectobj.GetDrawBorderThenFillAnim()}\r\n";
                        }
                        else if (shape.GetType() == typeof(ManimHelper.Mobject_Text))
                        {
                            var textobj = shape as ManimHelper.Mobject_Text;
                            pythonScene += $"{ManimHelper.PY_TAB}{ManimHelper.PY_TAB}{textobj.GetWriteAnim()}\r\n";
                        }
                        else if (shape.GetType() == typeof(ManimHelper.Mobject_TeX))
                        {
                            var texobj = shape as ManimHelper.Mobject_TeX;
                            pythonScene += $"{ManimHelper.PY_TAB}{ManimHelper.PY_TAB}{texobj.GetWriteAnim()}\r\n";
                        }
                        else if (shape.GetType() == typeof(ManimHelper.Mobject_PiCreature))
                        {
                            var piobj = shape as ManimHelper.Mobject_PiCreature;
                            pythonScene += $"{ManimHelper.PY_TAB}{ManimHelper.PY_TAB}{piobj.GetFadeInAnim()}\r\n";
                        }
                        else if (shape.GetType() == typeof(ManimHelper.Mobject_Graph))
                        {
                            var graphobj = shape as ManimHelper.Mobject_Graph;
                            pythonScene += graphobj.GetShowCreationAnim($"{ManimHelper.PY_TAB}{ManimHelper.PY_TAB}");
                            pythonScene += "\r\n";
                        }
                    }
                }
            }

            Console.Write(pythonScene);
            return pythonScene;
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

        public static bool IsPythonInstalled()
        {
            var versions = GetPythonVersions();
            if (versions.Count > 0)
                return true;
            return false;
        }
        public static List<Version> GetPythonVersions()
        {
            string pattern = @"[a-zA-Z]* ([0-9]*).([0-9]*).([0-9]*)$";
            string input = @"Python 3.7.3";

            var matches = System.Text.RegularExpressions.Regex.Matches(input, pattern);
            List<Version> versions = new List<Version>();
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);
                versions.Add(new Version(
                    Convert.ToInt32(m.Groups[1].Value),
                    Convert.ToInt32(m.Groups[2].Value),
                    Convert.ToInt32(m.Groups[3].Value)
                ));
            }
            return versions;
        }
        #endregion

        #region Media Playback
        bool IsPreviewing = false;
        private async void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPreviewing)
            {
                Player.Stop();
                IsPreviewing = false;
                //Player.Source = null;

                PreviewButton.Icon = new Uri(@"pack://application:,,,/Assets/Icons/Run_16x.png");
                PreviewButton.LargeIcon = new Uri(@"pack://application:,,,/Assets/Icons/Run_32x.png");
                PreviewButton.Header = "Start Preview";

                PlaybackPlayButton.IsEnabled = false;
                PlaybackStopButton.IsEnabled = false;
                Player.Visibility = Visibility.Collapsed;
                DisplayCanvas.Visibility = Visibility.Visible;
            }
            else
            {
                try
                {
                    File.WriteAllText(System.IO.Path.Combine(ManimHelper.InteractiveDirectory, "exported_scenes.py"), GenerateScene(SceneName));
                    //Common.RunCMD("cmd.exe", $@"py -3 extract_scene.py testing\exported_scenes.py {SceneName} -pmg", ProcessWindowStyle.Normal);
                    string vpath = await ManimHelper.RenderVideo(SceneName, new ManimHelper.ExportOptions()
                    {
                        LowQuality = true,
                        SavePNG = true
                    });
                    Console.WriteLine(vpath);

                    Player.Stretch = Stretch.Uniform;
                    Player.Visibility = Visibility.Visible;
                    Player.Source = new Uri(vpath, UriKind.Absolute);
                    Player.Play();
                }
                catch (DirectoryNotFoundException)
                {
                    var result = MessageBox.Show("Manim not found on your system.\r\nSelect OK to locate manim manually.", "Manim Error", MessageBoxButton.OKCancel, MessageBoxImage.Error, MessageBoxResult.OK);
                    if (result == MessageBoxResult.OK)
                    {
                        ShowManimLocator();
                    }
                }
            }
        }

        private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Console.WriteLine("Failed to load preview from " + Player.Source.LocalPath);
            Console.WriteLine("\tError: " + e.ErrorException);
        }

        private void Player_LoopMedia(object sender, RoutedEventArgs e)
        {
            Player.Stop();
            Player.Play();
        }

        private async void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            string scenePath = System.IO.Path.Combine(ManimHelper.ManimDirectory, "interactive\\exported_scenes.py");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(scenePath));
            File.WriteAllText(scenePath, GenerateScene(SceneName));
            //Common.RunCMD("cmd.exe", $@"py -3 extract_scene.py testing\exported_scenes.py {SceneName}", ProcessWindowStyle.Normal);
            string renderPath = await ManimHelper.RenderVideo(SceneName);
            Uri video = new Uri(renderPath, UriKind.Absolute);

            Player.Stop();
            Player.Stretch = Stretch.Uniform;
            Player.MediaEnded += Player_LoopMedia;
            Player.Visibility = Visibility.Visible;
            Player.Source = video;
            IsPreviewing = true;
            Player.Play();
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            PreviewButton.Icon = new Uri(@"pack://application:,,,/Assets/Icons/Stop_16x.png");
            PreviewButton.LargeIcon = new Uri(@"pack://application:,,,/Assets/Icons/Stop_32x.png");
            PreviewButton.Header = "Stop Preview";
            DisplayCanvas.Visibility = Visibility.Collapsed;
            PlaybackPlayButton.IsEnabled = true;
            PlaybackStopButton.IsEnabled = true;
            IsPreviewing = true;
            IsPlaying = true;
        }

        private bool IsPlaying = false;
        private void PlaybackPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                Player.Stop();
                Player.Play();
            }
            else
            {
                Player.Play();
            }
        }

        private void PlaybackStopButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Pause();
            IsPlaying = false;
        }
        #endregion

        #region Shape Creation
        private int curZIndex = 99;
        private int curID = 0;
        private void NewRectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectMobject(ManimHelper.Mobject_Rectangle.Draw("Rectangle" + curID, DisplayCanvas, new Rect(0.5, 0.5, 0.2, 0.2), "WHITE", "YELLOW_E", curZIndex++));
            curID++;
        }
        private void NewEllipseButton_Click(object sender, RoutedEventArgs e)
        {
            SelectMobject(ManimHelper.Mobject_Ellipse.Draw("Ellipse" + curID, DisplayCanvas, new Rect(0.5, 0.5, 0.2, 0.2), "WHITE", "YELLOW_E", curZIndex++));
            curID++;
        }
        private void NewTextboxButton_Click(object sender, RoutedEventArgs e)
        {
            SelectMobject(ManimHelper.Mobject_Text.Draw("Text" + curID, DisplayCanvas, new Rect(0.5, 0.5, 0.2, 0.2), "3blue1brown", "WHITE", 64, curZIndex++));
            curID++;
        }
        private void NewPiCreatureButton_Click(object sender, RoutedEventArgs e)
        {
            SelectMobject(ManimHelper.Mobject_PiCreature.Draw("PiCreature" + curID, DisplayCanvas, new Rect(0.5, 0.5, 0.2, 0.2), "DARK_BLUE", curZIndex++));
            curID++;
        }
        private void NewTeXboxButton_Click(object sender, RoutedEventArgs e)
        {
            SelectMobject(ManimHelper.Mobject_TeX.Draw("TeX" + curID, DisplayCanvas, new Rect(0.5, 0.5, 0.2, 0.2), @"x^2 = 0", "WHITE", curZIndex++));
            curID++;
        }

        private ManimHelper.Mobject_Graph SceneGraph;
        private async void NewGraphButton_Click(object sender, RoutedEventArgs e)
        {
            SceneGraph = await ManimHelper.Mobject_Graph.Draw("Graph" + curID, DisplayCanvas, new Rect(0.5, 0.5, 1.0, 1.0), "lambda x : (np.sin(x**2))", "DARK_BLUE", -Math.PI, Math.PI, -3, 3, curZIndex++);
            SelectMobject(SceneGraph);
            curID++;
            NewGraphButton.IsEnabled = false;
        }
        #endregion

        #region Selected Visual
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
            // Deselect any currently selected objects
            Deselect();
            MainRibbon.SelectedTabItem = FormatTab;

            // Set UI
            SelectedVisual = shape;
            IsLoadingSelected = true;
            SelectedVisual.DrawSelectionBorder();
            drawingGroup.Visibility = Visibility.Visible;
            SelectedVisual.DragStateChanged += SelectedVisual_DragStateChanged;
            UpdateDrawingToolsUI(SelectedVisual);
            IsLoadingSelected = false;
        }
        private void Deselect()
        {
            if (SelectedVisual != null)
            {
                SelectedVisual.DrawSelectionBorder(0);
                SelectedVisual = null;
            }
            foreach (Fluent.RibbonContextualTabGroup group in MainRibbon.ContextualGroups)
            {
                group.Visibility = Visibility.Collapsed;
            }
        }
        private void SelectedVisual_DragStateChanged(object sender, DragStateChanged e)
        {
            UpdateDrawingToolsUI(SelectedVisual);
        }
        #endregion

        #region Ribbon Stuff
        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var obj = SelectedVisual;
            Deselect();
            if (obj.GetType() == typeof(ManimHelper.Mobject_Graph))
            {
                NewGraphButton.IsEnabled = true;
            }
            DisplayCanvas.Children.Remove(obj);
            obj = null;
        }
        private void BringForwardButton_Click(object sender, RoutedEventArgs e)
        {
            int index = Panel.GetZIndex(SelectedVisual);
            Panel.SetZIndex(SelectedVisual, index + 1);
        }
        private void SendBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            int index = Panel.GetZIndex(SelectedVisual);
            Panel.SetZIndex(SelectedVisual, index - 1);
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
                ItemHeightBox.Text = Math.Round(item.GetRelativeRect().Height * DisplayCanvas.ActualHeight).ToString();
                ItemWidthBox.Text = Math.Round(item.GetRelativeRect().Width * DisplayCanvas.ActualWidth).ToString();
                ItemXBox.Text = Math.Round(item.GetRelativeRect().X * DisplayCanvas.ActualWidth).ToString();
                ItemYBox.Text = Math.Round(item.GetRelativeRect().Y * DisplayCanvas.ActualHeight).ToString();

                FillColorSelectBox.SelectedIndex = ManimHelper.Colors.Keys.ToList().IndexOf(item.Fill);
                FillColorDisplay.Background = Common.BrushFromHex(ManimHelper.Colors[item.Fill]);

                if (SelectedVisual.GetType() == typeof(ManimHelper.Mobject_Text))
                {
                    textGroup.Visibility = Visibility.Visible;
                    TextObjContentBox.Text = (SelectedVisual as ManimHelper.Mobject_Text).TextContent;
                }
                else if (SelectedVisual.GetType() == typeof(ManimHelper.Mobject_TeX))
                {
                    texGroup.Visibility = Visibility.Visible;
                    TeXObjContentBox.Text = (SelectedVisual as ManimHelper.Mobject_TeX).TextContent;
                }
                else if (SelectedVisual.GetType() == typeof(ManimHelper.Mobject_Graph))
                {
                    graphGroup.Visibility = Visibility.Visible;
                    var graph = (SelectedVisual as ManimHelper.Mobject_Graph);
                    GraphEquationContentBox.Text = graph.Config["function"].ToString();
                    GraphXMinBox.Text = graph.Config["x_min"].ToString();
                    GraphXMaxBox.Text = graph.Config["x_max"].ToString();
                    GraphYMinBox.Text = graph.Config["y_min"].ToString();
                    GraphYMaxBox.Text = graph.Config["y_max"].ToString();
                }
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
                try
                {
                    double NewRelative = Convert.ToDouble(ItemXBox.Text) / DisplayCanvas.Width;
                    SelectedVisual.SetX(NewRelative, DisplayCanvas);
                }
                catch (Exception)
                {
                    ItemXBox.Text = "0";
                }
            }
        }
        private void ItemYBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null)
            {
                try
                {
                    double NewRelative = Convert.ToDouble(ItemYBox.Text) / DisplayCanvas.Height;
                    SelectedVisual.SetY(NewRelative, DisplayCanvas);
                }
                catch (Exception)
                {
                    ItemYBox.Text = "0";
                }
            }
        }
        private void ItemWidthBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null && sender.GetType() != typeof(TextBox))
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(ItemWidthBox.Text))
                    {
                        double NewRelative = Convert.ToDouble(ItemWidthBox.Text) / DisplayCanvas.Width;
                        SelectedVisual.SetByWidth(NewRelative, DisplayCanvas);
                        ItemHeightBox.Text = SelectedVisual.Height.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    ItemWidthBox.Text = "0";
                }
            }
        }
        private void ItemHeightBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null && sender.GetType() != typeof(TextBox))
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(ItemWidthBox.Text))
                    {
                        double NewRelative = Convert.ToDouble(ItemHeightBox.Text) / DisplayCanvas.Height;
                        SelectedVisual.SetByHeight(NewRelative, DisplayCanvas);
                        ItemWidthBox.Text = SelectedVisual.Width.ToString();
                    }
                }
                catch (Exception)
                {
                    ItemHeightBox.Text = "0";
                }
            }
        }

        private void TextObjContentBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null)
            {
                (SelectedVisual as ManimHelper.Mobject_Text).TextContent = TextObjContentBox.Text;
            }
        }
        private void TeXObjContentBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoadingSelected && SelectedVisual != null)
            {
                (SelectedVisual as ManimHelper.Mobject_TeX).TextContent = TeXObjContentBox.Text;
            }
        }

        private void RenderGraphPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var graph = SelectedVisual as ManimHelper.Mobject_Graph;
            graph.UpdateGraphPreview(new Dictionary<string, object>()
            {
                { "function", GraphEquationContentBox.Text },
                { "function_color", (FillColorSelectBox.SelectedItem as ComboBoxItem).Content.ToString() },
                { "center_point", 0 },
                { "x_min", GraphXMinBox.Text},
                { "x_max", GraphXMaxBox.Text},
                { "y_min", GraphYMinBox.Text},
                { "y_max", GraphYMaxBox.Text},
                { "graph_origin", "0" },
            });
        }
        #endregion

        #region Manim Location
        private void LocateManimButton_Click(object sender, RoutedEventArgs e)
        {
            ShowManimLocator();
            ReloadManimLocationBox();
        }
        private void ManimLocationBox_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadManimLocationBox();
        }
        public string ShowManimLocator()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                ManimHelper.ManimDirectory = dialog.FileName;
                ReloadManimLocationBox();
                return dialog.FileName;
            }
            ReloadManimLocationBox();
            return "";
        }
        public void ReloadManimLocationBox()
        {
            if (String.IsNullOrWhiteSpace(ManimHelper.ManimDirectory))
            {
                ManimLocationBox.Text = "Manim not found";
                ManimLocationBox.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                ManimLocationBox.Text = ManimHelper.ManimDirectory;
                ManimLocationBox.Foreground = new SolidColorBrush(Colors.White);
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

            public Color this[String colorName] {
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

            public Color this[UInt32 colorType] {
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
}
