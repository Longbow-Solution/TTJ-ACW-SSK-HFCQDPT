using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACWSSK.Model
{
    public class Fare : BaseViewModel
    {
        private int _fareId;
        public int FareId
        {
            get { return _fareId; }
            set
            {
                if (value == _fareId)
                    return;

                _fareId = value;
                base.OnPropertyChanged("FareId");
            }
        }
        
        private string _fareCode;
        public string FareCode
        {
            get { return _fareCode; }
            set
            {
                if (value == _fareCode)
                    return;

                _fareCode = value;
                base.OnPropertyChanged("FareCode");
            }
        }

        private string _fareName;
        public string FareName
        {
            get { return _fareName; }
            set
            {
                if (value == _fareName)
                    return;

                _fareName = value;
                base.OnPropertyChanged("FareName");
            }
        }

        private List<FareConfig> _fareConfigs;
        public List<FareConfig> FareConfigs
        {
            get { return _fareConfigs; }
            set
            {
                if (value == _fareConfigs)
                    return;

                _fareConfigs = value;
                base.OnPropertyChanged("FareConfigs");
            }
        }
    }

    public class FareConfig : BaseViewModel
    {
        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                if (value == _startTime)
                    return;

                _startTime = value;
                base.OnPropertyChanged("StartTime");
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (value == _amount)
                    return;

                _amount = value;
                base.OnPropertyChanged("Amount");
            }
        }
    }

}
