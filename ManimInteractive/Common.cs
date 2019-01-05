using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
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
            hex = hex.Replace("#", string.Empty);
            byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            return Color.FromArgb(255, r, g, b);
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
        public static void RunCMD(string cmd, string args, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
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
}
