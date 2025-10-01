//BkashRefundResponse.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.BkashModels
{
    public class BkashRefundResponse
    {
        public string? refundTrxID { get; set; }
        public string? originalTrxID { get; set; }
        public decimal amount { get; set; }
        public string? currency { get; set; }
        public decimal charge { get; set; }
        public string? refundTime { get; set; }
        public string? transactionStatus { get; set; }
        public string? statusCode { get; set; }
        public string? statusMessage { get; set; }
    }
}