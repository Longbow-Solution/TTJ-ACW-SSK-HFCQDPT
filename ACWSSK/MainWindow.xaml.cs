using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using ACWSSK.App_Code;
using System.Diagnostics;


namespace ACWSSK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (GeneralVar.IsFullScreen)
            {
                this.WindowStyle = System.Windows.WindowStyle.None;
                this.WindowState = System.Windows.WindowState.Maximized;
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
            }

            if (GeneralVar.IsTopMost)
            {
                this.Topmost = true;
                Mouse.OverrideCursor = System.Windows.Input.Cursors.None;
            }
        }
    }
}
