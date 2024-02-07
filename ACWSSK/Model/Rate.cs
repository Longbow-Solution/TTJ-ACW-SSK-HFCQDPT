using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACWSSK.Model
{
    public class Rate : BaseViewModel
    {
        private int _rateId;
        public int RateId
        {
            get { return _rateId; }
            set
            {
                if (value == _rateId)
                    return;

                _rateId = value;
                base.OnPropertyChanged("RateId");
            }
        }

        private string _rateCode;
        public string RateCode
        {
            get { return _rateCode; }
            set
            {
                if (value == _rateCode)
                    return;

                _rateCode = value;
                base.OnPropertyChanged("RateCode");
            }
        }

        private string _rateName;
        public string RateName
        {
            get { return _rateName; }
            set
            {
                if (value == _rateName)
                    return;

                _rateName = value;
                base.OnPropertyChanged("RateName");
            }
        }

        private decimal _value;
        public decimal Value
        {
            get { return _value; }
            set
            {
                if (value == _value)
                    return;

                _value = value;
                base.OnPropertyChanged("Value");
            }
        }
    }
}
