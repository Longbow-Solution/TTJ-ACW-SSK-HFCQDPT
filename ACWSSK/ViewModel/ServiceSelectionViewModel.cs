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
    public class ServiceSelectionViewModel : BaseViewModel
    {
        const string TraceCategory = "ServiceSelectionViewModel";

        public ServiceSelectionViewModel()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ServiceSelectionViewModel Starting..."), TraceCategory);
                GeneralVar.CurrentAppState = "Online";

                Label_SelectService = ACWSSK.Properties.Resources.Label_SelectService;
                Label_MakePayment = ACWSSK.Properties.Resources.Label_MakePayment;
                Label_RedeemService = ACWSSK.Properties.Resources.Label_RedeemService;
                Label_Back = ACWSSK.Properties.Resources.Label_Back;

                if (GeneralVar.ACWMachineModel == eCarWashMachine.PENTAMASTER)
                {
                    ShowRedeemService = Visibility.Collapsed;
                }
                else
                    ShowRedeemService = Visibility.Collapsed;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ServiceSelectionViewModel Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] ServiceSelectionViewModel = {0}", ex.ToString()), TraceCategory);
            }
        }

        Visibility _ShowRedeemService = Visibility.Collapsed;
        public Visibility ShowRedeemService
        {
            get { return _ShowRedeemService; }
            set
            {
                _ShowRedeemService = value;
                OnPropertyChanged("ShowRedeemService");
            }
        }

        string _Label_SelectService;
        public string Label_SelectService
        {
            get { return _Label_SelectService; }
            set
            {
                _Label_SelectService = value;
                OnPropertyChanged("Label_SelectService");
            }
        }

        string _Label_RedeemService;
        public string Label_RedeemService
        {
            get { return _Label_RedeemService; }
            set
            {
                _Label_RedeemService = value;
                OnPropertyChanged("Label_RedeemService");
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

        string _Label_MakePayment;
        public string Label_MakePayment
        {
            get { return _Label_MakePayment; }
            set
            {
                _Label_MakePayment = value;
                OnPropertyChanged("Label_MakePayment");
            }
        }

        ICommand _BackServiceSelectionCommand;
        public ICommand BackServiceSelectionCommand
        {
            get
            {
                if (_BackServiceSelectionCommand == null)
                    _BackServiceSelectionCommand = new RelayCommand(BackServiceSelection);
                return _BackServiceSelectionCommand;
            }
        }

        ICommand _ServiceSelectionCommand;
        public ICommand ServiceSelectionCommand
        {
            get
            {
                if (_ServiceSelectionCommand == null)
                    _ServiceSelectionCommand = new RelayCommand<string>(ServiceSelection);
                return _ServiceSelectionCommand;
            }
        }

        void ServiceSelection(string service)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ServiceSelction = {0}", service), TraceCategory);

                if (service == "Payment")
                {
                    GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.PaymentSelection);
                }
                else if (service == "Service")
                {

                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] ServiceSelction = {0}", ex.ToString()), TraceCategory);
            }
        }

        void BackServiceSelection()
        {
            try
            {
                GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] BackServiceSelection = {0}", ex.ToString()), TraceCategory);
            }
        }
    }
}
