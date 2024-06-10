using ACWSSK.App_Code;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ACWSSK.ViewModel
{
    public class ServicingLoginViewModel : BaseViewModel
    {
        public ServicingLoginViewModel()
        {
            if (bwMaintenanceTimer == null)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Initial Maintenance Timer Starting..."), TraceCategory);

                bwMaintenanceTimer = new BackgroundWorker();
                bwMaintenanceTimer.WorkerReportsProgress = true;
                bwMaintenanceTimer.WorkerSupportsCancellation = true;
                bwMaintenanceTimer.ProgressChanged += bwMaintenanceTimer_ProgressChanged;
                bwMaintenanceTimer.DoWork += bwMaintenanceTimer_DoWork;
                bwMaintenanceTimer.RunWorkerCompleted += bwMaintenanceTimer_RunWorkerCompleted;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Initial Maintenance Timer Completed."), TraceCategory);
            }

            Username = "";
            Password = "";
            StartMaintenancebwTimer(new Action(BackToHome));
        }

        protected string TraceCategory = "ServicingLoginViewModel";
        private string _Password;
        private string _Username;

        private bool _IsError = false;
        private string _ErrorMessage = string.Empty;
        private bool _ValidUser = true;
        private bool _ValidPassword = true;
        private string _ProcessingFlow = string.Empty;

        public string Username
        {
            get { return _Username; }
            set
            {
                _Username = value;
                OnPropertyChanged("Username");
            }
        }

        public string Password
        {
            get { return _Password; }
            set
            {
                _Password = value;
                OnPropertyChanged("Password");
                OnPropertyChanged("PasswordIsEmpty");
            }
        }

        private string _IntructionDescription = "";
        public string IntructionDescription
        {
            get { return _IntructionDescription; }
            set
            {
                _IntructionDescription = value;
                OnPropertyChanged("IntructionDescription");
            }
        }

        private string _OnOffStatus = "On Line";
        public string OnOffStatus
        {
            get { return _OnOffStatus; }
            set
            {
                _OnOffStatus = value;
                OnPropertyChanged("OnOffStatus");
            }
        }

        public bool PasswordIsEmpty
        {
            get { return string.IsNullOrEmpty(Password); }
        }

        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                _ErrorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        public bool ValidUser
        {
            get { return _ValidUser; }
            set
            {
                _ValidUser = value;
                OnPropertyChanged("ValidUser");
            }
        }

        public bool ValidPassword
        {
            get { return _ValidPassword; }
            set
            {
                _ValidPassword = value;
                OnPropertyChanged("ValidPassword");

            }
        }

        public bool IsError
        {
            get { return _IsError; }
            set
            {
                _IsError = value;
                OnPropertyChanged("IsError");
            }
        }

        ICommand _LoginCommand;
        public ICommand LoginCommand
        {
            get
            {
                if (_LoginCommand == null)
                    _LoginCommand = new RelayCommand(Login);

                return _LoginCommand;
            }
        }

        void Login()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Login Starting..."), TraceCategory);

                if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                {
                    setErrorValue(!string.IsNullOrEmpty(Username), !string.IsNullOrEmpty(Password), "Username and Password cannot be empty!");
                    return;
                }
                bool ValidAccessRight = false;


                //if (GeneralVar.user == null)
                //    GeneralVar.user = new KFCFOS.Helper.User();

                //bool valid = GeneralVar.user.UserAuthentication(Username, Password, out _ValidUser, out _ValidPassword, out ValidAccessRight, out ValidCompanyComponent, "ManageLoginAuth");
                bool valid = false;
                if (Username == "123" && Password == "123")
                {
                    valid = true;
                }
                else if (Username == "123")
                {
                    _ValidUser = true;
                    _ValidPassword = false;
                }
                else
                {
                    _ValidUser = false;
                    _ValidPassword = false;
                }


                if (valid)
                {
                    CancelMaintenancebwTimer(false);
                    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Servicing);
                }
                else if (!_ValidUser)
                    setErrorValue(_ValidUser, _ValidPassword, "Invalid Username");
                else if (!_ValidPassword)
                    setErrorValue(_ValidUser, _ValidPassword, "Invalid Password");
                else if (!ValidAccessRight)
                    setErrorValue(_ValidUser, _ValidPassword, "No Access to Login Kiosk");

                //if(GeneralVar.IOBoard_Enabled)
                //{
                //    GeneralVar.IOBoard.alarmTrigger = true;
                //}

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Login Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] Login = {0}", ex.ToString()), TraceCategory);

            }
        }

        public void CancelMaintenancebwTimer(bool stopWithAction)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("CancelMaintenancebwTimer Starting..."), TraceCategory);

                this._stopWithAction = stopWithAction;
                idleDuration = 30000 / 1000;

                if (bwMaintenanceTimer.IsBusy && bwMaintenanceTimer.WorkerSupportsCancellation)
                    bwMaintenanceTimer.CancelAsync();

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("CancelMaintenancebwTimer Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CancelMaintenancebwTimer = {0}", ex.ToString()), TraceCategory);
            }
        }

        void setErrorValue(bool vUser, bool vPassword, string error)
        {
            ValidUser = vUser;
            ValidPassword = vPassword;
            IsError = !(vUser && vPassword);
            ErrorMessage = error;
        }

        #region Maintenance Timeout

        BackgroundWorker bwMaintenanceTimer;
        int idleDuration;
        bool _stopWithAction = false;
        Action bwMaintenanceAction;

        TimeSpan _TimeRemaining, _TimeRemainingRenewal;
        string _TimeRemainingLanguage;
        bool _IsTimeRemainingVisible = false;
        public TimeSpan TimeoutCountdown
        {
            get { return _TimeRemaining; }
            set { _TimeRemaining = value; OnPropertyChanged("TimeoutCountdown"); }
        }

        public string TimeRemainingLanguage
        {
            get { return _TimeRemainingLanguage; }
            set { _TimeRemainingLanguage = value; OnPropertyChanged("TimeRemainingLanguage"); }
        }

        public TimeSpan TimeoutRenewalCountdown
        {
            get { return _TimeRemainingRenewal; }
            set { _TimeRemainingRenewal = value; OnPropertyChanged("TimeoutRenewalCountdown"); }
        }

        public bool IsTimeRemainingVisible
        {
            get { return _IsTimeRemainingVisible; }
            set { _IsTimeRemainingVisible = value; OnPropertyChanged("IsTimeRemainingVisible"); }
        }

        private string _sTimeLeft;
        public string sTimeLeft
        {
            get { return _sTimeLeft; }
            set
            {
                _sTimeLeft = value;
                OnPropertyChanged("sTimeLeft");
            }
        }

        public string ProcessingFlow
        {
            get { return _ProcessingFlow; }
            set
            {
                _ProcessingFlow = value;
                OnPropertyChanged("ProcessingFlow");
            }
        }

        public bool IsBusy
        {
            get
            {
                if (bwMaintenanceTimer != null)
                    return bwMaintenanceTimer.IsBusy;
                return false;
            }
        }

        public void StartMaintenancebwTimer(Action action)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("StartMaintenancebwTimer Starting..."), TraceCategory);

                this._stopWithAction = true;

                idleDuration = 30000 / 1000;
                idleDuration += 10;
                if (this.bwMaintenanceAction != null)
                    this.bwMaintenanceAction = null;

                if (this.bwMaintenanceAction != action)
                    this.bwMaintenanceAction = action;

                if (!bwMaintenanceTimer.IsBusy)
                    bwMaintenanceTimer.RunWorkerAsync();

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("StartMaintenancebwTimer Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] StartMaintenancebwTimer = {0}", ex.ToString()), TraceCategory);
            }
        }

        public void ResetMaintenancebwTimer(Action action)
        {
            try
            {
                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ResetTimer Starting..."), traceCategory);

                idleDuration = 30000 / 1000;
                idleDuration += 10;
                _stopWithAction = true;
                if (this.bwMaintenanceAction != null)

                    if (this.bwMaintenanceAction != action)
                    {
                        this.bwMaintenanceAction = null;
                        this.bwMaintenanceAction = action;
                    }

                if (bwMaintenanceTimer != null && !bwMaintenanceTimer.IsBusy)
                    bwMaintenanceTimer.RunWorkerAsync();

                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ResetTimer Completed."), traceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] ResetMaintenancebwTimer = {0}", ex.ToString()), TraceCategory);
            }
        }

        void bwMaintenanceTimer_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "bwMaintenanceTimer_DoWork Starting...", TraceCategory);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("bwMaintenanceTimer_DoWork [Duration]= {0}", 30000), TraceCategory);


                BackgroundWorker bw = sender as BackgroundWorker;

                while (!bw.CancellationPending && idleDuration > 1)
                {
                    idleDuration--;
                    if (idleDuration < 10)
                        bw.ReportProgress(idleDuration, "p");
                    else
                        bw.ReportProgress(idleDuration, "v");
                    Thread.Sleep(1000);
                }
                bw.ReportProgress(idleDuration, "c");
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error]bwMaintenanceTimer_DoWork = {0}", ex.ToString()), TraceCategory);
            }
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "bwMaintenanceTimer_DoWork Starting...", TraceCategory);
        }

        void bwMaintenanceTimer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            IsTimeRemainingVisible = true;
            TimeoutCountdown = new TimeSpan(0, 0, e.ProgressPercentage);
            TimeRemainingLanguage = "Time Remaining";
            sTimeLeft = TimeRemainingLanguage + TimeoutCountdown.ToString();
        }

        void bwMaintenanceTimer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("bwTimer_RunWorkerCompleted Starting..."), TraceCategory);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("bwTimer_RunWorkerCompleted [_stopWithAction = {0}]", _stopWithAction), TraceCategory);
                if (bwMaintenanceAction != null && _stopWithAction)
                    bwMaintenanceAction();

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("bwTimer_RunWorkerCompleted Completed"), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] bwTimer_RunWorkerCompleted {0}", ex.ToString()), TraceCategory);
            }
        }

        public void BackToHome()
        {
            try
            {
                GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
            }
            catch (Exception)
            {

            }
        }

        #endregion
    }
}
