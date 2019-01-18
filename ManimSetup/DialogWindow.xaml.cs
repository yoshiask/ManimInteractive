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

namespace ManimSetup
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindowResult Result;

        public DialogWindow()
        {
            InitializeComponent();
        }

        public DialogWindow(string message, DialogWindowButtons style, bool showTitleBarButtons = true)
        {
            InitializeComponent();
            MessageBlock.Text = message;
            if (!showTitleBarButtons)
                TitleBarButtons.Visibility = Visibility.Collapsed;
            switch (style)
            {
                case DialogWindowButtons.OK_Cancel:
                    ButtonPrimary.Content = "OK";
                    ButtonSecondary.Content = "Cancel";
                    break;

                case DialogWindowButtons.Yes_No:
                    ButtonPrimary.Content = "Yes";
                    ButtonSecondary.Content = "No";
                    break;
            }
        }
        public DialogWindow(string message, string title, DialogWindowButtons style, bool showTitleBarButtons = true)
        {
            InitializeComponent();
            MessageBlock.Text = message;
            TitleBlock.Text = title;
            if (!showTitleBarButtons)
                TitleBarButtons.Visibility = Visibility.Collapsed;
            switch (style)
            {
                case DialogWindowButtons.OK_Cancel:
                    ButtonPrimary.Content = "OK";
                    ButtonSecondary.Content = "Cancel";
                    break;

                case DialogWindowButtons.Yes_No:
                    ButtonPrimary.Content = "Yes";
                    ButtonSecondary.Content = "No";
                    break;
            }
        }

        public enum DialogWindowResult
        {
            Primary,
            Secondary
        }
        public enum DialogWindowButtons
        {
            OK_Cancel,
            Yes_No,
        }

        private void ButtonPrimary_Click(object sender, RoutedEventArgs e)
        {
            Result = DialogWindowResult.Primary;
            Close();
        }

        private void ButtonSecondary_Click(object sender, RoutedEventArgs e)
        {
            Result = DialogWindowResult.Secondary;
            Close();
        }

        private void TitleBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
