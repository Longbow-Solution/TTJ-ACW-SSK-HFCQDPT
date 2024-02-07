using ACWSSK.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ACWSSK.Model
{
	public class CheckItem : BaseViewModel
	{
        private string _itemCode;
        public string ItemCode
        {
            get { return _itemCode; }
            set
            {
                if (value == _itemCode)
                    return;

                _itemCode = value;
                base.OnPropertyChanged("ItemCode");
            }
        }

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                if (value == _itemName)
                    return;

                _itemName = value;
                base.OnPropertyChanged("ItemName");
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                if (value == _status)
                    return;

                _status = value;
                base.OnPropertyChanged("Status");
            }
        }

        private string _statusDescription;
        public string StatusDescription
        {
            get { return _statusDescription; }
            set
            {
                if (value == _statusDescription)
                    return;

                _statusDescription = value;
                base.OnPropertyChanged("StatusDescription");
            }
        }

        private string _errorCode;
        public string ErrorCode
        {
            get { return _errorCode; }
            set
            {
                if (value == _errorCode)
                    return;

                _errorCode = value;
                base.OnPropertyChanged("ErrorCode");
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (value == _errorMessage)
                    return;

                _errorMessage = value;
                base.OnPropertyChanged("ErrorMessage");
            }
        }

        private string _errorDescription;
        public string ErrorDescription
        {
            get { return _errorDescription; }
            set
            {
                if (value == _errorDescription)
                    return;

                _errorDescription = value;
                base.OnPropertyChanged("ErrorDescription");
            }
        }

        private bool _canSkip;
        public bool CanSkip
        {
            get { return _canSkip; }
            set
            {
                if (value == _canSkip)
                    return;

                _canSkip = value;
                base.OnPropertyChanged("CanSkip");
            }
        }

        private string _BackgroundColor;
        public string BackgroundColor
        {
            get { return _BackgroundColor; }
            set { _BackgroundColor = value; OnPropertyChanged("BackgroundColor"); }
        }

        private string _CheckIcon;
        public string CheckIcon
        {
            get { return _CheckIcon; }
            set { _CheckIcon = value; OnPropertyChanged("CheckIcon"); }
        }

        private Visibility _statusDescVisibility = Visibility.Collapsed;
        public Visibility StatusDescVisibility
        {
            get { return _statusDescVisibility; }
            set { _statusDescVisibility = value; OnPropertyChanged("StatusDescVisibility"); }
        }
	}
}
