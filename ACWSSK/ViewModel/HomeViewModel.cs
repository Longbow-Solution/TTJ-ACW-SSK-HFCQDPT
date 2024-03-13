using ACWSSK.App_Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ACWSSK.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        const string TraceCategory = "HomeViewModel";

        public HomeViewModel()
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("HomeViewModel Starting..."), TraceCategory);
                Label_Welcome = "Hi, welcome!";
                GeneralVar.CurrentAppState = "Online";
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("HomeViewModel Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] HomeViewModel = {0}", ex.ToString()), TraceCategory);
            }
        }

        string _Label_Welcome;
        public string Label_Welcome
        {
            get { return _Label_Welcome; }
            set
            {
                _Label_Welcome = value;
                OnPropertyChanged("Label_Welcome");
            }
        }

        ICommand _LanguageSelectionCommand;
		public ICommand LanguageSelectionCommand
		{
			get
			{
				if (_LanguageSelectionCommand == null)
					_LanguageSelectionCommand = new RelayCommand<string>(LanguageSelection);
				return _LanguageSelectionCommand;
			}
		}

		void LanguageSelection(string language)
        {
            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("LanguageSelection = {0}", language), TraceCategory);

				string uiCulture;
				switch (language)
				{
					case "BM":                     
						uiCulture = "ms-my";
						break;
					case "EN":
						uiCulture = "en-us";
						break;
					case "ZH":
						uiCulture = "zh-cn";
						break;
					default:
                        uiCulture = "en-us";
						break;
				}
                GeneralVar.LanguageSelected = language;
                ACWSSK.Properties.Resources.Culture = new System.Globalization.CultureInfo(uiCulture);
                GeneralVar.vmMainWindow.SetModuleStage(eModuleStage.ServiceSelection);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] LanguageSelection = {0}", ex.ToString()), TraceCategory);
            }
        }
	}
}
