using System;

namespace ACWSSK.Model
{
    public class ACWAppAPI
    {
        public class ACWAppAPIResponse 
        {
            public Item item { get; set; }
        }
        public class Item
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string WalletId { get; set; }
            public string ReferenceKey { get; set; }
            public string ReferenceId { get; set; }
            public decimal Amount { get; set; }
            public string Token { get; set; }
            public string Type { get; set; }
            public string PaymentType { get; set; }
            public string Status { get; set; }
            public string Remark { get; set; }
            public string TransactionAt { get; set; }
            public object ExtraInfo { get; set; }
            public string CreatedAt { get; set; }
            public string UpdatedAt { get; set; }
        }

        public class ACWAppAPIResponseFailed
        {
            public Error error { get; set; }
        }

        public class ACWAppAPISandboxResponseFailed
        {
            public SBError error { get; set; }
        }

        public class Error
        {
            public string code { get; set; }
            public string message { get; set; }
        }

        public class SBError
        {
            public string code { get; set; }
            public string message { get; set; }
            public string debug { get; set; }
            public string stackTrace { get; set; }
        }

        public class PriceItem
        {
            public string branchKioskId { get; set; }
            public string price { get; set; }
        }

        public class GetKioskPriceResponse
        {
            public PriceItem item { get; set; }
        }

    }
}
