using ACWSSK.App_Code;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using LB_ipay88.MHGatewayService;
using LB_ipay88.Model;
using LB_ipay88.Utility.Builder;
using LB_ipay88.Utility.Reflector;
using LB_ipay88.Utility.Sender;
using System.Windows.Input;
using ACWSSK.Model;
using System.Timers;
using System.Windows.Controls;
using ACWSSK.View;
using System.Threading;
using System.Configuration;
using System.Windows.Threading;
using static ACWSSK.Model.QRReceiptModel;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;

namespace ACWSSK.ViewModel
{
    public class PaymentViewModel : BaseViewModel
    {
        const string TraceCategory = "PaymentViewModel";

        #region Field

        private int _txId = 0;
        private DateTime _txDate = DateTime.MinValue;
        private string _referenceNo = "";

        private System.Timers.Timer _triTimer;
        private bool IsCountdownTriDone;

        private System.Timers.Timer _txTimer;

        UserControl _CurrentPayment;
        Dictionary<ePaymentStage, UserControl> _Modules;
        private ePaymentMethod _SelectedPaymentMethod;
        AutoResetEvent _WaitDone = new AutoResetEvent(false);
        AutoResetEvent _WaitPrint = new AutoResetEvent(false);
        private ePaymentStage currentPaymentStage;

        #endregion

        #region Properties

        AutoResetEvent waitBarcode = new AutoResetEvent(false);

        private string _cardNo = string.Empty;
        public string CardNo
        {
            get { return _cardNo; }
            set
            {
                //if (value == _cardNo)
                //    return;

                _cardNo = value;
                OnPropertyChanged("CardNo");

                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("CardNo: {0}", _cardNo), TraceCategory);

                if (_cardNo.EndsWith("\r\n"))
                {
                    Trace.WriteLine(_cardNo);
                    //string cardNo = _cardNo.Replace("\r\n", "").Trim();
                    //System.Threading.ThreadPool.QueueUserWorkItem(o => PerformEWalletPayment(cardNo));
                    //PerformEWalletPayment(cardNo);
                    _barcodeScanned = _cardNo.Replace("\r\n", "").Trim();
                    waitBarcode.Set();
                    //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("CardNo: END", _cardNo), TraceCategory);
                }
            }
        }

        private string _barcodeScanned = string.Empty;

        Visibility _PaymentProcessVisibility = Visibility.Collapsed;
        public Visibility PaymentProcessVisibility
        {
            get { return _PaymentProcessVisibility; }
            set
            {
                _PaymentProcessVisibility = value;
                OnPropertyChanged("PaymentProcessVisibility");
            }
        }

        Visibility _ServiceProcessVisibility = Visibility.Collapsed;
        public Visibility ServiceProcessVisibility
        {
            get { return _ServiceProcessVisibility; }
            set
            {
                _ServiceProcessVisibility = value;
                OnPropertyChanged("ServiceProcessVisibility");
            }
        }

        Visibility _DemoVisibility = Visibility.Collapsed;
        public Visibility DemoVisibility
        {
            get { return _DemoVisibility; }
            set
            {
                _DemoVisibility = value;
                OnPropertyChanged("DemoVisibility");
            }
        }

        string _TextInstructionA;
        public string TextInstructionA
        {
            get { return _TextInstructionA; }
            set
            {
                _TextInstructionA = value;
                OnPropertyChanged("TextInstructionA");
            }
        }

        string _TextInstructionB;
        public string TextInstructionB
        {
            get { return _TextInstructionB; }
            set
            {
                _TextInstructionB = value;
                OnPropertyChanged("TextInstructionB");
            }
        }

        string _TextInstructionC;
        public string TextInstructionC
        {
            get { return _TextInstructionC; }
            set
            {
                _TextInstructionC = value;
                OnPropertyChanged("TextInstructionC");
            }
        }

        string _Label_Cancel;
        public string Label_Cancel
        {
            get { return _Label_Cancel; }
            set
            {
                _Label_Cancel = value;
                OnPropertyChanged("Label_Cancel");
            }
        }

        string _Label_PaymentProcessing;
        public string Label_PaymentProcessing
        {
            get { return _Label_PaymentProcessing; }
            set
            {
                _Label_PaymentProcessing = value;
                OnPropertyChanged("Label_PaymentProcessing");
            }
        }

        string _Label_PaymentSuccess;
        public string Label_PaymentSuccess
        {
            get { return _Label_PaymentSuccess; }
            set
            {
                _Label_PaymentSuccess = value;
                OnPropertyChanged("Label_PaymentSuccess");
            }
        }

        string _Label_PaymentFailed;
        public string Label_PaymentFailed
        {
            get { return _Label_PaymentFailed; }
            set
            {
                _Label_PaymentFailed = value;
                OnPropertyChanged("Label_PaymentFailed");
            }
        }

        string _Label_ScanQRFailed;
        public string Label_ScanQRFailed
        {
            get { return _Label_ScanQRFailed; }
            set
            {
                _Label_ScanQRFailed = value;
                OnPropertyChanged("Label_ScanQRFailed");
            }
        }

        string _Label_ScanEReceipt;
        public string Label_ScanEReceipt
        {
            get { return _Label_ScanEReceipt; }
            set
            {
                _Label_ScanEReceipt = value;
                OnPropertyChanged("Label_ScanEReceipt");
            }
        }

        string _Label_TryAgain;
        public string Label_TryAgain
        {
            get { return _Label_TryAgain; }
            set
            {
                _Label_TryAgain = value;
                OnPropertyChanged("Label_TryAgain");
            }
        }

        string _Label_MoveCar;
        public string Label_MoveCar
        {
            get { return _Label_MoveCar; }
            set
            {
                _Label_MoveCar = value;
                OnPropertyChanged("Label_MoveCar");
            }
        }

        string _Label_ScanLecshineApp;
        public string Label_ScanLecshineApp
        {
            get { return _Label_ScanLecshineApp; }
            set
            {
                _Label_ScanLecshineApp = value;
                OnPropertyChanged("Label_ScanLecshineApp");
            }
        }

        string _Label_Done;
        public string Label_Done
        {
            get { return _Label_Done; }
            set
            {
                _Label_Done = value;
                OnPropertyChanged("Label_Done");
            }
        }

        string _Label_GenerateReceipt;
        public string Label_GenerateReceipt
        {
            get { return _Label_GenerateReceipt; }
            set
            {
                _Label_GenerateReceipt = value;
                OnPropertyChanged("Label_GenerateReceipt");
            }
        }

        string _InstructionImage;
        public string InstructionImage
        {
            get { return _InstructionImage; }
            set
            {
                _InstructionImage = value;
                OnPropertyChanged("InstructionImage");
            }
        }

        //private Uri _videoUri;
        //public Uri VideoUri
        //{
        //    get { return _videoUri; }
        //    set
        //    {
        //        _videoUri = value;
        //        base.OnPropertyChanged("VideoUri");
        //    }
        //}

        private string _videoUri;
        public string VideoUri
        {
            get { return _videoUri; }
            set
            {
                _videoUri = value;
                base.OnPropertyChanged("VideoUri");
            }
        }

        decimal _Total = 0.00m;
        public decimal Total
        {
            get { return _Total; }
            set
            {
                _Total = value;
                OnPropertyChanged("Total");
            }
        }

        decimal _TotalFare = 0.00m;
        public decimal TotalFare
        {
            get { return _TotalFare; }
            set
            {
                _TotalFare = value;
                OnPropertyChanged("TotalFare");
            }
        }

        decimal _TotalTax = 0.00m;
        public decimal TotalTax
        {
            get { return _TotalTax; }
            set
            {
                _TotalTax = value;
                OnPropertyChanged("TotalTax");
            }
        }

        private ICommand _DemoPayCommand;
        public ICommand DemoPayCommand
        {
            get
            {
                if (_DemoPayCommand == null)
                    _DemoPayCommand = new RelayCommand<string>(DemoPaymentSelected);

                return _DemoPayCommand;
            }
        }

        private ICommand _DoneCommand;
        public ICommand DoneCommand
        {
            get
            {
                if (_DoneCommand == null)
                    _DoneCommand = new RelayCommand(Done);

                return _DoneCommand;
            }
        }

        private TimeSpan _timeoutCountdown;
        public TimeSpan TimeoutCountdown
        {
            get { return _timeoutCountdown; }
            set
            {
                if (value == _timeoutCountdown)
                    return;

                _timeoutCountdown = value;
                base.OnPropertyChanged("TimeoutCountdown");
            }
        }

        private TimeSpan _triggerServiceCountdown;
        public TimeSpan TriggerServiceCountdown
        {
            get { return _triggerServiceCountdown; }
            set
            {
                if (value == _triggerServiceCountdown)
                    return;

                _triggerServiceCountdown = value;
                base.OnPropertyChanged("TriggerServiceCountdown");
            }
        }

        private BitmapImage _QRReceipt;
        public BitmapImage QRReceipt
        {
            get { return _QRReceipt; }
            set { _QRReceipt = value; OnPropertyChanged("QRReceipt"); }

        }

        public UserControl CurrentPayment
        {
            get { return _CurrentPayment; }
            set
            {
                _CurrentPayment = value;
                OnPropertyChanged("CurrentPayment");
            }
        }

        Dictionary<ePaymentStage, UserControl> Modules
        {
            get
            {
                if (_Modules == null || _Modules.Count <= 0)
                {
                    _Modules = new Dictionary<ePaymentStage, UserControl>();
                    _Modules.Add(ePaymentStage.eWalletPayment, new PaymentPageView());
                    _Modules.Add(ePaymentStage.CardPayment, new PaymentPageView());
                    _Modules.Add(ePaymentStage.AppPayment, new PaymentPageView());
                    _Modules.Add(ePaymentStage.ProcessTransaction, new PaymentProcessView());
                    _Modules.Add(ePaymentStage.TrxTimeout, new PaymentTimeoutView());
                    _Modules.Add(ePaymentStage.Failed, new PaymentFailedView());
                    _Modules.Add(ePaymentStage.Success, new PaymentSuccessView());
                    _Modules.Add(ePaymentStage.PerformService, new PaymentServiceView());
                    _Modules.Add(ePaymentStage.ReceiptQR, new PaymentReceiptQR());
                }
                return _Modules;
            }
        }

        #endregion

        #region Contructor

        public PaymentViewModel()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "PaymentViewModel Starting...", TraceCategory);

                CardNo = string.Empty;
                _barcodeScanned = string.Empty;
                _isPrint = false;
                _TrxDTSuccess = new DateTime();
                _printCount = 0;
                IsEWalletProcess = false;

                //DetachKeyboardHook();
                DemoVisibility = Visibility.Collapsed;

                _txTimer = new System.Timers.Timer(1000);
                _txTimer.Elapsed += _txTimer_Elapsed;

                _triTimer = new System.Timers.Timer(1000);
                _triTimer.Elapsed += _triTimer_Elapsed;
                IsCountdownTriDone = false;



                TotalFare = GetTxPrice();
                TotalTax = TotalFare * (GeneralVar.CurrentTaxRate.Value / 100);
                Total = TotalFare + TotalTax;
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Total Fare = {0}, Total Tax = {1}, Total Amount = {2}", TotalFare.ToString("#.00"), TotalTax.ToString("0.00"), Total.ToString("#.00")), TraceCategory);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Tax Code = {0}, Tax Name = {1}, Tax Rate = {2}", GeneralVar.CurrentTaxRate.RateCode, GeneralVar.CurrentTaxRate.RateName, GeneralVar.CurrentTaxRate.Value.ToString("#.00")), TraceCategory);

                //ProcessStage(ePaymentStage.InitialPayment);
                if (Total > 0)
                {
                    //if (GeneralVar.EWallet_Enabled && GeneralVar.BarcodeReader_Enabled) { AttachKeyboardHook(); }

                    if (GeneralVar.CreditCardTerminal_Enabled)
                    {
                        // initial / trigger CC Terminal Payment 
                    }

                    System.Threading.Thread.Sleep(1000);
                    //ProcessStage(ePaymentStage.PendingPayment);
                    if (GeneralVar.ApplicationMode == AppMode.DEVT)
                        DemoVisibility = Visibility.Visible;

                    StartTxTimer();
                }


                if (GeneralVar.IOBoard_Enabled)
                {
                    if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Pre-Standby Tiggered"), TraceCategory);
                        GeneralVar.IOBoardCtrl.ResetPassCounter++;
                    }
                }
            }
            catch (Exception ex)
            {
                //GeneralVar.Crud.TxAlarm_Insert(DateTime.Now, (char)eAlarmTxType.Error, GeneralVar.AlarmCategory[eAlarmCategory.GNR], ex.Message);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PaymentViewModel: {0}", ex.ToString()), TraceCategory);
            }
        }

        public PaymentViewModel(ePaymentMethod paytype)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "PaymentViewModel Starting...", TraceCategory);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PaymentViewModel paytype = {0}", paytype), TraceCategory);

                Label_Cancel = ACWSSK.Properties.Resources.Label_Cancel;
                Label_ScanLecshineApp = ACWSSK.Properties.Resources.Label_ScanLecshineApp;
                Label_PaymentProcessing = ACWSSK.Properties.Resources.Label_PaymentProcessing;
                Label_PaymentSuccess = ACWSSK.Properties.Resources.Label_PaymentSuccess;
                Label_PaymentFailed = ACWSSK.Properties.Resources.Label_PaymentFailed;
                Label_ScanQRFailed = ACWSSK.Properties.Resources.Label_ScanQRFailed;
                Label_ScanEReceipt = ACWSSK.Properties.Resources.Label_ScanEReceipt;
                Label_MoveCar = ACWSSK.Properties.Resources.Label_MoveCar;
                Label_TryAgain = ACWSSK.Properties.Resources.Label_TryAgain;
                Label_Done = ACWSSK.Properties.Resources.Label_Done;
                Label_GenerateReceipt = ACWSSK.Properties.Resources.Label_GenerateReceipt;

                if (QRReceipt != null)
                    QRReceipt = null;

                _SelectedPaymentMethod = paytype;
                _barcodeScanned = string.Empty;
                _isPrint = false;
                _TrxDTSuccess = new DateTime();
                _printCount = 0;
                IsEWalletProcess = false;

                CardNo = string.Empty;
                //DetachKeyboardHook();
                DemoVisibility = Visibility.Collapsed;

                _txTimer = new System.Timers.Timer(1000);
                _txTimer.Elapsed += _txTimer_Elapsed;

                _triTimer = new System.Timers.Timer(1000);
                _triTimer.Elapsed += _triTimer_Elapsed;
                IsCountdownTriDone = false;

                TotalFare = GetTxPrice();
                TotalTax = TotalFare * (GeneralVar.CurrentTaxRate.Value / 100);
                Total = TotalFare + TotalTax;
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Total Fare = {0}, Total Tax = {1}, Total Amount = {2}", TotalFare.ToString("#.00"), TotalTax.ToString("0.00"), Total.ToString("#.00")), TraceCategory);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Tax Code = {0}, Tax Name = {1}, Tax Rate = {2}", GeneralVar.CurrentTaxRate.RateCode, GeneralVar.CurrentTaxRate.RateName, GeneralVar.CurrentTaxRate.Value.ToString("#.00")), TraceCategory);

                StartTxTimer();

                if (GeneralVar.ApplicationMode == AppMode.PROD)
                { }
                else
                {
                    DemoVisibility = Visibility.Visible;
                }

                if (_SelectedPaymentMethod == ePaymentMethod.eWallet)
                {
                    if (GeneralVar.EWallet_Enabled && GeneralVar.BarcodeReader_Enabled)
                    {
                        // AttachKeyboardHook();

                        Thread th = new Thread(() =>
                        {
                            PerformEWalletpaymentInThread();
                        });
                        th.Start();
                    }
                    SetPaymentStage(ePaymentStage.eWalletPayment);
                }
                if (_SelectedPaymentMethod == ePaymentMethod.CCards)
                {
                    if (GeneralVar.CreditCardTerminal_Enabled) { StartCCPayment(); }
                    SetPaymentStage(ePaymentStage.CardPayment);
                }
                if (_SelectedPaymentMethod == ePaymentMethod.App)
                {
                    if (GeneralVar.App_Enabled && GeneralVar.BarcodeReader_Enabled)
                    {
                        //AttachKeyboardHook();

                        Thread th = new Thread(() =>
                        {
                            PerformAppPaymentInThread();
                        });
                        th.Start();
                    }
                    SetPaymentStage(ePaymentStage.AppPayment);
                }


                if (GeneralVar.IOBoard_Enabled)
                {
                    if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Pre-Standby Tiggered"), TraceCategory);
                        GeneralVar.IOBoardCtrl.ResetPassCounter++;
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("GNR"), "E", ex.Message);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PaymentViewModel: {0}", ex.ToString()), TraceCategory);
            }
        }

        private void _txTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                TimeoutCountdown = TimeoutCountdown.Subtract(new TimeSpan(0, 0, 1));

                if (TimeoutCountdown.TotalSeconds > 0)
                    return;

                //cancel transactions
                _txTimer.Stop();
                SetPaymentStage(ePaymentStage.TrxTimeout);


                CancelPayment();
                System.Threading.Thread.Sleep(3000);
                GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] _triTimer_Elapsed: {0}", ex.ToString()), TraceCategory);
            }
        }

        private void StartTxTimer()
        {
            TimeoutCountdown = GeneralVar.TransactionTimeout;

            if (_txTimer.Enabled)
                _txTimer.Stop();

            _txTimer.Start();
        }

        private void StopTxTimer()
        {
            if (_txTimer.Enabled)
                _txTimer.Stop();
        }

        private void _triTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                TriggerServiceCountdown = TriggerServiceCountdown.Subtract(new TimeSpan(0, 0, 1));

                if (TriggerServiceCountdown.TotalSeconds > 0)
                    return;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("IsCountdownTriDone True..."), TraceCategory);
                IsCountdownTriDone = true;

                if (_triTimer.Enabled)
                    _triTimer.Stop();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] _triTimer_Elapsed: {0}", ex.ToString()), TraceCategory);
            }
        }

        private void StartTiggerSvcTimer()
        {
            TriggerServiceCountdown = GeneralVar.TriggerServiceInSecond;

            if (_triTimer.Enabled)
                _triTimer.Stop();

            _triTimer.Start();
        }

        private void StopTiggerSvcTimer()
        {
            if (_triTimer.Enabled)
                _triTimer.Stop();
        }

        #endregion

        #region Function

        #region e-Wallet

        bool IsEwalletCancel = false;

        LB_ipay88.Model.PaymentType eWalletPaymentType;
        ClientResponseModel EwalletResponse = new ClientResponseModel();
        public bool IsEWalletProcess = false;

        //private void PerformEWalletPayment(string barcodeNo) 
        //{
        //    lock (CardNo)
        //    {
        //        lock (_cardNo)
        //        {
        //            string paymentData = null;
        //            string paymentReferenceNo = null;
        //            int paymentTypeId = 0;

        //            if (!IsEWalletProcess)
        //            {
        //                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment IsEwalletProcess: {0}", IsEWalletProcess), TraceCategory);
        //                IsEWalletProcess = true;

        //                try
        //                {
        //                    if (string.IsNullOrEmpty(_barcodeScanned) || _barcodeScanned != barcodeNo) _barcodeScanned = barcodeNo;
        //                    else
        //                        return;

        //                    StopTxTimer();
        //                    DetachKeyboardHook();

        //                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment BarcodeNo: {0}", barcodeNo), TraceCategory);

        //                    SetPaymentStage(ePaymentStage.ProcessTransaction);
        //                    StartTransaction();

        //                    eWalletPaymentType = LB_ipay88.Model.PaymentType.UnifiedScan;
        //                    string prodDesc = "Auto-Car Wash";
        //                    Guid obj = Guid.NewGuid();
        //                    string referenceNoEW = _referenceNo;
        //                    string email = GeneralVar.Email;
        //                    string contactNumber = GeneralVar.Careline.Replace("+", "").Replace("-", "").Replace(" ", "");
        //                    string userName = GeneralVar.ComponentCode;
        //                    string conCatRemarks = _referenceNo;

        //                    decimal total = Convert.ToDecimal(Total);
        //                    ClientRequestModel cOptional =
        //                    ClientRequestModelBuilder.Create()
        //                    .Amount(total)
        //                    .SetMerchantKey(GeneralVar.EWalletMerchantKey)
        //                    .Currency("MYR")
        //                    .MerchantCode(GeneralVar.EWalletMerchantCode)
        //                    .PaymentId(eWalletPaymentType)
        //                    .ProdDesc(prodDesc)
        //                    .RefNo(referenceNoEW)
        //                    .TerminalID(GeneralVar.ComponentCode)
        //                    .UserContact(contactNumber)
        //                    .UserEmail(email)
        //                    .UserName(userName)
        //                    .xfield1(GeneralVar.CurrentArea.AreaCode)
        //                    .xfield2(GeneralVar.ComponentCode)
        //                    .BarcodeNo(barcodeNo)
        //                    .Remark(conCatRemarks)
        //                    .Build();

        //                    ClientResponseModel c = LB_ipay88.Utility.Sender.Payment.Pay(cOptional);

        //                    string EwalletLog = string.Format(@"
        //                                                        BarcodeCallBack ****************************************
        //                                                        Status : {0}
        //                                                        ErrDesc : {1}
        //                                                        TxnId/ApprovalCode : {2}
        //                                                        RefNo : {3}
        //                                                        QRCode : {4}
        //                                                        QRValue : {5}
        //                                                        MerchantCode : {6}
        //                                                        PaymentId : {7}
        //                                                        Remarks : {8}
        //                                                        Requery : {9}
        //                                                        PaymentName : {10}
        //                                                        END - BarcodeCallBack ****************************************",
        //                    c.Status, c.ErrDesc, c.TransId, c.RefNo, c.QRCode, c.QRValue, c.MerchantCode, c.PaymentId, c.Remark, c.Requery, LB_ipay88.Model.Payment.GetPaymentType(c.PaymentId).ToString());

        //                    EwalletResponse = c;

        //                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("BarcodeCallBack:- {0}", EwalletLog), TraceCategory);

        //                    if (c.TransId != null && c.Status == 1)
        //                    {
        //                        bool canGetDetails = GetPaymentDetails_EW(out paymentTypeId, out paymentReferenceNo, out paymentData);
        //                        UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, null, "N");

        //                        SetTrxStatus(true);
        //                        PerformService();
        //                    }
        //                    else if (c.Status != 1)
        //                    {
        //                        paymentTypeId = GeneralVar.DB_PaymentTypes.Where(p => p.PaymentTypeCode == "EWALLET").First().PaymentTypeId;
        //                        UpdateTransaction(paymentTypeId, null, null, c.ErrDesc, "F");
        //                        PrintReceipt(false, true, ePaymentMethod.eWallet);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("GNR"), "E", ex.Message);
        //                    UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, ex.Message.ToString(), "F");
        //                    PrintReceipt(false, true, ePaymentMethod.eWallet);
        //                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PerformPayment: {0}", ex.ToString()), TraceCategory);
        //                }
        //                finally
        //                {
        //                    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.MainIdle);
        //                }
        //            }
        //        }
        //    }
        //}

        ACWAppAPI.ACWAppAPIResponse _AppApiResponse;
        private void PerformEWalletpaymentInThread()
        {
            IsEwalletCancel = false;
            bool loop = true;

            while (loop)
            {
                //revert
                waitBarcode.Reset();
                waitBarcode.WaitOne();

                if (IsEwalletCancel)
                    break;

                string paymentData = null;
                string paymentReferenceNo = null;
                int paymentTypeId = 0;

                if (_SelectedPaymentMethod == ePaymentMethod.eWallet)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_barcodeScanned))
                            continue;
                        StopTxTimer();
                        //DetachKeyboardHook();

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment BarcodeNo: {0}", _barcodeScanned), TraceCategory);

                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            SetPaymentStage(ePaymentStage.ProcessTransaction);
                        }));

                        StartTransaction();

                        eWalletPaymentType = LB_ipay88.Model.PaymentType.UnifiedScan;
                        string prodDesc = "Auto-Car Wash";
                        Guid obj = Guid.NewGuid();
                        string referenceNoEW = _referenceNo;
                        string email = GeneralVar.CurrentComponent.Email;
                        string contactNumber = GeneralVar.CurrentComponent.Careline.Replace("+", "").Replace("-", "").Replace(" ", "");
                        string userName = GeneralVar.ComponentCode;
                        string conCatRemarks = _referenceNo;

                        decimal total = Convert.ToDecimal(Total);
                        ClientRequestModel cOptional =
                        ClientRequestModelBuilder.Create()
                        .Amount(total)
                        .SetMerchantKey(GeneralVar.CurrentComponent.EWalletMerchantKey)
                        .Currency("MYR")
                        .MerchantCode(GeneralVar.CurrentComponent.EWalletMerchantCode)
                        .PaymentId(eWalletPaymentType)
                        .ProdDesc(prodDesc)
                        .RefNo(referenceNoEW)
                        .TerminalID(GeneralVar.ComponentCode)
                        .UserContact(contactNumber)
                        .UserEmail(email)
                        .UserName(userName)
                        .xfield1(GeneralVar.CurrentArea.AreaCode)
                        .xfield2(GeneralVar.ComponentCode)
                        .BarcodeNo(_barcodeScanned)
                        .Remark(conCatRemarks)
                        .Build();

                        ClientResponseModel c = LB_ipay88.Utility.Sender.Payment.Pay(cOptional);

                        string EwalletLog = string.Format(@"
                                                                BarcodeCallBack ****************************************
                                                                Status : {0}
                                                                ErrDesc : {1}
                                                                TxnId/ApprovalCode : {2}
                                                                RefNo : {3}
                                                                QRCode : {4}
                                                                QRValue : {5}
                                                                MerchantCode : {6}
                                                                PaymentId : {7}
                                                                Remarks : {8}
                                                                Requery : {9}
                                                                PaymentName : {10}
                                                                END - BarcodeCallBack ****************************************",
                        c.Status, c.ErrDesc, c.TransId, c.RefNo, c.QRCode, c.QRValue, c.MerchantCode, c.PaymentId, c.Remark, c.Requery, LB_ipay88.Model.Payment.GetPaymentType(c.PaymentId).ToString());

                        EwalletResponse = c;

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("BarcodeCallBack:- {0}", EwalletLog), TraceCategory);

                        if (c.TransId != null && c.Status == 1)
                        {
                            bool canGetDetails = GetPaymentDetails_EW(out paymentTypeId, out paymentReferenceNo, out paymentData);
                            UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, null, "N");

                            SetTrxStatus(true);
                            PerformService(); //have button to print receipt
                        }
                        else if (c.Status != 1)
                        {
                            paymentTypeId = GeneralVar.DB_PaymentTypes.Where(p => p.PaymentTypeCode == "EWALLET").First().PaymentTypeId;
                            UpdateTransaction(paymentTypeId, null, null, c.ErrDesc, "F");
                            PrintReceipt(false, true, ePaymentMethod.eWallet);
                        }

                    }
                    catch (Exception ex)
                    {
                        GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("GNR"), "E", ex.Message);
                        UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, ex.Message.ToString(), "F");
                        PrintReceipt(false, true, ePaymentMethod.eWallet);
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PerformPayment: {0}", ex.ToString()), TraceCategory);
                    }
                    finally
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Ewallet End Loop"), TraceCategory);
                        loop = false;

                        //if (QRReceipt != null)
                        //{
                        //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        //    {
                        //        SetPaymentStage(ePaymentStage.ReceiptQR);
                        //    }));
                        //    _WaitDone.Reset();
                        //    _WaitDone.WaitOne(20000);
                        //}
                        
                        //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        //{
                        //    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
                        //}));

                        //GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
                    }
                }
                else if (_SelectedPaymentMethod == ePaymentMethod.App)
                {
                    try
                    {
                        //revert
                        if (string.IsNullOrEmpty(_barcodeScanned))
                            continue;
                        StopTxTimer();
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformAppPayment BarcodeNo: {0}", _barcodeScanned), TraceCategory);

                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            SetPaymentStage(ePaymentStage.ProcessTransaction);
                        }));

                        StartTransaction();

                        decimal total = Convert.ToDecimal(Total);
                        int convertedTotal = (int)(total * 100);

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformAppPayment 1"), TraceCategory);

                        ACWAppAPI.ACWAppAPIResponse appApiResponse;
                        ACWAppAPI.ACWAppAPIResponseFailed appApiResponseFailed;

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformAppPayment 2"), TraceCategory);

                        if (GeneralVar.ApplicationAPI.SendAppApi(_barcodeScanned, convertedTotal, out appApiResponse, out appApiResponseFailed))
                        {
                            string LECSHINELog = string.Format(@"
                                                                LECSHINEBarcodeCallBack ****************************************
                                                                Id : {0}
                                                                UserId : {1}
                                                                WalletId : {2}
                                                                ReferenceKey : {3}
                                                                ReferenceId : {4}
                                                                Amount : {5}
                                                                Token : {6}
                                                                Type : {7}
                                                                PaymentType : {8}
                                                                Status : {9}
                                                                Remark : {10}
                                                                TransactionAt : {11}
                                                                END - LECSHINEBarcodeCallBack ****************************************",
                         string.IsNullOrEmpty(_AppApiResponse.item.Id) ? string.Empty : _AppApiResponse.item.Id,
                        string.IsNullOrEmpty(_AppApiResponse.item.UserId) ? string.Empty : _AppApiResponse.item.UserId,
                        string.IsNullOrEmpty(_AppApiResponse.item.WalletId) ? string.Empty : _AppApiResponse.item.WalletId,
                        string.IsNullOrEmpty(_AppApiResponse.item.ReferenceKey) ? string.Empty : _AppApiResponse.item.ReferenceKey,
                        string.IsNullOrEmpty(_AppApiResponse.item.ReferenceId) ? string.Empty : _AppApiResponse.item.ReferenceId,
                        string.IsNullOrEmpty(_AppApiResponse.item.Amount.ToString()) ? string.Empty : _AppApiResponse.item.Amount.ToString(),
                        string.IsNullOrEmpty(_AppApiResponse.item.Token) ? string.Empty : _AppApiResponse.item.Token,
                        string.IsNullOrEmpty(_AppApiResponse.item.Type) ? string.Empty : _AppApiResponse.item.Type,
                        string.IsNullOrEmpty(_AppApiResponse.item.PaymentType) ? string.Empty : _AppApiResponse.item.PaymentType,
                        string.IsNullOrEmpty(_AppApiResponse.item.Status) ? string.Empty : _AppApiResponse.item.Status,
                        string.IsNullOrEmpty(_AppApiResponse.item.Remark) ? string.Empty : _AppApiResponse.item.Remark,
                        string.IsNullOrEmpty(_AppApiResponse.item.TransactionAt) ? string.Empty : _AppApiResponse.item.TransactionAt);

                            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("LECSHINEBarcodeCallBack:- {0}", LECSHINELog), TraceCategory);

                            _AppApiResponse = appApiResponse;
                            bool canGetDetails = GetPaymentDetails_App(out paymentTypeId, out paymentReferenceNo, out paymentData);
                            UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, null, "N");

                            SetTrxStatus(true);
                            PerformService();                         
                        }
                        else
                        {
                            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("[Error] PerformAppPayment Lecshine APP: Code = {0}, Message = {1} ", appApiResponseFailed.Code, appApiResponseFailed.Message), TraceCategory);

                            paymentTypeId = GeneralVar.DB_PaymentTypes.Where(p => p.PaymentTypeCode == "LECSHINEAPP").First().PaymentTypeId;
                            _AppApiResponse = appApiResponse;
                            UpdateTransaction(paymentTypeId, null, null, appApiResponseFailed.Code, "F");
                            PrintReceipt(false, true, ePaymentMethod.App, appApiResponseFailed.Code);
                        }
                    }
                    catch (Exception ex)
                    {
                        GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("GNR"), "E", ex.ToString());
                        UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, ex.Message.ToString(), "F");
                        PrintReceipt(false, true, ePaymentMethod.App);
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PerformPayment: {0}", ex.ToString()), TraceCategory);
                    }
                    finally
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("App End Loop"), TraceCategory);
                        loop = false;
                    }
                }
            }
        }

        private void PerformAppPaymentInThread()
        {
            IsEwalletCancel = false;
            bool loop = true;

            while (loop)
            {
                //revert
                waitBarcode.Reset();
                waitBarcode.WaitOne();

                string paymentData = string.Empty;
                string paymentReferenceNo = string.Empty;
                int paymentTypeId = 0;

                if (IsEwalletCancel)
                    break;

                try
                {
                    if (string.IsNullOrEmpty(_barcodeScanned))
                        continue;
                    StopTxTimer();
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformAppPayment BarcodeNo: {0}", _barcodeScanned), TraceCategory);

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        SetPaymentStage(ePaymentStage.ProcessTransaction);
                    }));

                    StartTransaction();

                    //decimal total = Convert.ToDecimal(Total);
                    //int convertedTotal = (int)(total);

                    int convertedTotal = (int)(Total * 100);
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("convertedTotal = {0}", convertedTotal), TraceCategory);

                    ACWAppAPI.ACWAppAPIResponse appApiResponse;
                    ACWAppAPI.ACWAppAPIResponseFailed appApiResponseFailed;
                    if (_AppApiResponse != null)
                        _AppApiResponse = null;
                    _AppApiResponse = new ACWAppAPI.ACWAppAPIResponse();

                    AppAPI app = new AppAPI();
                    if (app.SendAppApi(_barcodeScanned, convertedTotal, out appApiResponse, out appApiResponseFailed))
                    {
                        _AppApiResponse = appApiResponse;
                        string LECSHINELog = string.Format(@"
                                                                LECSHINEBarcodeCallBack ****************************************
                                                                Id : {0}
                                                                UserId : {1}
                                                                WalletId : {2}
                                                                ReferenceKey : {3}
                                                                ReferenceId : {4}
                                                                Amount : {5}
                                                                Token : {6}
                                                                Type : {7}
                                                                PaymentType : {8}
                                                                Status : {9}
                                                                Remark : {10}
                                                                TransactionAt : {11}
                                                                END - LECSHINEBarcodeCallBack ****************************************",
                     string.IsNullOrEmpty(_AppApiResponse.item.Id) ? string.Empty : _AppApiResponse.item.Id,
                    string.IsNullOrEmpty(_AppApiResponse.item.UserId) ? string.Empty : _AppApiResponse.item.UserId,
                    string.IsNullOrEmpty(_AppApiResponse.item.WalletId) ? string.Empty : _AppApiResponse.item.WalletId,
                    string.IsNullOrEmpty(_AppApiResponse.item.ReferenceKey) ? string.Empty : _AppApiResponse.item.ReferenceKey,
                    string.IsNullOrEmpty(_AppApiResponse.item.ReferenceId) ? string.Empty : _AppApiResponse.item.ReferenceId,
                    string.IsNullOrEmpty(_AppApiResponse.item.Amount.ToString()) ? string.Empty : _AppApiResponse.item.Amount.ToString(),
                    string.IsNullOrEmpty(_AppApiResponse.item.Token) ? string.Empty : _AppApiResponse.item.Token,
                    string.IsNullOrEmpty(_AppApiResponse.item.Type) ? string.Empty : _AppApiResponse.item.Type,
                    string.IsNullOrEmpty(_AppApiResponse.item.PaymentType) ? string.Empty : _AppApiResponse.item.PaymentType,
                    string.IsNullOrEmpty(_AppApiResponse.item.Status) ? string.Empty : _AppApiResponse.item.Status,
                    string.IsNullOrEmpty(_AppApiResponse.item.Remark) ? string.Empty : _AppApiResponse.item.Remark,
                    string.IsNullOrEmpty(_AppApiResponse.item.TransactionAt) ? string.Empty : _AppApiResponse.item.TransactionAt);

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("LECSHINEBarcodeCallBack:- {0}", LECSHINELog), TraceCategory);                      
                        bool canGetDetails = GetPaymentDetails_App(out paymentTypeId, out paymentReferenceNo, out paymentData);
                        UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, null, "N");

                        SetTrxStatus(true);
                        PerformService();
                    }
                    else
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("[Error] PerformAppPayment Lecshine APP: Code = {0}, Message = {1} ", appApiResponseFailed.Code, appApiResponseFailed.Message), TraceCategory);

                        paymentTypeId = GeneralVar.DB_PaymentTypes.Where(p => p.PaymentTypeCode == "LECSHINEAPP").First().PaymentTypeId;
                        _AppApiResponse = appApiResponse;
                        UpdateTransaction(paymentTypeId, null, null, appApiResponseFailed.Code, "F");
                        PrintReceipt(false, true, ePaymentMethod.App, appApiResponseFailed.Code);
                    }
                }
                catch (Exception ex)
                {
                    GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("GNR"), "E", ex.ToString());
                    UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, ex.Message.ToString(), "F");
                    PrintReceipt(false, true, ePaymentMethod.App, "-");
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PerformPayment: {0}", ex.ToString()), TraceCategory);
                }
                finally
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("App End Loop"), TraceCategory);
                    loop = false;
                }
            }
        }

        private bool GetPaymentDetails_EW(out int paymentTypeId, out string paymentReferenceNo, out string paymentReferenceName)
        {
            try
            {
                paymentTypeId = 0;
                string PaymentCode = string.Empty;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Starting GetPaymentDetails_EW...."), TraceCategory);

                if (LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.TouchNGoeWalletScan)
                { PaymentCode = "TNGD"; }
                else if (LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.MaybankScan)
                { PaymentCode = "MAE"; }
                else if (LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.BoostWalletScan || LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.BoostWalletQR)
                { PaymentCode = "BOOST"; }
                else if (LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.GrabpayScan)
                { PaymentCode = "GRAB"; }
                else if (LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.ShopeePayQR || LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.ShopeePayQR)
                { PaymentCode = "SHOPEE"; }
                else if (LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.AliPayQR || LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.AliPayScan)
                { PaymentCode = "ALI"; }
                else if (LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.UnionpayQR || LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.UnionpayScan)
                { PaymentCode = "UNION"; }
                else if (LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.MCashQR || LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId) == LB_ipay88.Model.PaymentType.MCashScan)
                { PaymentCode = "MCASH"; }
                else
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("EwalletResponse PaymentType: {0}", EwalletResponse.PaymentId), TraceCategory);
                    PaymentCode = "EWALLET";
                }

                paymentTypeId = GeneralVar.DB_PaymentTypes.Where(p => p.PaymentTypeCode == PaymentCode).First().PaymentTypeId;

                paymentReferenceNo = null;

                paymentReferenceName = string.Format("ApprovalCode={0}|CardType={1}|CCNo={2}|CCName={3}|MerchantCode={4}|PaymentId={5}|Status={6}|RefNo={7}|QRCode={8}|QRValue={9}|Provider={10}",
                    string.IsNullOrEmpty(EwalletResponse.TransId) ? string.Empty : EwalletResponse.TransId,
                    string.IsNullOrEmpty(EwalletResponse.CardType) ? string.Empty : EwalletResponse.CardType,
                    string.IsNullOrEmpty(EwalletResponse.CCNo) ? string.Empty : EwalletResponse.CCNo,
                    string.IsNullOrEmpty(EwalletResponse.CCName) ? string.Empty : EwalletResponse.CCName,
                    string.IsNullOrEmpty(EwalletResponse.MerchantCode) ? string.Empty : EwalletResponse.MerchantCode,
                    EwalletResponse.PaymentId,
                    EwalletResponse.Status,
                    string.IsNullOrEmpty(EwalletResponse.RefNo) ? string.Empty : EwalletResponse.RefNo,
                    string.IsNullOrEmpty(EwalletResponse.QRCode) ? string.Empty : EwalletResponse.QRCode,
                    string.IsNullOrEmpty(EwalletResponse.QRValue) ? string.Empty : EwalletResponse.QRValue,
                    "Ipay88");

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] GetPaymentDetails_EW: {0}", ex.ToString()), TraceCategory);

                paymentTypeId = -1;
                paymentReferenceNo = null;
                paymentReferenceName = null;

                return false;
            }
        }

        private bool GetPaymentDetails_App(out int paymentTypeId, out string paymentReferenceNo, out string paymentReferenceName)
        {
            try
            {
                paymentTypeId = 0;
                string PaymentCode = string.Empty;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Starting GetPaymentDetails_App...."), TraceCategory);

                paymentTypeId = GeneralVar.DB_PaymentTypes.Where(p => p.PaymentTypeCode == "LECSHINEAPP").First().PaymentTypeId;
                
                paymentReferenceNo = null;

                paymentReferenceName = string.Format("id={0}|userId={1}|walletId={2}|referenceKey={3}|referenceId={4}|amount={5}|token={6}|type={7}|paymentType={8}|status={9}|remark={10}|transactionAt={11}",
                    string.IsNullOrEmpty(_AppApiResponse.item.Id) ? string.Empty : _AppApiResponse.item.Id,
                    string.IsNullOrEmpty(_AppApiResponse.item.UserId) ? string.Empty : _AppApiResponse.item.UserId,
                    string.IsNullOrEmpty(_AppApiResponse.item.WalletId) ? string.Empty : _AppApiResponse.item.WalletId,
                    string.IsNullOrEmpty(_AppApiResponse.item.ReferenceKey) ? string.Empty : _AppApiResponse.item.ReferenceKey,
                    string.IsNullOrEmpty(_AppApiResponse.item.ReferenceId) ? string.Empty : _AppApiResponse.item.ReferenceId,
                    string.IsNullOrEmpty(_AppApiResponse.item.Amount.ToString()) ? string.Empty : _AppApiResponse.item.Amount.ToString(),
                    string.IsNullOrEmpty(_AppApiResponse.item.Token) ? string.Empty : _AppApiResponse.item.Token,
                    string.IsNullOrEmpty(_AppApiResponse.item.Type) ? string.Empty : _AppApiResponse.item.Type,
                    string.IsNullOrEmpty(_AppApiResponse.item.PaymentType) ? string.Empty : _AppApiResponse.item.PaymentType,
                    string.IsNullOrEmpty(_AppApiResponse.item.Status) ? string.Empty : _AppApiResponse.item.Status,
                    string.IsNullOrEmpty(_AppApiResponse.item.Remark) ? string.Empty : _AppApiResponse.item.Remark,
                    string.IsNullOrEmpty(_AppApiResponse.item.TransactionAt) ? string.Empty : _AppApiResponse.item.TransactionAt);

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] GetPaymentDetails_EW: {0}", ex.ToString()), TraceCategory);

                paymentTypeId = -1;
                paymentReferenceNo = null;
                paymentReferenceName = null;

                return false;
            }
        }

        #endregion

        #region Credit Card

        Thread _ccStartThread;
        XFDevice.ECPIIntegration.ECPIHelper.SalesResponse lastSalesResponse = new XFDevice.ECPIIntegration.ECPIHelper.SalesResponse();
        bool IsCCCancel = false;

        private void StartCCPayment()
        {
            _ccStartThread = new Thread(new ThreadStart(StartAcceptingCard));
            _ccStartThread.Start();
        }

        private void ReadCCAction()
        {
            CurrentPayment = Modules[ePaymentStage.ProcessTransaction];
        }

        public void StartAcceptingCard()
        {
            string paymentData = null;
            string paymentReferenceNo = null;
            int paymentTypeId = 0;
            IsCCCancel = false;

            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("StartAcceptingCard Starting..."), TraceCategory);

                lastSalesResponse = new XFDevice.ECPIIntegration.ECPIHelper.SalesResponse();
                Total = Convert.ToDecimal(Total.ToString("#.00"));
                GeneralVar.CoherantHandler.FullSalesCyble(Total, out lastSalesResponse, ReadCCAction);

                if (IsCCCancel) { }
                else
                {
                    StopTxTimer();

                    string CCLog = string.Format(@"
                CreditCardCallBack ****************************************
                Status : {0}
                TID : {1}
                CardNo : {2}
                CardType : {3}
                ApprovalCode : {4}
                RRN : {5}
                HashPAN : {6}
                END - CreditCardCallBack ****************************************",
                lastSalesResponse.State.ToString(), lastSalesResponse.SaleTID, lastSalesResponse.MaskPAN, lastSalesResponse.CCSchema, lastSalesResponse.APPRCode, lastSalesResponse.RRN, lastSalesResponse.HashPAN);

                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("CreditCardCallBack:- {0}", CCLog), TraceCategory);

                    if (lastSalesResponse.State == XFDevice.ECPIIntegration.ECPIHelper.eSalesState.Approved)
                    {
                        SetPaymentStage(ePaymentStage.ProcessTransaction);
                        StartTransaction();

                        bool canGetDetails = GetPaymentDetails_CC(out paymentTypeId, out paymentReferenceNo, out paymentData);
                        UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, null, "N");

                        SetTrxStatus(true);
                        PerformService();
                    }
                    else
                    {
                        PrintReceipt(false, false, ePaymentMethod.CCards);
                        System.Threading.Thread.Sleep(3000);
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralVar.AlarmHandler.InsertAlarm(GeneralVar.AlarmHandler.GetAlarmCategoryId("EDC"), "E", ex.Message);
                PrintReceipt(false, false, ePaymentMethod.CCards);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] StartAcceptingCard: {0}", ex.ToString()), TraceCategory);
            }
            finally
            {
                //if (QRReceipt != null)
                //{
                //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                //    {
                //        SetPaymentStage(ePaymentStage.ReceiptQR);
                //    }));
                //    _WaitDone.Reset();
                //    _WaitDone.WaitOne(20000);
                //}

                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                //{
                //    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
                //}));
                //GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
            }
        }

        private bool GetPaymentDetails_CC(out int paymentTypeId, out string paymentReferenceNo, out string paymentReferenceName)
        {
            try
            {
                paymentTypeId = 0;
                string PaymentCode = "BANKCARD";

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Starting GetPaymentDetails_CC...."), TraceCategory);

                if (lastSalesResponse.CCSchema == XFDevice.ECPIIntegration.ECPIHelper.eCreditCardSChema.VISA) { PaymentCode = "VISA"; }
                else if (lastSalesResponse.CCSchema == XFDevice.ECPIIntegration.ECPIHelper.eCreditCardSChema.MASTER) { PaymentCode = "MASTER"; }
                else if (lastSalesResponse.CCSchema == XFDevice.ECPIIntegration.ECPIHelper.eCreditCardSChema.MyDebit) { PaymentCode = "PMPC"; }
                else if (lastSalesResponse.CCSchema == XFDevice.ECPIIntegration.ECPIHelper.eCreditCardSChema.AMEX) { PaymentCode = "AMEX"; }
                else { PaymentCode = "BANKCARD"; }

                paymentTypeId = GeneralVar.DB_PaymentTypes.Where(p => p.PaymentTypeCode == PaymentCode).First().PaymentTypeId;

                paymentReferenceNo = null;

                paymentReferenceName = string.Format("CardNo={0}|ApprovalCode={1}|CardType={2}|TID={3}|RRN={4}|HashPAN={5}|Status={6}",
                    string.IsNullOrEmpty(lastSalesResponse.MaskPAN) ? string.Empty : lastSalesResponse.MaskPAN,
                    string.IsNullOrEmpty(lastSalesResponse.APPRCode) ? string.Empty : lastSalesResponse.APPRCode,
                    lastSalesResponse.CCSchema.ToString(),
                    string.IsNullOrEmpty(lastSalesResponse.SaleTID) ? string.Empty : lastSalesResponse.SaleTID,
                    string.IsNullOrEmpty(lastSalesResponse.RRN) ? string.Empty : lastSalesResponse.RRN,
                    string.IsNullOrEmpty(lastSalesResponse.HashPAN) ? string.Empty : lastSalesResponse.HashPAN,
                    lastSalesResponse.State.ToString()
                    );

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] GetPaymentDetails_CC: {0}", ex.ToString()), TraceCategory);

                paymentTypeId = -1;
                paymentReferenceNo = null;
                paymentReferenceName = null;

                return false;
            }
        }

        #endregion

        private void UpdateTransaction(int paymentTypeId, string paymentReferenceNo, string paymentData, string remarks, string status)
        {
            if (_txId == 0)
                return;

            try
            {
                GeneralVar.Query.BeginTransaction();

                int componentId = GeneralVar.CurrentComponent.Id;

                int rowAffected = GeneralVar.Query.acw_TxSales_Update(componentId, _txId, remarks, status, paymentTypeId, paymentReferenceNo, paymentData);
                if (rowAffected == 0)
                {
                    GeneralVar.Query.RollBackTransaction();
                    throw new Exception("[Database error] acw_TxSales_Update failed.");
                }

                GeneralVar.Query.CommitTransaction();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] UpdateTransaction : {0}", ex.ToString()), TraceCategory);
            }
        }

        private void SetPaymentStage(ePaymentStage stage)
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("SetPaymentStage Starting...."), TraceCategory);
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("SetPaymentStage = {0}", stage), TraceCategory);
            currentPaymentStage = stage;

            if (stage == ePaymentStage.eWalletPayment)
            {
                ConstructPaymentStepIntruction(ePaymentStep.ScanEWallet);
            }
            else if (stage == ePaymentStage.CardPayment)
            {
                ConstructPaymentStepIntruction(ePaymentStep.TapCard);
            }
            else if (stage == ePaymentStage.AppPayment)
            {
                ConstructPaymentStepIntruction(ePaymentStep.App);
            }
            else if (stage == ePaymentStage.PerformService)
            {
                TextInstructionB = string.Format("Car Wash will Start in {0}s.", Convert.ToInt32(GeneralVar.TriggerServiceInSecond.TotalSeconds).ToString());
                TextInstructionC = string.Format("Cucian akan Bermula dalam {0} saat.", Convert.ToInt32(GeneralVar.TriggerServiceInSecond.TotalSeconds).ToString());
            }
            //if (stage != ePaymentStage.eWalletPayment)

            CurrentPayment = Modules[stage];

            //else
            //    CurrentPayment = new PaymentPageView();

            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("SetPaymentStage Completed."), TraceCategory);
        }

        private void StartTransaction()
        {
            try
            {
                //Insert Sales Record
                _txId = GeneralVar.LastTx["acw_TxSales"].NextId();
                _txDate = DateTime.Now;
                _referenceNo = GeneralVar.LastTx["acw_TxSales"].NextReferenceNo();

                int componentId = GeneralVar.CurrentComponent.Id;
                int areaId = GeneralVar.CurrentComponent.AreaId;
                int stateId = GeneralVar.CurrentComponent.StateId;

                int rowAffected = 0;

                rowAffected = GeneralVar.Query.acw_TxSales_Insert(
                   GeneralVar.CurrentComponent.Id, _txId, _txDate, _referenceNo, areaId, stateId,
                   TotalFare, TotalTax, 0.00m, Total, Total, null, "P");

                if (rowAffected == 0)
                {
                    GeneralVar.Query.RollBackTransaction();
                    throw new Exception("[Database error] acw_TxSales_Insert failed.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] StartTransaction: {0}", ex.ToString()), TraceCategory);
            }
        }

        private decimal GetTxPrice()
        {
            decimal price = 0.00m;

            try
            {
                if (GeneralVar.Query.IsHoliday(GeneralVar.CurrentArea.HolidayGroupId))
                {
                    int fareId = GeneralVar.CurrentArea.Tariffs.FirstOrDefault(x => x.TariffId == 8).FareId;
                    FareConfig tbc = GeneralVar.CurrentFare.FirstOrDefault(y => y.FareId == fareId).FareConfigs.Where(x => DateTime.Now.TimeOfDay >= x.StartTime.TimeOfDay).OrderByDescending(y => y.StartTime).FirstOrDefault();
                    price = tbc.Amount;

                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GetTxPrice: AreaId = {0}, AreaName = {1}, TariffId = {2}", GeneralVar.CurrentArea.AreaId, GeneralVar.CurrentArea.AreaName, 8), TraceCategory);
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GetTxPrice: FareId = {0}, FareTime = {1}, FareAmount = {2}", fareId, tbc.StartTime.ToString("HH:mm:ss"), tbc.Amount.ToString("#.00")), TraceCategory);
                }
                else
                {
                    int Day = 1;

                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday) { Day = 7; }
                    else if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday) { Day = 6; }
                    else if (DateTime.Now.DayOfWeek == DayOfWeek.Friday) { Day = 5; }
                    else if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday) { Day = 4; }
                    else if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday) { Day = 3; }
                    else if (DateTime.Now.DayOfWeek == DayOfWeek.Tuesday) { Day = 2; }
                    else { Day = 1; }

                    int fareId = GeneralVar.CurrentArea.Tariffs.FirstOrDefault(x => x.TariffId == Day).FareId;
                    FareConfig tbc = GeneralVar.CurrentFare.FirstOrDefault(y => y.FareId == fareId).FareConfigs.Where(x => DateTime.Now.TimeOfDay >= x.StartTime.TimeOfDay).OrderByDescending(y => y.StartTime).FirstOrDefault();
                    price = tbc.Amount;

                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GetTxPrice: AreaId = {0}, AreaName = {1}, TariffId = {2}", GeneralVar.CurrentArea.AreaId, GeneralVar.CurrentArea.AreaName, Day), TraceCategory);
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GetTxPrice: FareId = {0}, FareTime = {1}, FareAmount = {2}", fareId, tbc.StartTime.ToString("HH:mm:ss"), tbc.Amount.ToString("#.00")), TraceCategory);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetTxPrice: {0}", ex.ToString()), TraceCategory);
            }

            return price;
        }

        private void PerformService()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Perform Service Starting..."), TraceCategory);

                //SetPaymentStage(ePaymentStage.PerformService);

                //IsCountdownTriDone = false;
                //StartTiggerSvcTimer();

                //while (!IsCountdownTriDone)
                //{
                //    System.Threading.Thread.Sleep(1000);
                //}

                SetPaymentStage(ePaymentStage.PerformService);  // gt button
                TriggerMachine();
                _WaitPrint.Reset();
                _WaitPrint.WaitOne(20000);
                if (QRReceipt != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        SetPaymentStage(ePaymentStage.ReceiptQR);
                    }));
                    _WaitDone.Reset();
                    _WaitDone.WaitOne(20000);
                }

                int counter = 0;
                if (GeneralVar.IOBoard_Enabled)
                {
                    if (GeneralVar.IOBoard_SensorControl_Enabled)
                    {
                        if (GeneralVar.ACWMachineModel == eCarWashMachine.HEFEI)
                        {
                            while (!GeneralVar.IOBoardCtrl.ACWActiveStatus && counter <= GeneralVar.StartPendingInSecond)
                            {
                                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("TriggerMachine ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                                //if (currentPaymentStage != ePaymentStage.PerformService && currentPaymentStage != ePaymentStage.ReceiptQR)
                                //    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.InService);
                                System.Threading.Thread.Sleep(1000);
                                counter++;
                            }
                            if (GeneralVar.IOBoardCtrl.ACWActiveStatus)
                                GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.InService);
                            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("TriggerMachine ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                        }
                        else if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                        {
                            while (GeneralVar.IOBoardCtrl.ACWSensorObjectStatus && counter <= GeneralVar.StartPendingInSecond)
                            {
                                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("TriggerMachine ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                                //if (currentPaymentStage != ePaymentStage.PerformService && currentPaymentStage != ePaymentStage.ReceiptQR)
                                //    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.InService);
                                System.Threading.Thread.Sleep(1000);
                                counter++;
                            }
                            if (!GeneralVar.IOBoardCtrl.ACWSensorObjectStatus)
                                GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.InService);
                            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("TriggerMachine ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                        }
                        else if (GeneralVar.ACWMachineModel == eCarWashMachine.PENTAMASTER)
                        {
                            while (!GeneralVar.IOBoardCtrl.PMWashing && counter <= GeneralVar.StartPendingInSecond)
                            {
                                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : Error = {0}, Washing = {1}", GeneralVar.IOBoardCtrl.PMErrorWash, GeneralVar.IOBoardCtrl.PMWashing), TraceCategory);
                                System.Threading.Thread.Sleep(1000);
                                counter++;
                            }
                            if (GeneralVar.IOBoardCtrl.PMWashing)
                                GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.InService);
                            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : Error = {0}, Washing = {1}", GeneralVar.IOBoardCtrl.PMErrorWash, GeneralVar.IOBoardCtrl.PMWashing), TraceCategory);
                        }
                    }
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
                }));

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Countdown Trigger Done."), TraceCategory);
                //StopTiggerSvcTimer();
                //int counter = 0;

                //if (GeneralVar.IOBoard_Enabled)
                //{
                //    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Start Trigger Car Wash..."), TraceCategory);
                //    GeneralVar.IOBoardCtrl.EntryPassCounter++;

                //    if (GeneralVar.IOBoard_SensorControl_Enabled)
                //    {
                //        if (GeneralVar.ACWMachineModel == eCarWashMachine.HEFEI)
                //        {
                //            while (!GeneralVar.IOBoardCtrl.ACWActiveStatus && counter <= GeneralVar.StartPendingInSecond)
                //            {
                //                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                //                System.Threading.Thread.Sleep(1000);
                //                counter++;
                //            }
                //            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                //        }
                //        else if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                //        {
                //            while (GeneralVar.IOBoardCtrl.ACWSensorObjectStatus && counter <= GeneralVar.StartPendingInSecond)
                //            {
                //                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                //                System.Threading.Thread.Sleep(1000);
                //                counter++;
                //            }
                //            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                //        }
                //    }
                //}

                //Print();
                

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Perform Service Completed..."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PerformService: {0}", ex.ToString()), TraceCategory);
            }
        }

        private void TriggerMachine()
        {
            try
            {
                int counter = 0;
                Thread th = new Thread(() =>
                {
                    try
                    {
                        if (GeneralVar.IOBoard_Enabled)
                        {
                            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Start Trigger Car Wash..."), TraceCategory);
                            GeneralVar.IOBoardCtrl.EntryPassCounter++;

                            //if (GeneralVar.IOBoard_SensorControl_Enabled)
                            //{
                            //    if (GeneralVar.ACWMachineModel == eCarWashMachine.HEFEI)
                            //    {
                            //        while (!GeneralVar.IOBoardCtrl.ACWActiveStatus && counter <= GeneralVar.StartPendingInSecond)
                            //        {
                            //            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("TriggerMachine ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                            //            //if (currentPaymentStage != ePaymentStage.PerformService && currentPaymentStage != ePaymentStage.ReceiptQR)
                            //            //    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.InService);
                            //            System.Threading.Thread.Sleep(1000);
                            //            counter++;
                            //        }
                            //        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("TriggerMachine ACW Status : NormalStatus = {0}, TermStatus = {1}, ActiveStatus = {2}, ErrorWashStatus = {3}", GeneralVar.IOBoardCtrl.ACWNormalStatus, GeneralVar.IOBoardCtrl.ACWTermStatus, GeneralVar.IOBoardCtrl.ACWActiveStatus, GeneralVar.IOBoardCtrl.ACWErrorWashStatus), TraceCategory);
                            //    }
                            //    else if (GeneralVar.ACWMachineModel == eCarWashMachine.QINGDAO)
                            //    {
                            //        while (GeneralVar.IOBoardCtrl.ACWSensorObjectStatus && counter <= GeneralVar.StartPendingInSecond)
                            //        {
                            //            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("TriggerMachine ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                            //            //if (currentPaymentStage != ePaymentStage.PerformService && currentPaymentStage != ePaymentStage.ReceiptQR)
                            //            //    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.InService);
                            //            System.Threading.Thread.Sleep(1000);
                            //            counter++;
                            //        }
                            //        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("TriggerMachine ACW Status : MaintenanceErrorStatus = {0}, SensorObjectStatus = {1}", GeneralVar.IOBoardCtrl.ACWErrorOperationStatus, GeneralVar.IOBoardCtrl.ACWSensorObjectStatus), TraceCategory);
                            //    }
                            //}
                        }
                    }
                    catch(Exception ex)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] Thread TriggerMachine: {0}", ex.ToString()), TraceCategory);

                    }
                });
                th.Start();
            }
            catch(Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] TriggerMachine: {0}", ex.ToString()), TraceCategory);
            }
        }

        private void PrintReceipt(bool isSuccess, bool isReceiptRequired, ePaymentMethod paymentMethod, string errorCode = null)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PrintReceipt Starting..."), TraceCategory);

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    if (isSuccess) { SetPaymentStage(ePaymentStage.Success); }
                    else { SetPaymentStage(ePaymentStage.Failed); }

                }));

                if (isReceiptRequired)
                {
                    if (GeneralVar.ReceiptPrinter_Enabled)
                    {
                        int printedLength = 0;

                        if (GeneralVar.IOBoard_Enabled)
                            GeneralVar.IOBoardCtrl.ReceiptPrinterOn = true;

                        if (paymentMethod == ePaymentMethod.eWallet)
                        {
                            bool isprint = false;

                            isprint = GeneralVar.RPTHandler.Print_SalesByEWallet(isSuccess, DateTime.Now, _referenceNo, TotalFare, GeneralVar.CurrentTaxRate.Value, TotalTax, 0.00m, Total, LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId).ToString(), EwalletResponse.MerchantCode, GeneralVar.ComponentCode, EwalletResponse.RefNo, EwalletResponse.TransId, EwalletResponse.PaymentId.ToString(), isSuccess ? "Success" : "Failed", ref printedLength);
                        }
                        else if (paymentMethod == ePaymentMethod.CCards)
                        {
                            bool isprint = false;

                            isprint = GeneralVar.RPTHandler.Print_SalesByCard(isSuccess, DateTime.Now, _referenceNo, TotalFare, GeneralVar.CurrentTaxRate.Value, TotalTax, 0.00m, Total, lastSalesResponse.MaskPAN, lastSalesResponse.CCSchema.ToString(), lastSalesResponse.SaleTID, lastSalesResponse.RRN, lastSalesResponse.APPRCode, lastSalesResponse.HashPAN, lastSalesResponse.State.ToString(), ref printedLength);
                        }
                        else if (paymentMethod == ePaymentMethod.App)
                        {
                            bool isprint = false;

                            isprint = GeneralVar.RPTHandler.Print_SalesByApp(isSuccess, DateTime.Now, _referenceNo, TotalFare, GeneralVar.CurrentTaxRate.Value, TotalTax, 0.00m, Total, errorCode, isSuccess? _AppApiResponse.item.Id : "-",
                                isSuccess ? _AppApiResponse.item.UserId : "-", isSuccess ? _AppApiResponse.item.WalletId : "-", isSuccess ? _AppApiResponse.item.ReferenceKey : "-", ref printedLength);
                        }

                        if (GeneralVar.IOBoard_Enabled)
                            GeneralVar.IOBoardCtrl.ReceiptPrinterOn = false;
                    }
                }

                PrepareQRReceipt(isSuccess, isReceiptRequired, paymentMethod, errorCode);

                string err = "";
                string jsonRequest = "";
                string jsonResponse = "";
                QRReceiptResponse qRReceiptResponse = null;

                if (GeneralVar.ACWAPICtrl.GenerateQRReceipt("http://vps3.longbow.com.my/TJAPI/api/SSK/DoCreateReceiptUrl", qrReceiptRequest, 30000, out qRReceiptResponse, out jsonRequest, out jsonResponse, out err) && qRReceiptResponse != null)
                {
                    if (QRReceipt != null)
                        QRReceipt = null;
                    QRReceipt = new BitmapImage();
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qRReceiptResponse.ResponseURL.ToString().Trim(), QRCodeGenerator.ECCLevel.Q);
                    QRCode qrCode = new QRCode(qrCodeData);
                    QRReceipt = ToBitmapImage(qrCode.GetGraphic(20));
                }
                else
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PrintReceipt Failed...."), TraceCategory);
                }

                if (QRReceipt != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        SetPaymentStage(ePaymentStage.ReceiptQR);
                    }));
                    _WaitDone.Reset();
                    _WaitDone.WaitOne(20000);
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
                }));
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PrintReceipt Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PrintReceipt: {0}", ex.ToString()), TraceCategory);
            }
        }

        public BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            var bitmapImage = new BitmapImage();
            try
            {
                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;                   
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();               
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] ToBitmapImage: {0}", ex.ToString()), TraceCategory);
            }
            return bitmapImage;
        }

        public void Done()
        {
            try 
            {
                _WaitDone.Set();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] Done: {0}", ex.ToString()), TraceCategory);

            }
        }

        QRReceiptRequest qrReceiptRequest = new QRReceiptRequest();
        private void PrepareQRReceipt(bool isSuccess, bool isReceiptRequired, ePaymentMethod paymentMethod, string errorCode = null)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PrepareQRReceipt Starting..."), TraceCategory);

                if (qrReceiptRequest != null)
                    qrReceiptRequest = null;
                qrReceiptRequest = new QRReceiptRequest();

                qrReceiptRequest.PaymentType = new List<ACWSSK.Model.QRReceiptModel.PaymentType>();

                QRReceiptModel.PaymentType type = new QRReceiptModel.PaymentType();

                qrReceiptRequest.ByApp = new List<ByApp>();
                qrReceiptRequest.ByAppFailed = new List<ByAppFailed>();
                qrReceiptRequest.ByEWallet = new List<ByEWallet>();
                qrReceiptRequest.ByEWalletFailed = new List<ByEWalletFailed>();
                qrReceiptRequest.ByCard = new List<ByCard>();
                qrReceiptRequest.ByCardFailed = new List<ByCardFailed>();

                string companyName = GeneralVar.CurrentComponent.CompanyName;
                string sSMNo = string.IsNullOrEmpty(GeneralVar.CurrentComponent.SSMNo) ? " " : GeneralVar.CurrentComponent.SSMNo;
                string address1 = string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address1) ? " " : GeneralVar.CurrentComponent.Address1;
                string address2 = string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address2) ? " " : GeneralVar.CurrentComponent.Address2;
                string address3 = string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address3) ? " " : GeneralVar.CurrentComponent.Address3;
                string address4 = string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4) ? (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo) ? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo)) : GeneralVar.CurrentComponent.Address4;
                string gSTNo = string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4) ? " " : (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo) ? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo));
                string refNo = _referenceNo;
                string txDate = DateTime.Now.ToString("dd MMM yyyy hh:mm tt");
                string componentCode = GeneralVar.CurrentComponent.ComponentCode;
                string amt = TotalFare.ToString("#.00");
                string sT = GeneralVar.CurrentTaxRate.Value.ToString("0.00") + "%";
                string tax = TotalTax.ToString("0.00");
                string ta = Total.ToString("#.00");
                string careline = string.IsNullOrEmpty(GeneralVar.CurrentComponent.Careline) ? " " : GeneralVar.CurrentComponent.Careline;
                string email = string.IsNullOrEmpty(GeneralVar.CurrentComponent.Email) ? " " : GeneralVar.CurrentComponent.Email;

                if (paymentMethod == ePaymentMethod.eWallet)
                {
                    
                    string eWPaymentType = string.IsNullOrEmpty(LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId).ToString()) ? "-" : LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId).ToString();
                    string eWMerchantCode = string.IsNullOrEmpty(EwalletResponse.MerchantCode) ? "-" : EwalletResponse.MerchantCode;
                    string eWTerminalId = GeneralVar.ComponentCode;
                    string eWRefNo = string.IsNullOrEmpty(EwalletResponse.RefNo) ? "-" : EwalletResponse.RefNo;
                    string eWApprCode = string.IsNullOrEmpty(EwalletResponse.TransId) ? "-" : EwalletResponse.TransId;
                    string eWPaymentId = string.IsNullOrEmpty(EwalletResponse.PaymentId.ToString()) ? "-" : EwalletResponse.PaymentId.ToString();
                    string eWRespCode = isSuccess ? "Success" : "Failed";

                    if (isSuccess)
                    {
                        type.ByEWallet = true;
                        ByEWallet wallet = new ByEWallet()
                        {
                            CompanyName = companyName,
                            SSMNo = sSMNo,
                            Address1 = address1,
                            Address2 = address2,
                            Address3 = address3,
                            Address4 = address4,
                            GSTNo = gSTNo,
                            RefNo = refNo,
                            TxDate = txDate,
                            ComponentCode = componentCode,
                            Amt = amt,
                            ST = sT,
                            Tax = tax,
                            TA = ta,
                            Careline = careline,
                            Email = email,
                            EWPaymentType = eWPaymentType,
                            EWMerchantCode = eWMerchantCode,
                            EWTerminalId = eWTerminalId,
                            EWRefNo = eWRefNo,
                            EWApprCode = eWApprCode,
                            EWPaymentId = eWPaymentId,
                            EWRespCode = eWRespCode
                        };
                        qrReceiptRequest.ByEWallet.Add(wallet);
                    }
                    else
                    {
                        type.ByEWalletFailed = true;
                        ByEWalletFailed wallet = new ByEWalletFailed()
                        {
                            CompanyName = companyName,
                            SSMNo = sSMNo,
                            Address1 = address1,
                            Address2 = address2,
                            Address3 = address3,
                            Address4 = address4,
                            GSTNo = gSTNo,
                            RefNo = refNo,
                            TxDate = txDate,
                            ComponentCode = componentCode,
                            Amt = amt,
                            ST = sT,
                            Tax = tax,
                            TA = ta,
                            Careline = careline,
                            Email = email,
                            EWPaymentType = eWPaymentType,
                            EWMerchantCode = eWMerchantCode,
                            EWTerminalId = eWTerminalId,
                            EWRefNo = eWRefNo,
                            EWApprCode = eWApprCode,
                            EWPaymentId = eWPaymentId,
                            EWRespCode = eWRespCode
                        };
                        qrReceiptRequest.ByEWalletFailed.Add(wallet);
                    }
                }
                else if (paymentMethod == ePaymentMethod.CCards)
                {
                    string cCCardType = string.IsNullOrEmpty(lastSalesResponse.CCSchema.ToString()) ? "-" : lastSalesResponse.CCSchema.ToString();
                    string cCCardNo = string.IsNullOrEmpty(lastSalesResponse.MaskPAN) ? "-" : lastSalesResponse.MaskPAN;
                    string cCTerminalId = string.IsNullOrEmpty(lastSalesResponse.SaleTID) ? "-" : lastSalesResponse.SaleTID;
                    string cCRefNo = string.IsNullOrEmpty(lastSalesResponse.RRN) ? "-" : lastSalesResponse.RRN;
                    string cCApprCode = string.IsNullOrEmpty(lastSalesResponse.APPRCode) ? "-" : lastSalesResponse.APPRCode;
                    string cCHashPAN = string.IsNullOrEmpty(lastSalesResponse.HashPAN) ? "-" : lastSalesResponse.HashPAN;
                    string cCRespCode = string.IsNullOrEmpty(lastSalesResponse.State.ToString()) ? "-" : lastSalesResponse.State.ToString();
                    
                    if (isSuccess)
                    {
                        type.ByCard = true;
                        ByCard card = new ByCard()
                        {
                            CompanyName = companyName,
                            SSMNo = sSMNo,
                            Address1 = address1,
                            Address2 = address2,
                            Address3 = address3,
                            Address4 = address4,
                            GSTNo = gSTNo,
                            RefNo = refNo,
                            TxDate = txDate,
                            ComponentCode = componentCode,
                            Amt = amt,
                            ST = sT,
                            Tax = tax,
                            TA = ta,
                            CCCardType = cCCardType,
                            CCCardNo = cCCardNo,
                            CCTerminalId = cCTerminalId,
                            CCRefNo = cCRefNo,
                            CCApprCode = cCApprCode,
                            CCHashPAN = cCHashPAN,
                            CCRespCode = cCRespCode,
                            Careline = careline,
                            Email = email
                        };
                        qrReceiptRequest.ByCard.Add(card);
                    }
                    else
                    {
                        type.ByCardFailed = true;
                        ByCardFailed card = new ByCardFailed()
                        {
                            CompanyName = companyName,
                            SSMNo = sSMNo,
                            Address1 = address1,
                            Address2 = address2,
                            Address3 = address3,
                            Address4 = address4,
                            GSTNo = gSTNo,
                            RefNo = refNo,
                            TxDate = txDate,
                            ComponentCode = componentCode,
                            Amt = amt,
                            ST = sT,
                            Tax = tax,
                            TA = ta,
                            CCCardType = cCCardType,
                            CCCardNo = cCCardNo,
                            CCTerminalId = cCTerminalId,
                            CCRefNo = cCRefNo,
                            CCApprCode = cCApprCode,
                            CCHashPAN = cCHashPAN,
                            CCRespCode = cCRespCode,
                            Careline = careline,
                            Email = email
                        };
                        qrReceiptRequest.ByCardFailed.Add(card);
                    }

                }
                else if (paymentMethod == ePaymentMethod.App)
                {
                    string code = errorCode;
                    string id = "-";
                    string userId = "-";
                    string walletId = "-";
                    string referenceKey = "-";

                    if (_AppApiResponse != null && _AppApiResponse.item != null)
                    {
                        id = _AppApiResponse.item.Id;
                        userId = _AppApiResponse.item.UserId;
                        walletId = _AppApiResponse.item.WalletId;
                        referenceKey = _AppApiResponse.item.ReferenceKey;
                    }                

                    if (isSuccess)
                    {
                        type.ByApp = true;
                        ByApp app = new ByApp() 
                        {
                            CompanyName = companyName,
                            SSMNo = sSMNo,
                            Address1 = address1,
                            Address2 = address2,
                            Address3 = address3,
                            Address4 = address4,
                            GSTNo = gSTNo,
                            RefNo = refNo,
                            TxDate = txDate,
                            ComponentCode = componentCode,
                            Amt = amt,
                            ST = sT,
                            Tax = tax,
                            TA = ta,
                            Id = id,
                            UserId = userId,
                            WalletId = walletId,
                            ReferenceKey = referenceKey
                        };
                        qrReceiptRequest.ByApp.Add(app);
                    }
                    else
                    {
                        type.ByAppFailed = true;
                        ByAppFailed app = new ByAppFailed()
                        {
                            CompanyName = companyName,
                            SSMNo = sSMNo,
                            Address1 = address1,
                            Address2 = address2,
                            Address3 = address3,
                            Address4 = address4,
                            GSTNo = gSTNo,
                            RefNo = refNo,
                            TxDate = txDate,
                            ComponentCode = componentCode,
                            Amt = amt,
                            ST = sT,
                            Tax = tax,
                            TA = ta,
                            Code = code
                        };
                        qrReceiptRequest.ByAppFailed.Add(app);
                    }
                }
                qrReceiptRequest.PaymentType.Add(type);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PrepareQRReceipt Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PrepareQRReceipt: {0}", ex.ToString()), TraceCategory);
            }
        }
        private void PrintSuccessReceipt(ePaymentMethod paymentMethod)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PrintSuccessReceipt Starting..."), TraceCategory);

                if (GeneralVar.ReceiptPrinter_Enabled)
                {
                    int printedLength = 0;

                    if (GeneralVar.IOBoard_Enabled)
                        GeneralVar.IOBoardCtrl.ReceiptPrinterOn = true;

                    if (paymentMethod == ePaymentMethod.eWallet)
                    {
                        bool isprint = false;

                        isprint = GeneralVar.RPTHandler.Print_SalesByEWallet(true, _TrxDTSuccess, _referenceNo, TotalFare, GeneralVar.CurrentTaxRate.Value, TotalTax, 0.00m, Total, LB_ipay88.Model.Payment.GetPaymentType(EwalletResponse.PaymentId).ToString(), EwalletResponse.MerchantCode, GeneralVar.ComponentCode, EwalletResponse.RefNo, EwalletResponse.TransId, EwalletResponse.PaymentId.ToString(), "Success", ref printedLength);
                    }
                    else if (paymentMethod == ePaymentMethod.App)
                    {
                        bool isprint = false;

                        isprint = GeneralVar.RPTHandler.Print_SalesByApp(true, _TrxDTSuccess, _referenceNo, TotalFare, GeneralVar.CurrentTaxRate.Value, TotalTax, 0.00m, Total, "", _AppApiResponse.item.Id, _AppApiResponse.item.UserId, _AppApiResponse.item.WalletId, _AppApiResponse.item.ReferenceKey, ref printedLength);

                    }
                    else
                    {
                        bool isprint = false;

                        isprint = GeneralVar.RPTHandler.Print_SalesByCard(true, _TrxDTSuccess, _referenceNo, TotalFare, GeneralVar.CurrentTaxRate.Value, TotalTax, 0.00m, Total, lastSalesResponse.MaskPAN, lastSalesResponse.CCSchema.ToString(), lastSalesResponse.SaleTID, lastSalesResponse.RRN, lastSalesResponse.APPRCode, lastSalesResponse.HashPAN, lastSalesResponse.State.ToString(), ref printedLength);
                    }

                    if (GeneralVar.IOBoard_Enabled)
                        GeneralVar.IOBoardCtrl.ReceiptPrinterOn = false;
                }
                //else
                //{
                //    System.Threading.Thread.Sleep(5000);
                //}

                PrepareQRReceipt(true, true, paymentMethod, "");

                string err = "";
                string jsonRequest = "";
                string jsonResponse = "";
                QRReceiptResponse qRReceiptResponse = null;
                if (QRReceipt != null)
                    QRReceipt = null;

                if (GeneralVar.ACWAPICtrl.GenerateQRReceipt("http://vps3.longbow.com.my/TJAPI/api/SSK/DoCreateReceiptUrl", qrReceiptRequest, 30000, out qRReceiptResponse, out jsonRequest, out jsonResponse, out err) && qRReceiptResponse != null)
                {
                    QRReceipt = new BitmapImage();
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qRReceiptResponse.ResponseURL.ToString().Trim(), QRCodeGenerator.ECCLevel.H);
                    QRCode qrCode = new QRCode(qrCodeData);
                    QRReceipt = ToBitmapImage(qrCode.GetGraphic(20));
                }
                else
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GenerateQRReceipt Failed...."), TraceCategory);
                }

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PrintSuccessReceipt Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PrintSuccessReceipt: {0}", ex.ToString()), TraceCategory);
            }
        }

        DateTime _TrxDTSuccess;
        private void SetTrxStatus(bool isSuccess)
        {
            try
            {
                if (isSuccess) { SetPaymentStage(ePaymentStage.Success); }
                else { SetPaymentStage(ePaymentStage.Failed); }

                _TrxDTSuccess = DateTime.Now;

                System.Threading.Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] SetTrxStatus: {0}", ex.ToString()), TraceCategory);
            }
        }

        #endregion

        private void DemoPaymentSelected(string param)
        {
            StopTxTimer();
            int obj = Convert.ToInt32(param);
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment param: {0}", param), TraceCategory);
            BackgroundWorker Worker = new BackgroundWorker();
            Worker.DoWork += delegate (object s, DoWorkEventArgs e)
            {
                if (_SelectedPaymentMethod == ePaymentMethod.CCards) { if (GeneralVar.CreditCardTerminal_Enabled) GeneralVar.CoherantHandler.CancelSales(); }
                //else if (_SelectedPaymentMethod == ePaymentMethod.CCards) { //DetachKeyboardHook();
                //                                                            }

                if (obj == 1)
                {
                    if (_SelectedPaymentMethod == ePaymentMethod.eWallet)
                    {
                        #region EWallet

                        string barcodeNo = "";
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment BarcodeNo: {0}", barcodeNo), TraceCategory);

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment Merchant Code: {0}", GeneralVar.CurrentComponent.EWalletMerchantCode), TraceCategory);
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment Merchant Key: {0}", GeneralVar.CurrentComponent.EWalletMerchantKey), TraceCategory);

                        SetPaymentStage(ePaymentStage.ProcessTransaction);
                        StartTransaction();
                        System.Threading.Thread.Sleep(3000);

                        eWalletPaymentType = LB_ipay88.Model.PaymentType.UnifiedScan;
                        ClientResponseModel c = new ClientResponseModel();

                        c.Status = 1;
                        c.ErrDesc = string.Empty;
                        c.TransId = "T003259064120";
                        c.RefNo = "S-001-220000011";
                        c.QRCode = "";
                        c.QRValue = string.Empty;
                        c.MerchantCode = "M17485_S0009";
                        c.PaymentId = 354;
                        c.Remark = "Car Wash RM16 Petronas Car Wash";

                        string EwalletLog = string.Format(@"
                BarcodeCallBack ****************************************
                Status : {0}
                ErrDesc : {1}
                TxnId/ApprovalCode : {2}
                RefNo : {3}
                QRCode : {4}
                QRValue : {5}
                MerchantCode : {6}
                PaymentId : {7}
                Remarks : {8}
                Requery : {9}
                PaymentName : {10}
                END - BarcodeCallBack ****************************************",
                     c.Status, c.ErrDesc, c.TransId, c.RefNo, c.QRCode, c.QRValue, c.MerchantCode, c.PaymentId, c.Remark, c.Requery, LB_ipay88.Model.Payment.GetPaymentType(c.PaymentId).ToString());

                        EwalletResponse = c;

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("BarcodeCallBack:- {0}", EwalletLog), TraceCategory);
                        string paymentIdDescription = EwalletResponse.PaymentId.ToString();
                        if (c.TransId != null && c.Status == 1)
                        {
                            string paymentData = null;
                            string paymentReferenceNo = null;
                            int paymentTypeId = 0;

                            bool canGetDetails = GetPaymentDetails_EW(out paymentTypeId, out paymentReferenceNo, out paymentData);
                            UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, null, "N");

                            SetTrxStatus(true);
                            PerformService();
                        }
                        else if (c.Status != 1)
                        {
                            UpdateTransaction(0, null, null, c.ErrDesc, "F");
                            PrintReceipt(false, true, ePaymentMethod.eWallet);
                        }

                        #endregion
                    }
                    else
                    {
                        #region Credit Cards

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformCCTerminalPayment..."), TraceCategory);

                        lastSalesResponse = new XFDevice.ECPIIntegration.ECPIHelper.SalesResponse();
                        lastSalesResponse.MaskPAN = "552115******5250";
                        lastSalesResponse.CCSchema = XFDevice.ECPIIntegration.ECPIHelper.eCreditCardSChema.VISA;
                        lastSalesResponse.APPRCode = "204201";
                        lastSalesResponse.SaleTID = "20000001";
                        lastSalesResponse.RRN = "671156230774";
                        lastSalesResponse.HashPAN = "AD04100AAED7107555D6722C929B8C3C52E4984E";
                        lastSalesResponse.State = XFDevice.ECPIIntegration.ECPIHelper.eSalesState.Approved;

                        string paymentData = null;
                        string paymentReferenceNo = null;
                        int paymentTypeId = 0;

                        string CCLog = string.Format(@"
                CreditCardCallBack ****************************************
                Status : {0}
                TID : {1}
                CardNo : {2}
                CardType : {3}
                ApprovalCode : {4}
                RRN : {5}
                HashPAN : {6}
                END - CreditCardCallBack ****************************************",
                 lastSalesResponse.State.ToString(), lastSalesResponse.SaleTID, lastSalesResponse.MaskPAN, lastSalesResponse.CCSchema, lastSalesResponse.APPRCode, lastSalesResponse.RRN, lastSalesResponse.HashPAN);


                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("CreditCardCallBack:- {0}", CCLog), TraceCategory);

                        if (lastSalesResponse.State == XFDevice.ECPIIntegration.ECPIHelper.eSalesState.Approved)
                        {
                            SetPaymentStage(ePaymentStage.ProcessTransaction);
                            StartTransaction();
                            System.Threading.Thread.Sleep(3000);
                            bool canGetDetails = GetPaymentDetails_CC(out paymentTypeId, out paymentReferenceNo, out paymentData);
                            UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, null, "N");

                            SetTrxStatus(true);
                            PerformService();
                        }
                        else
                        {
                            PrintReceipt(false, false, ePaymentMethod.CCards);
                            System.Threading.Thread.Sleep(3000);
                        }

                        #endregion
                    }
                }
                else if (obj == 0)
                {
                    if (_SelectedPaymentMethod == ePaymentMethod.eWallet)
                    {
                        #region EWallet

                        string barcodeNo = "";
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment BarcodeNo: {0}", barcodeNo), TraceCategory);

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment Merchant Code: {0}", GeneralVar.CurrentComponent.EWalletMerchantCode), TraceCategory);
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformEWalletPayment Merchant Key: {0}", GeneralVar.CurrentComponent.EWalletMerchantKey), TraceCategory);

                        SetPaymentStage(ePaymentStage.ProcessTransaction);
                        StartTransaction();
                        System.Threading.Thread.Sleep(3000);

                        eWalletPaymentType = LB_ipay88.Model.PaymentType.UnifiedScan;
                        ClientResponseModel c = new ClientResponseModel();

                        c.Status = 0;
                        c.ErrDesc = string.Empty;
                        c.TransId = "T003259064120";
                        c.RefNo = "";
                        c.QRCode = "";
                        c.QRValue = string.Empty;
                        c.MerchantCode = "M17485_S0009";
                        c.PaymentId = 0;
                        c.Remark = "Car Wash RM16 Petronas Car Wash";

                        string EwalletLog = string.Format(@"
                BarcodeCallBack ****************************************
                Status : {0}
                ErrDesc : {1}
                TxnId/ApprovalCode : {2}
                RefNo : {3}
                QRCode : {4}
                QRValue : {5}
                MerchantCode : {6}
                PaymentId : {7}
                Remarks : {8}
                Requery : {9}
                PaymentName : {10}
                END - BarcodeCallBack ****************************************",
                     c.Status, c.ErrDesc, c.TransId, c.RefNo, c.QRCode, c.QRValue, c.MerchantCode, c.PaymentId, c.Remark, c.Requery, LB_ipay88.Model.Payment.GetPaymentType(c.PaymentId).ToString());

                        EwalletResponse = c;

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("BarcodeCallBack:- {0}", EwalletLog), TraceCategory);
                        string paymentIdDescription = EwalletResponse.PaymentId.ToString();
                        if (c.TransId != null && c.Status == 1)
                        {
                            string paymentData = null;
                            string paymentReferenceNo = null;
                            int paymentTypeId = 0;

                            bool canGetDetails = GetPaymentDetails_EW(out paymentTypeId, out paymentReferenceNo, out paymentData);
                            UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, null, "N");

                            SetTrxStatus(true);
                            PerformService();
                        }
                        else if (c.Status != 1)
                        {
                            UpdateTransaction(0, null, null, c.ErrDesc, "F");
                            PrintReceipt(false, true, ePaymentMethod.eWallet);
                        }

                        #endregion
                    }
                    else
                    {
                        #region Credit Cards

                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PerformCCTerminalPayment..."), TraceCategory);

                        lastSalesResponse = new XFDevice.ECPIIntegration.ECPIHelper.SalesResponse();
                        lastSalesResponse.MaskPAN = "552115******5255";
                        lastSalesResponse.CCSchema = XFDevice.ECPIIntegration.ECPIHelper.eCreditCardSChema.AMEX;
                        lastSalesResponse.APPRCode = "204201";
                        lastSalesResponse.SaleTID = "20000001";
                        lastSalesResponse.RRN = "671156230774";
                        lastSalesResponse.HashPAN = "AD04100AAED7107555D6722C929B8C3C52E4984E";
                        lastSalesResponse.State = XFDevice.ECPIIntegration.ECPIHelper.eSalesState.Declined;

                        string paymentData = null;
                        string paymentReferenceNo = null;
                        int paymentTypeId = 0;

                        string CCLog = string.Format(@"
                CreditCardCallBack ****************************************
                Status : {0}
                TID : {1}
                CardNo : {2}
                CardType : {3}
                ApprovalCode : {4}
                RRN : {5}
                HashPAN : {6}
                END - CreditCardCallBack ****************************************",
                 lastSalesResponse.State.ToString(), lastSalesResponse.SaleTID, lastSalesResponse.MaskPAN, lastSalesResponse.CCSchema, lastSalesResponse.APPRCode, lastSalesResponse.RRN, lastSalesResponse.HashPAN);


                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("CreditCardCallBack:- {0}", CCLog), TraceCategory);

                        if (lastSalesResponse.State == XFDevice.ECPIIntegration.ECPIHelper.eSalesState.Approved)
                        {
                            SetPaymentStage(ePaymentStage.ProcessTransaction);
                            StartTransaction();
                            System.Threading.Thread.Sleep(3000);
                            bool canGetDetails = GetPaymentDetails_CC(out paymentTypeId, out paymentReferenceNo, out paymentData);
                            UpdateTransaction(paymentTypeId, paymentReferenceNo, paymentData, null, "N");

                            SetTrxStatus(true);
                            PerformService();
                        }
                        else
                        {
                            PrintReceipt(false, false, ePaymentMethod.CCards);
                            System.Threading.Thread.Sleep(3000);
                        }

                        #endregion
                    }
                }
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "DemoPaymentSelected Completed.", TraceCategory);

                //if (QRReceipt != null)
                //{
                //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                //    {
                //        SetPaymentStage(ePaymentStage.ReceiptQR);
                //    }));
                //    _WaitDone.Reset();
                //    _WaitDone.WaitOne(20000);
                //}

                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                //{
                //    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
                //}));

                //GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
            };
            Worker.RunWorkerAsync();
        }

        private void ConstructPaymentStepIntruction(ePaymentStep flow)
        {
            try
            {
                App.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(
                    () =>
                    {
                        TextInstructionA = ePaymentInstruction[flow].TextInstructionA;
                        TextInstructionB = string.Format("{1} RM {0}", Total.ToString("#.00"), ACWSSK.Properties.Resources.Label_Amount);
                        //InstructionImage = ePaymentInstruction[flow].InstructionImagePath;

                        //SetVideo(System.IO.Path.Combine(System.Environment.CurrentDirectory, ePaymentInstruction[flow].InstructionVideoPath));
                        SetVideo(string.Format("/ACWSSK;component/" + ePaymentInstruction[flow].InstructionVideoPath));

                    }));
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error]CreditCardCallBack:- {0}", ex.ToString()), TraceCategory);
            }           
        }

        public class cIntruction
        {
            public cIntruction() { }

            public string TextInstructionA { get; set; }
            public string TextInstructionB { get; set; }

            public string InstructionImagePath { get; set; }
            public string InstructionVideoPath { get; set; }
        }

        Dictionary<ePaymentStep, cIntruction> ePaymentInstruction = new Dictionary<ePaymentStep, cIntruction>()
        {
            { ePaymentStep.ScanEWallet, new cIntruction(){ TextInstructionA = ACWSSK.Properties.Resources.Label_ScanEWallet, TextInstructionB = "", InstructionVideoPath = "Resources/Images/GIF/ScanEwallet.gif"}},
            { ePaymentStep.TapCard, new cIntruction(){ TextInstructionA = ACWSSK.Properties.Resources.Label_TapCard, TextInstructionB = "", InstructionVideoPath = "Resources/Images/GIF/TapCard.gif"}},
            { ePaymentStep.App, new cIntruction(){ TextInstructionA = ACWSSK.Properties.Resources.Label_ScanLecshineApp, TextInstructionB = "", InstructionVideoPath = "Resources/Images/GIF/ScanLecshineApp.gif"}}
        };

        private void SetVideo(string videoPath)
        {
            VideoUri = videoPath;
            //VideoUri = new Uri(videoPath, UriKind.Absolute);
        }

        #region Cancel Command

        private ICommand _CancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                    _CancelCommand = new RelayCommand(Cancel);

                return _CancelCommand;
            }
        }

        void Cancel()
        {
            StopTxTimer();
            if (_SelectedPaymentMethod == ePaymentMethod.eWallet || _SelectedPaymentMethod == ePaymentMethod.App)
            {
                IsEwalletCancel = true;
                //if (GeneralVar.EWallet_Enabled && GeneralVar.BarcodeReader_Enabled) { DetachKeyboardHook(); }

                waitBarcode.Set();
            }
            else
            {
                if (GeneralVar.CreditCardTerminal_Enabled)
                {
                    IsCCCancel = true;
                    GeneralVar.CoherantHandler.CancelSales();
                }
            }

            GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
        }

        void CancelPayment()
        {
            if (_SelectedPaymentMethod == ePaymentMethod.eWallet)
            {
                //if (GeneralVar.EWallet_Enabled && GeneralVar.BarcodeReader_Enabled) { DetachKeyboardHook(); }
            }
            else
            {
                if (GeneralVar.CreditCardTerminal_Enabled)
                {
                    IsCCCancel = true;
                    GeneralVar.CoherantHandler.CancelSales();
                }
            }
        }

        #endregion

        #region Print Command

        private ICommand _PrintCommand;
        public ICommand PrintCommand
        {
            get
            {
                if (_PrintCommand == null)
                    _PrintCommand = new RelayCommand(Print);

                return _PrintCommand;
            }
        }

        bool _isPrint = false;
        int _printCount = 0;

        void Print()
        {
            try
            {


                Thread th = new Thread(() =>
                {
                    if (_isPrint && _printCount >= 1) return;

                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Print Starting..."), TraceCategory);

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        GeneralVar.vmMainWindow.ShowLoading = Visibility.Visible;
                    }));

                    _isPrint = true;
                    _printCount++;

                    if (_SelectedPaymentMethod == ePaymentMethod.eWallet)
                    { PrintSuccessReceipt(ePaymentMethod.eWallet); }
                    else if (_SelectedPaymentMethod == ePaymentMethod.CCards)
                    { PrintSuccessReceipt(ePaymentMethod.CCards); }
                    else if (_SelectedPaymentMethod == ePaymentMethod.App)
                    { PrintSuccessReceipt(ePaymentMethod.App); }

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        GeneralVar.vmMainWindow.ShowLoading = Visibility.Collapsed;                       
                    }));
                    _WaitPrint.Set();
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("Print Completed."), TraceCategory);
                });
                th.Start();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] Print: {0}", ex.Message), TraceCategory);
            }
            
        }

        #endregion

        #region Keyboard Barcode Handler

        //public void AttachKeyboardHook()
        //{
        //    if (!App.Current.Dispatcher.CheckAccess())
        //    {
        //        Action action = new Action(() => { AttachKeyboardHook(); });
        //        App.Current.Dispatcher.Invoke(action);
        //        return;
        //    }

        //    if (!GeneralVar.BarcodeReader_Enabled)
        //        return;

        //    GeneralVar.KeyboardHandler.KeyDown += new System.Windows.Forms.KeyEventHandler(KeyboardHandler_KeyDown);
        //}

        //public void DetachKeyboardHook()
        //{
        //    if (!App.Current.Dispatcher.CheckAccess())
        //    {
        //        Action action = new Action(() => { DetachKeyboardHook(); });
        //        App.Current.Dispatcher.Invoke(action);
        //        return;
        //    }

        //    if (!GeneralVar.BarcodeReader_Enabled)
        //        return;

        //    GeneralVar.KeyboardHandler.KeyDown -= new System.Windows.Forms.KeyEventHandler(KeyboardHandler_KeyDown);
        //    GeneralVar.KeyboardHandler.KeyDown += null;
        //}

        bool NextShiftOn = false;
        private void KeyboardHandler_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("KeyboardHandler_KeyDown KeyValue: {0}", e.KeyValue), TraceCategory);
            if (e.KeyValue == 13 || (e.KeyValue >= 48 && e.KeyValue <= 57) || (e.KeyValue >= 65 && e.KeyValue <= 90) || e.KeyValue == 186 || e.KeyValue == 191)
            {
                char key = new char();
                if (GeneralVar.KeyboardHandler.IsShift)
                    key = (char)e.KeyValue;
                else if (NextShiftOn)
                {
                    key = char.ToUpper((char)e.KeyValue);
                    NextShiftOn = false;
                }
                else
                    key = char.ToLower((char)e.KeyValue);
                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("KeyboardHandler_KeyDown Key: {0}", key), TraceCategory);
                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("KeyboardHandler_KeyDown Shift: {0}", e.Shift), TraceCategory);
                CardNo += key;
            }
            else if (e.KeyValue == 160 || e.KeyValue == 161)
            { NextShiftOn = true; }

            e.Handled = true;
        }

        #endregion
    }
}
