using ACWSSK.App_Code;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ACWSSK.ViewModel
{
    public class MainIdleViewModel : BaseViewModel
    {
        const string TraceCategory = "MainIdleViewModel";

        #region Properties

        private bool _canPayByEWallet;
        public bool CanPayByEWallet
        {
            get { return _canPayByEWallet; }
            set
            {
                if (value == _canPayByEWallet)
                    return;

                _canPayByEWallet = value;
                base.OnPropertyChanged("CanPayByEWallet");
            }
        }

        private bool _canPayByCard;
        public bool CanPayByCard
        {
            get { return _canPayByCard; }
            set
            {
                if (value == _canPayByCard)
                    return;

                _canPayByCard = value;
                base.OnPropertyChanged("CanPayByCard");
            }
        }

        private bool _canPayByApp;
        public bool CanPayByApp
        {
            get { return _canPayByApp; }
            set
            {
                if (value == _canPayByApp)
                    return;

                _canPayByApp = value;
                base.OnPropertyChanged("CanPayByApp");
            }
        }

        private Visibility _PayByAppVisibility;

        public Visibility PayByAppVisibility
        {
            get { return _PayByAppVisibility; }
            set { _PayByAppVisibility = value; OnPropertyChanged("PayByAppVisibility"); }
        }

        #endregion

        #region ICommand

        ICommand _PayByEWalletCommand;
        public ICommand PayByEWalletCommand
        {
            get
            {
                if (_PayByEWalletCommand == null)
                    _PayByEWalletCommand = new RelayCommand<object>(PerformEWalletPayment);
                return _PayByEWalletCommand;
            }
        }

        ICommand _PayByCardCommand;
        public ICommand PayByCardCommand
        {
            get
            {
                if (_PayByCardCommand == null)
                    _PayByCardCommand = new RelayCommand<object>(PerformCardPayment);
                return _PayByCardCommand;
            }
        }

        ICommand _PayByAppCommand;
        public ICommand PayByAppCommand
        {
            get
            {
                if (_PayByAppCommand == null)
                    _PayByAppCommand = new RelayCommand<object>(PerformAppPayment);
                return _PayByAppCommand;
            }
        }

        #endregion

        #region Contructor

        public MainIdleViewModel()
        {
            try
            {
                GeneralVar.CurrentAppState = "Online";
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "MainIdleViewModel Starting...", TraceCategory);
                
                CanPayByEWallet = (GeneralVar.AvailablePaymentMethod & ePaymentMethod.eWallet) > 0;
                CanPayByApp = (GeneralVar.AvailablePaymentMethod & ePaymentMethod.App) > 0;
                CanPayByCard = (GeneralVar.AvailablePaymentMethod & ePaymentMethod.CCards) > 0;

                if (GeneralVar.ApplicationMode == AppMode.PROD)
                {
                    if (CanPayByCard && !GeneralVar.CreditCardTerminal_Enabled)
                        CanPayByCard = false;

                    if (CanPayByEWallet && (!GeneralVar.BarcodeReader_Enabled || !GeneralVar.EWallet_Enabled))
                        CanPayByEWallet = false;


                    if (CanPayByApp && (!GeneralVar.BarcodeReader_Enabled || !GeneralVar.App_Enabled))
                        CanPayByApp = false;

                    PayByAppVisibility = GeneralVar.App_Enabled ? Visibility.Visible : Visibility.Collapsed;
                }
                else 
                {
                    CanPayByEWallet = true;
                    CanPayByCard = true;
                    CanPayByApp = true;
                    PayByAppVisibility = GeneralVar.App_Enabled ? Visibility.Visible : Visibility.Collapsed;

                }

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "MainIdleViewModel Completed...", TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] MainIdleViewModel: {0}", ex.ToString()), TraceCategory);
            }
        }

        #endregion

        #region Function

        private void PerformEWalletPayment(object arg)
        {
            //GeneralVar.vmMainWindow.SetPaymentModuleStage(ePaymentMethod.eWallet);
        }

        private void PerformCardPayment(object arg)
        {
            //GeneralVar.vmMainWindow.SetPaymentModuleStage(ePaymentMethod.CCards);
        }

        private void PerformAppPayment(object arg)
        {
            //GeneralVar.vmMainWindow.SetPaymentModuleStage(ePaymentMethod.App);
        }

        #endregion
    }
}
