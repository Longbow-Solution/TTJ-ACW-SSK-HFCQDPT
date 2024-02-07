using ACWSSK.App_Code;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACWSSK.Model
{
	public class Component : BaseViewModel
	{
        private int _id;
        public int Id
        {
            get { return _id; }
            set
            {
                if (value == _id)
                    return;

                _id = value;
                base.OnPropertyChanged("Id");
            }
        }

        private string _componentCode;
        public string ComponentCode
        {
            get { return _componentCode; }
            set
            {
                if (value == _componentCode)
                    return;

                _componentCode = value;
                base.OnPropertyChanged("ComponentCode");
            }
        }

        private string _componentName;
        public string ComponentName
        {
            get { return _componentName; }
            set
            {
                if (value == _componentName)
                    return;

                _componentName = value;
                base.OnPropertyChanged("ComponentName");
            }
        }

        private TimeSpan _networkOpeningTime;
        public TimeSpan NetworkOpeningTime
        {
            get { return _networkOpeningTime; }
            set
            {
                if (value == _networkOpeningTime)
                    return;

                _networkOpeningTime = value;
                base.OnPropertyChanged("NetworkOpeningTime");
            }
        }

        private TimeSpan _networkClosingTime;
        public TimeSpan NetworkClosingTime
        {
            get { return _networkClosingTime; }
            set
            {
                if (value == _networkClosingTime)
                    return;

                _networkClosingTime = value;
                base.OnPropertyChanged("NetworkClosingTime");
            }
        }

        private List<TimeSpan> _EDCSettlementTime;
        public List<TimeSpan> EDCSettlementTime
        {
            get { return _EDCSettlementTime; }
            set
            {
                if (value == _EDCSettlementTime)
                    return;

                _EDCSettlementTime = value;
                base.OnPropertyChanged("EDCSettlementTime");
            }
        }

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

        static string _eWalletMerchantKey;
        public string EWalletMerchantKey
        {
            get { return _eWalletMerchantKey; }
            set
            {
                if (value == _eWalletMerchantKey)
                    return;

                _eWalletMerchantKey = value;
                base.OnPropertyChanged("EWalletMerchantKey");
            }
        }

        static string _eWalletMerchantCode;
        public string EWalletMerchantCode
        {
            get { return _eWalletMerchantCode; }
            set
            {
                if (value == _eWalletMerchantCode)
                    return;

                _eWalletMerchantCode = value;
                base.OnPropertyChanged("EWalletMerchantCode");
            }
        }

        static string _CompanyName;
        public string CompanyName
        {
            get { return _CompanyName; }
            set
            {
                if (value == _CompanyName)
                    return;

                _CompanyName = value;
                base.OnPropertyChanged("CompanyName");
            }
        }

        static string _Address1;
        public string Address1
        {
            get { return _Address1; }
            set
            {
                if (value == _Address1)
                    return;

                _Address1 = value;
                base.OnPropertyChanged("Address1");
            }
        }

        static string _Address2;
        public string Address2
        {
            get { return _Address2; }
            set
            {
                if (value == _Address2)
                    return;

                _Address2 = value;
                base.OnPropertyChanged("Address2");
            }
        }

        static string _Address3;
        public string Address3
        {
            get { return _Address3; }
            set
            {
                if (value == _Address3)
                    return;

                _Address3 = value;
                base.OnPropertyChanged("Address3");
            }
        }

        static string _Address4;
        public string Address4
        {
            get { return _Address4; }
            set
            {
                if (value == _Address4)
                    return;

                _Address4 = value;
                base.OnPropertyChanged("Address4");
            }
        }

        static string _Careline;
        public string Careline
        {
            get { return _Careline; }
            set
            {
                if (value == _Careline)
                    return;

                _Careline = value;
                base.OnPropertyChanged("Careline");
            }
        }

        static string _Email;
        public string Email
        {
            get { return _Email; }
            set
            {
                if (value == _Email)
                    return;

                _Email = value;
                base.OnPropertyChanged("Email");
            }
        }

        static string _SSMNo;
        public string SSMNo
        {
            get { return _SSMNo; }
            set
            {
                if (value == _SSMNo)
                    return;

                _SSMNo = value;
                base.OnPropertyChanged("SSMNo");
            }
        }

        static string _SalesTaxNo;
        public string SalesTaxNo
        {
            get { return _SalesTaxNo; }
            set
            {
                if (value == _SalesTaxNo)
                    return;

                _SalesTaxNo = value;
                base.OnPropertyChanged("SalesTaxNo");
            }
        }

        private int _QDIotId;
        public int QDIotId
        {
            get { return _QDIotId; }
            set
            {
                if (value == _QDIotId)
                    return;

                _QDIotId = value;
                base.OnPropertyChanged("QDIotId");
            }
        }

        private string _QDOpenId;
        public string QDOpenId
        {
            get { return _QDOpenId; }
            set
            {
                if (value == _QDOpenId)
                    return;

                _QDOpenId = value;
                base.OnPropertyChanged("QDOpenId");
            }
        }

        private string _QDSecret;
        public string QDSecret
        {
            get { return _QDSecret; }
            set
            {
                if (value == _QDSecret)
                    return;

                _QDSecret = value;
                base.OnPropertyChanged("QDSecret");
            }
        }
	}
}
