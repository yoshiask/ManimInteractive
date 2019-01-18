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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ManimSetup.Frames
{
    /// <summary>
    /// Interaction logic for ProgressFrame.xaml
    /// </summary>
    public partial class ProgressFrame : Grid
    {
        public ProgressFrame()
        {
            InitializeComponent();

            var doc = new FlowDocument();
            ConsoleBox.Document = doc;
        }

        public void WriteLine(string text, Color foreground)
        {
            ConsoleBox.Document.Blocks.Add(
                new Paragraph(
                    new Run(text)
                    {
                        Foreground = new SolidColorBrush(foreground)
                    }
                )
            );
        }
    }
}
