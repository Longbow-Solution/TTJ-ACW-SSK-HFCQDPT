using ACWSSK.App_Code;
using ACWSSK.Controller;
using ACWSSK.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using XFDevice.FujitsuPrinter;

namespace ACWSSK.ViewModel
{
    public class SystemCheckViewModel : BaseViewModel
    {
        const string TraceCategory = "SystemCheckViewModel";

        #region Properties

        private BackgroundWorker _checkWorker;
        private Timer _failedTimeout;

        private ObservableCollection<CheckItem> _itemList;
        public ObservableCollection<CheckItem> ItemList
        {
            get { return _itemList; }
            set
            {
                if (value == _itemList)
                    return;

                _itemList = value;
                base.OnPropertyChanged("ItemList");
            }
        }

        private Visibility _CloseVisibility = Visibility.Hidden;
        public Visibility CloseVisibility
        {
            get { return _CloseVisibility; }
            set { _CloseVisibility = value; OnPropertyChanged("CloseVisibility"); }
        }

        #endregion

        #region Component Status Color

        const string colorDefault = "#E1DDD5";
        const string colorCheckPassed = "#B9C8A4";
        const string colorError = "#D2988B";
        const string colorWarning = "#E0E066";

        #endregion

        #region Component Icon

        const string imgTick = "/ACWSSK;component/Resources/Images/SystemCheck/tick.png";
        const string imgTickDisabled = "/ACWSSK;component/Resources/Images/SystemCheck/tick_disabled.png";
        const string imgDisabled = "/ACWSSK;component/Resources/Images/SystemCheck/disabled.png";
        const string imgError = "/ACWSSK;component/Resources/Images/SystemCheck/error.png";
        const string imgRunning = "/ACWSSK;component/Resources/Images/SystemCheck/running.png";
        const string imgWarning = "/ACWSSK;component/Resources/Images/SystemCheck/Alert.png";

        #endregion

        #region Toggle Status Description Command

        private ICommand _ToggleStatusDescCommand;
        public ICommand ToggleStatusDescCommand
        {
            get
            {
                if (_ToggleStatusDescCommand == null)
                    _ToggleStatusDescCommand = new RelayCommand<object>(ToggleStatusDesc);

                return _ToggleStatusDescCommand;
            }
        }

        private void ToggleStatusDesc(object state)
        {
            CheckItem c = ItemList.Where(i => i.ItemCode == (string)state).First();
            c.StatusDescVisibility = c.StatusDescVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        public SystemCheckViewModel()
        {
            _failedTimeout = new Timer(5000);
            _failedTimeout.Elapsed += _failedTimeout_Elapsed;
            _failedTimeout.AutoReset = false;

            InitiateItem();
        }

        public void InitiateItem()
        {
            ItemList = new ObservableCollection<CheckItem>();
            ItemList.Add(new CheckItem() { ItemCode = "DB", ItemName = "Database" });
            ItemList.Add(new CheckItem() { ItemCode = "BCR", ItemName = "Barcode Reader", BackgroundColor = colorDefault, CheckIcon = imgTickDisabled, CanSkip = !GeneralVar.BarcodeReader_Enabled });
            ItemList.Add(new CheckItem() { ItemCode = "EDC", ItemName = "Credit Card Terminal", BackgroundColor = colorDefault, CheckIcon = imgTickDisabled, CanSkip = !GeneralVar.CreditCardTerminal_Enabled });
            ItemList.Add(new CheckItem() { ItemCode = "RPT", ItemName = "Receipt Printer", BackgroundColor = colorDefault, CheckIcon = imgTickDisabled, CanSkip = !GeneralVar.ReceiptPrinter_Enabled });
            ItemList.Add(new CheckItem() { ItemCode = "IOB", ItemName = "IO Controller", BackgroundColor = colorDefault, CheckIcon = imgTickDisabled, CanSkip = !GeneralVar.IOBoard_Enabled });
        }

        private void _failedTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (GeneralVar.ApplicationMode == AppMode.PROD)
                OnFailed();
        }

        public void PerformCheck()
        {
            CloseVisibility = Visibility.Collapsed;

            _checkWorker = new BackgroundWorker();
            _checkWorker.WorkerSupportsCancellation = true;
            _checkWorker.DoWork += _checkWorker_DoWork;
            _checkWorker.RunWorkerCompleted += _checkWorker_RunWorkerCompleted;
            _checkWorker.RunWorkerAsync();
        }

        private void _checkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            foreach (var item in ItemList)
            {

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                switch (item.ItemCode)
                {
                    case "DB":
                        this.CheckDB(item);
                        break;

                    //case "UPS":
                    //    item.Status = string.Empty;
                    //    break;

                    case "IOB":
                        this.CheckIOB(item);
                        break;

                    case "EDC":
                        this.CheckEDC(item);
                        break;
                        
                    case "RPT":
                        this.CheckRPT(item);
                        break;

                    case "BCR":
                        this.CheckBCR(item);
                        break;
                        
                    default:
                        break;
                }
            }

            if (GeneralVar.ApplicationMode == AppMode.PROD && GeneralVar.AvailablePaymentMethod == ePaymentMethod.None)
                GeneralVar.APPException = new Exception("APP01");
        }

        private void CheckDB(CheckItem item)
        {
            UpdateStatus(item, eCheckState.Checking);

            UpdateStatusDesc(item, "Checking database file path...");
            if (!File.Exists(GeneralVar.DBFilePath))
            {
                UpdateStatus(item, eCheckState.Error);
                UpdateStatusDesc(item, "Database file could not be found.");

                item.ErrorCode = "DB01";
                item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];
                item.ErrorDescription = string.Format("Database file path = {0}", GeneralVar.DBFilePath);

                GeneralVar.DBException = new Exception("DB01", new Exception(item.ErrorDescription));
                return;
            }

            UpdateStatusDesc(item, "Checking database synchronization...");
            int initialized = GeneralVar.Query.CheckInitialized();

            if (initialized == -1)
            {
                UpdateStatus(item, eCheckState.Error);
                UpdateStatusDesc(item, "Database is not fully synchronized.");

                item.ErrorCode = "DB02";
                item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];

                GeneralVar.DBException = new Exception("DB02");
                return;
            }

            UpdateStatusDesc(item, "Retrieving component details...");
            GeneralVar.CurrentComponent = GeneralVar.Query.GetComponent(GeneralVar.ComponentCode);
            if (GeneralVar.CurrentComponent == null)
            {
                UpdateStatus(item, eCheckState.Error);
                UpdateStatusDesc(item, string.Format("Could not find component [ComponentCode = {0}].", GeneralVar.ComponentCode));

                item.ErrorCode = "DB03";
                item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];
                item.ErrorDescription = string.Format("Component code = {0}", GeneralVar.ComponentCode);

                GeneralVar.DBException = new Exception("DB03", new Exception(item.ErrorDescription));
                return;
            }

            if (initialized == 0)
            {
                UpdateStatusDesc(item, "Initializing database for first time use...");

                Dictionary<string, string> lastTx = new Dictionary<string, string>();

                try
                {
                    string[] txTables = new string[] { "acw_TxSales", "acw_TxAlarm", "acw_TxEDCSettlement"};

                    using (ACWWS.ACWWSClient client = new ACWWS.ACWWSClient())
                    {
                        string[] lLastTx = client.GetLastTx(txTables, GeneralVar.CurrentComponent.ComponentCode);

                        if (lLastTx.Length != txTables.Length)
                            throw new Exception("Retrieve last transaction failed, data length mismatch.");

                        foreach (var tx in lLastTx)
                        {
                            string[] s = tx.Split('|');

                            if (s.Length > 1) lastTx.Add(s[0], tx.Substring(tx.IndexOf("|") + 1));
                            else lastTx.Add(s[0], string.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CheckDatabase: {0}", ex.ToString()), TraceCategory);

                    UpdateStatus(item, eCheckState.Error);
                    UpdateStatusDesc(item, "Could not retrieve last transaction detail from web service.");

                    item.ErrorCode = "DB04";
                    item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];
                    item.ErrorDescription = string.Format("Exception message = {0}", ex.Message);

                    GeneralVar.DBException = new Exception("DB04", new Exception(item.ErrorDescription));
                    return;
                }

                GeneralVar.Query.BeginTransaction();
                initialized = GeneralVar.Query.InitializeDatabase(lastTx);

                if (initialized == 0)
                {
                    GeneralVar.Query.RollBackTransaction();

                    UpdateStatus(item, eCheckState.Error);
                    UpdateStatusDesc(item, "Database could not be initialized.");

                    item.ErrorCode = "DB05";
                    item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];

                    GeneralVar.DBException = new Exception("DB05");
                    return;
                }
                else
                    GeneralVar.Query.CommitTransaction();
            }

            UpdateStatusDesc(item, "Retrieving last transaction details...");
            GeneralVar.LastTx = GeneralVar.Query.GetLastTx();
            if (GeneralVar.LastTx == null)
            {
                UpdateStatus(item, eCheckState.Error);
                UpdateStatusDesc(item, "Unable to retrieve last transaction details.");

                item.ErrorCode = "DB06";
                item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];

                GeneralVar.DBException = new Exception("DB06");
                return;
            }

            UpdateStatusDesc(item, "Retrieving application settings...");
            bool canGetData = GeneralFunc.GetStaticData();
            bool canSetData = GeneralFunc.SetStaticData();

            if (!canGetData || !canSetData)
            {
                UpdateStatus(item, eCheckState.Error);
                UpdateStatusDesc(item, "Unable to retrieve application settings.");

                item.ErrorCode = "DB07";
                item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];

                GeneralVar.DBException = new Exception("DB07");
                return;
            }
            else
            {
                bool isSuccess = GeneralFunc.RefreshData();
                if (isSuccess)
                {
                    UpdateStatus(item, eCheckState.CheckPassed);
                    UpdateStatusDesc(item, "Tariff data is retrieved successfully.");
                }
                else
                {
                    UpdateStatus(item, eCheckState.Error);
                    UpdateStatusDesc(item, "Unable to retrieve tariff data.");

                    item.ErrorCode = "DB08";
                    item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];

                    GeneralVar.DBException = new Exception("DB07");
                    return;
                }
            }

            string err = "";
            
            try
            {
                string[] files = Directory.GetFiles(GeneralVar.SaveReceiptDirectory);

                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.LastWriteTime < DateTime.Now.AddHours(-24))
                            fi.Delete();
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex) { }


            UpdateStatus(item, eCheckState.CheckPassed);
            UpdateStatusDesc(item, "Database is initialized.");
        }

        private void CheckIOB(CheckItem item)
        {
            if (item.CanSkip)
            {
                UpdateStatus(item, eCheckState.Disabled);
                UpdateStatusDesc(item, "IO Controller is not enabled.");
                return;
            }

            UpdateStatus(item, eCheckState.Checking);
            UpdateStatusDesc(item, "Checking IO Controller...");

            try
            {
                GeneralVar.IOBoardCtrl = new Controller.IOBoardController();
                if (GeneralVar.IOBoardCtrl.IOBoardIsNormal)
                {
                    GeneralVar.IOBoardCtrl.StartProcessingIOBoard();

                    UpdateStatus(item, eCheckState.CheckPassed);
                    UpdateStatusDesc(item, "IO Controller is initialized.");
                }
                else 
                {
                    UpdateStatus(item, eCheckState.Error);
                    UpdateStatusDesc(item, "IO Controller is failed to initialized.");

                    item.ErrorCode = "IOB01";
                    item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];
                    item.ErrorDescription = string.Format("IO Controller is failed to initialized.");

                    GeneralVar.IOBException = new Exception("IOB01", new Exception(item.ErrorDescription));

                    int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("IOB");
                    string txType = "E";
                    string description = string.Format("[INIT] IO Controller is failed to initialized.");

                    GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);

                    return;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CheckIOController: {0}", ex.ToString()), TraceCategory);

                UpdateStatus(item, eCheckState.Error);
                UpdateStatusDesc(item, ex.Message);

                item.ErrorCode = "IOB00";
                item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];
                item.ErrorDescription = string.Format("Exception message = {0}", ex.Message);

                GeneralVar.IOLastError = GeneralVar.ErrorDictionary[item.ErrorCode];  //new Exception("IOB00", new Exception(item.ErrorDescription));
                GeneralVar.IOBException = new Exception("IOB00", new Exception(item.ErrorDescription));

                int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("IOB");
                string txType = "E";
                string description = string.Format("[INIT] Check component error. Exception Message = {0}", ex.Message);

                GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);
            }
        }

        private void CheckEDC(CheckItem item)
        {
            if (item.CanSkip)
            {
                UpdateStatus(item, eCheckState.Disabled);
                UpdateStatusDesc(item, "Credit Card Terminal is not enabled.");
                return;
            }

            UpdateStatus(item, eCheckState.Checking);
            UpdateStatusDesc(item, "Checking Credit Card Terminal...");

            try
            {
                GeneralVar.CoherantHandler = new XFDevice.ECPIIntegration.ECPIHelper();

                GeneralVar.CoherantHandler.Connect(GeneralVar.CreditCardTerminal_PortName);

                UpdateStatus(item, eCheckState.CheckPassed);
                UpdateStatusDesc(item, "Credit Card Terminal is initialized.");

                GeneralVar.AvailablePaymentMethod |= ePaymentMethod.CCards;
            }
            catch (Exception ex) 
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CheckCreditCardTerminal: {0}", ex.ToString()), TraceCategory);

                UpdateStatus(item, eCheckState.Error);
                UpdateStatusDesc(item, ex.Message);

                item.ErrorCode = "EDC00";
                item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];
                item.ErrorDescription = string.Format("Exception message = {0}", ex.Message);

                GeneralVar.BCRException = new Exception("EDC00", new Exception(item.ErrorDescription));

                int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("EDC");
                string txType = "E";
                string description = string.Format("[INIT] Check credit card terminal error. Exception Message = {0}", ex.Message);

                GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);
            }
        }

        private void CheckRPT(CheckItem item)
        {
            if (item.CanSkip)
            {
                UpdateStatus(item, eCheckState.Disabled);
                UpdateStatusDesc(item, "Receipt Printer is not enabled.");
                return;
            }

            UpdateStatus(item, eCheckState.Checking);
            UpdateStatusDesc(item, "Checking Receipt Printer...");

            try
            {
                GeneralVar.RPTHandler = new DocumentPrintHandler(GeneralVar.ReceiptPrinter_Port);

                GeneralVar.RPTHandler.InitializeNeoPrinter();

                XFDevice.FujitsuPrinter.FujitsuPrinter status = new FujitsuPrinter();
                var result = GeneralVar.RPTHandler.GetPrinterStatus(out status);

                if (!result)
                {
                    //UpdateStatus(item, eCheckState.Error);
                    UpdateStatusDesc(item, string.Format("\r\nGet printer status invalid result: Failed"));

                    item.ErrorCode = "RPT02";
                    item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];
                    item.ErrorDescription = string.Format("Get status result = Failed");

                    GeneralVar.RPTException = new Exception("RPT02", new Exception(item.ErrorDescription));

                    int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("RPT");
                    string txType = "E";
                    string description = string.Format("[INIT] Get printer status returns invalid result...");

                    GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);

                    //return;
                }

                if (status.LastPrinterStatus[ePrinterStatus.PaperNearEnd])
                {
                    //UpdateStatus(item, eCheckState.Error);
                    UpdateStatusDesc(item, "Printer status error: PaperEnd");

                    item.ErrorCode = "RPT03";
                    item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];

                    GeneralVar.RPTException = new Exception("RPT03");

                    int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("RPT");
                    string txType = "E";
                    string description = "[INIT] Printer status error: PaperEnd.";

                    GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);
                    //return;
                }

                if (status.LastPrinterStatus[ePrinterStatus.PaperEnd])
                {
                    if (GeneralVar.ReceiptPrinter_NearEmptyUsage > GeneralVar.ReceiptPrinter_NearEmptyBuffer)
                    {
                        //UpdateStatus(item, eCheckState.Error);
                        UpdateStatusDesc(item, "Printer status error: PaperEnd");

                        item.ErrorCode = "RPT04";
                        item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];

                        GeneralVar.RPTException = new Exception("RPT04");

                        int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("RPT");
                        string txType = "E";
                        string description = "[INIT] Printer status error: PaperEnd.";

                        GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);

                        //return;
                    }
                }
                else if (GeneralVar.ReceiptPrinter_NearEmptyUsage > 0)
                {
                    GeneralVar.ReceiptPrinter_NearEmptyUsage = 0;
                    //GeneralVar.Query.SetSetting("ReceiptPrinterNearEmptyUsage", GeneralVar.ReceiptPrinter_NearEmptyUsage.ToString());
                }

                if (status.LastPrinterStatus[ePrinterStatus.CoverOpen])
                {
                    //UpdateStatus(item, eCheckState.Error);
                    UpdateStatusDesc(item, "Printer status error: CoverUp");

                    item.ErrorCode = "RPT05";
                    item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];

                    GeneralVar.RPTException = new Exception("RPT05");

                    int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("RPT");
                    string txType = "E";
                    string description = "[INIT] Printer status error: CoverUp.";

                    GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);
                    //return;
                }

                UpdateStatus(item, eCheckState.CheckPassed);
                UpdateStatusDesc(item, "Receipt Printer is initialized.");
            }
            catch (Exception ex) 
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CheckReceiptPrinter: {0}", ex.ToString()), TraceCategory);

                UpdateStatus(item, eCheckState.Error);
                UpdateStatusDesc(item, ex.Message);

                item.ErrorCode = "RPT00";
                item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];
                item.ErrorDescription = string.Format("Exception message = {0}", ex.Message);

                GeneralVar.RPTException = new Exception("RPT00", new Exception(item.ErrorDescription));

                int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("RPT");
                string txType = "E";
                string description = string.Format("[INIT] Check component error. Exception Message = {0}", ex.Message);

                GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);
            }
        }

        private void CheckBCR(CheckItem item)
        {
            if (item.CanSkip)
            {
                UpdateStatus(item, eCheckState.Disabled);
                UpdateStatusDesc(item, "Barcode Reader is not enabled.");
                return;
            }

            UpdateStatus(item, eCheckState.Checking);
            UpdateStatusDesc(item, "Checking Barcode Reader...");
            
            try
            {
                //GeneralVar.KeyboardHandler = new KeyboardHookHandler();
                //GeneralVar.KeyboardHandler.SetKeyboardHook();

                UpdateStatus(item, eCheckState.CheckPassed);
                UpdateStatusDesc(item, "Barcode Reader is initialized.");

                GeneralVar.AvailablePaymentMethod |= ePaymentMethod.eWallet;
                GeneralVar.AvailablePaymentMethod |= ePaymentMethod.App;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CheckBarcodeReader: {0}", ex.ToString()), TraceCategory);

                UpdateStatus(item, eCheckState.Error);
                UpdateStatusDesc(item, ex.Message);

                item.ErrorCode = "BCR00";
                item.ErrorMessage = GeneralVar.ErrorDictionary[item.ErrorCode];
                item.ErrorDescription = string.Format("Exception message = {0}", ex.Message);

                GeneralVar.BCRException = new Exception("BCR00", new Exception(item.ErrorDescription));

                int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("BCR");
                string txType = "E";
                string description = string.Format("[INIT] Check Barcode Scanner error. Exception Message = {0}", ex.Message);

                GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);
            }
        }

        private void _checkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //System.Threading.Thread.Sleep(10000);
            if (e.Cancelled)
                return;

            if (ItemList.Any(i => i.Status.Contains("Error")))
            {
                CloseVisibility = Visibility.Visible;
                _failedTimeout.Start();
            }
            else
            {
                GeneralVar.APPException = null;
                OnSuccess();
            }
        }

        private void UpdateStatus(CheckItem item, eCheckState state)
        {
            switch (state)
            {
                case eCheckState.None:
                    item.Status = "";
                    item.BackgroundColor = colorDefault;
                    item.CheckIcon = imgTickDisabled;
                    break;
                case eCheckState.Disabled:
                    item.Status = "  :  Disabled";
                    item.BackgroundColor = colorDefault;
                    item.CheckIcon = imgDisabled;
                    break;
                case eCheckState.Checking:
                    item.Status = "  :  Checking...";
                    item.BackgroundColor = colorDefault;
                    item.CheckIcon = imgRunning;
                    break;
                case eCheckState.CheckPassed:
                    item.Status = "  :  OK";
                    item.BackgroundColor = colorCheckPassed;
                    item.CheckIcon = imgTick;
                    break;
                case eCheckState.Error:
                    item.Status = "  :  Error";
                    item.BackgroundColor = colorError;
                    item.CheckIcon = imgError;
                    break;
                case eCheckState.Warning:
                    item.Status = "  :  Warning";
                    item.BackgroundColor = colorWarning;
                    item.CheckIcon = imgWarning;
                    break;
            }
        }

        private void UpdateStatusDesc(CheckItem item, string message)
        {
            item.StatusDescription += (string.IsNullOrEmpty(item.StatusDescription) ? "" : "\r\n") + message;
        }

        public event EventHandler Success;
        private void OnSuccess()
        {
            if (Success != null)
                Success(this, null);
        }

        public event EventHandler Failed;
        private void OnFailed()
        {
            if (Failed != null)
                Failed(this, null);
        }
    }
}
