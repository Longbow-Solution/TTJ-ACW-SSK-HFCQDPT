using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using ACWSSK.ViewModel;
using ACWSSK.Controller;
using System.Net;
using ACWSSK.App_Code;
using System.IO.Ports;
using ACWSSK.Model;
using System.Data.SqlServerCe;
using XFDevice.ECPIIntegration;

namespace ACWSSK.App_Code
{
    public class GeneralVar
    {
        #region Trace Switch Level

        public static TraceSwitch SwcTraceLevel = new TraceSwitch("SwcTraceLevel", "Trace Level Switch");

        #endregion

        public static ePaymentMethod AvailablePaymentMethod { get; set; }

        #region Crypto

        public static string CryptoPassword = "Secret";
        public static string CryptoSalt = "Longbow Technologies";

        #endregion

        #region Connection String

        private static SqlCeConnectionStringBuilder _DBConnStringBuilder;
        public static SqlCeConnectionStringBuilder DBConnStringBuilder
        {
            get
            {
                if (_DBConnStringBuilder == null)
                {
                    _DBConnStringBuilder = new SqlCeConnectionStringBuilder(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                    _DBConnStringBuilder.Password = Crypto.Decrypt(_DBConnStringBuilder.Password, CryptoPassword, CryptoSalt);
                }
                return _DBConnStringBuilder;
            }
        }

        public static string DBConnString
        {
            get { return DBConnStringBuilder.ConnectionString; }
        }

        public static string DBFilePath
        {
            get { return DBConnStringBuilder.DataSource; }
        }

        private static CRUD _query;
        public static CRUD Query
        {
            get
            {
                if (_query == null)
                    _query = new CRUD(DBConnString);

                return _query;
            }
        }

        #endregion

        #region Component Config

        public static string ApplicationCode = "ACW";

        //public static string CompanyName { get; set; }
        //public static string CompanyAddress { get; set; }
        //public static string CompanyGSTRegNo { get; set; }
        //public static string Careline { get; set; }
        //public static string Email { get; set; }
        //public static string FaxNo { get; set; }

        public static TimeSpan TransactionTimeout { get; set; }
        public static TimeSpan TriggerServiceInSecond { get; set; }
        //public static TimeSpan IdleTimeout { get; set; }

        public static string ComponentCode
        {
            get { return ConfigurationManager.AppSettings["ComponentCode"]; }
        }

        private static Dictionary<string, Transaction> _lastTx;
        public static Dictionary<string, Transaction> LastTx
        {
            get { return _lastTx; }
            set { _lastTx = value; }
        }

        private static IEnumerable<PaymentType> _db_PaymentTypes;
        public static IEnumerable<PaymentType> DB_PaymentTypes
        {
            get { return _db_PaymentTypes; }
            set { _db_PaymentTypes = value; }
        }

        private static IEnumerable<AlarmCategory> _db_AlarmCategories;
        public static IEnumerable<AlarmCategory> DB_AlarmCategories
        {
            get { return _db_AlarmCategories; }
            set { _db_AlarmCategories = value; }
        }

        private static AlarmHandler _alarmHandler;
        public static AlarmHandler AlarmHandler
        {
            get
            {
                if (_alarmHandler == null)
                    _alarmHandler = new AlarmHandler();

                return _alarmHandler;
            }
            set
            {
                if (_alarmHandler != value)
                {
                    _alarmHandler = null;
                    _alarmHandler = value;
                }
            }
        }

        private static Exception _dbException;
        public static Exception DBException
        {
            get { return _dbException; }
            set { _dbException = value; }
        }

        private static List<TimeSpan> _RefreshDataTime;
        public static List<TimeSpan> RefreshDataTime
        {
            get { return _RefreshDataTime; }
            set {   _RefreshDataTime = value; }
        }

        #endregion

        #region IO Board Controller

        public static bool IOBoard_Enabled
        {
            get { return ConfigurationManager.AppSettings["IOBoard_Enabled"] == "True"; }
        }

        public static string IOBoard_PortName
        {
            get { return ConfigurationManager.AppSettings["IOBoard_PortName"]; }
        }

        public static string IOBoard_BaudRate
        {
            get { return ConfigurationManager.AppSettings["IOBoard_BaudRate"]; }
        }

        public static string IOBoard_Parity
        {
            get { return ConfigurationManager.AppSettings["IOBoard_Parity"]; }
        }

        public static string IOBoard_DataBits
        {
            get { return ConfigurationManager.AppSettings["IOBoard_DataBits"]; }
        }

        public static string IOBoard_StopBits
        {
            get { return ConfigurationManager.AppSettings["IOBoard_StopBits"]; }
        }

        public static int IOBoard_SleepTime
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_SleepTime"]); }
        }

        public static string IOBoard_Address
        {
            get { return ConfigurationManager.AppSettings["IOBoard_Address"]; }
        }

        public static int IOBoard_DO_CarWashMachine_Channel
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DO_CarWashMachine_Channel"]); }
        }

        public static int IOBoard_DO_ResetMachine_Channel
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DO_ResetMachine_Channel"]); }
        }

        public static int IOBoard_DO_EmergencyStop_Channel
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DO_EmergencyStop_Channel"]); }
        }

        public static int IOBoard_DI_ErrorOperationStatus_Y23
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DI_ErrorOperationStatus_Y23"]); }
        }

        public static int IOBoard_DI_SensorObjectStatus_X6
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DI_SensorObjectStatus_X6"]); }
        }

        public static int IOBoard_DI_ErrorWashStatus_Y5
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DI_ErrorWashStatus_Y5"]); }
        }

        public static int IOBoard_DI_ActiveStatus_Y3
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DI_ActiveStatus_Y3"]); }
        }

        public static int IOBoard_DI_TermStatus_Y2
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DI_TermStatus_Y2"]); }
        }

        public static int IOBoard_DI_NormalOperationStatus_Y1
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DI_NormalOperationStatus_Y1"]); }
        }

        public static bool IOBoard_SensorControl_Enabled
        {
            get { return ConfigurationManager.AppSettings["IOBoard_SensorControl_Enabled"] == "True"; }
        }

        public static int IOBoard_DI_PentaErrorWash
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DI_PentaErrorWash"]); }
        }

        public static int IOBoard_DI_PentaWashing
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DI_PentaWashing"]); }
        }

        public static int IOBoard_DO_PentaWS1
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DO_PentaWS1"]); }
        }

        public static int IOBoard_DO_PentaWS2
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DO_PentaWS2"]); }
        }

        public static int IOBoard_DO_PentaWS3
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DO_PentaWS3"]); }
        }

        public static int IOBoard_DO_PentaWS4
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DO_PentaWS4"]); }
        }

        public static int IOBoard_DO_PentaWS5
        {
            get { return int.Parse(ConfigurationManager.AppSettings["IOBoard_DO_PentaWS5"]); }
        }

        private static Exception _iobException;
        public static Exception IOBException
        {
            get { return _iobException; }
            set { _iobException = value; }
        }

        #endregion

        #region Barcode Scanner

        public static bool BarcodeReader_Enabled
		{
            get
            {
                return ConfigurationManager.AppSettings["BarcodeReader_Enabled"].ToLower() == "true";
            }
		}

        private static Exception _bcrException;
        public static Exception BCRException
        {
            get { return _bcrException; }
            set { _bcrException = value; }
        }

        private static KeyboardHookHandler _keyboardHandler;
        public static KeyboardHookHandler KeyboardHandler
        {
            get { return _keyboardHandler; }
            set { _keyboardHandler = value; }
        }

        #endregion

        #region Application

        public static AppMode ApplicationMode 
		{
			get
            {
                string temp = ConfigurationManager.AppSettings["ApplicationMode"];
                switch (temp)
                {
                    case "PROD":
                        return AppMode.PROD;
                        break;
                    case "DEVT":
                        return AppMode.DEVT;
                        break;
                    case "DEMO":
                        return AppMode.DEMO;
                        break;
                    default:
                        return AppMode.PROD;
                        break;
                }
			}
		}

        private static string _currentAppState = "Initializing";
        public static string CurrentAppState
        {
            get { return _currentAppState; }
            set { _currentAppState = value; }
        }

        private static Exception _appException;
        public static Exception APPException
        {
            get { return _appException; }
            set { _appException = value; }
        }

        public static eCarWashMachine ACWMachineModel
        {
            get
            {
                string temp = ConfigurationManager.AppSettings["AutoCarWash_MachineModel"];
                switch (temp)
                {
                    case "HF":
                        return eCarWashMachine.HEFEI;
                    case "QD":
                        return eCarWashMachine.QINGDAO;
                    case "PT":
                        return eCarWashMachine.PENTAMASTER;
                    default:
                        return eCarWashMachine.HEFEI;
                }
            }
        }

        private static string _QingDaoAPI_URL;
        public static string QingDaoAPI_URL
        {
            get
            {
                return _QingDaoAPI_URL;
            }
            set { _QingDaoAPI_URL = value; }
        }

        public static string QingDaoAPI_OpenId
        {
            get
            {
                return GeneralVar.CurrentComponent.QDOpenId;
            }
        }

        public static string QingDaoAPI_Secret
        {
            get
            {
                return GeneralVar.CurrentComponent.QDSecret;
            }
        }

        public static int QingDaoAPI_IotId
        {
            get
            {
                return GeneralVar.CurrentComponent.QDIotId;
            }
        }

        private static ACWAPI _ACWAPICtrl;
        public static ACWAPI ACWAPICtrl
        {
            get
            {
                if (_ACWAPICtrl == null)
                    _ACWAPICtrl = new ACWAPI();

                return _ACWAPICtrl;
            }
        }

        #endregion

        #region Credit Card Terminal

        public static bool CreditCardTerminal_Enabled
		{
			get {
                return ConfigurationManager.AppSettings["CreditCardTerminal_Enabled"].ToLower() == "true";
			}
		}

        public static string CreditCardTerminal_PortName
        {
            get { return ConfigurationManager.AppSettings["CreditCardTerminal_PortName"]; }
        }

        public static ECPIHelper _CoherantHandler;
        public static ECPIHelper CoherantHandler 
        {
            get { return _CoherantHandler; }
            set 
            {
                _CoherantHandler = value; 
            }
        }

        private static Exception _edcException;
        public static Exception EDCException
        {
            get { return _edcException; }
            set { _edcException = value; }
        }

        #endregion

        #region Lecshine App

        private static string _AppUrl;
        public static string AppUrl 
        {
            get { return _AppUrl; }
            set
            {
                _AppUrl = value;
            }
        }

        private static string _X_Client_ID;
        public static string X_Client_ID
        {
            get { return _X_Client_ID; }
            set
            {
                _X_Client_ID = value;
            }
        }

        private static string _X_Client_Secret;
        public static string X_Client_Secret
        {
            get { return _X_Client_Secret; }
            set
            {
                _X_Client_Secret = value;
            }
        }

        private static string _Cookie;
        public static string Cookie
        {
            get { return _Cookie; }
            set
            {
                _Cookie = value;
            }
        }

        #endregion

        #region E-Wallet

        public static bool App_Enabled
        {
            get
            {
                return ConfigurationManager.AppSettings["App_Enabled"].ToLower() == "true";
            }
        }

        public static bool EWallet_Enabled
        {
            get { return ConfigurationManager.AppSettings["EWallet_Enabled"].ToLower() == "true"; }
        }

        #endregion

		#region Receipt Printer

		public static bool ReceiptPrinter_Enabled
		{
            get
            {
                return ConfigurationManager.AppSettings["ReceiptPrinter_Enabled"].ToLower() == "true";
            }
		}

		public static int PaperSize_Width
		{
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["PaperSize_Width"]);
            }
		}

		public static int PaperSize_Height
		{
			get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["PaperSize_Height"]);
			}
		}

		public static eReceiptPrinterModel ReceiptPrinter_Model
		{
			get
			{
                return eReceiptPrinterModel.Fujitsu;
            }
		}

		public static string ReceiptPrinter_Port
		{
            get
            {
                return ConfigurationManager.AppSettings["ReceiptPrinter_Port"].ToString();
            }
		}

		public static string ReceiptTemplateDirectory
		{
            get
            {
                return ConfigurationManager.AppSettings["ReceiptTemplateDirectory"] + @"\";
            }
		}

		public static string SaveReceiptDirectory
		{
            get
            {
                return ConfigurationManager.AppSettings["SaveReceiptDirectory"] + @"\";
            }
		}

		public static int ReceiptPrinter_NearEmptyBuffer { get { return 5000; } }
		public static int ReceiptPrinter_NearEmptyUsage { get; set; }

		private static DocumentPrintHandler _rptHandler;
		public static DocumentPrintHandler RPTHandler
		{
			get { return _rptHandler; }
			set { _rptHandler = value; }
		}

		private static Exception _rptException;
		public static Exception RPTException
		{
			get { return _rptException; }
			set { _rptException = value; }
		}

		private static bool _isNearPaperEnd;
		public static bool IsNearPaperEnd
		{
			get { return _isNearPaperEnd; }
			set { _isNearPaperEnd = value; }
		}

		#endregion

		#region Static View Model

		static MainWindowViewModel _vmMainWindow;
        public static MainWindowViewModel vmMainWindow
        {
            get { return _vmMainWindow; }
            set { _vmMainWindow = value; }
        }

        static MainWindow _vMainWindow;
        public static MainWindow vMainWindow
        {
            get { return _vMainWindow; }
            set { _vMainWindow = value; }
        }

        private static IOBoardController _IoBoardCtrl;
        public static IOBoardController IOBoardCtrl
        {
            get { return _IoBoardCtrl; }
            set {
                if (value == _IoBoardCtrl)
                    return;

                _IoBoardCtrl = value;
            }
        } 

        #endregion

        #region TCP Communication

        // TCP Communication
        public static bool TCP_Enabled
        {
            get
            {
                return ConfigurationManager.AppSettings["TCP_Enabled"].ToLower() == "true";
            }
        }

        private static IPAddress _TCP_IPAddress;
        public static IPAddress TCP_IPAddress
        {
            get
            {
                if (_TCP_IPAddress == null)
                {
                    string ip = ConfigurationManager.AppSettings["TCP_IPAddress"];
                    IPAddress ipAddr;

                    if (IPAddress.TryParse(ip, out ipAddr))
                        _TCP_IPAddress = ipAddr;
                }
                return _TCP_IPAddress;
            }
        }

        public static int TCP_Port
        {
            get { return int.Parse(ConfigurationManager.AppSettings["TCP_Port"]); }
        }

        public static int TCP_SetStatus_Interval
        {
            get { return int.Parse(ConfigurationManager.AppSettings["TCP_SetStatus_Interval"]); }
        }

        public static int TCP_CheckCommand_Interval
        {
            get { return int.Parse(ConfigurationManager.AppSettings["TCP_CheckCommand_Interval"]); }
        }

        #endregion
        
        static string _ioLastError;
        public static string IOLastError 
        {
            get { return _ioLastError; }
            set 
            {
                _ioLastError = value;
            }
        }

        public static bool IsFullScreen 
		{
            get { return ConfigurationManager.AppSettings["FullScreen"].ToLower() == "true"; }
		}

        public static bool IsTopMost
		{
            get { return ConfigurationManager.AppSettings["TopMost"].ToLower() == "true"; }
		}

        public static int StartPendingInSecond
        {
            get { return int.Parse(ConfigurationManager.AppSettings["StartPendingInSecond"]); }
        }

		private static Component _currentComponent;
		public static Component CurrentComponent 
		{
			get { return _currentComponent; }
			set
			{
				_currentComponent = value;
			}
		}

        private static Area _currentArea;
        public static Area CurrentArea
        {
            get { return _currentArea; }
            set
            {
                if (_currentArea == null)
                    _currentArea = new Area();

                _currentArea = value;
            }
        }

        private static List<Fare> _currentFare;
        public static List<Fare> CurrentFare
        {
            get { return _currentFare; }
            set
            {
                if (_currentFare == null)
                    _currentFare = new List<Fare>();
                _currentFare = value;
            }
        }

        private static Rate _currentTaxRate;
        public static Rate CurrentTaxRate
        {
            get { return _currentTaxRate; }
            set
            {
                if (_currentTaxRate == null)
                    _currentTaxRate = new Rate();

                _currentTaxRate = value;
            }
        }

        private static Rate _currentSurchargeRate;
        public static Rate CurrentSurchargeRate
        {
            get { return _currentSurchargeRate; }
            set
            {
                if (_currentSurchargeRate == null)
                    _currentSurchargeRate = new Rate();

                _currentSurchargeRate = value;
            }
        }

        private static AppAPI _applicationAPI;
        public static AppAPI ApplicationAPI 
        {
            get { return _applicationAPI; }
            set
            {
                if (_applicationAPI == null)
                    _applicationAPI = new AppAPI();

                _applicationAPI = value;
            }
        }
        
        public static Dictionary<string, string> ErrorDictionary = new Dictionary<string, string>()
		{
			{"APP00", "General failure"},
			{"APP01", "Payment method unavailable"},

			{"DB00", "DB: General failure"},
			{"DB01", "DB: Database file could not be found"},
			{"DB02", "DB: Database is not fully synchronized"},
			{"DB03", "DB: Component is not in database"},
			{"DB04", "DB: Failed to retrieve latest transaction id from web service"},
			{"DB05", "DB: Failed to initialize database"},
			{"DB06", "DB: Failed to retrieve latest transaction id from database"},
			{"DB07", "DB: Failed to retrieve application settings"},
            {"DB08", "DB: Failed to retrieve tariff data"},
            {"DB09", "DB: Failed to create local table"},

			{"UPS00", "UPS: General failure"},
			{"UPS01", "UPS: Failed to open port"},
			{"UPS02", "UPS: Invalid response"},
			{"UPS03", "UPS: Main power offline"},

			{"IOB00", "IOB: General failure"},
            {"IOB01", "IOB: Failed to open port"},
            {"IOB02", "AutoCarWash Machine Error"},

			{"EDC00", "EDC: General failure"},
			{"EDC01", "EDC: Failed to open port"},
			{"EDC02", "EDC: Echo test failed"},
			{"EDC03", "EDC: Terminal full"},
			{"EDC04", "EDC: Pending for settlement"},

			{"RPT00", "RPT: General failure"},
			{"RPT01", "RPT: Failed to initialize"},
			{"RPT02", "RPT: Failed to get status"},
			{"RPT03", "RPT: Paper empty"},
			{"RPT04", "RPT: Paper near empty"},
			{"RPT05", "RPT: Cover open"},
			{"RPT06", "RPT: Print head open"},
			{"RPT07", "RPT: Recoverable error"},
			{"RPT08", "RPT: Unrecoverable error"},

			{"BCR00", "BCR: General failure"},
			{"BCR01", "BCR: Failed to open port"},
			{"BCR02", "BCR: Failed to set mode"}
		};

	}
}
