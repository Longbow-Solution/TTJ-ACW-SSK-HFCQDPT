using ACWSSK.App_Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XFDevice.FujitsuPrinter;

namespace ACWSSK.ViewModel
{
    public class MaintenanceViewModel : BaseViewModel
    {
        const string TraceCategory = "ACWSSK.ViewModel.MaintenanceViewModel";

        #region Properties

        private System.Timers.Timer checkingTimer;

        public bool IsChecking
        {
            get;
            set;
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (value == _errorMessage)
                    return;

                _errorMessage = value;
                base.OnPropertyChanged("ErrorMessage");
            }
        }

        #endregion

        #region Constructor

        public MaintenanceViewModel()
        {
            GeneralVar.CurrentAppState = "Error";

            GetErrorMessage();

            int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("GNR");
            string txType = "E";
            string description = "Application is out of service.";

            GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);

            StartCheckComponents();
        }

        #endregion

        #region Function

        private void GetErrorMessage()
        {
            List<string> messages = new List<string>();

            if (GeneralVar.APPException != null)
            {
                string message = GeneralVar.ErrorDictionary[GeneralVar.APPException.Message];
                if (GeneralVar.APPException.InnerException != null)
                    message = string.Format("{0} [{1}]", message, GeneralVar.APPException.InnerException.Message);

                messages.Add(message);
            }

            if (GeneralVar.DBException != null)
            {
                string message = GeneralVar.ErrorDictionary[GeneralVar.DBException.Message];
                if (GeneralVar.DBException.InnerException != null)
                    message = string.Format("{0} [{1}]", message, GeneralVar.DBException.InnerException.Message);

                messages.Add(message);
            }

            if (GeneralVar.IOBException != null)
            {
                string message = GeneralVar.ErrorDictionary[GeneralVar.IOBException.Message];
                if (GeneralVar.IOBException.InnerException != null)
                    message = string.Format("{0} [{1}]", message, GeneralVar.IOBException.InnerException.Message);

                messages.Add(message);
            }

            if (GeneralVar.EDCException != null)
            {
                string message = GeneralVar.ErrorDictionary[GeneralVar.EDCException.Message];
                if (GeneralVar.EDCException.InnerException != null)
                    message = string.Format("{0} [{1}]", message, GeneralVar.EDCException.InnerException.Message);

                messages.Add(message);
            }

            if (GeneralVar.RPTException != null)
            {
                string message = GeneralVar.ErrorDictionary[GeneralVar.RPTException.Message];
                if (GeneralVar.RPTException.InnerException != null)
                    message = string.Format("{0} [{1}]", message, GeneralVar.RPTException.InnerException.Message);

                messages.Add(message);
            }

            if (GeneralVar.BCRException != null)
            {
                string message = GeneralVar.ErrorDictionary[GeneralVar.BCRException.Message];
                if (GeneralVar.BCRException.InnerException != null)
                    message = string.Format("{0} [{1}]", message, GeneralVar.BCRException.InnerException.Message);

                messages.Add(message);
            }

            ErrorMessage = string.Join(" | ", messages);
        }

        public void StartCheckComponents()
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "StartCheckComponents starting...", TraceCategory);

            if (checkingTimer == null)
            {
                checkingTimer = new System.Timers.Timer(10000);
                checkingTimer.Elapsed += checkingTimer_Elapsed;
            }

            if (!checkingTimer.Enabled)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(o => checkingTimer_Elapsed(checkingTimer, null));
                checkingTimer.Start();
            }

            IsChecking = true;

            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "StartCheckComponents completed.", TraceCategory);
        }

        private void checkingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            checkingTimer.Stop();

            //CheckEDC();
            CheckIOB();
            CheckRPT();
            CheckAPP();

            if (!GeneralFunc.IsOutOfServiceMaintenance())
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "checkingTimer_Elapsed: Error resolved.", TraceCategory);
                StopCheckComponents();

                int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("GNR");
                string txType = "I";
                string description = "Application is back in operation.";

                GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);

                GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
                return;
            }

            GetErrorMessage();
            checkingTimer.Start();
        }

        private void CheckRPT()
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "CheckRPT starting...", TraceCategory);

            if (GeneralVar.RPTException == null)
                return;

            GeneralVar.RPTHandler.InitializeNeoPrinter();

            XFDevice.FujitsuPrinter.FujitsuPrinter status;
            var result = GeneralVar.RPTHandler.GetPrinterStatus(out status);

            if (!result)
            {
                GeneralVar.RPTException = new Exception("RPT02");
                return;
            }

            if (status.LastPrinterStatus[ePrinterStatus.PaperEnd])
            {
                GeneralVar.RPTException = new Exception("RPT03");
                return;
            }

            if (status.LastPrinterStatus[ePrinterStatus.PaperNearEnd])
            {
                if (GeneralVar.ReceiptPrinter_NearEmptyUsage > GeneralVar.ReceiptPrinter_NearEmptyBuffer)
                {
                    GeneralVar.RPTException = new Exception("RPT04");
                    return;
                }
            }
            else if (GeneralVar.ReceiptPrinter_NearEmptyUsage > 0)
            {
                GeneralVar.ReceiptPrinter_NearEmptyUsage = 0;
                GeneralVar.IsNearPaperEnd = false;

                GeneralVar.Query.SetSetting("ReceiptPrinterNearEmptyUsage", GeneralVar.ReceiptPrinter_NearEmptyUsage.ToString());
            }

            if (status.LastPrinterStatus[ePrinterStatus.CoverOpen])
            {
                GeneralVar.RPTException = new Exception("RPT05");
                return;
            }

            GeneralVar.RPTException = null;
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "CheckRPT completed.", TraceCategory);
        }

        private void CheckEDC()
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "CheckEDC starting...", TraceCategory);

            if (GeneralVar.EDCException == null)
                return;

            //if (GeneralVar.EDCException.Message == "EDC01")
            //{
            //    try
            //    {
            //        GeneralVar.EDCHandler.OpenPort(GeneralVar.CreditCardTerminal_Port, GeneralVar.CreditCardTerminal_BaudRate, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            //        GeneralVar.EDCException = new Exception("EDC02");
            //    }
            //    catch (Exception ex)
            //    {
            //        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CheckEDC: {0}", ex.ToString()), TraceCategory);
            //        return;
            //    }
            //}

            //if (GeneralVar.EDCException.Message == "EDC02")
            //{
            //    try
            //    {
            //        XFDevice.Ingenico.iSelf.Ingenico.eStatusCode statusCode = XFDevice.Ingenico.iSelf.Ingenico.eStatusCode.Approved;
            //        XFDevice.Ingenico.iSelf.Ingenico.CommandResult result = XFDevice.Ingenico.iSelf.Ingenico.CommandResult.NoResponse;

            //        result = GeneralVar.EDCHandler.EchoTest(out statusCode);

            //        if (result != XFDevice.Ingenico.iSelf.Ingenico.CommandResult.ValidCommand)
            //            return;

            //        if (statusCode != XFDevice.Ingenico.iSelf.Ingenico.eStatusCode.Approved)
            //            return;

            //        GeneralVar.AvailablePaymentMethod |= ePaymentMethod.Cards;
            //    }
            //    catch (Exception ex)
            //    {
            //        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CheckEDC: {0}", ex.ToString()), TraceCategory);
            //        return;
            //    }
            //}

            //if (GeneralVar.EDCException.Message == "EDC03" || GeneralVar.EDCException.Message == "EDC04")
            //{
            //    if (GeneralVar.EDCHandler.IsSettlementPending)
            //        return;
            //}

            GeneralVar.EDCException = null;
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "CheckEDC completed.", TraceCategory);
        }

        private void CheckAPP()
        {
            if (GeneralVar.APPException == null)
                return;

            if (GeneralVar.APPException.Message == "APP01")
            {
                if (GeneralVar.ApplicationMode == AppMode.PROD && GeneralVar.AvailablePaymentMethod == ePaymentMethod.None)
                    return;
            }

            GeneralVar.APPException = null;
        }

        private void CheckIOB() 
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "CheckIOB starting...", TraceCategory);

            if (GeneralVar.IOBException == null)
                return;
            else if (GeneralVar.IOBException.Message == "IOB02") 
            {
                if (GeneralVar.IOBoard_SensorControl_Enabled)
                {
                    if (GeneralVar.ACWMachineModel == eCarWashMachine.HEFEI)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory); 
        
                        if (GeneralVar.IOBoardCtrl.ACWNormalStatus && !GeneralVar.IOBoardCtrl.ACWTermStatus && !GeneralVar.IOBoardCtrl.ACWActiveStatus)
                        { GeneralVar.IOBException = null; }
                        else
                        { GeneralVar.IOBException = new Exception("IOB02"); }
                        return;
                    }
                    else if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                        if (!GeneralVar.IOBoardCtrl.ACWErrorOperationStatus)
                        { GeneralVar.IOBException = null; }
                        else
                        { GeneralVar.IOBException = new Exception("IOB02"); }         
                        return;
                    }
                    else if (GeneralVar.ACWMachineModel == eCarWashMachine.PENTAMASTER)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : Error = {0}, Washing = {1}", GeneralVar.IOBoardCtrl.PMErrorWash, GeneralVar.IOBoardCtrl.PMWashing), TraceCategory);
                        if (!GeneralVar.IOBoardCtrl.PMErrorWash)
                        { GeneralVar.IOBException = null; }
                        else
                        { GeneralVar.IOBException = new Exception("IOB02"); }
                        return;
                    }
                }
            }

            GeneralVar.IOBoardCtrl = new Controller.IOBoardController();

            if (GeneralVar.IOBoard_SensorControl_Enabled)
            {
                if (GeneralVar.IOBoardCtrl.IOBoardIsNormal)
                {
                    GeneralVar.IOBoardCtrl.StopProcessingIOBoard();
                    GeneralVar.IOBoardCtrl.StartProcessingIOBoard();
                }
                else
                {
                    GeneralVar.IOBoardCtrl.StopProcessingIOBoard();
                    GeneralVar.IOBException = new Exception("IOB01");
                    return;
                }
            }

            GeneralVar.IOBException = null;
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "CheckIOB completed.", TraceCategory);
        }

        private void StopCheckComponents()
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "StopCheckComponents starting...", TraceCategory);

            if (checkingTimer != null)
            {
                if (checkingTimer.Enabled)
                    checkingTimer.Stop();

                checkingTimer.Dispose();
                checkingTimer = null;
            }

            IsChecking = false;

            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "StopCheckComponents completed.", TraceCategory);
        }

        #endregion
    }
}
