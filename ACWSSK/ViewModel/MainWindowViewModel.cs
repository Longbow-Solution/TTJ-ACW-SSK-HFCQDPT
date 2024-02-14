using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ACWSSK.App_Code;
using ACWSSK.Controller;
using System.Timers;
using System.Windows;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Media;
using ACWSSK.Model;
using System.Collections.ObjectModel;
using XFDevice.FujitsuPrinter;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;

namespace ACWSSK.ViewModel
{
	public class MainWindowViewModel : BaseViewModel
	{
		#region Fields

        protected string TraceCategory = "MainWindowViewModel";
        private System.Timers.Timer _checkStateTimer;
        private System.Timers.Timer _refreshTimer;
        //private Timer _taskTimer;
        private eModuleStage _currentModuleStage;
        int serviceClickTimeout = 0;
        int serviceClickAttempt = 0;

        #endregion

        #region Properties

        private BaseViewModel _currentModule;
        public BaseViewModel CurrentModule
        {
            get { return _currentModule; }
            set
            {
                if (value == _currentModule)
                    return;

                _currentModule = value;
                base.OnPropertyChanged("CurrentModule");
            }
        }

        private string _componentCode;
        public string ComponentCode
        {
            get { return _componentCode; }
            set
            {
                if (value == _componentCode)
                    return;

                _componentCode = value;
                base.OnPropertyChanged("ComponentCode");
            }
        }

        private string _componentName;
        public string ComponentName
        {
            get { return _componentName; }
            set
            {
                if (value == _componentName)
                    return;

                _componentName = value;
                base.OnPropertyChanged("ComponentName");
            }
        }

        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                if (value == _currentDate)
                    return;

                _currentDate = value;
                base.OnPropertyChanged("CurrentDate");
            }
        }

        private string _currentDateString;
        public string CurrentDateString
        {
            get { return _currentDateString; }
            set
            {
                if (value == _currentDateString)
                    return;

                _currentDateString = value;
                base.OnPropertyChanged("CurrentDateString");
            }
        }

        private string _Label_TimeOutMessage;
        public string Label_TimeOutMessage
        {
            get { return _Label_TimeOutMessage; }
            set
            {
                _Label_TimeOutMessage = value;
                base.OnPropertyChanged("Label_TimeOutMessage");
            }
        }

        private string _Label_TimeOutNo;
        public string Label_TimeOutNo
        {
            get { return _Label_TimeOutNo; }
            set
            {
                _Label_TimeOutNo = value;
                base.OnPropertyChanged("Label_TimeOutNo");
            }
        }

        private string _Label_TimeOutYes;
        public string Label_TimeOutYes
        {
            get { return _Label_TimeOutYes; }
            set
            {
                _Label_TimeOutYes = value;
                base.OnPropertyChanged("Label_TimeOutYes");
            }
        }

        Visibility _ShowTimeOut = Visibility.Collapsed;
        public Visibility ShowTimeOut
        {
            get { return _ShowTimeOut; }
            set
            {
                _ShowTimeOut = value;
                OnPropertyChanged("ShowTimeOut");
            }
        }

        Visibility _ShowLoading = Visibility.Collapsed;
        public Visibility ShowLoading
        {
            get { return _ShowLoading; }
            set
            {
                _ShowLoading = value;
                OnPropertyChanged("ShowLoading");
            }
        }

        ICommand _TimeOutCommand;
        public ICommand TimeOutCommand
        {
            get
            {
                if (_TimeOutCommand == null)
                    _TimeOutCommand = new RelayCommand<string>(TimeOut);
                return _TimeOutCommand;
            }
        }

        ICommand _EasyMaintenanceCommand;
        public ICommand EasyMaintenanceCommand
        {
            get
            {
                if (_EasyMaintenanceCommand == null)
                    _EasyMaintenanceCommand = new RelayCommand(EasyMaintenance);
                return _EasyMaintenanceCommand;
            }
        }

        private HomeViewModel _vmHome;
        public HomeViewModel vmHome
        {
            get { return _vmHome; }
            set { _vmHome = value; OnPropertyChanged("vmHome"); }
        }

        private ServiceSelectionViewModel _vmServiceSelection;
        public ServiceSelectionViewModel vmServiceSelection
        {
            get { return _vmServiceSelection; }
            set { _vmServiceSelection = value; OnPropertyChanged("vmServiceSelection"); }
        }

        private PaymentSelectionViewModel _vmPaymentSelection;
        public PaymentSelectionViewModel vmPaymentSelection
        {
            get { return _vmPaymentSelection; }
            set { _vmPaymentSelection = value; OnPropertyChanged("vmPaymentSelection"); }
        }

        private MainIdleViewModel _vmMainIdle;
        public MainIdleViewModel vmMainIdle
        {
            get { return _vmMainIdle; }
            set { _vmMainIdle = value; OnPropertyChanged("vmMainIdle"); }
        }

        private SystemCheckViewModel _vmComponentCheck;
        public SystemCheckViewModel vmComponentCheck
        {
            get { return _vmComponentCheck; }
            set { _vmComponentCheck = value; OnPropertyChanged("vmComponentCheck"); }
        }

        private InServiceViewModel _vmInService;
        public InServiceViewModel vmInService
        {
            get { return _vmInService; }
            set { _vmInService = value; OnPropertyChanged("vmInService"); }
        }

        private PaymentViewModel _vmPayment;
        public PaymentViewModel vmPayment
        {
            get { return _vmPayment; }
            set { _vmPayment = value; OnPropertyChanged("vmPayment"); }
        }

        private ErrorViewModel _vmError;
        public ErrorViewModel vmError
        {
            get { return _vmError; }
            set { _vmError = value; OnPropertyChanged("vmError"); }
        }

        private MaintenanceViewModel _vmMaintenance;
        public MaintenanceViewModel vmMaintenance
        {
            get { return _vmMaintenance; }
            set { _vmMaintenance = value; OnPropertyChanged("vmMaintenance"); }
        }

        private OfflineViewModel _vmOffline;
        public OfflineViewModel vmOffline
        {
            get { return _vmOffline; }
            set { _vmOffline = value; OnPropertyChanged("vmOffline"); }
        }

        private ServicingViewModel _vmServicing;
        public ServicingViewModel vmServicing
        {
            get { return _vmServicing; }
            set { _vmServicing = value; OnPropertyChanged("vmServicing"); }
        }

        private ServicingLoginViewModel _vmServicingLogin;
        public ServicingLoginViewModel vmServicingLogin
        {
            get { return _vmServicingLogin; }
            set { _vmServicingLogin = value; OnPropertyChanged("vmServicingLogin"); }
        }

        public bool IsOffline
        {
            get
            {
                if (GeneralVar.CurrentComponent.NetworkClosingTime > GeneralVar.CurrentComponent.NetworkOpeningTime)
                {
                    if (CurrentDate.TimeOfDay >= GeneralVar.CurrentComponent.NetworkClosingTime || CurrentDate.TimeOfDay < GeneralVar.CurrentComponent.NetworkOpeningTime)
                        return true;
                    else
                        return false;
                }
                else if (GeneralVar.CurrentComponent.NetworkClosingTime < GeneralVar.CurrentComponent.NetworkOpeningTime)
                {
                    if (CurrentDate.TimeOfDay >= GeneralVar.CurrentComponent.NetworkClosingTime && CurrentDate.TimeOfDay < GeneralVar.CurrentComponent.NetworkOpeningTime)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        #endregion

		#region Constructor

		public MainWindowViewModel()
		{
			try
			{
                SetModuleStage(eModuleStage.Initialize);

                if (timeRemainingTimer != null)
                    timeRemainingTimer = null;

                timeRemainingTimer = new System.Timers.Timer(1000);
                timeRemainingTimer.Elapsed += timeRemainingTimer_Elapsed;

                if (timeRemainingTimeoutTimer != null)
                    timeRemainingTimeoutTimer = null;
                timeRemainingTimeoutTimer = new System.Timers.Timer(1000);
                timeRemainingTimeoutTimer.Elapsed += timeRemainingTimeoutTimer_Elapsed;
            }
			catch (Exception ex)
			{
				Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[error]MainWindowViewModel = {0} ", ex.ToString()), "MainWindowViewModel");
				App.Current.Shutdown(0);
			}
		}

        private void InitializeMainWindow()
        {
            try
            {
                if (GeneralVar.CurrentComponent != null)
                {
                    ComponentName = string.Format("{0}", GeneralVar.CurrentComponent.ComponentName.ToUpper());
                    ComponentCode = string.Format("({0})", GeneralVar.CurrentComponent.ComponentCode.ToUpper());
                }

                if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                { _checkStateTimer = new System.Timers.Timer(10000); }
                else
                {
                    _checkStateTimer = new System.Timers.Timer(1000);
                }

                _checkStateTimer.AutoReset = true;
                _checkStateTimer.Elapsed += new ElapsedEventHandler(checkStateTimer_Elapsed);

                System.Timers.Timer timer = new System.Timers.Timer(1000);
                timer.Elapsed += timer_Elapsed;
                timer.AutoReset = true;
                timer.Start();

                _refreshTimer = new System.Timers.Timer(60000);
                _refreshTimer.Elapsed += _refreshTimer_Elapsed;
                _refreshTimer.Start();

                //_taskTimer = new Timer(60000);
                //_taskTimer.Elapsed += _taskTimer_Elapsed;
                //_taskTimer.Start();

                //SetModuleStage(eModuleStage.MainIdle);
                SetModuleStage(eModuleStage.Home);
            }
            catch (Exception ex) 
            { }
        }

		#endregion

        #region Function

        public void TimeOut(string obj)
        {
            try 
            {
                ShowTimeOut = Visibility.Collapsed;
                if (obj == "Y")
                {
                    isStop = false;
                    timeRemainingTimeoutTimer.Stop();
                    TimeoutCountdown = new TimeSpan(0, 1, 30);

                    timeRemainingTimer.Start();
                    IsTimeRemainingVisible = true;
                }
                else if (obj == "C")
                {
                    timeRemainingTimeoutTimer.Stop();
                    autoResetTimeout.Set();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] TimeOut = {0}", ex.ToString()), TraceCategory);
            }
        }

        public void SetModuleStage(eModuleStage stage, string error = "")
        {
            try
            {
                if (stage == eModuleStage.Home)
                {
                    _checkStateTimer.Stop();
                    _checkStateTimer.Start();

                    if (GeneralFunc.IsOutOfService())
                    {
                        stage = eModuleStage.OutOfService;
                    }
                    else 
                    {
                        if (GeneralVar.IOBoard_Enabled)
                        {
                            if (GeneralVar.IOBoard_SensorControl_Enabled)
                            {
                                if (stage == eModuleStage.Home)
                                {
                                    #region Check Sensor Status 

                                    if (GeneralVar.ACWMachineModel == eCarWashMachine.HEFEI)
                                    {
                                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("SetModuleStage ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                                            
                                        if (GeneralVar.IOBoardCtrl.ACWTermStatus || GeneralVar.IOBoardCtrl.ACWActiveStatus)
                                        {
                                            stage = eModuleStage.InService;
                                        }
                                        else if ((!GeneralVar.IOBoardCtrl.ACWNormalStatus && !GeneralVar.IOBoardCtrl.ACWTermStatus && !GeneralVar.IOBoardCtrl.ACWActiveStatus) || GeneralVar.IOBoardCtrl.ACWErrorWashStatus)
                                        {
                                            stage = eModuleStage.OutOfService;
                                        }
                                    }
                                    else if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                                    {
                                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("SetModuleStage ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                                        if (!GeneralVar.IOBoardCtrl.ACWErrorOperationStatus && !GeneralVar.IOBoardCtrl.ACWSensorObjectStatus)
                                        {
                                            stage = eModuleStage.InService;
                                        }
                                        else if (GeneralVar.IOBoardCtrl.ACWErrorOperationStatus)
                                        {
                                            stage = eModuleStage.OutOfService;
                                        }
                                    }
                                    else if (GeneralVar.ACWMachineModel == eCarWashMachine.PENTAMASTER)
                                    {
                                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("SetModuleStage ACW Status : Error = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.PMErrorWash, GeneralVar.IOBoardCtrl.PMWashing), TraceCategory);
                                        if (!GeneralVar.IOBoardCtrl.PMErrorWash && GeneralVar.IOBoardCtrl.PMWashing)
                                        {
                                            stage = eModuleStage.InService;
                                        }
                                        else if (GeneralVar.IOBoardCtrl.PMErrorWash)
                                        {
                                            stage = eModuleStage.OutOfService;
                                        }
                                    }

                                    #endregion
                                }
                            }
                        }
                    }
                }
                
                _currentModuleStage = stage;

                switch (stage)
                {
                    case eModuleStage.Home:
                        StopCountDown();
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmHome = new HomeViewModel();
                            CurrentModule = vmHome;
                        }));
                        break;
                    case eModuleStage.ServiceSelection:
                        StartTimer();
                        TriggerLanguage();
                        ResetTimer(90);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {                          
                            vmServiceSelection = new ServiceSelectionViewModel();
                            CurrentModule = vmServiceSelection;
                        }));
                        break;
                    case eModuleStage.PaymentSelection:
                        ResetTimer(90);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmPaymentSelection = new PaymentSelectionViewModel();
                            CurrentModule = vmPaymentSelection;
                        }));
                        break;
                    case eModuleStage.MainIdle:
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmMainIdle = new MainIdleViewModel();
                            CurrentModule = vmMainIdle;
                        }));
                        break;
                    case eModuleStage.InService:
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmInService = new InServiceViewModel();
                            CurrentModule = vmInService;
                        }));
                        break;
                    case eModuleStage.PendingCarGoIn:
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmInService = new InServiceViewModel();
                            CurrentModule = vmInService;
                        }));
                        break;
                    case eModuleStage.Initialize:
                        if (_checkStateTimer != null) _checkStateTimer.Stop();
                        if (_refreshTimer != null) _refreshTimer.Stop();
                        //if (_taskTimer != null) _taskTimer.Stop();

                        vmComponentCheck = new SystemCheckViewModel();
                        CurrentModule = vmComponentCheck;
                        vmComponentCheck.Success += componentCheckVM_Success;
                        vmComponentCheck.Failed += componentCheckVM_Failed;
                        vmComponentCheck.PerformCheck();
                        break;
                    case eModuleStage.OutOfService:
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmMaintenance = new MaintenanceViewModel();
                            CurrentModule = vmMaintenance;
                        }));
                        break;

                    case eModuleStage.Error:
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmError = new ErrorViewModel(error);
                            CurrentModule = vmError;
                        }));
                        break;
                    case eModuleStage.Offline:
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmOffline = new OfflineViewModel();
                            CurrentModule = vmOffline;
                        }));
                        break;
                    case eModuleStage.Servicing:
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmServicing = new ServicingViewModel();
                            CurrentModule = vmServicing;
                        }));
                        break;
                    case eModuleStage.ServicingLogin:
                        StopCountDown();
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            vmServicingLogin = new ServicingLoginViewModel();
                            CurrentModule = vmServicingLogin;
                        }));
                        break;
                }
            }
            catch (Exception ex)
            {
                GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("GNR"), "E", ex.Message);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] SetModuleStage: {0}", ex.ToString()), TraceCategory);
            }
        }

        public void TriggerLanguage()
        {
            try
            {
                Label_TimeOutMessage = ACWSSK.Properties.Resources.Label_TimeOutMessage;
                Label_TimeOutNo = ACWSSK.Properties.Resources.Label_TimeOutNo;
                Label_TimeOutYes = ACWSSK.Properties.Resources.Label_TimeOutYes;
            }
            catch (Exception ex)
            {

            }
        }

        void EasyMaintenance()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("EasyMaintenance Starting..."), TraceCategory);

                serviceClickAttempt++;
                if (serviceClickAttempt >= 10)
                {
                    serviceClickAttempt = 0;
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("EasyMaintenance action = Servicing Mode"), TraceCategory);
                    SetModuleStage(eModuleStage.ServicingLogin);
                    //if (Stage == eStage.LandingPage || Stage == eStage.OutOfOrder || Stage == eStage.OffOperation || Stage == eStage.MenuCategory)
                    //{

                    //}
                }
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("EasyMaintenance Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] EasyMaintenance = {0}", ex.ToString()), TraceCategory);
            }
        }

        public void SetPaymentModuleStage(ePaymentMethod Pay) 
        {
            try
            {
                StopCountDown();
                _currentModuleStage = eModuleStage.Payment;

                _checkStateTimer.Stop();
                vmPayment = null;
                vmPayment = new PaymentViewModel(Pay);
                CurrentModule = vmPayment;
            }
            catch (Exception ex)
            {
                GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("GNR"), "E", ex.Message);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] SetPaymentModuleStage: {0}", ex.ToString()), TraceCategory);
            }            
        }

        private void componentCheckVM_Failed(object sender, EventArgs e)
        {
            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(5000);
                vmComponentCheck.Success -= componentCheckVM_Success;
                vmComponentCheck.Failed -= componentCheckVM_Failed;
                vmComponentCheck = null;
                InitializeMainWindow();
            });
            t.Start();
        }

        private void componentCheckVM_Success(object sender, EventArgs e)
        {
            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(5000);
                vmComponentCheck.Success -= componentCheckVM_Success;
                vmComponentCheck.Failed -= componentCheckVM_Failed;
                vmComponentCheck = null;
                InitializeMainWindow();
            });
            t.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrentDate = DateTime.Now;
            CurrentDateString = CurrentDate.ToString("ddd, dd MMM yyyy   hh:mmtt").ToUpper();

            if (serviceClickAttempt > 0)
            {
                if (serviceClickTimeout <= 1)
                {
                    serviceClickTimeout++;
                }
                else
                {
                    serviceClickTimeout = 0;
                    serviceClickAttempt = 0;
                }
            }
        }

        private void checkStateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (GeneralFunc.HasNewData)
                    GeneralFunc.SetData(true);

                if (IsOffline)
                {
                    if (_currentModuleStage == eModuleStage.Home)
                    {
                        SetModuleStage(eModuleStage.Offline);
                    }
                }
                else
                {
                    if (_currentModuleStage == eModuleStage.Offline)
                    {
                        SetModuleStage(eModuleStage.Home);
                    }
                }

                if (GeneralVar.IOBoard_Enabled)
                {
                    if (GeneralVar.IOBoard_SensorControl_Enabled)
                    {
                        if (_currentModuleStage == eModuleStage.InService)
                        {
                            #region InService Status Check Sensor

                            if (GeneralVar.ACWMachineModel == eCarWashMachine.HEFEI)
                            {
                                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("checkStateTimer_Elapsed ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                                if (GeneralVar.IOBoardCtrl.ACWNormalStatus && !GeneralVar.IOBoardCtrl.ACWTermStatus && !GeneralVar.IOBoardCtrl.ACWActiveStatus)
                                {
                                    SetModuleStage(eModuleStage.Home);
                                }
                                else if (GeneralVar.IOBoardCtrl.ACWErrorWashStatus)
                                {
                                    GeneralVar.IOBException = new Exception("IOB02");
                                    GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("IOB"), "E", GeneralVar.ErrorDictionary["IOB02"]);

                                    GeneralVar.IOBoardCtrl.EStopPassCounter++;
                                    SetModuleStage(eModuleStage.OutOfService);
                                }
                            }
                            else if(GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                            {
                                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("checkStateTimer_Elapsed ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                                if (!GeneralVar.IOBoardCtrl.ACWErrorOperationStatus && GeneralVar.IOBoardCtrl.ACWSensorObjectStatus)
                                {
                                    SetModuleStage(eModuleStage.Home);
                                }
                                else if (GeneralVar.IOBoardCtrl.ACWErrorOperationStatus) 
                                {
                                    GeneralVar.IOBException = new Exception("IOB02");
                                    GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("IOB"), "E", GeneralVar.ErrorDictionary["IOB02"]);

                                    SetModuleStage(eModuleStage.OutOfService);
                                }
                            }
                            else if (GeneralVar.ACWMachineModel == eCarWashMachine.PENTAMASTER)
                            {
                                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : Error = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.PMErrorWash, GeneralVar.IOBoardCtrl.PMWashing), TraceCategory);
                                if (!GeneralVar.IOBoardCtrl.PMErrorWash && !GeneralVar.IOBoardCtrl.PMWashing)
                                {
                                    SetModuleStage(eModuleStage.Home);
                                }
                                else if (GeneralVar.IOBoardCtrl.PMErrorWash)
                                {
                                    GeneralVar.IOBException = new Exception("IOB02");
                                    GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("IOB"), "E", GeneralVar.ErrorDictionary["IOB02"]);

                                    SetModuleStage(eModuleStage.OutOfService);
                                }
                            }

                            #endregion
                        }
                        else if (_currentModuleStage == eModuleStage.Home)
                        {
                            #region MainIdle Status Check Sensor 

                            if (GeneralVar.ACWMachineModel == eCarWashMachine.HEFEI)
                            {
                                if (GeneralVar.IOBoardCtrl.ACWTermStatus || GeneralVar.IOBoardCtrl.ACWActiveStatus)
                                {
                                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("checkStateTimer_Elapsed ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                                    SetModuleStage(eModuleStage.InService);
                                }
                                else if ((!GeneralVar.IOBoardCtrl.ACWNormalStatus && !GeneralVar.IOBoardCtrl.ACWTermStatus && !GeneralVar.IOBoardCtrl.ACWActiveStatus) || GeneralVar.IOBoardCtrl.ACWErrorWashStatus)
                                {
                                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("checkStateTimer_Elapsed ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                                    GeneralVar.IOBException = new Exception("IOB02");
                                    GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("IOB"), "E", GeneralVar.ErrorDictionary["IOB02"]);

                                    SetModuleStage(eModuleStage.OutOfService);
                                }
                            }
                            else if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                            {
                                if (!GeneralVar.IOBoardCtrl.ACWErrorOperationStatus && !GeneralVar.IOBoardCtrl.ACWSensorObjectStatus)
                                {
                                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("checkStateTimer_Elapsed ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                                    SetModuleStage(eModuleStage.InService);
                                }
                                else if (GeneralVar.IOBoardCtrl.ACWErrorOperationStatus)
                                {
                                    GeneralVar.IOBException = new Exception("IOB02");
                                    GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("IOB"), "E", GeneralVar.ErrorDictionary["IOB02"]);

                                    SetModuleStage(eModuleStage.OutOfService);
                                }
                            }
                            else if (GeneralVar.ACWMachineModel == eCarWashMachine.PENTAMASTER)
                            {
                                if (!GeneralVar.IOBoardCtrl.PMErrorWash && GeneralVar.IOBoardCtrl.PMWashing)
                                {
                                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : Error = {0}, Washing = {1}", GeneralVar.IOBoardCtrl.PMErrorWash, GeneralVar.IOBoardCtrl.PMWashing), TraceCategory);
                                    SetModuleStage(eModuleStage.InService);
                                }
                                else if (GeneralVar.IOBoardCtrl.PMErrorWash)
                                {
                                    GeneralVar.IOBException = new Exception("IOB02");
                                    GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("IOB"), "E", GeneralVar.ErrorDictionary["IOB02"]);

                                    SetModuleStage(eModuleStage.OutOfService);
                                }
                            }

                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private void _refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (GeneralVar.RefreshDataTime.Any(t => t.ToString(@"hh\:mm") == DateTime.Now.ToString("HH:mm")))
            {
                _refreshTimer.Stop();

                GeneralFunc.GetData(true);

                _refreshTimer.Start();
            }
        }

        //private void _taskTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    if (GeneralVar.CurrentComponent.EDCSettlementTime.Any(t => t.ToString(@"hh\:mm") == DateTime.Now.ToString("HH:mm")))
        //    {
        //        _taskTimer.Stop();

        //        if (GeneralVar.CreditCardTerminal_Enabled)
        //        {
        //            DoEDCSettlement();
        //        }

        //        _taskTimer.Start();
        //    }
        //}

        public void DoEDCSettlement() 
        {
            try 
            { }
            catch (Exception ex) 
            { }
        }

        public void ShowErrorRedirect(string errormsg)
        {
            SetModuleStage(eModuleStage.Error, errormsg);

            Action redirect = new Action(() =>
            {
                System.Threading.Thread.Sleep(5000);
                SetModuleStage(eModuleStage.Home);
            });

            System.Threading.ThreadPool.QueueUserWorkItem(o => redirect());
        }

        #endregion

        #region Custom Function

        //public void ClearSavedReceipt()
        //{
        //    try
        //    {
        //        string[] files = Directory.GetFiles(GeneralVar.SaveReceiptDirectory);

        //        foreach (string file in files)
        //        {
        //            try
        //            {
        //                FileInfo fi = new FileInfo(file);
        //                if (fi.LastWriteTime < DateTime.Now.AddHours(-24))
        //                    fi.Delete();
        //            }
        //            catch (Exception ex) { }
        //        }
        //    }
        //    catch (Exception ex) { }
        //}
        
		#endregion

        #region Reset Machine

        private ICommand _ResetMachineCommand;
        public ICommand ResetMachineCommand
        {
            get
            {
                if (_ResetMachineCommand == null)
                    _ResetMachineCommand = new RelayCommand(ResetMachine);

                return _ResetMachineCommand;
            }
        }

        public void ResetMachine()
        {
            try
            {
                if (GeneralVar.ApplicationMode == AppMode.PROD)
                {
                    if (GeneralVar.IOBoard_Enabled)
                    {
                        if (_currentModuleStage == eModuleStage.OutOfService || _currentModuleStage == eModuleStage.Servicing)
                        {
                            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Trigger Reset Auto-Car Wash Machine..."), TraceCategory);
                            GeneralVar.IOBoardCtrl.ResetPassCounter++;
                            //MessageBox.Show("Trigger Reset Machine");
                        }
                    }
                }
                else
                {
                    if (GeneralVar.IOBoard_Enabled)
                    {
                        if (_currentModuleStage == eModuleStage.OutOfService || _currentModuleStage == eModuleStage.Servicing)
                        {
                            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Trigger Reset Auto-Car Wash Machine..."), TraceCategory);
                            GeneralVar.IOBoardCtrl.ResetPassCounter++;
                            //MessageBox.Show("Trigger Reset Machine");
                        }
                    }
                    else 
                    { //MessageBox.Show("Trigger Reset Machine");
                           
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] ResetMachine = {0}", ex.ToString()), TraceCategory);
            }
        }

        public void StartMachine()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("StartMachine Starting..."), TraceCategory);
                if (GeneralVar.IOBoard_Enabled)
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Trigger Start Auto-Car Wash Machine..."), TraceCategory);
                    GeneralVar.IOBoardCtrl.EntryPassCounter++;                 
                }
                //MessageBox.Show("Trigger Start Machine");
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("StartMachine Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] StartMachine = {0}", ex.ToString()), TraceCategory);
            }
        }

        public void StopMachine()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("StopMachine Starting..."), TraceCategory);
                if (GeneralVar.IOBoard_Enabled)
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Trigger Stop Auto-Car Wash Machine..."), TraceCategory);
                    GeneralVar.IOBoardCtrl.EStopPassCounter++;          
                }
                //MessageBox.Show("Trigger Stop Machine");
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("StopMachine Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] StopMachine = {0}", ex.ToString()), TraceCategory);
            }
        }

        #endregion

        #region Timer

        int _TimeOutSecond = 90;
        TimeSpan _TimeRemaining, _TimeRemainingRenewal;
        bool _IsTimeRemainingVisible = false;

        public TimeSpan TimeoutCountdown
        {
            get { return _TimeRemaining; }
            set { _TimeRemaining = value; OnPropertyChanged("TimeoutCountdown"); }
        }

        public bool IsTimeRemainingVisible
        {
            get { return _IsTimeRemainingVisible; }
            set { _IsTimeRemainingVisible = value; OnPropertyChanged("IsTimeRemainingVisible"); }
        }

        public TimeSpan TimeoutRenewalCountdown
        {
            get { return _TimeRemainingRenewal; }
            set { _TimeRemainingRenewal = value; OnPropertyChanged("TimeoutRenewalCountdown"); }
        }

        public void StartTimer()
        {
            Thread t = new Thread(() => StartCountDown(_TimeOutSecond));
            t.Start();
        }

        void TimeOutTrigger()
        {
            StopCountDown();
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ShowLoading = Visibility.Visible;
            }));
            Thread.Sleep(900);
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ShowLoading = Visibility.Collapsed;
                SetModuleStage(eModuleStage.Home);
            }));
        }

        AutoResetEvent autoResetTimeout = new AutoResetEvent(false);
        public bool isStop = false;
        System.Timers.Timer timeRemainingTimer, timeRemainingTimeoutTimer;
        public void StartCountDown(int second)
        {

            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "StartCountDown Starting...", TraceCategory);
            isStop = false;
            TimeoutCountdown = new TimeSpan(0, 0, second);

            timeRemainingTimer.Start();
            IsTimeRemainingVisible = true;

            autoResetTimeout.WaitOne();
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "StartCountDown Completed.", TraceCategory);
            TimeOutTrigger();
        }

        public void StopCountDown()
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "StopCountDown Starting...", TraceCategory);
            timeRemainingTimer.Stop();
            isStop = true;
            IsTimeRemainingVisible = false;
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "StopCountDown Completed.", TraceCategory);
        }

        private void timeRemainingTimer_Elapsed(object sender, EventArgs e)
        {
            TimeoutCountdown = TimeoutCountdown.Subtract(new TimeSpan(0, 0, 1));
            //ShowTimeOut = Visibility.Collapsed;
            if (TimeoutCountdown.TotalSeconds > 0)
                return;
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "timeRemainingTimer_Elapsed Starting...", TraceCategory);
            timeRemainingTimer.Stop();

            isStop = true;
            IsTimeRemainingVisible = false;
            ShowLoading = Visibility.Collapsed;
            ShowTimeOut = Visibility.Visible;
            TimeoutRenewalCountdown = new TimeSpan(0, 0, 10);
            timeRemainingTimeoutTimer.Start();

            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "timeRemainingTimer_Elapsed Completed.", TraceCategory);
        }

        private void timeRemainingTimeoutTimer_Elapsed(object sender, EventArgs e)
        {
            TimeoutRenewalCountdown = TimeoutRenewalCountdown.Subtract(new TimeSpan(0, 0, 1));

            if (TimeoutRenewalCountdown.TotalSeconds > 0)
                return;

            timeRemainingTimeoutTimer.Stop();
            ShowTimeOut = Visibility.Collapsed;
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "timeRemainingTimeoutTimer_Elapsed Starting...", TraceCategory);
            autoResetTimeout.Set();
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "timeRemainingTimeoutTimer_Elapsed Completed.", TraceCategory);
        }

        public void ResetTimer(int second)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "ResetTimer Starting...", TraceCategory);
                TimeoutCountdown = new TimeSpan(0, 0, second);
                timeRemainingTimeoutTimer.Stop();
                if (isStop)
                {
                    StartTimer();
                }
                ShowTimeOut = Visibility.Collapsed;
                IsTimeRemainingVisible = true;
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "ResetTimer Completed.", TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] ResetTimeOut:{0}", ex.ToString()), TraceCategory);
            }
        }

        #endregion

        public void RefreshLanguage()
        {
            OnPropertyChanged("");
        }
    }
}