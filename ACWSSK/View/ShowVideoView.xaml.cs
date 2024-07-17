using ACWSSK.App_Code;
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

namespace ACWSSK.View
{
    /// <summary>
    /// Interaction logic for ShowVideoView.xaml
    /// </summary>
    public partial class ShowVideoView : UserControl
    {
        public ShowVideoView()
        {
            InitializeComponent();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = sender as MediaElement;
            GeneralVar.vmMainWindow._WaitShowVideo.Set(); // to go back to page
        }
    }
}
