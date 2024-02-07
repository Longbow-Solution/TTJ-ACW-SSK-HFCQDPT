using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACWSSK.Model
{
    public class Transaction : BaseViewModel
    {
        private string _referenceType;
        private int _componentId;

        public int Id { get; set; }
        public string ReferenceNo { get; set; }

        public Transaction() { }

        public Transaction(string referenceType, int componentId)
        {
            _referenceType = referenceType;
            _componentId = componentId;

            Id = 0;
            ReferenceNo = null;
        }

        public Transaction(int id)
        {
            Id = id;
            ReferenceNo = null;
        }

        public Transaction(int id, string referenceNo)
        {
            Id = id;
            ReferenceNo = referenceNo;
        }

        public int NextId()
        {
            Id += 1;
            return Id;
        }

        public string NextReferenceNo()
        {
            if (string.IsNullOrEmpty(ReferenceNo))
                return NewReferenceNo(_referenceType, _componentId);

            string[] s = ReferenceNo.Split('-');

            string refType = s[0];
            string compId = s[1];

            int year = int.Parse(s[2].Substring(0, 2));
            int sequence = int.Parse(s[2].Substring(2));
            int currentYear = int.Parse(DateTime.Now.ToString("yy"));

            if (currentYear == year)
                sequence++;
            else
            {
                year = currentYear;
                sequence = 1;
            }

            ReferenceNo = string.Format("{0}-{1}-{2}{3}", refType, compId, year.ToString("00"), sequence.ToString("000000"));
            return ReferenceNo;
        }

        public string NewReferenceNo(string refType, int compId)
        {
            int year = int.Parse(DateTime.Now.ToString("yy"));
            int sequence = 1;

            ReferenceNo = string.Format("{0}-{1}-{2}{3}", refType, compId.ToString("000"), year.ToString("00"), sequence.ToString("000000"));
            return ReferenceNo;
        }
    }
}
