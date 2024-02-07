using ACWSSK.App_Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACWSSK.ViewModel
{
    public class OfflineViewModel : BaseViewModel
    {
        const string TraceCategory = "OfflineViewModel";

        public DateTime NetworkOpeningTime
        {
            get { return new DateTime(GeneralVar.CurrentComponent.NetworkOpeningTime.Ticks); }
        }

        public DateTime NetworkClosingTime
        {
            get { return new DateTime(GeneralVar.CurrentComponent.NetworkClosingTime.Ticks); }
        }

        public OfflineViewModel()
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "ErrorViewModel Starting...", TraceCategory);
            GeneralVar.CurrentAppState = "Offline";
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "ErrorViewModel Completed.", TraceCategory);
        }
    }
}
