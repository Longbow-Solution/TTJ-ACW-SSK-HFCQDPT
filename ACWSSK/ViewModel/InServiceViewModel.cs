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
    public class InServiceViewModel : BaseViewModel
    {
        const string TraceCategory = "InServiceViewModel";

        #region Contructor

        public InServiceViewModel()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("InServiceViewModel Starting..."), TraceCategory);
                GeneralVar.CurrentAppState = "Online";
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("InServiceViewModel Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] InServiceViewModel: {0}", ex.ToString()), TraceCategory);
            }
        }

        #endregion
    }
}
