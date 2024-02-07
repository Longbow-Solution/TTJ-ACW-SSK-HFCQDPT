using ACWSSK.App_Code;
using ACWSSK.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ACWSSK.ViewModel
{
    public class ServicingViewModel : BaseViewModel
    {
        protected string TraceCategory = "ServicingViewModel";

        public ServicingViewModel()
        {
            if (MaintenanceListCollection != null)
                MaintenanceListCollection = null;

            MaintenanceListCollection = new ObservableCollection<Servicing>();
            MaintenanceListCollection.Add(new Servicing("1", "Reset", MaintenanceCommand, eServicingTask.Reset, true));
            MaintenanceListCollection.Add(new Servicing("2", "Stop", MaintenanceCommand, eServicingTask.Stop, true));
            MaintenanceListCollection.Add(new Servicing("3", "Start", MaintenanceCommand, eServicingTask.Start, true));
        }

        ObservableCollection<Servicing> _MaintenanceListCollection;
        public ObservableCollection<Servicing> MaintenanceListCollection
        {
            get { return _MaintenanceListCollection; }
            set
            {
                _MaintenanceListCollection = value;
                OnPropertyChanged("MaintenanceListCollection");
            }
        }

        ICommand _MaintenanceCommand;
        public ICommand MaintenanceCommand
        {
            get
            {
                if (_MaintenanceCommand == null)
                    _MaintenanceCommand = new RelayCommand<eServicingTask>(ServiceSetting);

                return _MaintenanceCommand;
            }
        }

        void ServiceSetting(eServicingTask command)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ServiceSetting = {0}", command), TraceCategory);
                switch (command)
                {
                    case eServicingTask.Reset:
                        GeneralVar.vmMainWindow.ShowLoading = Visibility.Visible;
                        Thread t = new Thread(() =>
                        {
                            Thread.Sleep(2000);
                            GeneralVar.vmMainWindow.ResetMachine();
                            GeneralVar.vmMainWindow.ShowLoading = Visibility.Collapsed;
                        });
                        t.Start();

                        break;
                    case eServicingTask.Stop:
                        GeneralVar.vmMainWindow.ShowLoading = Visibility.Visible;
                        Thread stop = new Thread(() =>
                        {
                            Thread.Sleep(2000);
                            GeneralVar.vmMainWindow.StopMachine();
                            GeneralVar.vmMainWindow.ShowLoading = Visibility.Collapsed;
                        });
                        stop.Start();

                        break;
                    case eServicingTask.Start:
                        GeneralVar.vmMainWindow.ShowLoading = Visibility.Visible;
                        Thread start = new Thread(() =>
                        {
                            Thread.Sleep(2000);
                            GeneralVar.vmMainWindow.StartMachine();
                            GeneralVar.vmMainWindow.ShowLoading = Visibility.Collapsed;
                        });
                        start.Start();

                        break;
                    case eServicingTask.Logout:
                        GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.Home);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] ServiceSetting = {0}", ex.ToString()), TraceCategory);
            }
        }
    }
}
