//BkashCreatePaymentResponse.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.BkashModels
{
    public class BkashCreatePaymentResponse
    {
        public string? paymentID { get; set; }
        public string? bkashURL { get; set; }
        public string? callbackURL { get; set; }
        public string? successCallbackURL { get; set; }
        public string? failureCallbackURL { get; set; }
        public string? cancelledCallbackURL { get; set; }
        public decimal amount { get; set; }
        public string? intent { get; set; }
        public string? currency { get; set; }
        public string? paymentCreateTime { get; set; }
        public string? transactionStatus { get; set; }
        public string? merchantInvoiceNumber { get; set; }
        public string? statusCode { get; set; }
        public string? statusMessage { get; set; }
    }
}