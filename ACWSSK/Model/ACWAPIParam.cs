using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACWSSK.Model
{
    public class CheckEquipStatusParam
    {
        private string _openId = string.Empty;
        private long _time = 0;
        private int _iotId = 0;

        public CheckEquipStatusParam(string openid, long time, int iotid) 
        {
            _openId = openid;
            _time = time;
            _iotId = iotid;
        }

        [JsonProperty("openId")]
        public string openId
        {
            get { return _openId; }
            set { _openId = value; }
        }

        [JsonProperty("time")]
        public long time
        {
            get { return _time; }
            set { _time = value; }
        }

        [JsonProperty("iotId")]
        public int iotId
        {
            get { return _iotId; }
            set { _iotId = value; }
        }

    }

    public class SendAppApiParam
    {
        private string _key = string.Empty;
        private int _amount = 0;

        public SendAppApiParam(string key, int amount)
        {
            _key = key;
            _amount = amount;
        }

        [JsonProperty("key")]
        public string key
        {
            get { return _key; }
            set { _key = value; }
        }

        [JsonProperty("amount")]
        public int amount
        {
            get { return _amount; }
            set { _amount = value; }
        }
    }
}
