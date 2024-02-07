using ACWSSK.App_Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for PaymentPageView.xaml
    /// </summary>
    public partial class PaymentPageView : UserControl
    {
        private const string TraceCategory = "ACWSSK.View.Payment.PaymentPageView";

        public PaymentPageView()
        {
            InitializeComponent();
        }

        private void MediaElement_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            PlayMedia(sender);
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayMedia(sender);
        }

        private void PlayMedia(object sender)
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "PlayMedia starting...", TraceCategory);

            try
            {
                MediaElement media = sender as MediaElement;

                if (media != null && media.Source != null && media.Visibility == System.Windows.Visibility.Visible)
                {
                    media.Position = TimeSpan.FromSeconds(0);
                    media.Play();
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PlayMedia: {0}", ex.ToString()), TraceCategory);
            }
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "PlayMedia completed.", TraceCategory);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(txtBarcode);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(txtBarcode);
        }
    }
}
