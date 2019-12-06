using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LibGit2Sharp;
using ManimSetup.Frames;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ManimSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Main;
        public static string assemblyPath = AppDomain.CurrentDomain.BaseDirectory;// System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        ProgressFrame CenterConsole;
        List<string> PackagesToInstall = new List<string>();

        public delegate void ConsoleWriteCallback(string message, Color foreground);

        public MainWindow()
        {
            InitializeComponent();
            Main = this;
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

        private void TitleGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static void CancelSetup()
        {
            var dialog = new DialogWindow("Are you sure you want to cancel setup?", "Manim Setup", DialogWindow.DialogWindowButtons.Yes_No, false);
            dialog.ShowDialog();

            switch (dialog.Result)
            {
                case DialogWindow.DialogWindowResult.Primary:
                    Application.Current.MainWindow.Close();
                    break;

                case DialogWindow.DialogWindowResult.Secondary:

                    break;
            }
        }

        public void ResumeSetup()
        {
            if (PackagesToInstall.Count > 0)
            {
                foreach (string package in PackagesToInstall)
                {
                    switch (package)
                    {
                        case "python":

                            break;

                        case "manim":
                            DownloadManim();
                            break;

                        case "cairo":
                            InstallCairo();
                            break;

                        case "ffmpeg":
                            InstallFFmpeg();
                            break;

                        case "latex":
                            InstallMiKTeX();
                            break;
                    }
                }
            }
        }

        private void WriteLine(string message, Color foreground)
        {
            CenterConsole.WriteLine(message, foreground);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelSetup();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CancelSetup();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void InstallDirButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.EnsurePathExists = true;
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                InstallDirBox.Text = dialog.FileName;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Show console frame
            CenterConsole = new ProgressFrame()
            {
                Height = CenterGrid.Height,
                Width = CenterGrid.Width,
                Margin = new Thickness(30, 20, 30, 90)
            };
            TopGrid.Children.Remove(CenterGrid);
            TopGrid.Children.Add(CenterConsole);
            Grid.SetRow(CenterConsole, 1);

            // Get install directory
            string InstallDir = InstallDirBox.Text;
            Directory.CreateDirectory(InstallDir);

            PackagesToInstall.Add("python");
            PackagesToInstall.Add("manim");

            if (CairoCheckbox.IsChecked.Value)
                PackagesToInstall.Add("cairo");

            if (ffmpegCheckbox.IsChecked.Value)
                PackagesToInstall.Add("ffmpeg");

            if (LatexCheckbox.IsChecked.Value)
                PackagesToInstall.Add("latex");

            Console.SetOut(new ControlWriter(CenterConsole.ConsoleBox));
            ResumeSetup();
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data, Colors.White);
        }
        private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            CenterConsole.WriteLine(e.Data, Colors.DarkRed);
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
        public static bool CheckForPython32(string version)
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
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static bool CheckForPython64(string version)
        {
            string pyPath64 = Registry.GetValue($@"HKLM\SOFTWARE\Wow6432Node\Python\PythonCore\{version}\InstallPath", "(Default)", null) as string;
            if (pyPath64 != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Install Functions
        public static void InstallPython()
        {
            //Main.CenterConsole.WriteLine("Installing Python 3.7.1...", Colors.White);
            Console.WriteLine("Installing Python 3.7.1...");
            if (!CheckForPython32("37"))
            {
                Process procPyInst = new Process();
                procPyInst.StartInfo = new ProcessStartInfo()
                {
                    FileName = assemblyPath + "\\Installers\\python-3.7.1.exe",
                    Arguments = "/qb!"
                };
                procPyInst.OutputDataReceived += Main.Proc_OutputDataReceived;
                procPyInst.ErrorDataReceived += Main.Proc_ErrorDataReceived;
                procPyInst.Start();
                procPyInst.WaitForExit();
                //Main.CenterConsole.WriteLine("Python installation completed!\r\n", Colors.Green);
                Console.WriteLine("Python installation completed!\r\n");
            }
            //Main.CenterConsole.WriteLine("Python 3.7 already installed\r\n", Colors.Green);
            Console.WriteLine("Python 3.7 already installed\r\n");
        }
        public void DownloadManim()
        {
            string InstallDir = InstallDirBox.Text;

            // Download manim
            CenterConsole.WriteLine("Downloading manim from https://github.com/3b1b/manim.git...", Colors.White);
            //Repository.Clone("https://github.com/3b1b/manim.git", InstallDir);
            InstallDir = Path.Combine(InstallDir, "manim");
            CenterConsole.WriteLine($"Manim downloaded to {InstallDir}\r\n", Colors.Green);
        }
        public void InstallCairo()
        {
            CenterConsole.WriteLine("Installing pycairo 1.18.0...", Colors.White);
        }
        public void InstallFFmpeg()
        {
            CenterConsole.WriteLine("Installing ffmpeg v.12/20/2018...", Colors.White);
        }
        public void InstallMiKTeX()
        {
            CenterConsole.WriteLine("Installing MiKTeX 2.9.6850...", Colors.White);
        }
        #endregion
    }

    public class ControlWriter : TextWriter
    {
        private RichTextBox textbox;
        public ControlWriter(RichTextBox textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            textbox.Document.Blocks.Add(
                new Paragraph(
                    new Run(value.ToString())
                    {
                        Foreground = new SolidColorBrush(Colors.White)
                    }
                )
            );
        }

        public override void Write(string value)
        {
            textbox.Document.Blocks.Add(
                new Paragraph(
                    new Run(value)
                    {
                        Foreground = new SolidColorBrush(Colors.White)
                    }
                )
            );
        }

        public override Encoding Encoding {
            get { return Encoding.ASCII; }
        }
    }
}
