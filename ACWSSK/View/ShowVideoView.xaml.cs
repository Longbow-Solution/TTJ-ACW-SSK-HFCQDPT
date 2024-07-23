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
using System.Windows.Threading;

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

        private void HandleMediaFailure()
        {
            GeneralVar.vmMainWindow._WaitShowVideo.Set();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = sender as MediaElement;
            GeneralVar.vmMainWindow._WaitShowVideo.Set(); // to go back to page
        }

        public void PlayAdvertisementRequest()
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    //AdvertisementVideo.Stop();
                    AdvertisementVideo.Play();
                }));
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PlayAdvertisementRequest = {0}", ex.ToString()), "ShowVideoView");
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "MediaElement_MediaOpened", "ShowVideoView");
            MediaElement mediaElement = sender as MediaElement;

            if (mediaElement != null)
            {
                // Log the current properties of the MediaElement
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, $"MediaElement Source: {mediaElement.Source}", "ShowVideoView");
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, $"NaturalVideoWidth: {mediaElement.NaturalVideoWidth}", "ShowVideoView");
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, $"NaturalVideoHeight: {mediaElement.NaturalVideoHeight}", "ShowVideoView");
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, $"Position: {mediaElement.Position}", "ShowVideoView");
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, $"HasVideo: {mediaElement.HasVideo}", "ShowVideoView");
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, $"HasAudio: {mediaElement.HasAudio}", "ShowVideoView");

                // Additional checks
                if (mediaElement.NaturalVideoWidth > 0 && mediaElement.NaturalVideoHeight > 0)
                {
                    // Video dimensions are valid, you can start playing the video
                    //mediaElement.Play();
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "Video started playing.", "ShowVideoView");
                }
                else
                {
                    // Video dimensions are not valid, log an error
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, "Video dimensions are not valid.", "ShowVideoView");
                }
            }
            else
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, "MediaElement_MediaOpened: Sender is not a MediaElement.", "ShowVideoView");
            }
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, $"MediaElement: MediaFailed - {e.ErrorException}", "ShowVideoView");
            HandleMediaFailure();
        }
    }
}
