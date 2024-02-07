using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACWSSK.Model
{
    public class QRReceiptModel
    {
        public class ByApp
        {
            public string CompanyName { get; set; }
            public string SSMNo { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Address4 { get; set; }
            public string GSTNo { get; set; }
            public string RefNo { get; set; }
            public string TxDate { get; set; }
            public string ComponentCode { get; set; }
            public string Amt { get; set; }
            public string ST { get; set; }
            public string Tax { get; set; }
            public string TA { get; set; }
            public string Id { get; set; }
            public string UserId { get; set; }
            public string WalletId { get; set; }
            public string ReferenceKey { get; set; }
            public string Careline { get; set; }
            public string Email { get; set; }
        }

        public class ByAppFailed
        {
            public string CompanyName { get; set; }
            public string SSMNo { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Address4 { get; set; }
            public string GSTNo { get; set; }
            public string RefNo { get; set; }
            public string TxDate { get; set; }
            public string ComponentCode { get; set; }
            public string Amt { get; set; }
            public string ST { get; set; }
            public string Tax { get; set; }
            public string TA { get; set; }
            public string EWPaymentType { get; set; }
            public string Code { get; set; }
            public string Careline { get; set; }
            public string Email { get; set; }
        }

        public class ByCard
        {
            public string CompanyName { get; set; }
            public string SSMNo { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Address4 { get; set; }
            public string GSTNo { get; set; }
            public string RefNo { get; set; }
            public string TxDate { get; set; }
            public string ComponentCode { get; set; }
            public string Amt { get; set; }
            public string ST { get; set; }
            public string Tax { get; set; }
            public string TA { get; set; }
            public string CCCardType { get; set; }
            public string CCCardNo { get; set; }
            public string CCTerminalId { get; set; }
            public string CCRefNo { get; set; }
            public string CCApprCode { get; set; }
            public string CCHashPAN { get; set; }
            public string CCRespCode { get; set; }
            public string Careline { get; set; }
            public string Email { get; set; }
        }

        public class ByCardFailed
        {
            public string CompanyName { get; set; }
            public string SSMNo { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Address4 { get; set; }
            public string GSTNo { get; set; }
            public string RefNo { get; set; }
            public string TxDate { get; set; }
            public string ComponentCode { get; set; }
            public string Amt { get; set; }
            public string ST { get; set; }
            public string Tax { get; set; }
            public string TA { get; set; }
            public string CCCardType { get; set; }
            public string CCCardNo { get; set; }
            public string CCTerminalId { get; set; }
            public string CCRefNo { get; set; }
            public string CCApprCode { get; set; }
            public string CCHashPAN { get; set; }
            public string CCRespCode { get; set; }
            public string Careline { get; set; }
            public string Email { get; set; }
        }

        public class ByEWallet
        {
            public string CompanyName { get; set; }
            public string SSMNo { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Address4 { get; set; }
            public string GSTNo { get; set; }
            public string RefNo { get; set; }
            public string TxDate { get; set; }
            public string ComponentCode { get; set; }
            public string Amt { get; set; }
            public string ST { get; set; }
            public string Tax { get; set; }
            public string TA { get; set; }
            public string EWPaymentType { get; set; }
            public string EWMerchantCode { get; set; }
            public string EWTerminalId { get; set; }
            public string EWRefNo { get; set; }
            public string EWApprCode { get; set; }
            public string EWPaymentId { get; set; }
            public string EWRespCode { get; set; }
            public string Careline { get; set; }
            public string Email { get; set; }
        }

        public class ByEWalletFailed
        {
            public string CompanyName { get; set; }
            public string SSMNo { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string Address3 { get; set; }
            public string Address4 { get; set; }
            public string GSTNo { get; set; }
            public string RefNo { get; set; }
            public string TxDate { get; set; }
            public string ComponentCode { get; set; }
            public string Amt { get; set; }
            public string ST { get; set; }
            public string Tax { get; set; }
            public string TA { get; set; }
            public string EWPaymentType { get; set; }
            public string EWMerchantCode { get; set; }
            public string EWTerminalId { get; set; }
            public string EWRefNo { get; set; }
            public string EWApprCode { get; set; }
            public string EWPaymentId { get; set; }
            public string EWRespCode { get; set; }
            public string Careline { get; set; }
            public string Email { get; set; }
        }

        public class PaymentType
        {
            public bool ByApp { get; set; }
            public bool ByCard { get; set; }
            public bool ByEWallet { get; set; }
            public bool ByAppFailed { get; set; }
            public bool ByCardFailed { get; set; }
            public bool ByEWalletFailed { get; set; }
        }

        public class QRReceiptRequest
        {
            public List<PaymentType> PaymentType { get; set; }
            public List<ByApp> ByApp { get; set; }
            public List<ByEWallet> ByEWallet { get; set; }
            public List<ByCard> ByCard { get; set; }
            public List<ByAppFailed> ByAppFailed { get; set; }
            public List<ByCardFailed> ByCardFailed { get; set; }
            public List<ByEWalletFailed> ByEWalletFailed { get; set; }
        }

        public class QRReceiptResponse
        {
            public string ReturnCode { get; set; }
            public string Remark { get; set; }
            public string ResponseURL { get; set; }
        }


    }
}
