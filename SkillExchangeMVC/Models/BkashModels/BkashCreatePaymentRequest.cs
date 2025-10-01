//BkashCreatePaymentRequest.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.BkashModels
{
    public class BkashCreatePaymentRequest
    {
        public string mode { get; set; } = "0011"; // Default for bKash
        public string payerReference { get; set; } = "";
        public string callbackURL { get; set; } = "";
        public decimal amount { get; set; }
        public string currency { get; set; } = "BDT";
        public string intent { get; set; } = "sale";
        public string merchantInvoiceNumber { get; set; } = "";
    }
}