using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using ACWSSK.ViewModel;
using ACWSSK.App_Code;
using System.Diagnostics;
using System.Threading;

namespace ACWSSK
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        const string TraceCategory = "ACWSSK.App";

        private MainWindow mainV;
        private DFMonitoringClient.DFSocketClientHandler client;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                {
                    MessageBox.Show(Application.Current.MainWindow, "ACWSSSK is already running...", "ACWSSSK", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    App.Current.Shutdown();
                    return;
                }

                GeneralVar.CurrentComponent = GeneralVar.Query.GetComponent(GeneralVar.ComponentCode);
                
                if (GeneralVar.CurrentComponent == null || string.IsNullOrEmpty(GeneralVar.CurrentComponent.ComponentCode))
                {
                    MessageBox.Show(string.Format("Failed to Get Component Details. Please Contact Administrator"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                }

                if (GeneralVar.TCP_Enabled)
                {
                    System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(GeneralVar.TCP_IPAddress, GeneralVar.TCP_Port);
                    client = new DFMonitoringClient.DFSocketClientHandler("ACW", GeneralVar.ComponentCode, "SSK", remoteEP, GeneralVar.TCP_SetStatus_Interval);
                    client.ConnectFailed += client_ConnectFailed;

                    client.SetStatus += client_SetStatus;
                    client.SetStatusFailed += client_SetStatusFailed;

                    client.GetCommandSuccess += client_GetCommandSuccess;
                    client.GetCommandFailed += client_GetCommandFailed;

                    client.SetResponseFailed += client_SetResponseFailed;
                    client.StartHandle();
                }

                StartApplication();
            }
            catch (Exception ex) 
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] OnStartup:{0}", ex.ToString()), "Apps");
                MessageBox.Show(ex.ToString(), "Apps", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }

        }

        #region Kiosk Status Monitoring

        private void client_ConnectFailed(object sender, DFMonitoringClient.ConnectFailedEventArgs e)
        {
            //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] client_ConnectFailed: {0}", e.Error.ToString()), TraceCategory);
        }

        private void client_SetStatus(object sender, DFMonitoringClient.SetStatusEventArgs e)
        {
            e.ClientStatus.DeviceStatus = new List<DFMonitoringClient.DFDeviceStatus>();

            string code = "";
            string status = "";
            DFMonitoringClient.DFSeverityLevel severity = DFMonitoringClient.DFSeverityLevel.Info;
            string details = "";

            #region Application state

            code = "APP";
            status = GeneralVar.CurrentAppState;

            if (GeneralVar.CurrentAppState == "Error")
                severity = DFMonitoringClient.DFSeverityLevel.Error;
            else if (GeneralVar.CurrentAppState == "Offline")
                severity = DFMonitoringClient.DFSeverityLevel.None;
            else
            {
                severity = DFMonitoringClient.DFSeverityLevel.Info;
            }

            e.ClientStatus.DeviceStatus.Add(new DFMonitoringClient.DFDeviceStatus() { Code = code, Status = status, Severity = severity, Details = details });

            #endregion

            #region Credit Card Terminal

            code = "CC";
            if (!GeneralVar.CreditCardTerminal_Enabled)
            {
                status = "Disabled";
                severity = DFMonitoringClient.DFSeverityLevel.None;
            }
            else if (GeneralVar.EDCException != null)
            {
                status = GeneralVar.EDCException.Message;
                severity = DFMonitoringClient.DFSeverityLevel.Error;
            }
            else
            {
                status = "OK";
                severity = DFMonitoringClient.DFSeverityLevel.Info;
            }
            e.ClientStatus.DeviceStatus.Add(new DFMonitoringClient.DFDeviceStatus() { Code = code, Status = status, Severity = severity, Details = details });

            #endregion

            #region Receipt Printer

            code = "P";
            if (!GeneralVar.ReceiptPrinter_Enabled)
            {
                status = "Disabled";
                severity = DFMonitoringClient.DFSeverityLevel.None;
            }
            else if (GeneralVar.RPTException != null)
            {
                status = GeneralVar.RPTException.Message;
                severity = DFMonitoringClient.DFSeverityLevel.Error;
            }
            else if (GeneralVar.IsNearPaperEnd)
            {
                status = "Printer: PaperStatus.NearPaperEnd.";
                severity = DFMonitoringClient.DFSeverityLevel.Warning;
            }
            else
            {
                status = "OK";
                severity = DFMonitoringClient.DFSeverityLevel.Info;
            }
            e.ClientStatus.DeviceStatus.Add(new DFMonitoringClient.DFDeviceStatus() { Code = code, Status = status, Severity = severity, Details = details });

            #endregion

            #region IO Board

            code = "IO";

            if(!GeneralVar.IOBoard_Enabled)
            {
                status = "Disabled";
                severity = DFMonitoringClient.DFSeverityLevel.None;
            }
            else if (GeneralVar.IOBException != null) 
            {
                status = string.Format("{0} - {1}", GeneralVar.EDCException.Message, GeneralVar.EDCException.InnerException.Message);
                severity = DFMonitoringClient.DFSeverityLevel.Error;
            }
            else
            {
                status = "OK";
                severity = DFMonitoringClient.DFSeverityLevel.Info;
            }

            e.ClientStatus.DeviceStatus.Add(new DFMonitoringClient.DFDeviceStatus() { Code = code, Status = status, Severity = severity, Details = details });

            #endregion
        }

        private void client_SetStatusFailed(object sender, DFMonitoringClient.SetStatusFailedEventArgs e)
        {
            //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] client_SetStatusFailed: {0}", e.Error.ToString()), TraceCategory);
        }

        private void client_GetCommandSuccess(object sender, DFMonitoringClient.GetCommandSuccessEventArgs e)
        {
            //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] client_GetCommandSuccess: {0}", e.Error.ToString()), TraceCategory);
        }

        private void client_GetCommandFailed(object sender, DFMonitoringClient.GetCommandFailedEventArgs e)
        {
            //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] client_GetCommandFailed: {0}", e.Error.ToString()), TraceCategory);
        }

        private void client_SetResponseFailed(object sender, DFMonitoringClient.SetResponseFailedEventArgs e)
        {
            //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] client_SetResponseFailed: {0}", e.Error.ToString()), TraceCategory);
        }

        #endregion

        public void StartApplication()
        {
            mainV = new MainWindow();
            GeneralVar.vmMainWindow = new MainWindowViewModel();

            mainV.DataContext = GeneralVar.vmMainWindow;
            mainV.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error]Exit : {0}", ex.ToString()), "App.xaml");
            }
            Environment.Exit(0);
        }

        public enum eStage 
        {
            Operation,
            OffLine
        }
    }
}
