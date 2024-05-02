using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ACWSSK.App_Code;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows.Threading;
using IOBoard;

namespace ACWSSK.Controller
{
    public class IOBoardController
    {
        #region Private Variable

		private Edam controller;
        public AutoResetEvent autoSet;
        private AutoResetEvent autoEvent;
        private Thread processIOBoardThread;

        private bool isProcessingIOBoard;
		protected string TraceCategory = "IOBoardController";

        bool isBlinking = false;

        private bool _ReceiptPrinterOn;
        public bool ReceiptPrinterOn
        {
            get { return _ReceiptPrinterOn; }
            set { _ReceiptPrinterOn = value; }
        }

        private bool _PaymentTerminalOn;
        public bool PaymentTerminalOn
        {
            get { return _PaymentTerminalOn; }
            set { _PaymentTerminalOn = value; }
        }

        private bool _BarcodeScannerOn;
        public bool BarcodeScannerOn
        {
            get { return _BarcodeScannerOn; }
            set { _BarcodeScannerOn = value; }
        }

        #endregion

        public bool IOBoardIsNormal;
       
        #region Contructor

        public IOBoardController() 
        {
            controller = new Edam();
            string portName = GeneralVar.IOBoard_PortName;
            int baudRate = int.Parse(GeneralVar.IOBoard_BaudRate);
            Parity parity = (Parity)Enum.Parse(typeof(Parity), GeneralVar.IOBoard_Parity);
            int dataBits = int.Parse(GeneralVar.IOBoard_DataBits);
            StopBits stopBits = (StopBits)Enum.Parse(typeof(StopBits), GeneralVar.IOBoard_StopBits);
						
            if (controller.OpenPort(portName, baudRate, parity, dataBits, stopBits))
            {
                IOBoardIsNormal = true;
            }
            else
            {
                IOBoardIsNormal = false;
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceWarning, "[Warning] IOBoardController: IO Board Controller port failed to open", "ACWSSK.Controller.IOBoardController");
            }
        }

        #endregion

        #region Properties

        public int EntryPassCounter
        {
            get;
            set;
        }

        public int ResetPassCounter
        {
            get;
            set;
        }

        public int EStopPassCounter 
        {
            get;
            set;
        }

        public AutoResetEvent AutoEvent
        {
            get { return autoEvent; }
            private set { autoEvent = value; }
        }

        private static bool _CWErrorWashStatus = false;
        public static bool CWErrorWashStatus
        {
            get { return _CWErrorWashStatus; }
            set
            {
                _CWErrorWashStatus = value;
            }
        }

		private static bool _CWActiveStatus = false;
		public static bool CWActiveStatus
		{
            get { return _CWActiveStatus; }
			set
			{
                _CWActiveStatus = value;
			}
		}

        private static bool _CWTermStatus = false;
        public static bool CWTermStatus
        {
            get { return _CWTermStatus; }
            set
            {
                _CWTermStatus = value;
            }
        }

        private static bool _CWNormalStatus = false;
        public static bool CWNormalStatus
        {
            get { return _CWNormalStatus; }
            set
            {
                _CWNormalStatus = value;
            }
        }

        private static bool _CWErrorOperationStatus = false;
        public static bool CWErrorOperationStatus
        {
            get { return _CWErrorOperationStatus; }
            set
            {
                _CWErrorOperationStatus = value;
            }
        }

        private static bool _CWSensorObjectStatus = false;
        public static bool CWSensorObjectStatus
        {
            get { return _CWSensorObjectStatus; }
            set
            {
                _CWSensorObjectStatus = value;
            }
        }

        private static bool _PMInputError = false;
        public static bool PMInputError
        {
            get { return _PMInputError; }
            set
            {
                _PMInputError = value;
            }
        }

        private static bool _PMInputWashing = false;
        public static bool PMInputWashing
        {
            get { return _PMInputWashing; }
            set
            {
                _PMInputWashing = value;
            }
        }

        #endregion

        #region Public Function

        public bool ClosePort() 
        {
            controller.ClosePort();
            return true;
        }

        public void StartProcessingIOBoard()
        {
            try
            {
                autoEvent = new AutoResetEvent(false);
                autoSet = new AutoResetEvent(false);
                isProcessingIOBoard = true;

                if (GeneralVar.ACWMachineModel == eCarWashMachine.HEFEI)
                {
                    processIOBoardThread = new Thread(new ThreadStart(ProcessIOBoard));
                }
                else if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                {
                    processIOBoardThread = new Thread(new ThreadStart(ProcessIOBoardQD));
                }
                else if (GeneralVar.ACWMachineModel == eCarWashMachine.PENTAMASTER)
                    processIOBoardThread = new Thread(new ThreadStart(ProcessIOBoardPT));


                processIOBoardThread.Start();
            }
            catch (Exception ex) 
            {
                GeneralVar.IOLastError = ex.Message;
                GeneralVar.IOBException = new Exception("IOB00", new Exception(ex.Message));
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error]StartProcessingIOBoard : {0}", ex.ToString()), "IOBoardController");
            }
        }

        public void StopProcessingIOBoard()
        {
            try
            {
                isProcessingIOBoard = false;

                if (processIOBoardThread.ThreadState == System.Threading.ThreadState.Running || processIOBoardThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                {
                    processIOBoardThread.Interrupt();

                    if (!processIOBoardThread.Join(5000))
                        processIOBoardThread.Abort();
                }
            }
            catch (Exception ex)
            {
                GeneralVar.IOLastError = ex.Message;
                GeneralVar.IOBException = new Exception("IOB00", new Exception(ex.Message));
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[error] StopProcessingTicket : {0}", ex.ToString()), "IOboardController");
            }
        }

        #endregion

        #region Private Function

        private void ProcessIOBoard() 
        {
			Dictionary<Edam.eDigitalInput, bool> inputChannel;
			Dictionary<Edam.eDigitalOutput, bool> outputChannel;

            while (isProcessingIOBoard)
            {
                try                                                            
                {
                    IOBoardIsNormal = true;
                    isBlinking = false;

                    Thread.Sleep(GeneralVar.IOBoard_SleepTime);
					if (!controller.DigitalDataIn(Convert.ToByte(GeneralVar.IOBoard_Address), out outputChannel, out inputChannel))
						throw new Exception("Check IOBoard channel fail.");
					else
					{
                        CWNormalStatus = inputChannel[(Edam.eDigitalInput)GeneralVar.IOBoard_DI_NormalOperationStatus_Y1];
                        CWTermStatus = inputChannel[(Edam.eDigitalInput)GeneralVar.IOBoard_DI_TermStatus_Y2];
                        CWActiveStatus = inputChannel[(Edam.eDigitalInput)GeneralVar.IOBoard_DI_ActiveStatus_Y3];
                        CWErrorWashStatus = inputChannel[(Edam.eDigitalInput)GeneralVar.IOBoard_DI_ErrorWashStatus_Y5];
					}

                    if (EntryPassCounter > 0)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessIOBoard: Starting Car Wash Operation....."), TraceCategory);
                        EntryPassCounter--;

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_CarWashMachine_Channel, true))
							throw new Exception("Send Output Fail");
                            
                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_CarWashMachine_Channel, false))
							throw new Exception("Send Output Fail");
							
					}

                    if (ResetPassCounter > 0)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessIOBoard: Reset Car Wash Machine....."), TraceCategory);
                        ResetPassCounter--;

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_ResetMachine_Channel, true))
                            throw new Exception("Send Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_ResetMachine_Channel, false))
                            throw new Exception("Send Output Fail");

                    }

                    if (EStopPassCounter > 0)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessIOBoard: Emergency Stop Car Wash Machine....."), TraceCategory);
                        EStopPassCounter--;

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_EmergencyStop_Channel, true))
                            throw new Exception("Send Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_EmergencyStop_Channel, false))
                            throw new Exception("Send Output Fail");
                    }

                }
                catch (Exception ex)
                {
                    GeneralVar.IOLastError = ex.Message;
                    GeneralVar.IOBException = new Exception("IOB00", new Exception(ex.Message));
                    IOBoardIsNormal = false;
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[error] ProcessIOBoard Exception : {0}", ex.ToString()), "IOBoardController");
                }
            }
        }

        private void ProcessIOBoardQD()
        {
            Dictionary<Edam.eDigitalInput, bool> inputChannel;
            Dictionary<Edam.eDigitalOutput, bool> outputChannel;

            while (isProcessingIOBoard)
            {
                try
                {
                    IOBoardIsNormal = true;
                    isBlinking = false;

                    Thread.Sleep(GeneralVar.IOBoard_SleepTime);
                    if (!controller.DigitalDataIn(Convert.ToByte(GeneralVar.IOBoard_Address), out outputChannel, out inputChannel))
                        throw new Exception("Check IOBoard channel fail.");
                    else
                    {
                        CWErrorOperationStatus = inputChannel[(Edam.eDigitalInput)GeneralVar.IOBoard_DI_ErrorOperationStatus_Y23];
                        CWSensorObjectStatus = inputChannel[(Edam.eDigitalInput)GeneralVar.IOBoard_DI_SensorObjectStatus_X6];
                    }

                    if (EntryPassCounter > 0)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessIOBoardQD: Starting Car Wash Operation....."), TraceCategory);
                        EntryPassCounter--;

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_CarWashMachine_Channel, true))
                            throw new Exception("Send Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_CarWashMachine_Channel, false))
                            throw new Exception("Send Output Fail");

                    }

                    if (ResetPassCounter > 0)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessIOBoardQD: Reset Car Wash Machine....."), TraceCategory);
                        ResetPassCounter--;

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_ResetMachine_Channel, true))
                            throw new Exception("Send Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_ResetMachine_Channel, false))
                            throw new Exception("Send Output Fail");

                    }

                    if (EStopPassCounter > 0)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessIOBoardQD: Emergency Stop Car Wash Machine....."), TraceCategory);
                        EStopPassCounter--;

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_EmergencyStop_Channel, true))
                            throw new Exception("Send Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_EmergencyStop_Channel, false))
                            throw new Exception("Send Output Fail");
                    }
                    
                }
                catch (Exception ex)
                {
                    GeneralVar.IOLastError = ex.Message;
                    GeneralVar.IOBException = new Exception("IOB00", new Exception(ex.Message));
                    IOBoardIsNormal = false;
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[error] ProcessIOBoardQD Exception : {0}", ex.ToString()), "IOBoardController");
                }
            }
        }

        private void ProcessIOBoardPT()
        {
            Dictionary<Edam.eDigitalInput, bool> inputChannel;
            Dictionary<Edam.eDigitalOutput, bool> outputChannel;

            while (isProcessingIOBoard)
            {
                try
                {
                    IOBoardIsNormal = true;
                    isBlinking = false;

                    Thread.Sleep(GeneralVar.IOBoard_SleepTime);
                    if (!controller.DigitalDataIn(Convert.ToByte(GeneralVar.IOBoard_Address), out outputChannel, out inputChannel))
                        throw new Exception("Check IOBoard channel fail.");
                    else
                    {
                        PMInputError = inputChannel[(Edam.eDigitalInput)GeneralVar.IOBoard_DI_PentaErrorWash];
                        PMInputWashing = inputChannel[(Edam.eDigitalInput)GeneralVar.IOBoard_DI_PentaWashing];
                    }

                    if (EntryPassCounter > 0)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessIOBoardPT: Starting Car Wash Operation....."), TraceCategory);
                        EntryPassCounter--;

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_CarWashMachine_Channel, true))
                            throw new Exception("Send IOBoard_DO_CarWashMachine_Channel Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_CarWashMachine_Channel, false))
                            throw new Exception("Send IOBoard_DO_CarWashMachine_Channel Output Fail");

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_PentaWS1, true))
                            throw new Exception("Send IOBoard_DO_PentaWS1 Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_PentaWS1, false))
                            throw new Exception("Send IOBoard_DO_PentaWS1 Output Fail");

                        int ioAddress = 0;
                        int ioPin = 0;
                        int languagePin = 0;
                        switch (GeneralVar.LanguageSelected)
                        {
                            case "BM":
                                languagePin = GeneralVar.IOBoard_DO_PentaBM;
                                break;
                            case "EN":
                                languagePin = GeneralVar.IOBoard_DO_PentaBI;
                                break;
                            case "ZH":
                                languagePin = GeneralVar.IOBoard_DO_PentaBC;
                                break;
                            default:
                                languagePin = GeneralVar.IOBoard_DO_PentaBI;
                                break;
                        }

                        GeneralFunc.MapToAddressAndPin(languagePin, out ioAddress, out ioPin);

                        if (!controller.DigitalDataOut(Convert.ToByte(ioAddress), (Edam.eDigitalOutput)ioPin, true))
                            throw new Exception("Send IOBoard_DO_Language Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(ioAddress), (Edam.eDigitalOutput)ioPin, false))
                            throw new Exception("Send IOBoard_DO_Language Output Fail");
                    }

                    if (ResetPassCounter > 0)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessIOBoardQD: Reset Car Wash Machine....."), TraceCategory);
                        ResetPassCounter--;

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_ResetMachine_Channel, true))
                            throw new Exception("Send Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_ResetMachine_Channel, false))
                            throw new Exception("Send Output Fail");

                    }

                    if (EStopPassCounter > 0)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessIOBoardQD: Emergency Stop Car Wash Machine....."), TraceCategory);
                        EStopPassCounter--;

                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_EmergencyStop_Channel, true))
                            throw new Exception("Send Output Fail");

                        Thread.Sleep(100);
                        if (!controller.DigitalDataOut(Convert.ToByte(GeneralVar.IOBoard_Address), (Edam.eDigitalOutput)GeneralVar.IOBoard_DO_EmergencyStop_Channel, false))
                            throw new Exception("Send Output Fail");
                    }

                }
                catch (Exception ex)
                {
                    GeneralVar.IOLastError = ex.Message;
                    GeneralVar.IOBException = new Exception("IOB00", new Exception(ex.Message));
                    IOBoardIsNormal = false;
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[error] ProcessIOBoardQD Exception : {0}", ex.ToString()), "IOBoardController");
                }
            }
        }

        #endregion

        #region HeFei DInput

        public bool ACWErrorWashStatus { get { return CWErrorWashStatus; } }

        public bool ACWActiveStatus { get { return CWActiveStatus; } }

        public bool ACWTermStatus { get { return CWTermStatus; } }

        public bool ACWNormalStatus { get { return CWNormalStatus; } }

        #endregion

        #region QingDao DInput

        public bool ACWErrorOperationStatus { get { return CWErrorOperationStatus; } }

        public bool ACWSensorObjectStatus { get { return CWSensorObjectStatus; } }

        #endregion

        #region PentaMaster DInput

        public bool PMErrorWash { get { return PMInputError; } }

        public bool PMWashing { get { return PMInputWashing; } }

        #endregion
    }
}
