//BkashRefundRequest.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.BkashModels
{
    public class BkashRefundRequest
    {
        public string paymentID { get; set; } = "";
        public decimal amount { get; set; }
        public string trxID { get; set; } = "";
        public string sku { get; set; } = "payment";
        public string reason { get; set; } = "";
    }
}