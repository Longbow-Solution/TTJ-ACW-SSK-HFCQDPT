using ACWSSK.App_Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace ACWSSK.ViewModel
{
    public class ErrorViewModel : BaseViewModel
    {
        const string TraceCategory = "ErrorViewModel";

        private string _customDisplayError;
        public string CustomDisplayError
        {
            get { return _customDisplayError; }
            set
            {
                if (value == _customDisplayError)
                    return;

                _customDisplayError = value;
                base.OnPropertyChanged("CustomDisplayError");
            }
        }

        private Visibility _hasCustomError;
        public Visibility HasCustomError
        {
            get { return _hasCustomError; }
            set
            {
                if (value == _hasCustomError)
                    return;

                _hasCustomError = value;
                base.OnPropertyChanged("HasCustomError");
            }
        }

        public ErrorViewModel(string error)
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "ErrorViewModel Starting...", TraceCategory);

            _customDisplayError = error;

            if (string.IsNullOrEmpty(error)) { HasCustomError = Visibility.Collapsed; }
            else { HasCustomError = Visibility.Visible; }

            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, "ErrorViewModel Completed.", TraceCategory);
        }
    }
}
