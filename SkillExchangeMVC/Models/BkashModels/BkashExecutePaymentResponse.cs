//BkashExecutePaymentResponse.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.BkashModels
{
    public class BkashExecutePaymentResponse
    {
        public string? paymentID { get; set; }
        public string? paymentCreateTime { get; set; }
        public string? transactionStatus { get; set; }
        public decimal amount { get; set; }
        public string? currency { get; set; }
        public string? intent { get; set; }
        public string? merchantInvoiceNumber { get; set; }
        public string? trxID { get; set; }
        public string? statusCode { get; set; }
        public string? statusMessage { get; set; }
        public string? paymentExecuteTime { get; set; }
        public string? payerReference { get; set; }
    }
}