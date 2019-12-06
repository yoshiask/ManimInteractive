using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ManimInteractive
{
    /// <summary>
    /// Interaction logic for RenderProgressWindow.xaml
    /// </summary>
    public partial class RenderProgressWindow : Window
    {
        public RenderProgressWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(string message, int percent = -1)
        {
            int p = 0;
            try
            {
                p = percent;
                if (percent == -1)
                {
                    int i = message.IndexOf('%');
                    char[] messageCh = message.ToCharArray();
                    int A, B;
                    int.TryParse(messageCh[i - 1].ToString(), out B);
                    string strA = messageCh[i - 2].ToString();
                    if (!String.IsNullOrWhiteSpace(strA))
                    {
                        int.TryParse(strA, out A);
                        p = A * 10 + B;
                    }
                    else
                    {
                        p = B;
                    }
                    
                    /*if (int.TryParse(message.ToArray()[i - 1].ToString(), out A))
                    {
                        if (int.TryParse(message.ToArray()[i - 2].ToString(), out B))
                            p = int.Parse(A.ToString() + B.ToString());
                        else if (message.ToArray()[i - 2] == ' ')
                            p = A;
                    }*/
                }
                Dispatcher.Invoke(() => Title = $"Rendering... ({p}%)");
                Dispatcher.Invoke(() => ProgressDisplay.IsIndeterminate = false);
            }
            catch {
                Dispatcher.Invoke(() => Title = $"Rendering...");
                Dispatcher.Invoke(() => ProgressDisplay.IsIndeterminate = true);
            }
            
            Dispatcher.Invoke(() => ProgressDisplay.Value = p);
            Dispatcher.Invoke(() => ConsoleBox.AppendText(message + "\r\n"));
            Dispatcher.Invoke(() => ConsoleBox.ScrollToEnd());
            Console.WriteLine(message);
        }
    }
}
