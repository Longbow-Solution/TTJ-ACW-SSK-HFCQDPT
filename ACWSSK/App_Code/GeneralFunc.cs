
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ACWSSK.Model;
//using DFLicenseControl;

namespace ACWSSK.App_Code
{
    public class GeneralFunc
    {
        const string TraceCategory = "CTSSSK.App_Code.GeneralFunc";

        static CRUD query = new CRUD(GeneralVar.DBConnString);

        static List<PaymentType> _paymentTypes;
        static List<AlarmCategory> _alarmCategories;
        static List<TimeSpan> _refreshDataTime;

        static Area _currentArea;
        static List<Fare> _currentFare;
        static Rate _currentTaxRates;
        static Rate _currentSurchargeRates;

        static Component _component;

        static string _companyName;
        static string _companyAddress;
        static string _companyGSTRegNo;
        static string _careline;
        static string _email;
        static string _faxNo;
        static int _transactionTimeout;
        static int _triggerServiceInSecond;
        static string _qingdaoAPIURL;

        static string _LecshineAppUrl;
        static string _LecshineXClientID;
        static string _LecshineXClientSecret;
        static string _LecshineAppCookie;

        static bool _isStaticRefreshed;
        static bool _isRefreshed;

        public static bool GetStaticData()
        {
            try
            {
                #region Payment Types

                var paymentTypes = query.GetPaymentType();
                _paymentTypes = paymentTypes;
                #endregion

                #region Alarm Categories

                var alarmCategories = query.GetAlarmCategory();
                _alarmCategories = alarmCategories;

                #endregion

                _isStaticRefreshed = true;
                return true;

            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] GetStaticData: {0}", ex.ToString()), TraceCategory);
                return false;
            }
        }

        public static bool SetStaticData()
        {
            if (_isStaticRefreshed)
            {
                GeneralVar.DB_PaymentTypes = _paymentTypes;
                GeneralVar.DB_AlarmCategories = _alarmCategories;

                _isStaticRefreshed = false;
            }

            return true;
        }

        public static bool GetData(bool includeStatic = false)
        {
            try
            {
                #region General Setting

                _component = query.GetComponent(GeneralVar.ComponentCode);

                //_companyName = query.GetSetting("CompanyName");// "TGV Cinemas Sdn Bhd (305598-W)";
                //_companyAddress = query.GetSetting("CompanyAddress");// "Level 6, Menara Maxis\r\nKuala Lumpur City Centre\r\n50088 Kuala Lumpur";
                //_companyGSTRegNo = query.GetSetting("CompanyGSTRegNo");// "112233445566";
                //_careline = query.GetSetting("Careline");// "1300 222 848";
                //_email = query.GetSetting("Email");// "1300 222 848";
                //_faxNo = query.GetSetting("FaxNo");// "603 2381 3139";

                _refreshDataTime = ConstructTimeSpanList(query.GetSetting("RefreshDataTime"));
                //_qingdaoAPIURL = query.GetSetting("QingDaoAPIURL");

                string transactionTimeoutSecondString = query.GetSetting("TransactionTimeoutInSecond");
                _transactionTimeout = 0;
                if (string.IsNullOrEmpty(transactionTimeoutSecondString) || !int.TryParse(transactionTimeoutSecondString, out _transactionTimeout))
                    _transactionTimeout = 180; // 3 minutes default

                string TriggerServiceInSecondString = query.GetSetting("TriggerServiceInSecond");
                _triggerServiceInSecond = 0;
                if (string.IsNullOrEmpty(TriggerServiceInSecondString) || !int.TryParse(TriggerServiceInSecondString, out _triggerServiceInSecond))
                    _triggerServiceInSecond = 60; // 3 minutes default

                _LecshineAppUrl = query.GetSetting("LecshineAppUrl");
                _LecshineXClientID = query.GetSetting("LecshineXClientID");
                _LecshineXClientSecret = query.GetSetting("LecshineXClientSecret");
                _LecshineAppCookie = query.GetSetting("LecshineAppCookie");
                
                #endregion

                #region Get & Set Price Configuration

                _currentArea = query.GetArea(_component.AreaId, query.GetActiveAreaRevision(_component.AreaId));
                if (_currentArea == null)
                    _currentArea = GeneralVar.CurrentArea;

                _currentFare = new List<Fare>();

                if (_currentArea == null) 
                {
                    _currentArea = GeneralVar.CurrentArea;
                    _currentFare = GeneralVar.CurrentFare; 
                }
                else
                {
                    foreach (var fareId in _currentArea.Tariffs.Select(s => s.FareId).Distinct())
                    {
                        var temp = query.GetFare(fareId, query.GetActiveFareRevision(fareId));
                        if (temp == null) { }
                        else
                        {
                            _currentFare.Add(temp);
                        }
                    }
                }

                _currentTaxRates = query.GetRate(_currentArea.TaxId, query.GetActiveRateRevision(_currentArea.TaxId));
                if (_currentTaxRates == null) { _currentTaxRates = GeneralVar.CurrentTaxRate; }

                _currentSurchargeRates = query.GetRate(_currentArea.SurchargeId, query.GetActiveRateRevision(_currentArea.SurchargeId));
                if (_currentSurchargeRates == null) { _currentSurchargeRates = GeneralVar.CurrentSurchargeRate; }
                
                #endregion

                _isRefreshed = true;

                if (includeStatic)
                    GetStaticData();

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] GetData: {0}", ex.ToString()), TraceCategory);
                return false;
            }
        }

        public static List<TimeSpan> ConstructTimeSpanList(string time) 
        {
            List<TimeSpan> result = new List<TimeSpan>();

            try 
            {
                foreach (string t in time.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    result.Add(TimeSpan.Parse(t));
            }
            catch (Exception ex) 
            {
                result = new List<TimeSpan>();
            }

            return result;
        }

        public static bool SetData(bool includeStatic = false)
        {
            if (_isRefreshed)
            {
                GeneralVar.CurrentComponent = _component;

                //GeneralVar.CompanyName = _companyName;
                //GeneralVar.CompanyAddress = _companyAddress;
                //GeneralVar.CompanyGSTRegNo = _companyGSTRegNo;
                //GeneralVar.Careline = _careline;
                //GeneralVar.Email = _email;
                //GeneralVar.FaxNo = _faxNo;
                GeneralVar.QingDaoAPI_URL = _qingdaoAPIURL;

                GeneralVar.RefreshDataTime = _refreshDataTime;

                GeneralVar.TransactionTimeout = new TimeSpan(0, 0, _transactionTimeout);
                GeneralVar.TriggerServiceInSecond = new TimeSpan(0, 0, _triggerServiceInSecond);
                //GeneralVar.IdleTimeout = new TimeSpan(0, 0, _idleTimeout);

                GeneralVar.AppUrl = _LecshineAppUrl;
                GeneralVar.X_Client_ID = _LecshineXClientID;
                GeneralVar.X_Client_Secret = _LecshineXClientSecret;
                GeneralVar.Cookie = _LecshineAppCookie;

                GeneralVar.CurrentTaxRate = _currentTaxRates;
                GeneralVar.CurrentSurchargeRate = _currentSurchargeRates;

                GeneralVar.CurrentArea = _currentArea;
                GeneralVar.CurrentFare = _currentFare;

                _isRefreshed = false;
            }

            if (includeStatic)
                SetStaticData();

            return true;
        }

        public static bool RefreshData(bool includeStatic = false)
        {
            bool canGetData = GetData(includeStatic);
            bool canSetData = SetData(includeStatic);

            return canGetData && canSetData;
        }

        public static bool HasNewData
        {
            get { return _isRefreshed || _isStaticRefreshed; }
        }

        public static bool IsOutOfService()
        {
            if (GeneralVar.APPException != null || GeneralVar.IOBException != null)
                return true;

            if (GeneralVar.ApplicationMode == AppMode.PROD && (GeneralVar.AvailablePaymentMethod == ePaymentMethod.None || !GeneralVar.IOBoard_Enabled))
                return true;

            int paymentEnabled = 0;
            bool isCheckEDC = GeneralVar.CreditCardTerminal_Enabled;
            bool isCheckEW = GeneralVar.BarcodeReader_Enabled;

            bool isEDCOFS = GeneralVar.EDCException != null;
            bool isEWOFS = GeneralVar.BCRException != null;

            paymentEnabled += isCheckEDC ? 1 : 0;
            paymentEnabled += isCheckEW ? 1 : 0;

            int paymentOFS = 0;
            paymentOFS += (isCheckEDC && isEDCOFS) ? 1 : 0;
            paymentOFS += (isCheckEW && isEWOFS) ? 1 : 0;

            if (GeneralVar.ApplicationMode == AppMode.PROD && paymentEnabled == paymentOFS)
                return true;

            return false;
        }

        public static bool IsOutOfServiceMaintenance()
        {
            if (GeneralVar.APPException != null || GeneralVar.IOBException != null)
                return true;

            if (GeneralVar.ApplicationMode == AppMode.PROD && (GeneralVar.AvailablePaymentMethod == ePaymentMethod.None || !GeneralVar.IOBoard_Enabled))
                return true;

            if (GeneralVar.IOBoard_Enabled)
            {
                if (GeneralVar.ACWMachineModel == eCarWashMachine.HEFEI)
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);

                    if ((!GeneralVar.IOBoardCtrl.ACWNormalStatus && !GeneralVar.IOBoardCtrl.ACWTermStatus && !GeneralVar.IOBoardCtrl.ACWActiveStatus) || GeneralVar.IOBoardCtrl.ACWErrorWashStatus) { return true; }
                }
                else if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                    if (GeneralVar.IOBoardCtrl.ACWErrorOperationStatus) { return true; }
                }
                else if (GeneralVar.ACWMachineModel == eCarWashMachine.PENTAMASTER)
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : Error = {0}, Washing = {1}", GeneralVar.IOBoardCtrl.PMErrorWash, GeneralVar.IOBoardCtrl.PMWashing), TraceCategory);
                    if (GeneralVar.IOBoardCtrl.PMErrorWash) { return true; }
                }
            }

            int paymentEnabled = 0;
            bool isCheckEDC = GeneralVar.CreditCardTerminal_Enabled;
            bool isCheckEW = GeneralVar.BarcodeReader_Enabled;

            bool isEDCOFS = GeneralVar.EDCException != null;
            bool isEWOFS = GeneralVar.BCRException != null;

            paymentEnabled += isCheckEDC ? 1 : 0;
            paymentEnabled += isCheckEW ? 1 : 0;

            int paymentOFS = 0;
            paymentOFS += (isCheckEDC && isEDCOFS) ? 1 : 0;
            paymentOFS += (isCheckEW && isEWOFS) ? 1 : 0;

            if (GeneralVar.ApplicationMode == AppMode.PROD && paymentEnabled == paymentOFS)
                return true;

            return false;
        }

		//public static List<DFMonitoringClient.DFDeviceStatus> GetSystemStatusMonitoring()
		//{
		//	List<DFMonitoringClient.DFDeviceStatus> param = new List<DFMonitoringClient.DFDeviceStatus>();

		//	string app = "";
		//	#region Application Status

		//	DFMonitoringClient.DFSeverityLevel appSeverity = DFMonitoringClient.DFSeverityLevel.Info;

		//	if (GeneralVar.stage == ACWSSK.App.eStage.Operation)
		//		app = "Online";
		//	else if (GeneralVar.stage == ACWSSK.App.eStage.OffLine)
		//	{
		//		app = "OutofService";
		//		appSeverity = DFMonitoringClient.DFSeverityLevel.Error;
		//	}

		//	param.Add(new DFMonitoringClient.DFDeviceStatus() { Code = "APP", Severity = appSeverity, Status = app, Details = "" });

		//	#endregion


		//	string io = "", ioDetails = "";
		//	#region IO Controller

		//	DFMonitoringClient.DFSeverityLevel ioSeverity = DFMonitoringClient.DFSeverityLevel.Info;

		//	if (!GeneralVar.IOBoardController_Enabled)
		//	{
		//		io = "Offline";
		//		ioSeverity = DFMonitoringClient.DFSeverityLevel.None;
		//	}
		//	else if (!string.IsNullOrEmpty(GeneralVar.IOLastError))
		//	{
		//		io = GeneralVar.IOLastError;
		//		ioSeverity = DFMonitoringClient.DFSeverityLevel.Error;
		//	}
		//	else
		//		io = "Online";

		//	io = string.Format("[{0}] {1}", "Adam", io);
		//	param.Add(new DFMonitoringClient.DFDeviceStatus() { Code = "IO", Severity = ioSeverity, Status = io, Details = ioDetails });

		//	#endregion

		//	return param;
		//}
       
    }
}
