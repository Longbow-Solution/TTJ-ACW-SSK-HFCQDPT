using ACWSSK.App_Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ACWSSK.ViewModel
{
    public class PaymentSelectionViewModel : BaseViewModel
    {
        const string TraceCategory = "PaymentSelectionViewModel";

        public PaymentSelectionViewModel()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PaymentSelectionViewModel Starting..."), TraceCategory);
                GeneralVar.CurrentAppState = "Online";

                Label_SelectPaymentMethod = ACWSSK.Properties.Resources.Label_SelectPaymentMethod;
                Label_LecshineApp = ACWSSK.Properties.Resources.Label_LecshineApp;
                Label_Card = ACWSSK.Properties.Resources.Label_Card;
                Label_EWallet = ACWSSK.Properties.Resources.Label_EWallet;
                Label_Back = ACWSSK.Properties.Resources.Label_Back;

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

                PayByEWalletVisibility = CanPayByEWallet ? Visibility.Visible : Visibility.Collapsed;
                PayByCardVisibility = CanPayByCard ? Visibility.Visible : Visibility.Collapsed;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("PaymentSelectionViewModel Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] PaymentSelectionViewModel = {0}", ex.ToString()), TraceCategory);
            }
        }

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

        private Visibility _PayByCardVisibility;
        public Visibility PayByCardVisibility
        {
            get { return _PayByCardVisibility; }
            set { _PayByCardVisibility = value; OnPropertyChanged("PayByCardVisibility"); }
        }

        private Visibility _PayByEWalletVisibility;
        public Visibility PayByEWalletVisibility
        {
            get { return _PayByEWalletVisibility; }
            set { _PayByEWalletVisibility = value; OnPropertyChanged("PayByEWalletVisibility"); }
        }

        string _Label_SelectPaymentMethod;
        public string Label_SelectPaymentMethod
        {
            get { return _Label_SelectPaymentMethod; }
            set
            {
                _Label_SelectPaymentMethod = value;
                OnPropertyChanged("Label_SelectPaymentMethod");
            }
        }

        string _Label_LecshineApp;
        public string Label_LecshineApp
        {
            get { return _Label_LecshineApp; }
            set
            {
                _Label_LecshineApp = value;
                OnPropertyChanged("Label_LecshineApp");
            }
        }

        string _Label_Back;
        public string Label_Back
        {
            get { return _Label_Back; }
            set
            {
                _Label_Back = value;
                OnPropertyChanged("Label_Back");
            }
        }

        string _Label_Card;
        public string Label_Card
        {
            get { return _Label_Card; }
            set
            {
                _Label_Card = value;
                OnPropertyChanged("Label_Card");
            }
        }

        string _Label_EWallet;
        public string Label_EWallet
        {
            get { return _Label_EWallet; }
            set
            {
                _Label_EWallet = value;
                OnPropertyChanged("Label_EWallet");
            }
        }

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

        ICommand _BackPaymentSelectionCommand;
        public ICommand BackPaymentSelectionCommand
        {
            get
            {
                if (_BackPaymentSelectionCommand == null)
                    _BackPaymentSelectionCommand = new RelayCommand(BackPaymentSelection);
                return _BackPaymentSelectionCommand;
            }
        }       

        #endregion

        #region Function

        private void PerformEWalletPayment(object arg)
        {
            GeneralVar.vmMainWindow.SetPaymentModuleStage(ePaymentMethod.eWallet);
        }

        private void PerformCardPayment(object arg)
        {
            GeneralVar.vmMainWindow.SetPaymentModuleStage(ePaymentMethod.CCards);
        }

        private void PerformAppPayment(object arg)
        {

            GeneralVar.vmMainWindow.SetPaymentModuleStage(ePaymentMethod.App);
        }

        private void BackPaymentSelection()
        {
            GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.ServiceSelection);
        }

        #endregion
    }
}
