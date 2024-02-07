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
            public string Code { get; set; }
            public string Message { get; set; }
            public string Debug { get; set; }
            public string StackTrace { get; set; }

        }
    }
}
