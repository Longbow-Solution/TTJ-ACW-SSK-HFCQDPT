using ACWSSK.App_Code;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACWSSK.Model
{
    public class CheckEquipStatusResponse
    {
        private int _code = 0;
        private string _iotStatus = string.Empty;
        private string _msg = string.Empty;

        [JsonProperty("code")]
        public int code
        {
            get { return _code; }
            set { _code = value; }
        }

        [JsonProperty("iotStatus")]
        public string iotStatus
        {
            get { return _iotStatus; }
            set { _iotStatus = value; }
        }

        [JsonProperty("msg")]
        public string msg
        {
            get { return _msg; }
            set { _msg = value; }
        }

        [JsonIgnore]
        public eQDIotStatus eIotStatus
        {
            get
            {
                switch (iotStatus) 
                {
                    case "READY":
                        return eQDIotStatus.READY;
                        break;
                    case "NOT_READY":
                        return eQDIotStatus.NOT_READY;
                        break;
                    case "WASHING":
                        return eQDIotStatus.WASHING;
                        break;
                    case "OFFLINE":
                        return eQDIotStatus.OFFLINE;
                        break;
                    case "CMD_ERROR":
                        return eQDIotStatus.CMD_ERROR;
                        break;
                    case "NOT_ANSWER":
                        return eQDIotStatus.NOT_ANSWER;
                        break;
                    case "LOCKING":
                        return eQDIotStatus.LOCKING;
                        break;
                    case "ERROR":
                        return eQDIotStatus.ERROR;
                        break;
                    default:
                        return eQDIotStatus.ERROR;
                }
            }
        }
    }
}
