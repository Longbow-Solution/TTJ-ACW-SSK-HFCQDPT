using ACWSSK.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ACWSSK.Model
{
    public class Servicing : BaseViewModel
    {
        public string DisplayCode { get; set; }
        public ICommand Command { get; set; }
        public eServicingTask CommandParameter { get; set; }
        public bool IsEnabled { get; set; }

        public Servicing(string displayCode, string displayName, ICommand command, eServicingTask commandParameter)
        {
            DisplayCode = displayCode;
            DisplayName = displayName;
            Command = command;
            CommandParameter = commandParameter;
        }

        public Servicing(string displayCode, string displayName, ICommand command, eServicingTask commandParameter, bool isEnabled)
        {
            DisplayCode = displayCode;
            DisplayName = displayName;
            Command = command;
            CommandParameter = commandParameter;
            IsEnabled = isEnabled;
        }
    }
}
