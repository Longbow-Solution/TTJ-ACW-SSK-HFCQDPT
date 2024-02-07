using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Documents;
using System.ComponentModel;

namespace IOBoard
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SerialPortHandler
    {
        #region Trace
        protected TraceSwitch swcTraceLevel = new TraceSwitch("SwcTraceLevel", "Trace Switch Level");
        private string traceCategory = "SerialPort";
        #endregion

        #region Enum
        public enum TransmissionType { Text, Hex }

        public enum MessageType { Incoming, Outgoing, Normal, Warning, Error, MasterMessage }
        #endregion

        #region Variables
		//public static string appMode = string.Empty;
        private Brush[] MessageColor = { Brushes.Blue, Brushes.Green, Brushes.Black, Brushes.Orange, Brushes.Red, Brushes.Purple };
        private TransmissionType transType = TransmissionType.Hex;
        
        protected SerialPort comPort = new SerialPort();
        #endregion

        #region Properties
        public TransmissionType SelectedTransmissionType
        {
            get { return transType; }
            set { transType = value; }
        }
        #endregion

        #region Constructors
        protected SerialPortHandler(){}
        #endregion

        #region Virtual WriteData & DataReceived Events
        protected virtual void comPort_WriteData(byte[] buffer)
        {
            comPort.Write(buffer, 0, buffer.Length);
        }

        protected virtual void comPort_WriteData(string text)
        {
            switch (SelectedTransmissionType)
            {
                case TransmissionType.Text:
                    comPort.Write(text);
                    break;
                case TransmissionType.Hex:
                    byte[] newMsg = ConvertHexToByteArray(text);
                    comPort.Write(newMsg, 0, newMsg.Length);
                    break;
                default:
                    comPort.Write(text);
                    break;
            }
        }

        protected virtual void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e) { }
        #endregion

        #region Serial Port Handler
        public bool OpenPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
			try
			{
				if (comPort != null && comPort.IsOpen)
					comPort.Close();

				comPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
				comPort.Handshake = Handshake.None;
				comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
				comPort.Open();
				
				return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] OpenPort : {0}", ex.ToString()), "IOBoard");
				return false;
			}
        }

        public bool ClosePort()
        {
            Trace.WriteLineIf(swcTraceLevel.TraceInfo, "ClosePort Starting...", traceCategory);
			try
			{
				if (!comPort.IsOpen)
					return true;

				comPort.DataReceived -= new SerialDataReceivedEventHandler(comPort_DataReceived);
				comPort.Close();

				Trace.WriteLineIf(swcTraceLevel.TraceInfo, "ClosePort: Port closed at " + DateTime.Now, traceCategory);
				Trace.WriteLineIf(swcTraceLevel.TraceInfo, "ClosePort Completed", traceCategory);

				return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] ClosePort : {0}", ex.ToString()), "IOBoard");
				return false;
			}
        }
        #endregion

        #region General 
        private byte[] ConvertHexToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            byte[] comBuffer = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length; i += 2)
                comBuffer[i / 2] = (byte)Convert.ToByte(hex.Substring(i, 2), 16);

            return comBuffer;
        }
        #endregion

        #region Rich Text Box - Used In Demo Mode
        //private RichTextBox _rtbSource;
        //public RichTextBox rtbSource
        //{
        //    get { return _rtbSource; }
        //    set { _rtbSource = value; }
        //}

        //protected RichTextBox _rtbMessage;
        //public RichTextBox rtbMessage
        //{
        //    get { return _rtbMessage; }
        //    set { _rtbMessage = value; }
        //}

        //public void DisplaySource(MessageType messageType, string source)
        //{
        //    if (appMode.ToUpper() == "DEMO")
        //    {
        //        rtbSource.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        //        {
        //            TextRange tr = new TextRange(rtbSource.Document.ContentEnd, rtbSource.Document.ContentEnd);
        //            tr.Text = source;
        //            tr.ApplyPropertyValue(TextElement.FontWeightProperty, "Bold");
        //            tr.ApplyPropertyValue(TextElement.ForegroundProperty, MessageColor[(int)messageType]);
        //            rtbSource.ScrollToEnd();
        //        }));
        //    }
        //}

        //public void DisplayMessage(MessageType messageType, string message)
        //{
        //    if (appMode.ToUpper() == "DEMO")
        //    {
        //        rtbMessage.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        //        {
        //            TextRange tr = new TextRange(rtbMessage.Document.ContentEnd, rtbMessage.Document.ContentEnd);
        //            tr.Text = message + " ";
        //            tr.ApplyPropertyValue(TextElement.FontWeightProperty, "Bold");
        //            tr.ApplyPropertyValue(TextElement.ForegroundProperty, MessageColor[(int)messageType]);
        //            rtbMessage.ScrollToEnd();
        //        }));
        //    }
        //}
        #endregion
    }
}
