using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CliWrap;
using DocumentFormat.OpenXml;

namespace ManimInteractive
{
    public static class Common
    {
        /// <summary>
        /// Creates Color from HEX code
        /// NOTE: Does not read Alpha, only RGB
        /// </summary>
        /// <param name="hex">HEX code string</param>
        public static Color ColorFromHex(string hex)
        {
            try
            {
                hex = hex.Replace("#", string.Empty);
                byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
                byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
                byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
                return Color.FromArgb(255, r, g, b);
            }
            catch (FormatException)
            {
                return Colors.White;
            }
        }

        /// <summary>
        /// Creates Brush from HEX code
        /// </summary>
        /// <param name="hex">HEX code string</param>
        /// <returns></returns>
        public static SolidColorBrush BrushFromHex(string hex)
        {
            return new SolidColorBrush(ColorFromHex(hex));
        }

        public static Color Desaturate(this Color color, double amount)
        {
            return Color.FromArgb(
                Convert.ToByte(color.A * amount),
                color.R,
                color.G,
                color.B
            );
        }

        public static string FindManimColorMatch(Color color)
        {
            string targetColor = color.ToString();

            foreach (string mcolor in ManimHelper.Colors.Values)
            {
                if (mcolor == targetColor)
                {
                    int colorindex = ManimHelper.Colors.Values.ToList().IndexOf(mcolor);
                    return ManimHelper.Colors.Keys.ToList()[colorindex];
                }
            }
            return "";
        }

        /// <summary>
        /// Opens the console application and runs the specified command
        /// </summary>
        /// <param name="cmd">The command line executeble (when in doubt use "cmd.exe")</param>
        /// <param name="args">Commands and arguments</param>
        public static void RunCMDOld(string cmd, string args, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
        {
            //Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = windowStyle; // Change to 'Hidden' for release
            startInfo.WorkingDirectory = ManimHelper.ManimDirectory;
            startInfo.FileName = cmd;
            startInfo.Arguments = "/C " + args;

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }

        /// <summary>
        /// Opens the console application and runs the specified command
        /// </summary>
        /// <param name="cmd">The command line executeble (when in doubt use "cmd.exe")</param>
        /// <param name="args">Commands and arguments</param>
        public static async Task<CliWrap.Models.ExecutionResult> RunCMD(string cmd, string args, bool showWindow = true)
        {
            Console.WriteLine($"Command: {cmd} /C {args}");
            var window = new RenderProgressWindow();
            var cli = Cli.Wrap(cmd)
                .SetStandardOutputCallback(l => Console.WriteLine($"StdOut> {l}")) // triggered on every line in stdout
                .SetStandardErrorCallback(l => window.UpdateProgress(l)) // triggered on every line in stderr
                .SetArguments("/C " + args)
                .SetWorkingDirectory(ManimHelper.ManimDirectory);
            window.Show();
            var result = await cli.ExecuteAsync();
            window.Close();

            var exitCode = result.ExitCode;
            var stdOut = result.StandardOutput;
            var stdErr = result.StandardError;
            var startTime = result.StartTime;
            var exitTime = result.ExitTime;
            var runTime = result.RunTime;

            return result;
        }
    }

    public class EventLengthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan timelineDuration = (TimeSpan)values[0];
            TimeSpan relativeTime = (TimeSpan)values[1];
            double containerWidth = (double)values[2];
            double factor = relativeTime.TotalSeconds / timelineDuration.TotalSeconds;
            double rval = factor * containerWidth;

            if (targetType == typeof(Thickness))
            {
                return new Thickness(rval, 0, 0, 0);
            }
            else
            {
                return rval;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class StringExtensions
    {
        public static string[] Lines(this string s)
        {
            string[] sep = { @"\r\n" };
            return s.Split(sep, StringSplitOptions.None);
        }
    }
}
