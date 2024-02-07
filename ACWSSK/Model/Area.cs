using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACWSSK.Model
{
    public class Area : BaseViewModel
    {
        private int _areaId;
        public int AreaId
        {
            get { return _areaId; }
            set
            {
                if (value == _areaId)
                    return;

                _areaId = value;
                base.OnPropertyChanged("AreaId");
            }
        }

        private string _areaCode;
        public string AreaCode
        {
            get { return _areaCode; }
            set
            {
                if (value == _areaCode)
                    return;

                _areaCode = value;
                base.OnPropertyChanged("AreaCode");
            }
        }

        private string _areaName;
        public string AreaName
        {
            get { return _areaName; }
            set
            {
                if (value == _areaName)
                    return;

                _areaName = value;
                base.OnPropertyChanged("AreaName");
            }
        }

        private int _stateId;
        public int StateId
        {
            get { return _stateId; }
            set
            {
                if (value == _stateId)
                    return;

                _stateId = value;
                base.OnPropertyChanged("StateId");
            }
        }

        private string _stateName;
        public string StateName
        {
            get { return _stateName; }
            set
            {
                if (value == _stateName)
                    return;

                _stateName = value;
                base.OnPropertyChanged("StateName");
            }
        }

        private int _taxId;
        public int TaxId
        {
            get { return _taxId; }
            set
            {
                if (value == _taxId)
                    return;

                _taxId = value;
                base.OnPropertyChanged("TaxId");
            }
        }

        private string _taxName;
        public string TaxName
        {
            get { return _taxName; }
            set
            {
                if (value == _taxName)
                    return;

                _taxName = value;
                base.OnPropertyChanged("TaxName");
            }
        }

        private int _surchargeId;
        public int SurchargeId
        {
            get { return _surchargeId; }
            set
            {
                if (value == _surchargeId)
                    return;

                _surchargeId = value;
                base.OnPropertyChanged("SurchargeId");
            }
        }

        private string _surchargeName;
        public string SurchargeName
        {
            get { return _surchargeName; }
            set
            {
                if (value == _surchargeName)
                    return;

                _surchargeName = value;
                base.OnPropertyChanged("SurchargeName");
            }
        }

        private int _holidayGroupId;
        public int HolidayGroupId
        {
            get { return _holidayGroupId; }
            set
            {
                if (value == _holidayGroupId)
                    return;

                _holidayGroupId = value;
                base.OnPropertyChanged("HolidayGroupId");
            }
        }

        private string _holidayGroupName;
        public string HolidayGroupName
        {
            get { return _holidayGroupName; }
            set
            {
                if (value == _holidayGroupName)
                    return;

                _holidayGroupName = value;
                base.OnPropertyChanged("HolidayGroupName");
            }
        }

        private List<AreaTariff> _Tariffs;
        public List<AreaTariff> Tariffs
        {
            get { return _Tariffs; }
            set
            {
                if (value == _Tariffs)
                    return;

                _Tariffs = value;
                base.OnPropertyChanged("Tariffs");
            }
        }
    }

    public class AreaTariff : BaseViewModel
    {
        private int _tariffId;
        public int TariffId
        {
            get { return _tariffId; }
            set
            {
                if (value == _tariffId)
                    return;

                _tariffId = value;
                base.OnPropertyChanged("TariffId");
            }
        }

        public string TariffDay
        {
            get
            { 
                if(_tariffId == 0) { return "Exclusive Day"; }
                else if (_tariffId == 1) { return "Monday"; }
                else if (_tariffId == 2) { return "Tuesday"; }
                else if (_tariffId == 3) { return "Wednesday"; }
                else if (_tariffId == 4) { return "Thursday"; }
                else if (_tariffId == 5) { return "Friday"; }
                else if (_tariffId == 6) { return "Saturday"; }
                else if (_tariffId == 7) { return "Sunday"; }
                else { return "Unknown Day"; }
            }
        }

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
    }
}
